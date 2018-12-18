using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Threading;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Animation)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2482,
		"Animating a `View` that is currently animating will throw `System.InvalidOperationException`", 
		PlatformAffected.All)]
	public class Issue2482 : TestContentPage
	{
		Label _result;
		int _clicks;

		const string ButtonId = "SpinButton";
		const string Success = "Success";

		protected override void Init()
		{
			var layout = new StackLayout();

			var instructions = new Label { Text = "Tap the button below twice quickly." 
												+ " If the application crashes, this test has failed." };

			_result = new Label { Text = Success, IsVisible = false };

			var button = new Button
			{
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "Quickly Double Tap This Button",
				HeightRequest = 200,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				AutomationId = ButtonId
			};

			button.Clicked += async (sender, args) =>
			{
				await button.RotateTo(539, 3000, Easing.CubicOut);
				await button.RotateTo(0, 3000, Easing.CubicIn);

				_clicks += 1;

				if (_clicks == 2)
				{
					_result.IsVisible = true;
				}
			};

			layout.Children.Add(instructions);
			layout.Children.Add(_result);
			layout.Children.Add(button);

			Content = layout;
		}

#if UITEST
		[Test]
		public void AnimationCancel()
		{
			RunningApp.WaitForElement(ButtonId);
			RunningApp.DoubleTap(ButtonId);
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}