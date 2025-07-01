using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Compatibility.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.GTK
{
	internal class GtkPlatformServices : IPlatformServices
	{
		public bool IsInvokeRequired => Thread.CurrentThread.IsBackground;

		public void BeginInvokeOnMainThread(Action action)
		{
			GLib.Idle.Add(delegate
			{ action(); return false; });
		}

		public Ticker CreateTicker()
		{
			return new GtkTicker();
		}

		public async Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken)
		{
			using (var client = new HttpClient())
			{
				// Do not remove this await otherwise the client will dispose before
				// the stream even starts
				var result = await StreamWrapper.GetStreamAsync(uri, cancellationToken, client).ConfigureAwait(false);

				return result;
			}
		}

		public IIsolatedStorageFile GetUserStoreForApplication()
		{
			return new GtkIsolatedStorageFile();
		}

		public void StartTimer(TimeSpan interval, Func<bool> callback)
		{
			GLib.Timeout.Add((uint)interval.TotalMilliseconds, () =>
			{
				var result = callback();
				return result;
			});
		}

		private static int Hex(int v)
		{
			if (v < 10)
				return '0' + v;
			return 'a' + v - 10;
		}

		public void QuitApplication()
		{
			Gtk.Application.Quit();
		}

		public SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			return Platform.GetNativeSize(view, widthConstraint, heightConstraint);
		}

		public AppTheme RequestedTheme => AppTheme.Unspecified;
	}
}