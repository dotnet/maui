﻿using System;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 31806, "[8.1/UWP] PopToRootAsync crashes due to not setting the current page correctly", PlatformAffected.WinRT)]
	public class Bugzilla31806 : TestContentPage
	{
		protected override void Init()
		{
			Content = new StackLayout
			{
				Children =
				{
					new Button
					{
						Text = "Navigate to a new page",
						Command = new Command(() =>
						{
							Navigation.PushAsync(new ContentPage
							{
								Content = new StackLayout
								{
									Children =
									{
										new Label
										{
											Text = "Pressing this button should let the navigation return to the repro list"
										},
										new Button
										{
											Text = "Call PopToRootAsync()",
											Command = new Command(() =>
											{
												Navigation.PopToRootAsync();
											})
										}
									}
								}
							});
						})
					}
				}
			};
		}
	}
}
