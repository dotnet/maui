using System.Collections.Generic;
using System.Reflection;
using NUnit;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.WindowsUniversal.Tests;
using Xamarin.Forms.Controls.Tests;

[assembly: Dependency(typeof(PlatformTestSettings))]
namespace Xamarin.Forms.ControlGallery.WindowsUniversal.Tests
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
