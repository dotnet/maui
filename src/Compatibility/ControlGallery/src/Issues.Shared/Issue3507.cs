﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3507, "[UWP] Scrollview with null content crashes on UWP",
		PlatformAffected.UWP)]
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
	[NUnit.Framework.Category(UITestCategories.ListView)]
#endif
	public class Issue3507 : TestContentPage
	{
		Label label;
		ScrollView scrollView;
		protected override void Init()
		{
			scrollView = new ScrollView();
			label = new Label();

			Content = new StackLayout()
			{
				Children =
				{
					label,
					scrollView
				}
			};
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await Task.Delay(500);
			scrollView.Content = new StackLayout();
			await Task.Delay(500);
			scrollView.Content = null;
			await Task.Delay(500);
			label.Text = "Success";
		}

#if UITEST
		[MovedToAppium]
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void NullContentOnScrollViewDoesntCrash()
		{
			RunningApp.WaitForElement("Success");
		}
#endif
	}
}
