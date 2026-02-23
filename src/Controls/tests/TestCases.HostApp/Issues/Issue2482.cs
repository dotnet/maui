namespace Maui.Controls.Sample.Issues
{

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
				BackgroundColor = Colors.Black,
				TextColor = Colors.White,
				Text = "Quickly Double Tap This Button",
				HeightRequest = 200,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				AutomationId = ButtonId
			};

			button.Clicked += async (sender, args) =>
			{
				await button.RotateToAsync(539, 3000, Easing.CubicOut);
				await button.RotateToAsync(0, 3000, Easing.CubicIn);

				_clicks += 1;

				if (_clicks == 2)
				{
					_result.IsVisible = true;
				}
			};

			var labelRunsBackground = new Label() { Text = "This should start updating with the time in a few seconds" };
			layout.Children.Add(labelRunsBackground);

#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			Device.StartTimer(TimeSpan.FromSeconds(1), () =>
			{
				labelRunsBackground.Dispatcher.Dispatch(() => labelRunsBackground.Text = DateTime.Now.ToString("HH:mm:ss"));
				return true;
			});
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete

			var threadpoolButton = new Button { Text = "Update Instructions from Thread Pool" };
			layout.Children.Add(threadpoolButton);

			this.Dispatcher.Dispatch(() => { instructions.Text = "updated from thread pool 1"; });

			threadpoolButton.Clicked += (o, a) =>
			{
				Task.Run(() =>
				{
					this.Dispatcher.Dispatch(() => { instructions.Text = "updated from thread pool 2"; });
				});
			};

			layout.Children.Add(instructions);
			layout.Children.Add(_result);
			layout.Children.Add(button);

			if (DeviceInfo.Platform == DevicePlatform.WinUI)
				layout.Children.Add(new Label { Text = "\xE76E", FontFamily = "Segoe MDL2 Assets", FontSize = 32 });

			Content = layout;
		}
	}
}