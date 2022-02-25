using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using ElmSharp;
using Microsoft.Maui.Controls.Internals;
using Color = Microsoft.Maui.Graphics.Color;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	internal class TizenPlatformServices : IPlatformServices
	{
		static SynchronizationContext s_context;

		public TizenPlatformServices()
		{
			s_context = SynchronizationContext.Current;
		}

		#region IPlatformServices implementation

		public void BeginInvokeOnMainThread(Action action)
		{
			s_context.Post((o) => action(), null);
		}

		public void StartTimer(TimeSpan interval, Func<bool> callback)
		{
			Timer timer = null;
			bool invoking = false;
			TimerCallback onTimeout = o =>
			{
				if (!invoking)
				{
					invoking = true;
					BeginInvokeOnMainThread(() =>
						{
							if (!callback())
							{
								timer.Dispose();
							}
							invoking = false;
						}
					);
				}
			};
			timer = new Timer(onTimeout, null, Timeout.Infinite, Timeout.Infinite);
			// set interval separarately to prevent calling onTimeout before `timer' is assigned
			timer.Change(interval, interval);
		}

		public void QuitApplication()
		{
			Forms.Context.Exit();
		}

		public bool IsInvokeRequired => !EcoreMainloop.IsMainThread;

		#endregion

		public AppTheme RequestedTheme => AppTheme.Unspecified;

		static MD5 CreateChecksum()
		{
			return MD5.Create();
		}
	}
}
