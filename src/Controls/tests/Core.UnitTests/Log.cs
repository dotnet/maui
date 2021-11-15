using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	internal abstract class LogListener
	{
		public abstract void Warning(string category, string message);
	}

	internal class Logger : LogListener
	{
		public IReadOnlyList<string> Messages
		{
			get { return messages; }
		}

		public override void Warning(string category, string message)
		{
			messages.Add("[" + category + "] " + message);
		}

		readonly List<string> messages = new List<string>();
	}

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
