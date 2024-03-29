﻿using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5168, "Incorrect increments in stepper with small increments... - Droid", PlatformAffected.Android)]
	public class Issue5168 : TestContentPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			Label header = new Label
			{
				Text = "Please make use of the stepper below to step up and back to zero to ensure no Exponent value is displayed...",
				FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
				HorizontalOptions = LayoutOptions.Center
			};

			Stepper stepper = new Stepper
			{
				Minimum = 0.0,
				Maximum = 1.0,
				Increment = 0.05
			};

			Label valueLabel = new Label
			{
				Text = $"Value - {stepper.Value.ToString()}"
			};

			stepper.ValueChanged += (s, e) =>
			{
				valueLabel.Text = e.NewValue.ToString();
			};

			Content = new StackLayout
			{
				Padding = new Thickness(20),
				Children =
				{
					header,
					stepper,
					valueLabel
				}
			};
		}

	}
}