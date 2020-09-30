using System.Collections.Generic;
using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class Log
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