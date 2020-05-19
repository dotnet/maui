using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	public class MasterDetailToolbarBug : MasterDetailPage
	{
		public MasterDetailToolbarBug ()
		{
			Title = "MasterDetailToolbarBug";

			Master = new ContentPage () {
				Title = "Master"
			};
			Detail = new ContentPage () {
				Title = "Detail",
			};
			Detail.ToolbarItems.Add (new ToolbarItem ("ToolbarItem2", "Icon.png", ()
			                                                                      => { }));
		}
	}

	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 1598, "MasterDetailContainer does not handle adding of views which are already its children", PlatformAffected.Android)]
	public class Issue1598 : ContentPage
	{
		MasterDetailToolbarBug _secondPage = new MasterDetailToolbarBug ();

		public Issue1598 ()
		{
			Title = "XamarinTest MainMenu";

			var menu1 = new MainMenuCell ("MasterDetail - Toolbar bug", "Icon.png");
			menu1.Tapped += (o, e) => {
				Navigation.PushAsync (_secondPage);
			};

			var menu = new TableView () {
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

			ToolbarItems.Add (new ToolbarItem ("ToolbarItem1", "bank.png", () => { }));
		}

		class MainMenuCell : ViewCell
		{
			public MainMenuCell (string title, string iconFile)
			{
				View = new StackLayout () {
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
#pragma warning disable 618
							Font = Font.SystemFontOfSize (NamedSize.Large,
#pragma warning restore 618
							                              FontAttributes.Bold)
						}
					}
				};
			}
		}
	}
}
