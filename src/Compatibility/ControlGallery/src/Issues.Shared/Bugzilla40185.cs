﻿using System;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 40185, "[UWP] ContentPage does not have proper right bounds in landscape", PlatformAffected.WinRT)]
	public class Bugzilla40185 : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Button
					{
						Text = "Switch Main Page",
						Command = new Command(SwitchMainPage)
					}
				}
			};
		}

		void SwitchMainPage()
		{
			Application.Current.MainPage = new ContentPage
			{
				BackgroundColor = Colors.White,
				Content = new Label
				{
					Text = "This text should be in bounds in landscape mode.",
					HorizontalTextAlignment = TextAlignment.End,
					VerticalTextAlignment = TextAlignment.Center,
					TextColor = Colors.Black
				}
			};
		}
	}
}
