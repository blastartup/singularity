using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace

namespace Singularity
{
	public class Synchronizer<TImpl, TIRead, TIWrite> where TImpl : TIWrite, TIRead
	{
		ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
		TImpl _shared;

		public Synchronizer(TImpl shared)
		{
			_shared = shared;
		}

		public TValue Read<TValue>(Func<TIRead, TValue> functor)
		{
			_lock.EnterReadLock();
			try
			{
				return functor.Invoke(_shared);
			}
			finally
			{
				_lock.ExitReadLock();
			}
		}

		public void Write(Action<TIWrite> functor)
		{
			_lock.EnterWriteLock();
			try
			{
				functor(_shared);
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}
	}

	public class ReplyMessageSynchronizer<TImpl> : Synchronizer<TImpl, IReadReplyMessageFromShared, IWriteReplyMessageToShared> where TImpl : IReadReplyMessageFromShared, IWriteReplyMessageToShared
	{
		public ReplyMessageSynchronizer(TImpl shared) : base(shared)
		{
		}
	}
}
