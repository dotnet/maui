namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31584, "Page OnAppearing triggered twice when navigating via ShellItem change with PresentationMode set to Modal", PlatformAffected.All)]
public partial class Issue31584 : Shell
{
	public static int s_modalAppearingCount = 0;

	public Issue31584()
	{
		Routing.RegisterRoute("ModalPage", typeof(ModalPage31584));

		Items.Add(new ShellItem
		{
			Route = "MainPage",
			Items = { new ShellContent { ContentTemplate = new DataTemplate(() => new MainPage31584()) } }
		});

		Items.Add(new ShellItem
		{
			Route = "Home",
			Items = { new ShellContent { ContentTemplate = new DataTemplate(() => new HomePage31584()) } }
		});
	}

	public partial class MainPage31584 : ContentPage
	{
		public MainPage31584()
		{
			Title = "Main Page";
			Content = new VerticalStackLayout
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Button
					{
						AutomationId = "NavigateToModalPageBtn",
						Text = "Navigate to Modal Page",
						Command = new Command(OnButtonClicked)
					}
				}
			};
		}

		void OnButtonClicked()
		{
			Shell.Current.GoToAsync("//Home/ModalPage");
		}
	}

	public partial class HomePage31584 : ContentPage
	{
		public HomePage31584()
		{
			Title = "Home Page";
			Content = new VerticalStackLayout
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					new Label { Text = "Home" }
				}
			};
		}
	}

	public partial class ModalPage31584 : ContentPage
	{
		Label _countLabel;
		Label _statusLabel;

		public ModalPage31584()
		{
			Title = "ModalPage";
			Shell.SetPresentationMode(this, PresentationMode.Modal);

			_countLabel = new Label 
			{ 
				Text = $"OnAppearing Count: {s_modalAppearingCount}",
			};
			
			_statusLabel = new Label 
			{ 
				Text = "Status: Not triggered yet",
				AutomationId = "Issue31584StatusLabel"
			};

			Content = new VerticalStackLayout
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Spacing = 16,
				Children =
				{
					new Label { Text = "My modal page" },
					_countLabel,
					_statusLabel
				}
			};
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			s_modalAppearingCount++;
			if (s_modalAppearingCount == 1)
			{
				_statusLabel.Text = "Success";
				_countLabel.Text = $"OnAppearing triggered once: {s_modalAppearingCount} ";
			}
			else
			{
				_statusLabel.Text = "Failure";
				_countLabel.Text = $"OnAppearing triggered more than once: {s_modalAppearingCount} ";
			}
		}
	}
}