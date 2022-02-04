using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.ViewManagement;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal class WindowsPlatformServices : IPlatformServices, IPlatformInvalidate
	{
		readonly UISettings _uiSettings = new UISettings();

		public WindowsPlatformServices()
		{
			_uiSettings.ColorValuesChanged += UISettingsColorValuesChanged;
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

		public double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes)
		{
			return size.GetFontSize();
		}

		public string RuntimePlatform => Device.UWP;

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

		public SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			return Platform.GetNativeSize(view, widthConstraint, heightConstraint);
		}

		void UISettingsColorValuesChanged(UISettings sender, object args)
		{
			Application.Current.Dispatcher.DispatchIfRequired(() =>
				Application.Current?.TriggerThemeChanged(new AppThemeChangedEventArgs(Application.Current.RequestedTheme)));
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

		public OSAppTheme RequestedTheme =>
			Microsoft.UI.Xaml.Application.Current.RequestedTheme == ApplicationTheme.Dark
			? OSAppTheme.Dark
			: OSAppTheme.Light;
	}
}