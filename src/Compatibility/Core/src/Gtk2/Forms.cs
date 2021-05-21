using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Maui.Controls.Compatibility.Internals;
using Microsoft.Maui.Controls.Compatibility.Platform.GTK;

namespace Microsoft.Maui.Controls.Compatibility
{
	public static class Forms
	{
		internal static string BarTextColor = "Xamarin.BarTextColor";
		internal static string BarBackgroundColor = "Xamarin.BarBackgroundColor";

		const string LogFormat = "[{0}] {1}";

		public static bool IsInitialized { get; private set; }
		static bool FlagsSet { get; set; }

		static IReadOnlyList<string> s_flags;
		public static IReadOnlyList<string> Flags => s_flags ?? (s_flags = new List<string>().AsReadOnly());

		public static void Init(IEnumerable<Assembly> rendererAssemblies = null)
		{
			if (IsInitialized)
				return;

			Log.Listeners.Add(new DelegateLogListener((c, m) => Debug.WriteLine(LogFormat, c, m)));

			Registrar.ExtraAssemblies = rendererAssemblies?.ToArray();

			Device.SetIdiom(TargetIdiom.Desktop);
			Device.SetFlags(s_flags);
			Device.PlatformServices = new GtkPlatformServices();
			Device.Info = new GtkDeviceInfo();
			Color.SetAccent(Color.FromHex("#3498DB"));
			ExpressionSearch.Default = new GtkExpressionSearch();

			Registrar.RegisterAll(new[]
			{
				typeof(ExportCellAttribute),
				typeof(ExportImageSourceHandlerAttribute),
				typeof(ExportRendererAttribute)
			});

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
