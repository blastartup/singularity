using System;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace

namespace Singularity
{
	public abstract class BaseFaultManager<TTraceLog> : IFaultManager where TTraceLog : class
	{
		protected BaseFaultManager(String route, Guid? transitId, String version)
		{
			_route = route;
			_transitId = transitId;
			_version = version;
		}

		#region Tracing

		public Boolean LogTrace(ITraceLog traceLog)
		{
			var entity = LogTraceCore(traceLog.Description, traceLog.MethodName, traceLog.StatusCode);
			if (entity == null)
			{
				return false;
			}
			return Save();
		}

		protected abstract TTraceLog LogTraceCore(String description, String methodName, Int32? statusCode);
		protected abstract Boolean Save();

		#endregion

		#region Exceptions

		public async Task<Boolean> LogExceptionAsync(IExceptionLog exceptionLog)
		{
			var exceptionMessagesTask = new Task<Tuple<String, String>>(() => ExceptionMessages(exceptionLog.Exception));
			await exceptionMessagesTask;
			var exceptionMessages = exceptionMessagesTask.Result;

			var traceLogEntity = LogTraceCore(exceptionLog.Description, exceptionLog.MethodName, exceptionLog.StatusCode);
			if (traceLogEntity != null)
			{
				var exceptionLogged = new ExceptionLogged(InsertAsync(traceLogEntity, exceptionMessages.Item1, exceptionMessages.Item2), exceptionLog) as IExceptionLogged;
				var sendEmailTask = new Task(() => { SendEmailAsync(exceptionLogged);});
				await sendEmailTask;
				return true;
			}
			return false;
		}

		public Boolean LogException(IExceptionLog exceptionLog)
		{
			var exceptionMessages = ExceptionMessages(exceptionLog.Exception);

			var traceLogEntity = LogTraceCore(exceptionLog.Description, exceptionLog.MethodName, exceptionLog.StatusCode);
			if (traceLogEntity != null)
			{
				var exceptionLogged = new ExceptionLogged(InsertAsync(traceLogEntity, exceptionMessages.Item1, exceptionMessages.Item2), exceptionLog);
				SendEmailAsync(exceptionLogged);
				return true;
			}
			return false;
		}

		private Tuple<String, String> ExceptionMessages(Exception exception)
		{
			var exceptionMessages = new DelimitedStringBuilder();
			var exceptionStackTraces = new DelimitedStringBuilder();

			while (exception != null)
			{
				exceptionMessages.Add(exception.Message);
				exceptionStackTraces.Add(exception.StackTrace);

				exception = exception.InnerException;
			}
			return new Tuple<String, String>(exceptionMessages.ToDelimitedString("<br/>"), exceptionStackTraces.ToDelimitedString("<br/>"));
		}

		protected abstract void SendEmailAsync(IExceptionLogged exceptionMemo);
		protected abstract Int32 InsertAsync(TTraceLog traceLogEntity, String description, String stackTrace);

		#endregion


		protected String Route
		{
			get { return _route; }
		}
		private String _route;

		protected Guid? TransitId
		{
			get { return _transitId; }
		}
		private Guid? _transitId;

		protected String Version
		{
			get { return _version; }
		}
		private String _version;


	}
}
