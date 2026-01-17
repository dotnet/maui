using Microsoft.Maui.Controls;
using System.Collections.Generic;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 33415, "ApplyQueryAttributes gets called with empty Dictionary on back", PlatformAffected.All)]
	public class Issue33415 : TestShell
	{
		protected override void Init()
		{
			// Start on a landing page, not the test page
			var landingPage = new ContentPage 
			{ 
				Content = new Button 
				{ 
					AutomationId = "NavigateToMainPageButton", 
					Text = "Navigate to Main Page with Parameters",
					Command = new Command(async () => 
					{
						await Shell.Current.GoToAsync("MainPage", new Dictionary<string, object>
						{
							{ "TestKey", "TestValue" }
						});
					})
				}
			};
			Items.Add(new ShellContent { Title = "Landing", Content = landingPage });
			
			// Register the main test page as a route (not starting page)
			Routing.RegisterRoute("MainPage", typeof(Issue33415MainPage));
			Routing.RegisterRoute("SecondPage", typeof(Issue33415SecondPage));
		}
	}

	public class Issue33415MainPage : ContentPage, IQueryAttributable
	{
		private readonly Label _statusLabel;
		private readonly Label _callCountLabel;
		private readonly Button _navigateButton;
		private int _applyQueryCallCount = 0;

		public Issue33415MainPage()
		{
			_statusLabel = new Label
			{
				AutomationId = "StatusLabel",
				Text = "Status: Not called",
				Margin = new Thickness(10)
			};

			_callCountLabel = new Label
			{
				AutomationId = "CallCountLabel",
				Text = "Call count: 0",
				Margin = new Thickness(10)
			};

			_navigateButton = new Button
			{
				AutomationId = "NavigateButton",
				Text = "Navigate to Second Page",
				Margin = new Thickness(10)
			};

			_navigateButton.Clicked += OnNavigateClicked;

			Content = new StackLayout
			{
				Padding = new Thickness(10),
				Children =
				{
					new Label
					{
						Text = "Issue 33415 - ApplyQueryAttributes called with empty Dictionary on back",
						FontSize = 16,
						FontAttributes = FontAttributes.Bold,
						Margin = new Thickness(0, 0, 0, 20)
					},
					_statusLabel,
					_callCountLabel,
					_navigateButton
				}
			};
		}

		public void ApplyQueryAttributes(IDictionary<string, object> query)
		{
			_applyQueryCallCount++;
			_callCountLabel.Text = $"Call count: {_applyQueryCallCount}";

			if (query.Count == 0)
			{
				_statusLabel.Text = "Status: ApplyQueryAttributes called with EMPTY dictionary";
			}
			else
			{
				_statusLabel.Text = $"Status: ApplyQueryAttributes called with {query.Count} parameter(s)";
				
				// According to documentation, calling Clear() should prevent
				// ApplyQueryAttributes from being called again on back navigation
				query.Clear();
			}
		}

		private async void OnNavigateClicked(object sender, System.EventArgs e)
		{
			// Navigate to second page - pass through the same parameter for testing
			await Shell.Current.GoToAsync("SecondPage", new Dictionary<string, object>
			{
				{ "TestKey", "TestValue" }
			});
		}
	}

	[QueryProperty(nameof(TestKey), "TestKey")]
	public class Issue33415SecondPage : ContentPage
	{
		private readonly Label _receivedLabel;
		private string _testKey;

		public string TestKey
		{
			get => _testKey;
			set
			{
				_testKey = value;
				_receivedLabel.Text = $"Received: {value ?? "null"}";
			}
		}

		public Issue33415SecondPage()
		{
			_receivedLabel = new Label
			{
				AutomationId = "ReceivedLabel",
				Text = "Received: null",
				Margin = new Thickness(10)
			};

			var backButton = new Button
			{
				AutomationId = "BackButton",
				Text = "Go Back",
				Margin = new Thickness(10)
			};

			backButton.Clicked += async (s, e) => await Shell.Current.GoToAsync("..");

			Content = new StackLayout
			{
				Padding = new Thickness(10),
				Children =
				{
					new Label
					{
						Text = "Second Page",
						FontSize = 16,
						FontAttributes = FontAttributes.Bold,
						Margin = new Thickness(0, 0, 0, 20)
					},
					_receivedLabel,
					backButton
				}
			};
		}
	}
}
