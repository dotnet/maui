using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using AbsoluteLayoutFlags = Microsoft.Maui.Layouts.AbsoluteLayoutFlags;

namespace Maui.Controls.Sample.Issues;

[Preserve(AllMembers = true)]
[Issue(IssueTracker.Bugzilla, 40092, "Ensure android devices with fractional scale factors (3.5) don't have a white line around the border"
	, PlatformAffected.Android)]

public class Bugzilla40092 : TestContentPage
{
	const string Black = "black";
	const string White = "white";
	const string Ok = "Ok";
	protected override void Init()
	{
		AbsoluteLayout mainLayout = new AbsoluteLayout()
		{
			BackgroundColor = Colors.White,
			AutomationId = White
		};

		// The root page of your application

		var thePage = new ContentView
		{
			BackgroundColor = Colors.Red,
			Content = mainLayout
		};

		BoxView view = new BoxView()
		{
			Color = Colors.Black,
			AutomationId = Black
		};

		mainLayout.Add(view);
		Content = thePage;

	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await DisplayAlert("Instruction", "If you see just the black color, the test pass. (Ignore the navigation bar)", Ok);
	}
}
