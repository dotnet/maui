using System.Collections.Generic;
using System.Reflection;
using NUnit;
using System.Maui;
using System.Maui.ControlGallery.iOS.Tests;
using System.Maui.Controls.Tests;
using System.Maui.Internals;

[assembly: Dependency(typeof(PlatformTestSettings))]
namespace System.Maui.ControlGallery.iOS.Tests
{
	[Preserve(AllMembers = true)]
	public class PlatformTestSettings : IPlatformTestSettings
	{
		public PlatformTestSettings()
		{
			TestRunSettings = new Dictionary<string, object>
			{
				{ FrameworkPackageSettings.RunOnMainThread, false }
			};
		}

		public Assembly Assembly { get => Assembly.Load("System.Maui.Platform.iOS.UnitTests, Version = 2.0.0.0, Culture = neutral, PublicKeyToken = null"); }
		public Dictionary<string, object> TestRunSettings { get; }
	}
}