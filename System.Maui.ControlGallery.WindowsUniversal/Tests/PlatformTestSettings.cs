using System.Collections.Generic;
using System.Reflection;
using NUnit;
using System.Maui;
using System.Maui.ControlGallery.WindowsUniversal.Tests;
using System.Maui.Controls.Tests;

[assembly: Dependency(typeof(PlatformTestSettings))]
namespace System.Maui.ControlGallery.WindowsUniversal.Tests
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

		public Assembly Assembly { get => Assembly.Load("System.Maui.Platform.UAP.UnitTests, Version = 2.0.0.0, Culture = neutral, PublicKeyToken = null"); }
		public Dictionary<string, object> TestRunSettings { get; }
	}
}
