using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.WPF
{
	public class WPFPlatformServices : IPlatformServices
	{
		public bool IsInvokeRequired
		{
			get { return System.Windows.Application.Current == null ? false : !System.Windows.Application.Current.Dispatcher.CheckAccess(); }
		}

		public string RuntimePlatform => Device.WPF;

		public void OpenUriAction(Uri uri)
		{
			System.Diagnostics.Process.Start(uri.AbsoluteUri);
		}

		public void BeginInvokeOnMainThread(Action action)
		{
			System.Windows.Application.Current?.Dispatcher.BeginInvoke(action);
		}

		public Ticker CreateTicker()
		{
			return new WPFTicker();
		}

		public Assembly[] GetAssemblies()
		{
			return AppDomain.CurrentDomain.GetAssemblies();
		}

		public string GetHash(string input) => Crc64.GetHash(input);

		string IPlatformServices.GetMD5Hash(string input) => GetHash(input);

		public double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes)
		{
			switch (size)
			{
				case NamedSize.Default:
					if (typeof(Label).IsAssignableFrom(targetElementType))
						return (double)System.Windows.Application.Current.Resources["FontSizeNormal"];
					return (double)System.Windows.Application.Current.Resources["FontSizeMedium"];
				case NamedSize.Micro:
					return (double)System.Windows.Application.Current.Resources["FontSizeSmall"] - 3;
				case NamedSize.Small:
					return (double)System.Windows.Application.Current.Resources["FontSizeSmall"];
				case NamedSize.Medium:
					if (useOldSizes)
						goto case NamedSize.Default;
					return (double)System.Windows.Application.Current.Resources["FontSizeMedium"];
				case NamedSize.Large:
					return (double)System.Windows.Application.Current.Resources["FontSizeLarge"];
				case NamedSize.Body:
					return (double)System.Windows.Application.Current.Resources["FontSizeBody"];
				case NamedSize.Caption:
					return (double)System.Windows.Application.Current.Resources["FontSizeCaption"];
				case NamedSize.Header:
					return (double)System.Windows.Application.Current.Resources["FontSizeHeader"];
				case NamedSize.Subtitle:
					return (double)System.Windows.Application.Current.Resources["FontSizeSubtitle"];
				case NamedSize.Title:
					return (double)System.Windows.Application.Current.Resources["FontSizeTitle"];
				default:
					throw new ArgumentOutOfRangeException("size");
			}
		}

		public Color GetNamedColor(string name)
		{
			// Not supported on this platform
			return Color.Default;
		}

		public Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken)
		{
			var tcs = new TaskCompletionSource<Stream>();

			try
			{
				HttpWebRequest request = WebRequest.CreateHttp(uri);
				request.BeginGetResponse(ar =>
				{
					if (cancellationToken.IsCancellationRequested)
					{
						tcs.SetCanceled();
						return;
					}

					try
					{
						Stream stream = request.EndGetResponse(ar).GetResponseStream();
						tcs.TrySetResult(stream);
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}, null);
			}
			catch (Exception ex)
			{
				tcs.TrySetException(ex);
			}

			return tcs.Task;
		}

		public IIsolatedStorageFile GetUserStoreForApplication()
		{
			return new WPFIsolatedStorageFile(IsolatedStorageFile.GetUserStoreForAssembly());
		}

		public void StartTimer(TimeSpan interval, Func<bool> callback)
		{
			var timer = new DispatcherTimer(DispatcherPriority.Background, System.Windows.Application.Current.Dispatcher) { Interval = interval };
			timer.Start();
			timer.Tick += (sender, args) =>
			{
				bool result = callback();
				if (!result)
					timer.Stop();
			};
		}

		public void QuitApplication()
		{
			System.Windows.Application.Current.Shutdown();
		}

		public SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			return Platform.GetNativeSize(view, widthConstraint, heightConstraint);
		}

		public OSAppTheme RequestedTheme => OSAppTheme.Unspecified;
	}
}