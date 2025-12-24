using System;
using System.Threading.Tasks;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33287, "DisplayAlertAsync throws NullReferenceException when page is no longer displayed", PlatformAffected.All)]
public partial class Issue33287 : NavigationPage
{
	public Issue33287() : base(new Issue33287MainPage())
	{
	}
}

public partial class Issue33287MainPage : ContentPage
{
	public Issue33287MainPage()
	{
		Title = "Issue 33287";
		
		var layout = new VerticalStackLayout 
		{ 
			Padding = 20, 
			Spacing = 10 
		};

		layout.Children.Add(new Label 
		{ 
			Text = "DisplayAlertAsync NullReferenceException Test",
			FontSize = 18,
			FontAttributes = FontAttributes.Bold
		});

		layout.Children.Add(new Label 
		{ 
			Text = "1. Tap 'Navigate to Second Page'\n2. Immediately tap 'Go Back'\n3. Wait 5 seconds - should NOT crash",
			TextColor = Colors.Gray
		});

		layout.Children.Add(new Button
		{
			Text = "Navigate to Second Page",
			AutomationId = "NavigateButton",
			Command = new Command(async () =>
			{
				Console.WriteLine("[Issue33287] Navigating to SecondPage");
				StatusLabel.Text = "Status: Navigating...";
				await Navigation.PushAsync(new Issue33287SecondPage(this));
			})
		});

		StatusLabel = new Label
		{
			Text = "Status: Ready",
			AutomationId = "StatusLabel",
			FontAttributes = FontAttributes.Bold
		};
		layout.Children.Add(StatusLabel);

		Content = layout;
	}

	public Label StatusLabel { get; private set; }

	public void UpdateStatus(string status)
	{
		StatusLabel.Text = $"Status: {status}";
		Console.WriteLine($"[Issue33287] {status}");
	}
}

public class Issue33287SecondPage : ContentPage
{
	private readonly Issue33287MainPage _mainPage;

	public Issue33287SecondPage(Issue33287MainPage mainPage)
	{
		_mainPage = mainPage;
		Title = "Second Page";
		
		var layout = new VerticalStackLayout 
		{ 
			Padding = 20, 
			Spacing = 10 
		};

		layout.Children.Add(new Label 
		{ 
			Text = "Second Page - Tap 'Go Back' quickly!",
			FontSize = 18,
			FontAttributes = FontAttributes.Bold
		});

		layout.Children.Add(new Button
		{
			Text = "Go Back",
			AutomationId = "GoBackButton",
			Command = new Command(async () =>
			{
				Console.WriteLine("[Issue33287] Going back...");
				await Navigation.PopAsync();
			})
		});

		Content = layout;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		
		Console.WriteLine("[Issue33287] SecondPage.OnAppearing - Starting 5 second delay");
		_mainPage?.UpdateStatus("On second page, waiting 5 seconds...");

		// Delay before showing alert
		await Task.Delay(5000);

		Console.WriteLine("[Issue33287] Attempting to show DisplayAlertAsync");
		_mainPage?.UpdateStatus("Showing alert from unloaded page...");

		try
		{
			// This should cause NullReferenceException if page is no longer displayed
			await DisplayAlertAsync("Test Alert", "This alert was delayed", "OK");
			
			Console.WriteLine("[Issue33287] ✅ DisplayAlertAsync succeeded (page still loaded or fix applied)");
			_mainPage?.UpdateStatus("✅ Alert shown successfully");
		}
		catch (NullReferenceException ex)
		{
			Console.WriteLine($"[Issue33287] ❌ NullReferenceException caught: {ex.Message}");
			_mainPage?.UpdateStatus("❌ NullReferenceException occurred!");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[Issue33287] ❌ Exception: {ex.GetType().Name}: {ex.Message}");
			_mainPage?.UpdateStatus($"❌ Exception: {ex.GetType().Name}");
		}
	}
}
