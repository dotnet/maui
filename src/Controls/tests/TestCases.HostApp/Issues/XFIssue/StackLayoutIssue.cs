using System.Diagnostics;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "StackLayout issue", PlatformAffected.All, NavigationBehavior.PushModalAsync)]
public class StackLayoutIssue : TestContentPage
{
	protected override void Init()
	{
#pragma warning disable CS0618 // Type or member is obsolete
		var logo = new Image
		{
			Source = "cover1.jpg",
			WidthRequest = 20,
			HeightRequest = 20,
			VerticalOptions = LayoutOptions.FillAndExpand,
		};
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
		var winPrizeLabel = new Label
		{
			Text = "Win a Xamarin Prize",
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center,
			VerticalOptions = LayoutOptions.FillAndExpand
		};
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
		switch (Device.RuntimePlatform)
		{
			case Device.iOS:
				winPrizeLabel.FontFamily = "HelveticaNeue-UltraLight";
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
				winPrizeLabel.FontSize = Device.GetNamedSize(NamedSize.Large, winPrizeLabel);
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
				break;
		}
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete

		StackLayout form = MakeForm();

		var spinButton = new Button
		{
			Text = "Spin"
		};

		var mainLayout = new StackLayout
		{
			logo,
			winPrizeLabel,
			form,
			spinButton
		};

		Content = mainLayout;
		Padding = new Thickness(50);
	}

	StackLayout MakeForm()
	{
		var nameEntry = new Entry
		{
			Placeholder = "Full Name"
		};
		var emailEntry = new Entry
		{
			Placeholder = "Email"
		};

		var companyEntry = new Entry
		{
			Placeholder = "Company"
		};

		var switchContainer = new StackLayout
		{
			Orientation = StackOrientation.Horizontal
		};

		var switchLabel = new Label
		{
			Text = "Completed Azure Mobile Services Challenge?"
		};
		var switchElement = new Microsoft.Maui.Controls.Switch();

		switchContainer.Add(switchLabel);
		switchContainer.Add(switchElement);

		var stackLayout = new StackLayout
		{
			nameEntry,
			emailEntry,
			companyEntry,
			switchContainer
		};

		stackLayout.MinimumWidthRequest = 50;

		var entryContainer = stackLayout;

		var qrButton = new Image
		{
			Source = "cover1.jpg",
			WidthRequest = 100,
			HeightRequest = 100
		};

		var result = new StackLayout
		{
			Orientation = StackOrientation.Horizontal
		};

		result.Add(entryContainer);
		result.Add(qrButton);

		result.SizeChanged += (sender, args) =>
		{
			Debug.WriteLine(result.Bounds);
		};

		return result;
	}
}
