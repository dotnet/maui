using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 33356, "[iOS] Clicking on search suggestions fails to navigate to detail page correctly", PlatformAffected.iOS)]
	public partial class Issue33356 : Shell
	{
		public Issue33356()
		{
			Routing.RegisterRoute(nameof(Issue33356DetailPage), typeof(Issue33356DetailPage));
			InitializeComponent();
		}
	}

	public class Issue33356SearchHandler : SearchHandler
	{
		private readonly List<Issue33356Animal> _animals = new()
		{
			new Issue33356Animal { Name = "Bengal Cat", Description = "A domestic cat breed" },
			new Issue33356Animal { Name = "Black Cat", Description = "A cat with black fur" },
			new Issue33356Animal { Name = "Siamese Cat", Description = "A breed of domestic cat" },
			new Issue33356Animal { Name = "Persian Cat", Description = "A long-haired breed" },
			new Issue33356Animal { Name = "Dog", Description = "Man's best friend" },
			new Issue33356Animal { Name = "Elephant", Description = "Large mammal" }
		};

		protected override void OnQueryChanged(string oldValue, string newValue)
		{
			base.OnQueryChanged(oldValue, newValue);

			if (string.IsNullOrWhiteSpace(newValue))
			{
				ItemsSource = null;
			}
			else
			{
				ItemsSource = _animals
					.Where(a => a.Name.Contains(newValue, StringComparison.OrdinalIgnoreCase))
					.ToList();
			}
		}

		protected override async void OnItemSelected(object item)
		{
			base.OnItemSelected(item);

			if (item is Issue33356Animal animal)
			{
				// Clear the search
				Query = string.Empty;
				ItemsSource = null;

				// Navigate to detail page - this is where the bug occurs on iOS
				await Shell.Current.GoToAsync(nameof(Issue33356DetailPage), true, new Dictionary<string, object>
				{
					{ "Animal", animal }
				});
			}
		}
	}

	public class Issue33356Animal
	{
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
	}

	[QueryProperty(nameof(Animal), "Animal")]
	public class Issue33356DetailPage : ContentPage
	{
		private Issue33356Animal _animal;

		public Issue33356Animal Animal
		{
			get => _animal;
			set
			{
				_animal = value;
				UpdateUI();
			}
		}

		public Issue33356DetailPage()
		{
			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 10,
				Children =
				{
					new Label
					{
						AutomationId = "DetailPageTitle",
						Text = "Detail Page",
						FontSize = 24,
						FontAttributes = FontAttributes.Bold
					},
					new Label
					{
						AutomationId = "AnimalName",
						Text = "Loading...",
						FontSize = 18
					},
					new Label
					{
						AutomationId = "AnimalDescription",
						Text = "",
						FontSize = 14,
						TextColor = Colors.Gray
					},
					new Label
					{
						AutomationId = "NavigationSuccess",
						Text = "Navigation Successful!",
						FontSize = 16,
						TextColor = Colors.Green,
						FontAttributes = FontAttributes.Bold
					}
				}
			};
		}

		private void UpdateUI()
		{
			if (_animal != null && Content is VerticalStackLayout layout)
			{
				var nameLabel = layout.Children.OfType<Label>().FirstOrDefault(l => l.AutomationId == "AnimalName");
				var descLabel = layout.Children.OfType<Label>().FirstOrDefault(l => l.AutomationId == "AnimalDescription");
				
				if (nameLabel != null)
					nameLabel.Text = _animal.Name;
				if (descLabel != null)
					descLabel.Text = _animal.Description;
			}
		}
	}
}
