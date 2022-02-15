using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using ElmSharp;
using Microsoft.Maui.Controls.Compatibility.Internals;
using TAppControl = Tizen.Applications.AppControl;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	internal class TizenPlatformServices : IPlatformServices
	{
		static SynchronizationContext s_context;

		public TizenPlatformServices()
		{
			s_context = SynchronizationContext.Current;
		}

		public class TizenTicker : Ticker
		{
			readonly Timer _timer;

			public TizenTicker()
			{
				_timer = new Timer((object o) => HandleElapsed(o), this, Timeout.Infinite, Timeout.Infinite);
			}

			protected override void EnableTimer()
			{
				_timer.Change(16, 16);
			}

			protected override void DisableTimer()
			{
				_timer.Change(-1, -1);
			}

			void HandleElapsed(object state)
			{
				s_context.Post((o) => SendSignals(-1), null);
			}
		}
		#region IPlatformServices implementation

		public void BeginInvokeOnMainThread(Action action)
		{
			s_context.Post((o) => action(), null);
		}

		public Ticker CreateTicker()
		{
			return new TizenTicker();
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

		public async Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken)
		{
			using (var client = new HttpClient())
			using (HttpResponseMessage response = await client.GetAsync(uri, cancellationToken))
				return await response.Content.ReadAsStreamAsync();
		}

		public IIsolatedStorageFile GetUserStoreForApplication()
		{
			return new TizenIsolatedStorageFile();
		}

		public void QuitApplication()
		{
			Forms.Context.Exit();
		}

		public bool IsInvokeRequired => !EcoreMainloop.IsMainThread;

		#endregion

		public SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			return Platform.GetNativeSize(view, widthConstraint, heightConstraint);
		}

		public OSAppTheme RequestedTheme => OSAppTheme.Unspecified;

		static MD5 CreateChecksum()
		{
			return MD5.Create();
		}
	}
}