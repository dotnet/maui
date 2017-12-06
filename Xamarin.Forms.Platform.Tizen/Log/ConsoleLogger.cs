using System;
using System.IO;

namespace Xamarin.Forms.Platform.Tizen
{
	/// <summary>
	/// Logs a message to the console.
	/// </summary>
	internal class ConsoleLogger : ILogger
	{
		public void Debug(string tag, string message, string file, string func, int line)
		{
			Print("D", tag, message, file, func, line);
		}

		public void Verbose(string tag, string message, string file, string func, int line)
		{
			Print("V", tag, message, file, func, line);
		}

		public void Info(string tag, string message, string file, string func, int line)
		{
			Print("I", tag, message, file, func, line);
		}

		public void Warn(string tag, string message, string file, string func, int line)
		{
			Print("W", tag, message, file, func, line);
		}

		public void Error(string tag, string message, string file, string func, int line)
		{
			Print("E", tag, message, file, func, line);
		}

		public void Fatal(string tag, string message, string file, string func, int line)
		{
			Print("F", tag, message, file, func, line);
		}

		/// <summary>
		/// Formats and prints the log information.
		/// </summary>
		/// <param name="level">Log level</param>
		/// <param name="tag">Log tag</param>
		/// <param name="message">Log message</param>
		/// <param name="file">Full path to the file</param>
		/// <param name="func">Function name</param>
		/// <param name="line">Line number</param>
		void Print(string level, string tag, string message, string file, string func, int line)
		{
			Uri f = new Uri(file);
			Console.WriteLine(
				String.Format(
					"\n[{6:yyyy-MM-dd HH:mm:ss.ffff} {0}/{1}]\n{2}: {3}({4}) > {5}",
					level,  // 0
					tag,  // 1
					Path.GetFileName(f.AbsolutePath),  // 2
					func,  // 3
					line,  // 4
					message,  // 5
					DateTime.Now  // 6
				)
			);
		}
	}
}

