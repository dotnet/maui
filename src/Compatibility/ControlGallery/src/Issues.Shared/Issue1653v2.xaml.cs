using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if APP
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1653, "ScrollView exceeding bounds - v2", PlatformAffected.Android | PlatformAffected.iOS | PlatformAffected.WinPhone)]
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Issue1653v2 : ContentPage
	{
		public Issue1653v2()
		{
			InitializeComponent();

			for (int i = 0; i < 40; i++)
				addonGroupStack.Children.Add(new Label { Text = "Testing 123" });
		}
	}
#endif
}

