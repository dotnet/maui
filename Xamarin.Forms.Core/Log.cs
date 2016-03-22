using System.Collections.Generic;

namespace Xamarin.Forms
{
	internal static class Log
	{
		static Log()
		{
			Listeners = new SynchronizedList<LogListener>();
		}

		public static IList<LogListener> Listeners { get; }

		public static void Warning(string category, string message)
		{
			foreach (LogListener listener in Listeners)
				listener.Warning(category, message);
		}

		public static void Warning(string category, string format, params object[] args)
		{
			Warning(category, string.Format(format, args));
		}
	}
}