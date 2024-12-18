namespace Maui.Controls.Sample.Issues;

public partial class Issue5949_2 : ContentPage
{
	public const string BackButton = "5949GoBack";
	public const string ToolBarItem = "Login";

	public Issue5949_2()
	{
		InitializeComponent();
		ToolbarItems.Add(new ToolbarItem(ToolBarItem, null, () => Navigation.PushAsync(LoginPage())) { AutomationId = ToolBarItem });
		BindingContext = new _5949ViewModel();
	}

	class _5949ViewModel
	{
		public _5949ViewModel()
		{
			Items = new List<string>
			{
				"one", "two", "three"
			};
		}

		public List<string> Items { get; set; }
	}

	ContentPage LoginPage()
	{
		var page = new ContentPage
		{
			Title = "Issue 5949"
		};

		var button = new Button { Text = "Back", AutomationId = BackButton };

		button.Clicked += ButtonClicked;

		page.Content = button;

		return page;
	}

	private void ButtonClicked(object sender, EventArgs e)
	{
		Application.Current.MainPage = new Issue5949_1();
	}
}