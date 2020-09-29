using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
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

			var instructions = new Label
			{
				Text = "Tap the button below twice quickly."
												+ " If the application crashes, this test has failed."
			};

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

			var labelRunsBackground = new Label() { Text = "This should start updating with the time in a few seconds" };
			layout.Children.Add(labelRunsBackground);

			Device.StartTimer(TimeSpan.FromSeconds(1), () =>
			{
				labelRunsBackground.Dispatcher.BeginInvokeOnMainThread(() => labelRunsBackground.Text = DateTime.Now.ToString("HH:mm:ss"));
				return true;
			});

			var threadpoolButton = new Button { Text = "Update Instructions from Thread Pool" };
			layout.Children.Add(threadpoolButton);

			this.Dispatcher.BeginInvokeOnMainThread(() => { instructions.Text = "updated from thread pool 1"; });

			threadpoolButton.Clicked += (o, a) =>
			{
				Task.Run(() =>
				{
					this.Dispatcher.BeginInvokeOnMainThread(() => { instructions.Text = "updated from thread pool 2"; });
				});
			};

			layout.Children.Add(instructions);
			layout.Children.Add(_result);
			layout.Children.Add(button);

			Content = layout;
		}

#if UITEST
		[Test]
		[Ignore("Fails intermittently on TestCloud")]
		[Category(Core.UITests.UITestCategories.ManualReview)]
		public void AnimationCancel()
		{
			RunningApp.WaitForElement(ButtonId);
			RunningApp.DoubleTap(ButtonId);
			RunningApp.WaitForElement(Success, timeout: TimeSpan.FromSeconds(25));
		}
#endif
	}
}