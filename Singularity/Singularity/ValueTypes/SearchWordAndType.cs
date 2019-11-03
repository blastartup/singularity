﻿using System;
using System.Diagnostics;

// ReSharper disable once CheckNamespace

namespace Singularity
{
	[DebuggerStepThrough]
	public struct SearchWordAndType : IStateEmpty
	{
		public SearchWordAndType(String searchWord) : this(searchWord, ESearchTypes.Exactly) {}

		public SearchWordAndType(String searchWord, ESearchTypes searchType)
		{
			_word = searchWord;
			_searchType = searchType;
		}

		public String Word
		{
			get => _word;
			set => _word = value;
		}

		private String _word;

		public ESearchTypes Type
		{
			get => _searchType;
			set => _searchType = value;
		}

		private ESearchTypes _searchType;

		public Boolean IsEmpty => Word.IsEmpty();
	}
}