using T = Tizen;

namespace Xamarin.Forms.Platform.Tizen
{
	/// <summary>
	/// Logs a message to the dlog.
	/// </summary>
	internal class DlogLogger : ILogger
	{
		public void Debug(string tag, string message, string file, string func, int line)
		{
			T.Log.Debug(tag, message, file, func, line);
		}

		public void Verbose(string tag, string message, string file, string func, int line)
		{
			T.Log.Verbose(tag, message, file, func, line);
		}

		public void Info(string tag, string message, string file, string func, int line)
		{
			T.Log.Info(tag, message, file, func, line);
		}

		public void Warn(string tag, string message, string file, string func, int line)
		{
			T.Log.Warn(tag, message, file, func, line);
		}

		public void Error(string tag, string message, string file, string func, int line)
		{
			T.Log.Error(tag, message, file, func, line);
		}

		public void Fatal(string tag, string message, string file, string func, int line)
		{
			T.Log.Fatal(tag, message, file, func, line);
		}
	}
}

