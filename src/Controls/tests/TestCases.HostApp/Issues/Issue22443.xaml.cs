using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using System;
using System.Threading.Tasks;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 22443, "App Crash on Scroll Animation while navigating away from Page", PlatformAffected.Android)]
public partial class Issue22443NavPage : NavigationPage
{
	public Issue22443NavPage() : base(new Issue22443())
	{

	}
}

public partial class Issue22443 : ContentPage
{
	public Issue22443()
	{
		InitializeComponent();
	}

	void Button_Clicked(object sender, EventArgs e)
	{
		Navigation.PushAsync(new Issue22443ScrollPage());
	}
}

public class Issue22443ScrollPage : ContentPage
{
	private ScrollView _scrollView;

	public Issue22443ScrollPage()
	{
		_scrollView = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Children =
				{
					new BoxView
					{
						HeightRequest = 10000,
						Background = new LinearGradientBrush(
							new GradientStopCollection
							{
								new GradientStop(Colors.White, .1f),
								new GradientStop(Colors.Black, .9f)
							})
					},
					new Label
					{
						FontSize = 40,
						Text = "End of Second Page",
						HorizontalOptions = LayoutOptions.Center,
					},
				}
			}
		};

		Content = _scrollView;
	}

	protected override async void OnAppearing()
	{
		MainThread.BeginInvokeOnMainThread(() =>_scrollView!.ScrollToAsync(0, 10000, true));

		await Task.Delay(200);
		await Navigation.PopAsync();
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		_scrollView?.Handler?.DisconnectHandler();
	}
}
