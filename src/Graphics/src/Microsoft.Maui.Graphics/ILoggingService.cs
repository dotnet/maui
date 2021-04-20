using System;

namespace Microsoft.Maui.Graphics
{
	public enum LogType
	{
		DEBUG,
		INFO,
		WARNING,
		ERROR,
		FATAL
	}

	public interface ILoggingService
	{
		void Log(LogType logType, string message);
		void Log(LogType logType, string message, Exception exception);
	}
}
