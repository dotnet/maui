using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.WPF;
using WSolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace Xamarin.Forms
{
	public static class Forms
	{
		public static bool IsInitialized { get; private set; }

		static bool FlagsSet { get; set; }

		static IReadOnlyList<string> s_flags;
		public static IReadOnlyList<string> Flags => s_flags ?? (s_flags = new List<string>().AsReadOnly());

		public static void Init(IEnumerable<Assembly> rendererAssemblies = null)
		{
			if (IsInitialized)
				return;

			string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

			System.Windows.Application.Current.Resources.MergedDictionaries.Add(new System.Windows.ResourceDictionary
			{
				Source = new Uri(string.Format("/{0};component/WPFResources.xaml", assemblyName), UriKind.Relative)
			});

			var accentColor = (WSolidColorBrush)System.Windows.Application.Current.Resources["AccentColor"];
			Color.SetAccent(Color.FromRgba(accentColor.Color.R, accentColor.Color.G, accentColor.Color.B, accentColor.Color.A));

			Log.Listeners.Add(new DelegateLogListener((c, m) => Console.WriteLine("[{0}] {1}", m, c)));
			Registrar.ExtraAssemblies = rendererAssemblies?.ToArray();

			Device.SetTargetIdiom(TargetIdiom.Desktop);
			Device.PlatformServices = new WPFPlatformServices();
			Device.Info = new WPFDeviceInfo();
			ExpressionSearch.Default = new WPFExpressionSearch();

			Registrar.RegisterAll(new[] { typeof(ExportRendererAttribute), typeof(ExportCellAttribute), typeof(ExportImageSourceHandlerAttribute) });

			Ticker.SetDefault(new WPFTicker());
			Device.SetIdiom(TargetIdiom.Desktop);
			Device.SetFlags(s_flags);

			IsInitialized = true;
		}

		public static void SetFlags(params string[] flags)
		{
			if (FlagsSet)
			{
				return;
			}

			if (IsInitialized)
			{
				throw new InvalidOperationException($"{nameof(SetFlags)} must be called before {nameof(Init)}");
			}

			s_flags = flags.ToList().AsReadOnly();
			FlagsSet = true;
		}
	}
}
