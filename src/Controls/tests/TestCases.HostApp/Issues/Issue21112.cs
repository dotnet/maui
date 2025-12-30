using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 21112, "TableView TextCell command executes only once", PlatformAffected.UWP)]
	public class Issue21112 : NavigationPage
	{
		public Issue21112() : base(new MainPage())
		{

		}
	}

	public class MainPage : ContentPage
	{
		public MainPage()
		{
			var stackLayout = new StackLayout();
			BindingContext = new Issue21112ViewModel();

			var headerTextCell = new TextCell
			{
				Text = "Header demo",
				Detail = "Absolute positioning and sizing"
			};

			headerTextCell.SetBinding(TextCell.CommandProperty, new Binding("NavigateCommand"));
			headerTextCell.CommandParameter = typeof(DemoPage);

			var tableView = new TableView
			{
				Intent = TableIntent.Menu,
				Root = new TableRoot
				{
					new TableSection("XAML")
					{
						headerTextCell
					}
				}
			};

			var button = new Button
			{
				Text = "Trigger TextCell Click",
				AutomationId = "MainPageButton"
			};

			button.Clicked += (sender, e) =>
			{
				headerTextCell.Command.Execute(headerTextCell.CommandParameter);
			};

			stackLayout.Children.Add(tableView);
			stackLayout.Children.Add(button);

			Content = stackLayout;
		}
	}

	public partial class DemoPage : ContentPage
	{
		public DemoPage()
		{
			Title = "Text demo";

			var button = new Button
			{
				AutomationId = "NavigatedPageButton",
				Text = "Click"
			};

			var label = new Label
			{
				AutomationId = "NavigatedPageLabel",
				Text = "Main Page"
			};

			button.Clicked += (sender, e) =>
			{
				Navigation.PopAsync();
			};

			Content = new StackLayout
			{
				Children =
				{
					button,
					label
				}
			};
		}
	}

	public class Issue21112ViewModel
	{
		public ICommand NavigateCommand { get; set; }

		[UnconditionalSuppressMessage("TrimAnalysis", "IL2111", 
			Justification = "The lambda expression in NavigateCommand is only used with known page types that have public parameterless constructors.")]
		public Issue21112ViewModel()
		{
			NavigateCommand = new Command<Type>(async ([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type pageType) =>
			{
				Page page = Activator.CreateInstance(pageType) as Page;
				if (page is not null)
				{
					await Application.Current.MainPage.Navigation.PushAsync(page);
				}
			});
		}
	}
}
