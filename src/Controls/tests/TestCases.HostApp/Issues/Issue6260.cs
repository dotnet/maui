namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 6260, "[Android] infinite layout loop",
		PlatformAffected.Android)]
	public class Issue6260 : TestContentPage
	{
		const string text = "If this number keeps increasing test has failed: ";
		static string success = text + "0";

		protected override void Init()
		{
			int measurecount = 0;

			var button = new Button()
			{
				Text = "Click me",
				BackgroundColor = Colors.Green,

			};

			var label = new Label()
			{
				AutomationId = success,
				Text = success
			};

			this.Appearing += (_, __) =>
			{
				button.ImageSource = "coffee.png";
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
				Device.BeginInvokeOnMainThread(() =>
				{
					button.MeasureInvalidated += (___, ____) =>
					{
						measurecount++;
						label.Text = text + measurecount.ToString();
					};
				});
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
			};

#pragma warning disable CS0618 // Type or member is obsolete
			Content = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = "Welcome to Xamarin.Forms!",
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.CenterAndExpand
					},
					new Entry(),
					button,
					label
				}
			};
#pragma warning restore CS0618 // Type or member is obsolete
		}
	}
}
