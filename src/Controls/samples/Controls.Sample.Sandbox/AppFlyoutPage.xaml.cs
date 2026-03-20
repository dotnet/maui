using Maui.Controls.Sample.Services;

namespace Maui.Controls.Sample;

public partial class AppFlyoutPage : FlyoutPage
{
	public class MenuItem
	{
		public string Title { get; set; } = string.Empty;
		public Type PageType { get; set; } = typeof(Page);
	}

	public List<MenuItem> MenuItems { get; set; }

	public AppFlyoutPage()
	{
		InitializeComponent();

		MenuItems = new List<MenuItem>
		{
			new MenuItem { Title = "page 1", PageType = typeof(Page1) },
			new MenuItem { Title = "page 2", PageType = typeof(Page2) }
		};

		BindingContext = this;

		// Set default detail page
		Detail = new NavigationPage(new Page1());
	}

	private void OnMenuItemSelected(object? sender, SelectionChangedEventArgs e)
	{
		if (e.CurrentSelection.FirstOrDefault() is MenuItem selectedItem)
		{
			var navigationService = new NavigationService();
			navigationService.Navigate(selectedItem.PageType);

			// Close the flyout after selection
			IsPresented = false;
		}
	}
}
