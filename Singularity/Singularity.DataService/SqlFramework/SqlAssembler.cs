﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Singularity.DataService.SqlFramework
{
	public abstract class SqlAssembler
	{
		protected SqlAssembler(SqlDataReader sqlDataReader)
		{
			DataReader = sqlDataReader;
		}

		/// <summary>
		/// Safely return a value of a given column from a SqlDataReader, whether or not that column name was specified in the original SQL command.
		/// </summary>
		/// <typeparam name="TValue">The data type expected of the column value returned.</typeparam>
		/// <param name="columnName">Name of column whose value is to be returned.</param>
		/// <returns>Should the column exist, then its value is returned.  If the value is DBNull or the column doesn't exist, the default value of the T data type is returned.</returns>
		/// <remarks>Currently Nothing (AKA null) is never returned.</remarks>
		protected TValue ReadValue<TValue>(String columnName)
		{
			return ReadValue(columnName, default(TValue));
		}

		protected TValue ReadValue<TValue>(String columnName, TValue defaultValue)
		{
			TValue result = defaultValue;
			try
			{
				Object value = DataReader[columnName];
				if (value != null)
				{
					result = (TValue)value;
				}
			}
			catch(IndexOutOfRangeException) { }
			catch(InvalidCastException) { }
			

			return result;
		}

		public SqlDataReader DataReader { get; private set; }
	}

	public abstract class SqlAssembler<TClass> : SqlAssembler
		where TClass : class, new()
	{
		protected SqlAssembler(SqlDataReader sqlDataReader) : base(sqlDataReader)
		{
		}

		public List<TClass> AssembleClassList()
		{
			var classList = new List<TClass>();
			while (DataReader.Read())
			{
				TClass newClass = new TClass();
				AssembleClassCore(newClass);
				classList.Add(newClass);
			}
			return classList;
		}

		/// <summary>
		/// After performing a Read(), assemble the Class if a row exists.
		/// </summary>
		/// <returns>InvoiceItemClass populated by your database read.</returns>
		/// <remarks>Bypasses the default behaviour where the AssembleClass will automatically do a Read() for you.</remarks>
		public TClass ReadAndAssembleClass()
		{
			TClass newClass = null;
			if (DataReader.Read())
			{
				newClass = new TClass();
				AssembleClassCore(newClass);
			}
			return newClass;
		}

		/// <summary>
		/// Assemble the Class after you have already called a Read() prior to calling this Function and a row exists.
		/// </summary>
		/// <returns>InvoiceItemClass populated by your database read.</returns>
		/// <remarks>Bypasses the default behaviour where the AssembleClass will automatically do a Read() for you.</remarks>
		public TClass AssembleClass()
		{
			TClass newClass = null;
			if (DataReader.HasRows)
			{
				newClass = new TClass();
				AssembleClassCore(newClass);
			}
			return newClass;
		}

		protected abstract void AssembleClassCore(TClass newClass);
	}
}
