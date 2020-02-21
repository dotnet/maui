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
				// Creating/modifying any renderers off the UI thread causes problems
				// so we want to force the tests to run on main
				{ FrameworkPackageSettings.RunOnMainThread, true }
			};
		}

		public Assembly Assembly { get => Assembly.GetExecutingAssembly(); }
		public Dictionary<string, object> TestRunSettings { get; }
	}
}
