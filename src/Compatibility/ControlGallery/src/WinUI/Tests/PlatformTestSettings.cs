using System.Collections.Generic;
using System.Reflection;
using NUnit;
using Xamarin.Forms;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Windows.Tests;
using Xamarin.Forms.Controls.Tests;

[assembly: Dependency(typeof(PlatformTestSettings))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Windows.Tests
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
