namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 33420, "System.InvalidCastException when using QueryPropertyAttribute with nullable types", PlatformAffected.All)]
	public partial class Issue33420 : Shell
	{
		public Issue33420()
		{
			InitializeComponent();

			Routing.RegisterRoute(nameof(Issue33420DetailsPage), typeof(Issue33420DetailsPage));
		}
	}

	public class Issue33420MainPage : ContentPage
	{
		public Issue33420MainPage()
		{
			var button = new Button
			{
				Text = "Navigate with Nullable",
				AutomationId = "NavigateButton"
			};
			
			button.Clicked += async (s, e) =>
			{
				try
				{
					var param = new Dictionary<string, object>
					{
						{ nameof(Issue33420DetailsPage.ID), (long?)1 }
					};
					await Shell.Current.GoToAsync(nameof(Issue33420DetailsPage), param);
				}
				catch (Exception ex)
				{
					// If navigation fails, update button text to indicate error
					button.Text = $"Error: {ex.GetType().Name}";
				}
			};

			Content = new VerticalStackLayout
			{
				Children =
				{
					new Label
					{
						Text = "Test nullable QueryProperty navigation",
						AutomationId = "MainLabel"
					},
					button
				},
				Padding = 10,
				Spacing = 10
			};
		}
	}

	[QueryProperty(nameof(ID), nameof(ID))]
	public class Issue33420DetailsPage : ContentPage
	{
		private readonly Label _resultLabel;

		public long? ID
		{
			get => (long?)GetValue(IDProperty);
			set => SetValue(IDProperty, value);
		}

		public static readonly BindableProperty IDProperty =
			BindableProperty.Create(nameof(ID), typeof(long?), typeof(Issue33420DetailsPage), null, propertyChanged: OnIDChanged);

		private static void OnIDChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is Issue33420DetailsPage page)
			{
				page._resultLabel.Text = $"Success: ID={newValue}";
			}
		}

		public Issue33420DetailsPage()
		{
			_resultLabel = new Label
			{
				Text = "Waiting for navigation...",
				AutomationId = "ResultLabel"
			};

			Content = new VerticalStackLayout
			{
				Children = { _resultLabel },
				Padding = 10
			};
		}
	}
}
