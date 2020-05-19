using System;

namespace Xamarin.Forms.Platform.Tizen
{
	/// <summary>
	/// Reports log messages with various log levels.
	/// </summary>
	public interface ILogger
	{
		/// <summary>
		/// Reports a debug log message.
		/// </summary>
		/// <param name="tag">Log tag</param>
		/// <param name="message">Log message</param>
		/// <param name="file">Full path to the file</param>
		/// <param name="func">Function name</param>
		/// <param name="line">Line number</param>
		void Debug(string tag, string message, string file, string func, int line);

		/// <summary>
		/// Reports a verbose log message.
		/// </summary>
		/// <param name="tag">Log tag</param>
		/// <param name="message">Log message</param>
		/// <param name="file">Full path to the file</param>
		/// <param name="func">Function name</param>
		/// <param name="line">Line number</param>
		void Verbose(string tag, string message, string file, string func, int line);

		/// <summary>
		/// Reports an information log message.
		/// </summary>
		/// <param name="tag">Log tag</param>
		/// <param name="message">Log message</param>
		/// <param name="file">Full path to the file</param>
		/// <param name="func">Function name</param>
		/// <param name="line">Line number</param>
		void Info(string tag, string message, string file, string func, int line);

		/// <summary>
		/// Reports a warning log message.
		/// </summary>
		/// <param name="tag">Log tag</param>
		/// <param name="message">Log message</param>
		/// <param name="file">Full path to the file</param>
		/// <param name="func">Function name</param>
		/// <param name="line">Line number</param>
		void Warn(string tag, string message, string file, string func, int line);

		/// <summary>
		/// Reports an error log message.
		/// </summary>
		/// <param name="tag">Log tag</param>
		/// <param name="message">Log message</param>
		/// <param name="file">Full path to the file</param>
		/// <param name="func">Function name</param>
		/// <param name="line">Line number</param>
		void Error(string tag, string message, string file, string func, int line);

		/// <summary>
		/// Reports a fatal error log message.
		/// </summary>
		/// <param name="tag">Log tag</param>
		/// <param name="message">Log message</param>
		/// <param name="file">Full path to the file</param>
		/// <param name="func">Function name</param>
		/// <param name="line">Line number</param>
		void Fatal(string tag, string message, string file, string func, int line);
	}
}

