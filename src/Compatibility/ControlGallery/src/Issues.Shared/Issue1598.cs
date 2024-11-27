using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	public class FlyoutPageToolbarBug : FlyoutPage
	{
		public FlyoutPageToolbarBug()
		{
			Title = "FlyoutPageToolbarBug";

			Flyout = new ContentPage()
			{
				Title = "Flyout"
			};
			Detail = new ContentPage()
			{
				Title = "Detail",
			};
			Detail.ToolbarItems.Add(new ToolbarItem("ToolbarItem2", "Icon.png", ()
																				=>
			{ }));
		}
	}
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.TableView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1598, "FlyoutPageContainer does not handle adding of views which are already its children", PlatformAffected.Android)]
	public class Issue1598 : ContentPage
	{
		FlyoutPageToolbarBug _secondPage = new FlyoutPageToolbarBug();

		public Issue1598()
		{
			Title = "XamarinTest MainMenu";

			var menu1 = new MainMenuCell("FlyoutPage - Toolbar bug", "Icon.png");
			menu1.Tapped += (o, e) =>
			{
				Navigation.PushAsync(_secondPage);
			};

			var menu = new TableView()
			{
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand,
				HasUnevenRows = true,
				Intent = TableIntent.Menu,
				Root = new TableRoot {
					new TableSection ("Menu") {
						menu1,
					}
				}
			};

			Content = menu;

			ToolbarItems.Add(new ToolbarItem("ToolbarItem1", "bank.png", () => { }));
		}

		class MainMenuCell : ViewCell
		{
			public MainMenuCell(string title, string iconFile)
			{
				View = new StackLayout()
				{
					HorizontalOptions = LayoutOptions.FillAndExpand,
					VerticalOptions = LayoutOptions.FillAndExpand,
					Orientation = StackOrientation.Horizontal,
					Spacing = 15,
					Padding = 10,
					Children = {
						new Image () {
							Source = ImageSource.FromFile (iconFile),
							VerticalOptions = LayoutOptions.CenterAndExpand,
						},
						new Label () {
							Text = title,
							VerticalOptions = LayoutOptions.CenterAndExpand,
							FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
							FontAttributes = FontAttributes.Bold
						}
					}
				};
			}
		}
	}
}
