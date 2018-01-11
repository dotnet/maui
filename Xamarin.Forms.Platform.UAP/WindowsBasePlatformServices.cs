using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.LockScreen;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.UWP
{
	internal abstract class WindowsBasePlatformServices : IPlatformServices
	{
		readonly CoreDispatcher _dispatcher;

		protected WindowsBasePlatformServices(CoreDispatcher dispatcher)
		{
			if (dispatcher == null)
				throw new ArgumentNullException(nameof(dispatcher));

			_dispatcher = dispatcher;
		}

		public void BeginInvokeOnMainThread(Action action)
		{
			_dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()).WatchForError();
		}

		public Ticker CreateTicker()
		{
			return new WindowsTicker();
		}

		public virtual Assembly[] GetAssemblies()
		{
			var options = new QueryOptions { FileTypeFilter = { ".exe", ".dll" } };

			StorageFileQueryResult query = Package.Current.InstalledLocation.CreateFileQueryWithOptions(options);
			IReadOnlyList<StorageFile> files = query.GetFilesAsync().AsTask().Result;

			var assemblies = new List<Assembly>(files.Count);
			foreach (StorageFile file in files)
			{
				try
				{
					Assembly assembly = Assembly.Load(new AssemblyName { Name = Path.GetFileNameWithoutExtension(file.Name) });

					assemblies.Add(assembly);
				}
				catch (IOException)
				{
				}
				catch (BadImageFormatException)
				{
				}
			}

			Assembly thisAssembly = GetType().GetTypeInfo().Assembly;
			// this happens with .NET Native
			if (!assemblies.Contains(thisAssembly))
				assemblies.Add(thisAssembly);

			Assembly xamlAssembly = typeof(Xamarin.Forms.Xaml.IMarkupExtension).GetTypeInfo().Assembly;
			if (!assemblies.Contains(xamlAssembly))
				assemblies.Add(xamlAssembly);

			return assemblies.ToArray();
		}

		public string GetMD5Hash(string input)
		{
			HashAlgorithmProvider algorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
			IBuffer buffer = algorithm.HashData(Encoding.Unicode.GetBytes(input).AsBuffer());
			return CryptographicBuffer.EncodeToHexString(buffer);
		}

		public double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes)
		{
			return size.GetFontSize();
		}

		public async Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken)
		{
			using (var client = new HttpClient())
			{
				HttpResponseMessage streamResponse = await client.GetAsync(uri.AbsoluteUri).ConfigureAwait(false);

				if (!streamResponse.IsSuccessStatusCode)
				{
					Log.Warning("HTTP Request", $"Could not retrieve {uri}, status code {streamResponse.StatusCode}");
					return null;
				}

				return await streamResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
			}
		}

		public IIsolatedStorageFile GetUserStoreForApplication()
		{
			return new WindowsIsolatedStorage(ApplicationData.Current.LocalFolder);
		}

		// Per https://docs.microsoft.com/en-us/windows-hardware/drivers/partnerapps/create-a-kiosk-app-for-assigned-access:
		// "Each view or window has its own dispatcher. In assigned access mode, you should not use the MainView dispatcher, 
		// instead you should use the CurrentView dispatcher." Checking to see if this isn't null (i.e. the current window is
		// running above lock) calls through GetCurrentView(), and otherwise through MainView.
		public bool IsInvokeRequired => LockApplicationHost.GetForCurrentView() != null
			? !CoreApplication.GetCurrentView().Dispatcher.HasThreadAccess
			: !CoreApplication.MainView.CoreWindow.Dispatcher.HasThreadAccess;

		public string RuntimePlatform => Device.UWP;

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

		public void QuitApplication()
		{
			Log.Warning(nameof(WindowsBasePlatformServices), "Platform doesn't implement QuitApp");
		}
	}
}