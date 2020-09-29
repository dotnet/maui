using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class ShellStoreTests
	{
#if UITEST

		IApp RunningApp;
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
		public void LoadsWithoutCrashing()
		{
			RunningApp.WaitForElement("Welcome to the HomePage");
		}
#endif
	}
}
