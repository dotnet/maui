using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Windows.System;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.WinPhone;

namespace Xamarin.Forms
{
	internal class WP8PlatformServices : IPlatformServices
	{
		static readonly MD5CryptoServiceProvider Checksum = new MD5CryptoServiceProvider();

		public void BeginInvokeOnMainThread(Action action)
		{
			Deployment.Current.Dispatcher.BeginInvoke(action);
		}

		public Ticker CreateTicker()
		{
			return new WinPhoneTicker();
		}

		public Assembly[] GetAssemblies()
		{
			return AppDomain.CurrentDomain.GetAssemblies();
		}

		public string GetMD5Hash(string input)
		{
			byte[] bytes = Checksum.ComputeHash(Encoding.UTF8.GetBytes(input));
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
					if (typeof(Label).IsAssignableFrom(targetElementType))
						return (double)System.Windows.Application.Current.Resources["PhoneFontSizeNormal"];
					return (double)System.Windows.Application.Current.Resources["PhoneFontSizeMedium"];
				case NamedSize.Micro:
					return (double)System.Windows.Application.Current.Resources["PhoneFontSizeSmall"] - 3;
				case NamedSize.Small:
					return (double)System.Windows.Application.Current.Resources["PhoneFontSizeSmall"];
				case NamedSize.Medium:
					if (useOldSizes)
						goto case NamedSize.Default;
					return (double)System.Windows.Application.Current.Resources["PhoneFontSizeMedium"];
				case NamedSize.Large:
					return (double)System.Windows.Application.Current.Resources["PhoneFontSizeLarge"];
				default:
					throw new ArgumentOutOfRangeException("size");
			}
		}

		public Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken)
		{
			var tcs = new TaskCompletionSource<Stream>();

			try
			{
				HttpWebRequest request = WebRequest.CreateHttp(uri);
				request.AllowReadStreamBuffering = true;
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
			return new _IsolatedStorageFile(IsolatedStorageFile.GetUserStoreForApplication());
		}

		public bool IsInvokeRequired
		{
			get { return !Deployment.Current.Dispatcher.CheckAccess(); }
		}

		public string RuntimePlatform => Device.WinPhone;

		public void OpenUriAction(Uri uri)
		{
			Launcher.LaunchUriAsync(uri).WatchForError();
		}

		public void StartTimer(TimeSpan interval, Func<bool> callback)
		{
			var timer = new DispatcherTimer { Interval = interval };
			timer.Start();
			timer.Tick += (sender, args) =>
			{
				bool result = callback();
				if (!result)
					timer.Stop();
			};
		}

		static int Hex(int v)
		{
			if (v < 10)
				return '0' + v;
			return 'a' + v - 10;
		}

		public void QuitApplication()
		{
			Log.Warning(nameof(WP8PlatformServices), "Platform doesn't implement QuitApp");
		}

		public class _IsolatedStorageFile : IIsolatedStorageFile
		{
			readonly IsolatedStorageFile _isolatedStorageFile;

			public _IsolatedStorageFile(IsolatedStorageFile isolatedStorageFile)
			{
				_isolatedStorageFile = isolatedStorageFile;
			}

			public Task CreateDirectoryAsync(string path)
			{
				_isolatedStorageFile.CreateDirectory(path);
				return Task.FromResult(true);
			}

			public Task<bool> GetDirectoryExistsAsync(string path)
			{
				return Task.FromResult(_isolatedStorageFile.DirectoryExists(path));
			}

			public Task<bool> GetFileExistsAsync(string path)
			{
				return Task.FromResult(_isolatedStorageFile.FileExists(path));
			}

			public Task<DateTimeOffset> GetLastWriteTimeAsync(string path)
			{
				return Task.FromResult(_isolatedStorageFile.GetLastWriteTime(path));
			}

			public Task<Stream> OpenFileAsync(string path, Internals.FileMode mode, Internals.FileAccess access)
			{
				Stream stream = _isolatedStorageFile.OpenFile(path, (System.IO.FileMode)mode, (System.IO.FileAccess)access);
				return Task.FromResult(stream);
			}

			public Task<Stream> OpenFileAsync(string path, Internals.FileMode mode, Internals.FileAccess access, Internals.FileShare share)
			{
				Stream stream = _isolatedStorageFile.OpenFile(path, (System.IO.FileMode)mode, (System.IO.FileAccess)access, (System.IO.FileShare)share);
				return Task.FromResult(stream);
			}
		}
	}
}