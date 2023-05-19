using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if APP
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1554, "NRE: NavigationProxy.set_Inner", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone)]
	public partial class Issue1554 : ContentPage
	{
		public Issue1554()
		{
			BindingContext = new
			{
				Values = new[] { "ABC", "DEF", "GHI" }
			};

			InitializeComponent();
		}
	}
#endif
}

