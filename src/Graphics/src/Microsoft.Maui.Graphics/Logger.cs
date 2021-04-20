using System;

namespace Microsoft.Maui.Graphics
{
	public static class Logger
	{
		private static ILoggingService _registeredService;

		public static ILoggingService RegisteredService
		{
			get
			{
				if (_registeredService == null)
				{
					_registeredService = new ConsoleLoggingService();
					_registeredService.Log(LogType.WARNING, "No logging service was registered.  Falling back to console logging.");
				}

				return _registeredService;
			}
		}

		public static void RegisterService(ILoggingService service)
		{
			_registeredService = service;
		}

		public static void Debug(params object[] parameters)
		{
			Log(LogType.DEBUG, parameters);
		}

		public static void Warn(params object[] parameters)
		{
			Log(LogType.WARNING, parameters);
		}

		public static void Error(params object[] parameters)
		{
			Log(LogType.ERROR, parameters);
		}

		public static void Fatal(params object[] parameters)
		{
			Log(LogType.FATAL, parameters);
		}

		public static void Info(params object[] parameters)
		{
			Log(LogType.INFO, parameters);
		}

		public static void Log(LogType logType, params object[] parameters)
		{
			if (parameters == null || parameters.Length == 0)
				return;

			if (parameters.Length == 1)
			{
				if (parameters[0] is Exception exception)
				{
					RegisteredService.Log(logType, exception.Message, exception);
					return;
				}

				var value = parameters[0];
				if (value != null)
				{
					RegisteredService.Log(logType, value.ToString());
					return;
				}
			}

			var format = parameters[0] != null ? parameters[0].ToString() : "";
			var message = format;

			try
			{
				var args = new object[parameters.Length - 1];
				Array.Copy(parameters, 1, args, 0, parameters.Length - 1);

				message = string.Format(format, args);
			}
			catch (Exception exc)
			{
				RegisteredService.Log(LogType.INFO, string.Format("An error occured formatting the logging message: [{0}]", format), exc);
			}

			if (parameters[parameters.Length - 1] is Exception ex)
			{
				RegisteredService.Log(logType, message, ex);
			}
			else
			{
				RegisteredService.Log(logType, message);
			}
		}
	}
}
