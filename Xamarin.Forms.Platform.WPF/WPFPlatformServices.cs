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
			get { return !System.Windows.Application.Current.Dispatcher.CheckAccess(); }
		}

		public string RuntimePlatform => Device.WPF;

		public void OpenUriAction(Uri uri)
		{
			//TODO : OpenUriAction
		}
		
		public void BeginInvokeOnMainThread(Action action)
		{
			System.Windows.Application.Current.Dispatcher.BeginInvoke(action);
		}

		public Ticker CreateTicker()
		{
			return new WPFTicker();
		}

		public Assembly[] GetAssemblies()
		{
			return AppDomain.CurrentDomain.GetAssemblies();
		}

		public string GetMD5Hash(string input)
		{
			// MSDN - Documentation -https://msdn.microsoft.com/en-us/library/system.security.cryptography.md5(v=vs.110).aspx
			using (MD5 md5Hash = MD5.Create()) 
			{
				// Convert the input string to a byte array and compute the hash.
				byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

				// Create a new Stringbuilder to collect the bytes
				// and create a string.
				StringBuilder sBuilder = new StringBuilder();

				// Loop through each byte of the hashed data 
				// and format each one as a hexadecimal string.
				for (int i = 0; i < data.Length; i++)
				{
					sBuilder.Append(data[i].ToString("x2"));
				}

				// Return the hexadecimal string.
				return sBuilder.ToString();
			}
		}

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
			var timer = new DispatcherTimer { Interval = interval };
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
	}
}
