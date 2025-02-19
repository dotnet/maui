using System.Collections.Generic;
using System.Reflection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.ControlGallery.Android.Tests;
using Microsoft.Maui.Controls.ControlGallery.Tests;
using NUnit;

[assembly: Dependency(typeof(PlatformTestSettings))]
namespace Microsoft.Maui.Controls.ControlGallery.Android.Tests
{
	public class PlatformTestSettings : IPlatformTestSettings
	{
		public PlatformTestSettings()
		{
			TestRunSettings = new Dictionary<string, object>
			{
				// Creating/modifying any renderers off the UI thread causes problems
				// so we want to force the tests to run on main
				{ FrameworkPackageSettings.RunOnMainThread, false }
			};
		}

		public Assembly Assembly { get => Assembly.Load("Xamarin.Forms.Platform.Android.UnitTests, Version = 2.0.0.0, Culture = neutral, PublicKeyToken = null"); }
		public Dictionary<string, object> TestRunSettings { get; }
	}
}