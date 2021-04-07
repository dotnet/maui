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
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.Maui.Controls.Internals;
using IOPath = System.IO.Path;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal abstract class WindowsBasePlatformServices : IPlatformServices, IPlatformInvalidate
	{
		const string WrongThreadError = "RPC_E_WRONG_THREAD";
#pragma warning disable CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
		readonly Microsoft.System.DispatcherQueue _dispatcher;
#pragma warning restore CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
		readonly UISettings _uiSettings = new UISettings();

#pragma warning disable CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
		protected WindowsBasePlatformServices(Microsoft.System.DispatcherQueue dispatcher)
#pragma warning restore CS8305 // Type is for evaluation purposes only and is subject to change or removal in future updates.
		{
			_dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
			_uiSettings.ColorValuesChanged += UISettingsColorValuesChanged;
		}

		public void BeginInvokeOnMainThread(Action action)
		{
			//if (CoreApplication.Views.Count == 1)
			{
				// This is the normal scenario - one window only

				// TODO WINUI
				_dispatcher.TryEnqueue(() =>
				{
					action();
				});//.WatchForError();
				return;
			}

			// WINUI3 
			//await TryAllDispatchers(action);
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

			LoadAllAssemblies(AppDomain.CurrentDomain.GetAssemblies(), assemblies);

			Assembly thisAssembly = GetType().GetTypeInfo().Assembly;
			// this happens with .NET Native
			if (!assemblies.Contains(thisAssembly))
				assemblies.Add(thisAssembly);

			Assembly coreAssembly = typeof(Microsoft.Maui.Controls.Label).GetTypeInfo().Assembly;
			if (!assemblies.Contains(coreAssembly))
				assemblies.Add(coreAssembly);

			Assembly xamlAssembly = typeof(Microsoft.Maui.Controls.Xaml.Extensions).GetTypeInfo().Assembly;
			if (!assemblies.Contains(xamlAssembly))
				assemblies.Add(xamlAssembly);

			return assemblies.ToArray();
		}

		void LoadAllAssemblies(Assembly[] files, List<Assembly> loaded)
		{
			for (var i = 0; i < files.Length; i++)
			{
				var asm = files[i];
				if (!loaded.Contains(asm))
				{
					loaded.Add(asm);
				}
			}
		}

		void LoadAllAssemblies(AssemblyName[] files, List<Assembly> loaded)
		{
			for (var i = 0; i < files.Length; i++)
			{
				try
				{
					var asm = Assembly.Load(files[i]);
					if (!loaded.Contains(asm))
					{
						loaded.Add(asm);
						LoadAllAssemblies(asm.GetReferencedAssemblies(), loaded);
					}
				}
				catch (IOException)
				{
				}
				catch (BadImageFormatException)
				{
				}
			}
		}

		public string GetHash(string input) => Crc64.GetHash(input);

		string IPlatformServices.GetMD5Hash(string input) => GetHash(input);

		public double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes)
		{
			return size.GetFontSize();
		}

		public Color GetNamedColor(string name)
		{
			if (!Microsoft.UI.Xaml.Application.Current?.Resources.ContainsKey(name) ?? true)
				return Color.Default;

			return ((Windows.UI.Color)Microsoft.UI.Xaml.Application.Current?.Resources[name]).ToFormsColor();
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

		void UISettingsColorValuesChanged(UISettings sender, object args)
		{
			_dispatcher.TryEnqueue(() => Application.Current?.TriggerThemeChanged(new AppThemeChangedEventArgs(Application.Current.RequestedTheme)));
		}

		public void Invalidate(VisualElement visualElement)
		{
			var renderer = Platform.GetRenderer(visualElement);
			if (renderer == null)
			{
				return;
			}

			renderer.ContainerElement.InvalidateMeasure();
		}

		//async Task TryAllDispatchers(Action action)
		//{
		//	// Our best bet is Window.Current; most of the time, that's the Dispatcher we need
		//	var currentWindow = Window.Current;

		//	if (currentWindow?.Dispatcher != null)
		//	{
		//		try
		//		{
		//			await TryDispatch(currentWindow.Dispatcher, action);
		//			return;
		//		}
		//		catch (Exception ex) when (ex.Message.Contains(WrongThreadError))
		//		{
		//			// The current window is not the one we need 
		//		}
		//	}

		//	// Either Window.Current was the wrong Dispatcher, or Window.Current was null because we're on a 
		//	// non-UI thread (e.g., one from the thread pool). So now it's time to try all the available Dispatchers 

		//	var views = CoreApplication.Views;

		//	for (int n = 0; n < views.Count; n++)
		//	{
		//		var dispatcher = views[n].Dispatcher;

		//		if (dispatcher == null || dispatcher == currentWindow?.Dispatcher)
		//		{
		//			// Obviously null Dispatchers are no good, and we already tried the one from currentWindow
		//			continue;
		//		}

		//		// We need to ignore Deactivated/Never Activated windows, but it's possible we can't access their 
		//		// properties from this thread. So we'll check those using the Dispatcher
		//		bool activated = false;

		//		await TryDispatch(dispatcher, () => {
		//			var mode = views[n].CoreWindow.ActivationMode;
		//			activated = (mode == CoreWindowActivationMode.ActivatedInForeground
		//				|| mode == CoreWindowActivationMode.ActivatedNotForeground);
		//		});

		//		if (!activated)
		//		{
		//			// This is a deactivated (or not yet activated) window; move on
		//			continue;
		//		}

		//		try
		//		{
		//			await TryDispatch(dispatcher, action);
		//			return;
		//		}
		//		catch (Exception ex) when (ex.Message.Contains(WrongThreadError))
		//		{
		//			// This was the incorrect dispatcher; move on to try another one
		//		}
		//	}
		//}

		//async Task<bool> TryDispatch(CoreDispatcher dispatcher, Action action)
		//{
		//	if (dispatcher == null)
		//	{
		//		throw new ArgumentNullException(nameof(dispatcher));
		//	}

		//	var taskCompletionSource = new TaskCompletionSource<bool>();

		//	await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
		//		try
		//		{
		//			action();
		//			taskCompletionSource.SetResult(true);
		//		}
		//		catch (Exception ex)
		//		{
		//			taskCompletionSource.SetException(ex);
		//		}
		//	});

		//	return await taskCompletionSource.Task;
		//}

		public OSAppTheme RequestedTheme => Microsoft.UI.Xaml.Application.Current.RequestedTheme == ApplicationTheme.Dark ? OSAppTheme.Dark : OSAppTheme.Light;
	}
}
