using System;

// ReSharper disable once CheckNamespace
namespace Singularity.DataService.SqlFramework
{
	public sealed class SqlColumn
	{
		public SqlColumn()
		{
			DefaultSchema = "dbo";
			//RuleSchema = "dbo";
		}

		public SqlColumn(String name) : this()
		{
			Name = name;
		}

		public SqlColumn(String name, ESqlDataTypes eSqlDataType) : this(name)
		{
			ESqlDataType = eSqlDataType;
		}

		// /// <summary>Clears classification fields</summary>
		// /// <returns></returns>
		//public void RemoveClassification()
		//{
		//	if (!this.IsSupportedProperty("SensitivityLabelId"))
		//		return;
		//	this.SensitivityLabelId = string.Empty;
		//	this.SensitivityLabelName = string.Empty;
		//	this.SensitivityInformationTypeId = string.Empty;
		//	this.SensitivityInformationTypeName = string.Empty;
		//}

		/// <summary>
		/// Appends corresponding data classification script to the main query
		/// </summary>
		//public void ScriptDataClassification(StringCollection queries, ScriptingPreferences sp, Boolean forCreateScript = false)
		//{
		//	if (!this.IsSupportedProperty("SensitivityLabelId", sp) || this.State != SqlSmoState.Creating && this.State != SqlSmoState.Existing)
		//		return;
		//	Boolean flag1 = !string.IsNullOrEmpty(SensitivityLabelId) || !string.IsNullOrEmpty(SensitivityLabelName) || !string.IsNullOrEmpty(SensitivityInformationTypeId) || !string.IsNullOrEmpty(SensitivityInformationTypeName);
		//	Boolean flag2 = this.State == SqlSmoState.Existing && (property1.Dirty || property2.Dirty || property3.Dirty || property4.Dirty);
		//	if (flag1 || flag2)
		//	{
		//		if (!(this.Parent is Table))
		//			throw new WrongPropertyValueException(ExceptionTemplatesImpl.NoDataClassificationOnNonTables);
		//		if (this.GetPropValueOptional<Boolean>("Computed", false))
		//			throw new WrongPropertyValueException(ExceptionTemplatesImpl.NoDataClassificationOnComputedColumns);
		//		if (string.IsNullOrEmpty((String)property2.Value) && !string.IsNullOrEmpty((String)property1.Value))
		//			throw new WrongPropertyValueException(ExceptionTemplatesImpl.SensitivityLabelNameIsMissing);
		//		if (string.IsNullOrEmpty((String)property4.Value) && !string.IsNullOrEmpty((String)property3.Value))
		//			throw new WrongPropertyValueException(ExceptionTemplatesImpl.SensitivityInformationTypeNameIsMissing);
		//	}
		//	if ((this.State == SqlSmoState.Creating || forCreateScript) && flag1)
		//	{
		//		queries.Add(this.ScriptAddDataClassification());
		//	}
		//	else
		//	{
		//		if (this.State != SqlSmoState.Existing || !flag2)
		//			return;
		//		queries.Add(flag1 ? this.ScriptAddDataClassification() : this.ScriptDropDataClassification());
		//	}
		//}

		public String Name { get; set; }
		public Int32 OrdinalPosition { get; set; }

		// if (ESqlDataType == ESqlDataTypes.UserDefinedDataType) return DataType.UserDefinedDataTypeToEnum(this.GetServerObject().Databases[this.GetDBName()].UserDefinedDataTypes[this.DataType.Name, this.DataType.Schema]);
		public ESqlDataTypes ESqlDataType { get; set; }

		public Boolean Nullable { get; set; }
		public SqlDefaultConstraint SqlDefaultConstraint { get; set; }
		public Int32 Length { get; set; }
		public Int32 Precision { get; set; }
		public Boolean IsPrimaryKey { get; set; }

		public String Default
		{
			get => _default ?? "";
			set
			{
				if (_default != null)
				{
					throw new InvalidOperationException("Default can only be set once.");
				}

				_default = value;
			}
		}
		private String _default;

		//public String DefaultConstraintName
		//{
		//	get => _defaultConstraintName ?? "";
		//	set
		//	{
		//		if (_defaultConstraintName != null)
		//		{
		//			throw new InvalidOperationException("DefaultConstraintName can only be set once.");
		//		}

		//		_defaultConstraintName = value;
		//	}
		//}
		//private String _defaultConstraintName;

		public String DefaultSchema
		{
			get => _defaultSchema ?? "";
			set
			{
				if (_defaultSchema != null)
				{
					throw new InvalidOperationException("DefaultSchema can only be set once.");
				}

				_defaultSchema = value;
			}
		}
		private String _defaultSchema;

		//public String RuleSchema
		//{
		//	get => _ruleSchema ?? "";
		//	set
		//	{
		//		if (_ruleSchema != null)
		//		{
		//			throw new InvalidOperationException("RuleSchema can only be set once.");
		//		}

		//		_ruleSchema = value;
		//	}
		//}
		//private String _ruleSchema;

		//public String Collation { get; set; }

		//public Int32 ColumnEncryptionKeyId
		//{
		//	get => _columnEncryptionKeyId.GetValueOrDefault(0);
		//	set
		//	{
		//		if (_columnEncryptionKeyId != null)
		//		{
		//			throw new InvalidOperationException("ColumnEncryptionKeyId can only be set once.");
		//		}

		//		_columnEncryptionKeyId = value;
		//	}
		//}
		//private Int32? _columnEncryptionKeyId;

		//public String ColumnEncryptionKeyName
		//{
		//	get => _columnEncryptionKeyName ?? "";
		//	set
		//	{
		//		if (_columnEncryptionKeyName != null)
		//		{
		//			throw new InvalidOperationException("ColumnEncryptionKeyName can only be set once.");
		//		}

		//		_columnEncryptionKeyName = value;
		//	}
		//}
		//private String _columnEncryptionKeyName;

		public Boolean Computed
		{
			get => _computed.GetValueOrDefault(false);
			set
			{
				if (_computed != null)
				{
					throw new InvalidOperationException("Computed can only be set once.");
				}

				_computed = value;
			}
		}
		private Boolean? _computed;

		public String ComputedText
		{
			get => _computedText ?? "";
			set
			{
				if (_computedText != null)
				{
					throw new InvalidOperationException("ComputedText can only be set once.");
				}

				_computedText = value;
			}
		}
		private String _computedText;

		//public String EncryptionAlgorithm
		//{
		//	get => _encryptionAlgorithm ?? "";
		//	set
		//	{
		//		if (_encryptionAlgorithm != null)
		//		{
		//			throw new InvalidOperationException("EncryptionAlgorithm can only be set once.");
		//		}

		//		_encryptionAlgorithm = value;
		//	}
		//}
		//private String _encryptionAlgorithm;

		//public ColumnEncryptionType EncryptionType
		//{
		//	get => _encryptionType ?? 0;
		//	set
		//	{
		//		if (_encryptionType != null)
		//		{
		//			throw new InvalidOperationException("EncryptionType can only be set once.");
		//		}

		//		_encryptionType = value;
		//	}
		//}
		//private ColumnEncryptionType _encryptionType;

		//public Boolean Identity { get; set; }
		//{
		//	get => _identity ?? 0;
		//	set
		//	{
		//		if (_identity != null)
		//		{
		//			throw new InvalidOperationException("EncryptionType can only be set once.");
		//		}

		//		_identity = value;
		//	}
		//}
		//private Boolean _identity;

		//public Int64 IdentityIncrement { get; set; }
		//{
		//	get => _identityIncrement ?? 0;
		//	set
		//	{
		//		if (_identityIncrement != null)
		//		{
		//			throw new InvalidOperationException("IdentityIncrement can only be set once.");
		//		}

		//		_identityIncrement = value;
		//	}
		//}
		//private Int64 _identityIncrement;

		//public Decimal IdentityIncrementAsDecimal { get; set; }
		//{
		//	get => _identityIncrementAsDecimal ?? 0;
		//	set
		//	{
		//		if (_identityIncrementAsDecimal != null)
		//		{
		//			throw new InvalidOperationException("IdentityIncrementAsDecimal can only be set once.");
		//		}

		//		_identityIncrementAsDecimal = value;
		//	}
		//}
		//private Decimal _identityIncrementAsDecimal;

		//public Int64 IdentitySeed { get; set; }
		//{
		//	get => _identitySeed ?? 0;
		//	set
		//	{
		//		if (_identitySeed != null)
		//		{
		//			throw new InvalidOperationException("IdentitySeed can only be set once.");
		//		}

		//		_identitySeed = value;
		//	}
		//}
		//private Int64 _identitySeed;

		//public Decimal IdentitySeedAsDecimal { get; set; }
		//{
		//	get => _identitySeedAsDecimal ?? 0d;
		//	set
		//	{
		//		if (_identitySeedAsDecimal != null)
		//		{
		//			throw new InvalidOperationException("IdentitySeedAsDecimal can only be set once.");
		//		}

		//		_identitySeedAsDecimal = value;
		//	}
		//}
		//private Decimal _identitySeedAsDecimal;



		//public GeneratedAlwaysType GeneratedAlwaysType { get; set; }
		//public GraphType GraphType { get; set; }
		//public Int32 ID { get; set; }
		//public Boolean InPrimaryKey { get; set; }
		//public Boolean IsClassified { get; set; }

		//public Boolean IsColumnSet { get; set; }
		//{
		//	get => _isColumnSet ?? false;
		//	set
		//	{
		//		if (_isColumnSet != null)
		//		{
		//			throw new InvalidOperationException("IsColumnSet can only be set once.");
		//		}

		//		_isColumnSet = value;
		//	}
		//}
		//private Boolean _isColumnSet;


		//public Boolean IsDeterministic { get; set; }

		//public Boolean IsFileStream { get; set; }
		//{
		//	get => _isFileStream ?? false;
		//	set
		//	{
		//		if (_isFileStream != null)
		//		{
		//			throw new InvalidOperationException("IsFileStream can only be set once.");
		//		}

		//		_isFileStream = value;
		//	}
		//}
		//private Boolean _isFileStream;

		//public Boolean IsForeignKey { get; set; }
		//public Boolean IsFullTextIndexed { get; set; }
		//public Boolean IsHidden { get; set; }
		//public Boolean IsMasked { get; set; }
		//public Boolean IsPersisted { get; set; }
		//public Boolean IsPrecise { get; set; }
		//public Boolean IsSparse { get; set; }
		//public String MaskingFunction { get; set; }

		//public Boolean NotForReplication { get; set; }
		//{
		//	get => _notForReplication ?? false;
		//	set
		//	{
		//		if (_notForReplication != null)
		//		{
		//			throw new InvalidOperationException("NotForReplication can only be set once.");
		//		}

		//		_notForReplication = value;
		//	}
		//}
		//private Boolean _notForReplication;


		//public Boolean RowGuidCol { get; set; }

		//public String Rule { get; set; }
		//{
		//	get => _rule ?? "";
		//	set
		//	{
		//		if (_rule != null)
		//		{
		//			throw new InvalidOperationException("Rule can only be set once.");
		//		}

		//		_rule = value;
		//	}
		//}
		//private String _rule;


		//public String SensitivityInformationTypeId { get; set; }
		//public String SensitivityInformationTypeName { get; set; }
		//public String SensitivityLabelId { get; set; }
		//public String SensitivityLabelName { get; set; }
		//public Int32 StatisticalSemantics { get; set; }
		//public String DistributionColumnName { get; set; }
		//public Boolean IsDistributedColumn { get; set; }

		//public ExtendedPropertyCollection ExtendedProperties
		//{
		//	get
		//	{
		//		this.ThrowIfBelowVersion80((String)null);
		//		this.CheckObjectState();
		//		if (this.m_ExtendedProperties == null)
		//			this.m_ExtendedProperties = new ExtendedPropertyCollection((SqlSmoObject)this);
		//		return this.m_ExtendedProperties;
		//	}
		//}

		//public Boolean IsEncrypted
		//{
		//	get
		//	{
		//		if (this.IsSupportedProperty("ColumnEncryptionKeyID"))
		//			return null != this.GetPropValueOptionalAllowNull("ColumnEncryptionKeyID");
		//		return false;
		//	}
		//}

		//public DefaultConstraint DefaultConstraint
		//{
		//	get
		//	{
		//		this.InitDefaultConstraint(false);
		//		return this.defaultConstraint;
		//	}
		//	internal set
		//	{
		//		this.defaultConstraint = value;
		//		this.DefaultConstraintName = this.defaultConstraint == null ? string.Empty : this.defaultConstraint.Name;
		//	}
		//}

		//public DataTable EnumForeignKeys()
		//public DataTable EnumIndexes()
	}

	public class SqlDefaultConstraint
	{
		public String Text { get; set; }
		public String Value { get; set; }
	}

}
