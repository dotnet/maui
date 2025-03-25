using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 3809, "SetUseSafeArea is wiping out Page Padding ")]
public class Issue3809 : TestFlyoutPage
{
	const string _setPagePadding = "Set Page Padding";
	const string _safeAreaText = "Safe Area Enabled: ";
	const string _paddingLabel = "paddingLabel";
	const string _safeAreaAutomationId = "SafeAreaAutomation";

	Label label = null;
	protected override void Init()
	{
		label = new Label()
		{
			AutomationId = _paddingLabel
		};

		Flyout = new ContentPage() { Title = "Flyout" };
		Button button = null;
		button = new Button()
		{
			Text = $"{_safeAreaText}{true}",
			AutomationId = _safeAreaAutomationId,
			Command = new Command(() =>
			{
				bool safeArea = !Detail.On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>().UsingSafeArea();
				Detail.On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>().SetUseSafeArea(safeArea);
				button.Text = $"{_safeAreaText}{safeArea}";
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
				Device.BeginInvokeOnMainThread(() => label.Text = $"{Detail.Padding.Left}, {Detail.Padding.Top}, {Detail.Padding.Right}, {Detail.Padding.Bottom}");
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
			})
		};

		Detail = new ContentPage()
		{
			Title = "Details",
			Content = new StackLayout()
			{
				new Microsoft.Maui.Controls.ListView(ListViewCachingStrategy.RecycleElement)
				{
					ItemsSource = Enumerable.Range(0,200).Select(x=> x.ToString()).ToList()
				},
				label,
				button,
				new Button()
				{
					Text = _setPagePadding,
					Command = new Command(() =>
					{
						Detail.Padding = new Thickness(25, 25, 25, 25);
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
						Device.BeginInvokeOnMainThread(() => label.Text = $"{Detail.Padding.Left}, {Detail.Padding.Top}, {Detail.Padding.Right}, {Detail.Padding.Bottom}");
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
					})
				}
			}
		};

		Detail.Padding = new Thickness(25, 25, 25, 25);
		Detail.On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>().SetUseSafeArea(true);
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		label.Text = $"{Detail.Padding.Left}, {Detail.Padding.Top}, {Detail.Padding.Right}, {Detail.Padding.Bottom}";
	}
}
