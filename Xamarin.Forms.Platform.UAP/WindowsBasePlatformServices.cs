using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.UWP
{
	internal abstract class WindowsBasePlatformServices : IPlatformServices
	{
		const string WrongThreadError = "RPC_E_WRONG_THREAD";
		readonly CoreDispatcher _dispatcher;

		protected WindowsBasePlatformServices(CoreDispatcher dispatcher)
		{
			_dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
		}

		public async void BeginInvokeOnMainThread(Action action)
		{
			if (CoreApplication.Views.Count == 1)
			{
				// This is the normal scenario - one window only
				_dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action()).WatchForError();
				return;
			}

			await TryAllDispatchers(action);
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

			Assembly coreAssembly = typeof(Xamarin.Forms.Label).GetTypeInfo().Assembly;
			if (!assemblies.Contains(coreAssembly))
				assemblies.Add(coreAssembly);

			Assembly xamlAssembly = typeof(Xamarin.Forms.Xaml.Extensions).GetTypeInfo().Assembly;
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
				// Do not remove this await otherwise the client will dispose before
				// the stream even starts
				var result = await StreamWrapper.GetStreamAsync(uri, cancellationToken, client).ConfigureAwait(false);

				return result;
			}
		}

		public IIsolatedStorageFile GetUserStoreForApplication()
		{
			return new WindowsIsolatedStorage(ApplicationData.Current.LocalFolder);
		}

		public bool IsInvokeRequired => !_dispatcher?.HasThreadAccess ?? true;

		public string RuntimePlatform => Device.UWP;

		public void OpenUriAction(Uri uri)
		{
			Launcher.LaunchUriAsync(uri).WatchForError();
		}

		public void StartTimer(TimeSpan interval, Func<bool> callback)
		{
			var timerTick = 0L;
			var stopWatch = new Stopwatch();
			stopWatch.Start();
			void renderingFrameEventHandler(object sender, object args)
			{
				var newTimerTick = stopWatch.ElapsedMilliseconds / (long)interval.TotalMilliseconds;
				if (newTimerTick == timerTick)
					return;
				timerTick = newTimerTick;
				bool result = callback();
				if (result)
					return;
				CompositionTarget.Rendering -= renderingFrameEventHandler;
			}
			CompositionTarget.Rendering += renderingFrameEventHandler;
		}

		public void QuitApplication()
		{
			Log.Warning(nameof(WindowsBasePlatformServices), "Platform doesn't implement QuitApp");
		}

		public SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			return Platform.GetNativeSize(view, widthConstraint, heightConstraint);
		}

		async Task TryAllDispatchers(Action action)
		{
			// Our best bet is Window.Current; most of the time, that's the Dispatcher we need
			var currentWindow = Window.Current;

			if (currentWindow?.Dispatcher != null)
			{
				try
				{
					await TryDispatch(currentWindow.Dispatcher, action);
					return;
				}
				catch (Exception ex) when (ex.Message.Contains(WrongThreadError))
				{
					// The current window is not the one we need 
				}
			}

			// Either Window.Current was the wrong Dispatcher, or Window.Current was null because we're on a 
			// non-UI thread (e.g., one from the thread pool). So now it's time to try all the available Dispatchers 

			var views = CoreApplication.Views;

			for (int n = 0; n < views.Count; n++)
			{
				var dispatcher = views[n].Dispatcher;

				if (dispatcher == null || dispatcher == currentWindow?.Dispatcher)
				{
					// Obviously null Dispatchers are no good, and we already tried the one from currentWindow
					continue;
				}

				// We need to ignore Deactivated/Never Activated windows, but it's possible we can't access their 
				// properties from this thread. So we'll check those using the Dispatcher
				bool activated = false;

				await TryDispatch(dispatcher, () => {
					var mode = views[n].CoreWindow.ActivationMode;
					activated = (mode == CoreWindowActivationMode.ActivatedInForeground
						|| mode == CoreWindowActivationMode.ActivatedNotForeground);
				});

				if (!activated)
				{
					// This is a deactivated (or not yet activated) window; move on
					continue;
				}

				try
				{
					await TryDispatch(dispatcher, action);
					return;
				}
				catch (Exception ex) when (ex.Message.Contains(WrongThreadError))
				{
					// This was the incorrect dispatcher; move on to try another one
				}
			}
		}

		async Task<bool> TryDispatch(CoreDispatcher dispatcher, Action action)
		{
			if (dispatcher == null)
			{
				throw new ArgumentNullException(nameof(dispatcher));
			}

			var taskCompletionSource = new TaskCompletionSource<bool>();

			await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
				try
				{
					action();
					taskCompletionSource.SetResult(true);
				}
				catch (Exception ex)
				{
					taskCompletionSource.SetException(ex);
				}
			});

			return await taskCompletionSource.Task;
		}
	}
}