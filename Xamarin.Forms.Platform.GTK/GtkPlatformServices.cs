using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.GTK
{
	internal class GtkPlatformServices : IPlatformServices
	{
		private static readonly MD5CryptoServiceProvider Checksum = new MD5CryptoServiceProvider();

		public bool IsInvokeRequired => Thread.CurrentThread.IsBackground;

		public string RuntimePlatform => Device.GTK;

		public void BeginInvokeOnMainThread(Action action)
		{
			GLib.Idle.Add(delegate { action(); return false; });
		}

		public Ticker CreateTicker()
		{
			return new GtkTicker();
		}

		public Assembly[] GetAssemblies()
		{
			return AppDomain.CurrentDomain.GetAssemblies();
		}

		public string GetMD5Hash(string input)
		{
			var bytes = Checksum.ComputeHash(Encoding.UTF8.GetBytes(input));
			var ret = new char[32];
			for (var i = 0; i < 16; i++)
			{
				ret[i * 2] = (char)Hex(bytes[i] >> 4);
				ret[i * 2 + 1] = (char)Hex(bytes[i] & 0xf);
			}
			return new string(ret);
		}

		public double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes)
		{
			switch (size)
			{
				case NamedSize.Default:
					return 11;
				case NamedSize.Micro:
                case NamedSize.Caption:
                    return 12;
				case NamedSize.Medium:
					return 17;
				case NamedSize.Large:
					return 22;
                case NamedSize.Small:
                case NamedSize.Body:
                    return 14;
                case NamedSize.Header:
                    return 46;
                case NamedSize.Subtitle:
                    return 20;
                case NamedSize.Title:
                    return 24;
                default:
					throw new ArgumentOutOfRangeException(nameof(size));
			}
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

		public void OpenUriAction(Uri uri)
		{
			System.Diagnostics.Process.Start(uri.AbsoluteUri);
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
	}
}
