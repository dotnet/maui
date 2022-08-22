using System;
using System.Threading;

namespace Microsoft.Maui.Essentials
{

	public static partial class MainThread
	{

		static void PlatformBeginInvokeOnMainThread(Action action)
		{
			if (action == null)
				throw new ArgumentNullException(nameof(action));

			Gtk.Application.Invoke((o, args) =>
			{
				action();
			});
		}

		static int? UIThreadId = null;

		static bool PlatformIsMainThread
		{
			get
			{
				if (UIThreadId != null)
					return Thread.CurrentThread.ManagedThreadId == UIThreadId;

				Gtk.Application.Invoke((sender, args) =>
				{
					UIThreadId = Thread.CurrentThread.ManagedThreadId;
				});

				return false;

			}
		}

	}

}