using System.Collections.Generic;
using System.Reflection;
using NUnit;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS.Tests;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Tests;
using Microsoft.Maui.Controls.Internals;

[assembly: Dependency(typeof(PlatformTestSettings))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.iOS.Tests
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

		public Assembly Assembly { get => Assembly.Load("Microsoft.Maui.Controls.Compatibility.UnitTests, Version = 2.0.0.0, Culture = neutral, PublicKeyToken = null"); }
		public Dictionary<string, object> TestRunSettings { get; }
	}
}