using System.Collections.Generic;
using System.Reflection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Tests;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI.Tests;
using NUnit;

[assembly: Dependency(typeof(PlatformTestSettings))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI.Tests
{
	public class PlatformTestSettings : IPlatformTestSettings
	{
		public PlatformTestSettings()
		{
			TestRunSettings = new Dictionary<string, object>
			{
				{ FrameworkPackageSettings.RunOnMainThread, false }
			};
		}

		public Assembly Assembly { get => Assembly.Load("Xamarin.Forms.Platform.UAP.UnitTests, Version = 2.0.0.0, Culture = neutral, PublicKeyToken = null"); }
		public Dictionary<string, object> TestRunSettings { get; }
	}
}
