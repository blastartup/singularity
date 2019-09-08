using System;
using System.Collections;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Singularity.DataService.SqlFramework
{
	public sealed class SqlUserDefinedDataTypeCollection : SqlObject, IEnumerable<SqlUserDefinedDataType>, ICollection<SqlUserDefinedDataType>
	{
		public SqlUserDefinedDataTypeCollection() : base()
		{
		}

		public SqlUserDefinedDataTypeCollection(String name) : base(name)
		{
		}

		public Int32 Count => InternalList.Count;
		public Boolean IsReadOnly => false;
		public void Add(SqlUserDefinedDataType item) => InternalList.Add(item);
		public void Clear() => InternalList.Clear();
		public Boolean Contains(SqlUserDefinedDataType item) => InternalList.Contains(item);
		public void CopyTo(SqlUserDefinedDataType[] array, Int32 arrayIndex) => InternalList.CopyTo(array, arrayIndex);
		public IEnumerator<SqlUserDefinedDataType> GetEnumerator() => InternalList.GetEnumerator();
		public Boolean Remove(SqlUserDefinedDataType item) => InternalList.Remove(item);
		public SqlUserDefinedDataType this[Int32 index]
		{
			get => InternalList[index];
			set => InternalList[index] = value;
		}

		IEnumerator IEnumerable.GetEnumerator() => InternalList.GetEnumerator();

		private List<SqlUserDefinedDataType> InternalList => _internalList ?? (_internalList = new List<SqlUserDefinedDataType>());
		private List<SqlUserDefinedDataType> _internalList;
	}
}