namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 8715, "NullReferenceException Microsoft.Maui.Controls.Platform.iOS.StructuredItemsViewRenderer [Bug]",
		PlatformAffected.iOS)]
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Issue8715 : TestShell
	{
		public Issue8715()
		{
			InitializeComponent();
		}

		protected override void Init()
		{
		}
	}

	public class _8715AboutPage : ContentPage
	{
		public _8715AboutPage()
		{
			Content = new Label { Text = "Open the flyout. Tap 'CollectionView'. Tap the 'Toggle' button. Now tap it again. Open the flyout. Tap 'About'. Open the flyout yet again. Tap 'CollectionView' yet again. If the application didn't crash, the test has passed." };
			Title = "8715 About";
		}
	}

	public class _8715ItemsPage : ContentPage
	{
		public _8715ItemsPage()
		{
			Title = "8715 Items";

			var layout = new StackLayout();

			var cv = new CollectionView() { IsVisible = false };
			var button = new Button() { Text = "Toggle" };

			button.Clicked += (sender, args) => { cv.IsVisible = !cv.IsVisible; };
			cv.SetBinding(ItemsView.ItemsSourceProperty, new Binding("."));

			cv.ItemTemplate = new DataTemplate(() =>
			{

				var l = new StackLayout();

				var label1 = new Label();
				label1.SetBinding(Label.TextProperty, new Binding("."));

				var label2 = new Label();
				label2.SetBinding(Label.TextProperty, new Binding("."));

				l.Children.Add(label1);
				l.Children.Add(label2);

				return l;

			});

			layout.Children.Add(button);
			layout.Children.Add(cv);

			Content = layout;

			var list = new List<string>() { "one", "two" };
			BindingContext = list;
		}
	}
}