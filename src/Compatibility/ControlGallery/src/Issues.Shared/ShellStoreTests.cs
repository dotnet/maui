using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class ShellStoreTests
	{
#if UITEST

		Xamarin.UITest.IApp RunningApp;
		[SetUp]
		public void ShellStoreSetup()
		{
			RunningApp = AppSetup.Setup();
			if (RunningApp.Query("SwapRoot - Store Shell").Length > 0)
				RunningApp.Tap("SwapRoot - Store Shell");
			else
				RunningApp.NavigateTo("SwapRoot - Store Shell");

			RunningApp.WaitForElement("Welcome to the HomePage");
		}

		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void LoadsWithoutCrashing()
		{
			RunningApp.WaitForElement("Welcome to the HomePage");
		}
#endif
	}
}
