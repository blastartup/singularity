﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Singularity.DataService
{
	public abstract class EfDbContext : DbContext
	{
		protected EfDbContext() : base() { }

		protected EfDbContext(String nameOrConnectionString) : base(nameOrConnectionString)
		{ }

		protected override DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry, IDictionary<Object, Object> items)
		{
			if (ValidateEntryFunc == null)
			{
				return base.ValidateEntity(entityEntry, items);
			}
			return ValidateEntryFunc(entityEntry, items) ?? base.ValidateEntity(entityEntry, items);
		}

		#region BulkInsert

		// https://www.codeproject.com/Articles/1173837/BulkInsert-with-the-Entity-Framework

		public void BulkInsertAll(IList entities, SqlTransaction transaction = null, Boolean recursive = false)
		{
			BulkInsertAll(entities, transaction, recursive, new Dictionary<Object, Object>(new IdentityEqualityComparer<Object>()));
		}

		private void BulkInsertAll(IList entities, SqlTransaction transaction, Boolean recursive, Dictionary<Object, Object> savedEntities)
		{
			if (entities.Count == 0) return;

			var objectContext = ((IObjectContextAdapter)this).ObjectContext;
			var workspace = objectContext.MetadataWorkspace;

			Type t = entities[0].GetType();

			var mappings = GetMappings(workspace, objectContext.DefaultContainerName, t.Name);
			if (recursive)
			{
				foreach (var fkMapping in mappings.ToForeignKeyMappings)
				{
					var navProperties = new HashSet<Object>();
					var modifiedEntities = new List<Object[]>();
					foreach (var entity in entities)
					{
						var navProperty = GetProperty(fkMapping.NavigationPropertyName, entity);
						var navPropertyKey = GetProperty(fkMapping.ToProperty, entity);

						if (navProperty != null && navPropertyKey == 0)
						{
							var currentValue = GetProperty(fkMapping.FromProperty, navProperty);
							if (currentValue > 0)
							{
								SetProperty(fkMapping.ToProperty, entity, currentValue);
							}
							else
							{
								navProperties.Add(navProperty);
								modifiedEntities.Add(new Object[] { entity, navProperty });
							}
						}
					}
					if (navProperties.Any())
					{
						BulkInsertAll(navProperties.ToArray(), transaction, true, savedEntities);
						foreach (var modifiedEntity in modifiedEntities)
						{
							var e = modifiedEntity[0];
							var p = modifiedEntity[1];
							SetProperty(fkMapping.ToProperty, e, GetProperty(fkMapping.FromProperty, p));
						}
					}
				}
			}

			var validEntities = new ArrayList();
			var ignoredEntities = new ArrayList();
			foreach (dynamic entity in entities)
			{
				if (savedEntities.ContainsKey(entity))
				{
					ignoredEntities.Add(entity);
					continue;
				}
				validEntities.Add(entity);
				savedEntities.Add(entity, entity);
			}
			BulkInsertAll(validEntities, t, mappings, transaction);

			if (recursive)
			{
				foreach (var fkMapping in mappings.FromForeignKeyMappings)
				{
					var navigationPropertyName = fkMapping.NavigationPropertyName;

					var navPropertyEntities = new List<dynamic>();
					foreach (var entity in entities)
					{
						if (fkMapping.BuiltInTypeKind == BuiltInTypeKind.CollectionType ||
							 fkMapping.BuiltInTypeKind ==
							 BuiltInTypeKind.CollectionKind)
						{
							var navProperties = GetProperty(navigationPropertyName, entity);

							foreach (var navProperty in navProperties)
							{
								SetProperty(fkMapping.ToProperty, navProperty, GetProperty(fkMapping.FromProperty, entity));
								navPropertyEntities.Add(navProperty);
							}
						}
						else
						{
							var navProperty = GetProperty(navigationPropertyName, entity);
							if (navProperty != null)
							{
								SetProperty(fkMapping.ToProperty, navProperty, GetProperty(fkMapping.FromProperty, entity));
								navPropertyEntities.Add(navProperty);
							}
						}
					}

					if (navPropertyEntities.Any())
					{
						BulkInsertAll(navPropertyEntities.ToArray(), transaction, true, savedEntities);
					}
				}
			}
		}

		private void BulkInsertAll(IList entities, Type t, Mappings mappings, SqlTransaction transaction = null)
		{
			Set(t).ToString();
			var tableName = GetTableName(t);
			var columnMappings = mappings.ColumnMappings;

			var conn = (SqlConnection)Database.Connection;
			if (conn.State == ConnectionState.Closed)
				conn.Open();
			var bulkCopy = new SqlBulkCopy(conn, SqlBulkCopyOptions.Default, transaction) { DestinationTableName = tableName };

			var properties = t.GetProperties().Where(p => columnMappings.ContainsKey(p.Name)).ToArray();
			var table = new DataTable();
			foreach (var property in properties)
			{
				Type propertyType = property.PropertyType;

				// Nullable properties need special treatment.
				if (propertyType.IsGenericType &&
					 propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
				{
					propertyType = Nullable.GetUnderlyingType(propertyType);
				}

				// Ignore all properties that we have no mappings for.
				if (columnMappings.ContainsKey(property.Name))
				{
					// Since we cannot trust the CLR type properties to be in the same order as
					// the table columns we use the SqlBulkCopy column mappings.
					table.Columns.Add(new DataColumn(property.Name, propertyType));
					var clrPropertyName = property.Name;
					var tableColumnName = columnMappings[property.Name].ColumnProperty.Name;
					bulkCopy.ColumnMappings.Add(new SqlBulkCopyColumnMapping(clrPropertyName, tableColumnName));
				}
			}

			// Add all our entities to our data table
			foreach (var entity in entities)
			{
				var e = entity;
				table.Rows.Add(properties.Select(property => GetPropertyValue(property.GetValue(e, null))).ToArray());
			}

			var cmd = conn.CreateCommand();
			cmd.Transaction = transaction;

			// Check to see if the table has a primary key with auto identity set. If so
			// set the generated primary key values on the entities.
			var pkColumn = columnMappings.Values.Where(m => m.ColumnProperty.IsStoreGeneratedIdentity).Select(m => m.ColumnProperty).SingleOrDefault();

			if (pkColumn != null)
			{
				var pkColumnName = pkColumn.Name;
				var pkColumnType = Type.GetType(pkColumn.PrimitiveType.ClrEquivalentType.FullName);

				// Get the number of existing rows in the table.
				cmd.CommandText = $@"SELECT COUNT(*) FROM {tableName}";
				var result = cmd.ExecuteScalar();
				var count = Convert.ToInt64(result);

				// Get the identity increment value
				cmd.CommandText = $"SELECT IDENT_INCR('{tableName}')";
				result = cmd.ExecuteScalar();
				dynamic identIncrement = Convert.ChangeType(result, pkColumnType);

				// Get the last identity value generated for our table
				cmd.CommandText = $"SELECT IDENT_CURRENT('{tableName}')";
				result = cmd.ExecuteScalar();
				dynamic identcurrent = Convert.ChangeType(result, pkColumnType);

				var nextId = identcurrent + (count > 0 ? identIncrement : 0);

				bulkCopy.BulkCopyTimeout = 5 * 60;
				bulkCopy.WriteToServer(table);

				cmd.CommandText = $"SELECT SCOPE_IDENTITY()";
				result = cmd.ExecuteScalar();
				dynamic lastId = Convert.ChangeType(result, pkColumnType);

				cmd.CommandText = $"SELECT {pkColumnName} From {tableName} WHERE {pkColumnName} >= {nextId} and {pkColumnName} <= {lastId}";
				var reader = cmd.ExecuteReader();
				var ids = (from IDataRecord r in reader
							  let pk = r[pkColumnName]
							  select pk)
								.OrderBy(i => i)
								.ToArray();
				if (ids.Length != entities.Count) throw new ArgumentException("More id values generated than we had entities. Something went wrong, try again.");


				for (Int32 i = 0; i < entities.Count; i++)
				{
					SetProperty(pkColumnName, entities[i], ids[i]);
				}
			}
			else
			{
				bulkCopy.BulkCopyTimeout = 5 * 60;
				bulkCopy.WriteToServer(table);
			}
		}

		private String GetTableName(Type t)
		{
			var dbSet = Set(t);
			var sql = dbSet.ToString();
			var regex = new Regex(@"FROM (?<table>.*) AS");
			var match = regex.Match(sql);
			return match.Groups["table"].Value;
		}

		private Object GetPropertyValue(Object o)
		{
			if (o == null)
				return DBNull.Value;
			return o;
		}

		private Mappings GetMappings(MetadataWorkspace workspace, String containerName, String entityName)
		{
			var columnMappings = new Dictionary<String, CLR2ColumnMapping>();
			var storageMapping = workspace.GetItem<GlobalItem>(containerName, DataSpace.CSSpace);
			dynamic temp = storageMapping.GetType().InvokeMember(
				 "EntitySetMappings",
				 BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance,
				 null, storageMapping, null);
			var entitySetMaps = new List<EntitySetMapping>();
			foreach (var t in temp)
			{
				entitySetMaps.Add((EntitySetMapping)t);
			}

			var entitySetMap = entitySetMaps.Single(m => m.EntitySet.ElementType.Name == entityName);
			var typeMappings = entitySetMap.EntityTypeMappings;
			EntityTypeMapping typeMapping = typeMappings[0];
			var fragments = typeMapping.Fragments;
			var fragment = fragments[0];
			var properties = fragment.PropertyMappings;
			foreach (var property in properties.Where(p => p is ScalarPropertyMapping).Cast<ScalarPropertyMapping>())
			{
				var clrProperty = property.Property;
				var columnProperty = property.Column;
				columnMappings.Add(clrProperty.Name, new CLR2ColumnMapping
				{
					CLRProperty = clrProperty,
					ColumnProperty = columnProperty,
				});
			}


			var foreignKeyMappings = new List<ForeignKeyMapping>();
			var navigationProperties =
				 typeMapping.EntityType.DeclaredMembers.Where(m => m.BuiltInTypeKind == BuiltInTypeKind.NavigationProperty)
					  .Cast<NavigationProperty>()
					  .Where(p => p.RelationshipType is AssociationType)
					  .ToArray();

			foreach (var navigationProperty in navigationProperties)
			{
				var relType = (AssociationType)navigationProperty.RelationshipType;

				if (foreignKeyMappings.All(m => m.Name != relType.Name))
				{
					var fkMapping = new ForeignKeyMapping
					{
						NavigationPropertyName = navigationProperty.Name,
						BuiltInTypeKind = navigationProperty.TypeUsage.EdmType.BuiltInTypeKind,
						Name = relType.Name,
						FromType = relType.Constraint.FromProperties.Single().DeclaringType.Name,
						FromProperty = relType.Constraint.FromProperties.Single().Name,
						ToType = relType.Constraint.ToProperties.Single().DeclaringType.Name,
						ToProperty = relType.Constraint.ToProperties.Single().Name,
					};
					foreignKeyMappings.Add(fkMapping);
				}
			}

			return new Mappings
			{
				ColumnMappings = columnMappings,
				ToForeignKeyMappings = foreignKeyMappings.Where(m => m.ToType == entityName).ToArray(),
				FromForeignKeyMappings = foreignKeyMappings.Where(m => m.FromType == entityName).ToArray()
			};
		}

		private dynamic GetProperty(String property, Object instance)
		{
			var type = instance.GetType();
			return type.InvokeMember(property, BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance, Type.DefaultBinder, instance, null);
		}

		private void SetProperty(String property, Object instance, Object value)
		{
			var type = instance.GetType();
			type.InvokeMember(property, BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance, Type.DefaultBinder, instance, new[] { value });
		}

		private class IdentityEqualityComparer<T> : IEqualityComparer<T> where T : class
		{
			public Int32 GetHashCode(T value)
			{
				return RuntimeHelpers.GetHashCode(value);
			}

			public Boolean Equals(T left, T right)
			{
				return left == right; // Reference identity comparison
			}
		}

		private class Mappings
		{
			public Dictionary<String, CLR2ColumnMapping> ColumnMappings { get; set; }
			public ForeignKeyMapping[] ToForeignKeyMappings { get; set; }
			public ForeignKeyMapping[] FromForeignKeyMappings { get; set; }
		}

		private class CLR2ColumnMapping
		{
			public EdmProperty CLRProperty { get; set; }
			public EdmProperty ColumnProperty { get; set; }
		}

		private class ForeignKeyMapping
		{
			public BuiltInTypeKind BuiltInTypeKind { get; set; }
			public String NavigationPropertyName { get; set; }
			public String Name { get; set; }
			public String FromType { get; set; }
			public String FromProperty { get; set; }
			public String ToType { get; set; }
			public String ToProperty { get; set; }
		}

		#endregion

		public Func<DbEntityEntry, IDictionary<Object, Object>, DbEntityValidationResult> ValidateEntryFunc { get; set; }

		protected internal abstract DateTime Now { get; }
	}
}
