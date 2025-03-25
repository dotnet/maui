using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 31602, "not possible to programmatically open master page after iPad landscape -> portrait rotation, also tests 31664")]
public class Bugzilla31602 : TestFlyoutPage
{
	protected override void Init()
	{
		BindingContext = new MasterViewModel1();
		((MasterViewModel1)BindingContext).MasterPage = this;

		Flyout = new SidemenuPage();
		Detail = new NavigationPage(new DetailPage1(Flyout as SidemenuPage));
	}

	public class SidemenuPage : ContentPage
	{
		public SidemenuPage()
		{
			Title = "Side";
			IconImageSource = "menu_icon.png";
			var lbl = new Label { Text = "SideMenu", AutomationId = "SideMenu" };
			var btn = new Button { Text = "Menu Opener" };

			btn.SetBinding(Button.CommandProperty, new Binding("OpenSideMenuCommand"));

			var stackpanel = new StackLayout { VerticalOptions = LayoutOptions.Center };

			stackpanel.Children.Add(btn);
			stackpanel.Children.Add(lbl);
			Content = stackpanel;
		}

		public void ChangeIcon()
		{
			IconImageSource = "bank.png";
		}
	}

	public class DetailPage1 : ContentPage
	{
		SidemenuPage _sideMenu;

		public DetailPage1(SidemenuPage menu)
		{
			_sideMenu = menu;
			Title = "Detail";
			var btns = new Button { Text = "Change Icon" };
			btns.Clicked += (object sender, EventArgs e) =>
			{
				_sideMenu.ChangeIcon();
			};
			var btn = new Button { Text = "Sidemenu Opener", AutomationId = "Sidemenu Opener" };
			btn.SetBinding(Button.CommandProperty, new Binding("OpenSideMenuCommand"));
			Content = new StackLayout { Children = { btns, btn } };
		}
	}


	public class MasterViewModel1 : INotifyPropertyChanged
	{
		public FlyoutPage MasterPage;

		public event PropertyChangedEventHandler PropertyChanged = delegate { };

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		bool _isMenuOpen;

		public bool IsMenuOpen
		{
			get { return _isMenuOpen; }
			private set
			{
				if (_isMenuOpen != value)
				{
					System.Diagnostics.Debug.WriteLine("setting new sidemenu visibility flag to: " + value);
					_isMenuOpen = value;
					OnPropertyChanged();
				}
			}
		}

		public ICommand OpenSideMenuCommand { get; private set; }

		public MasterViewModel1()
		{
			IsMenuOpen = true;
			OpenSideMenuCommand = new Command(OpenSideMenu);
		}

		void OpenSideMenu()
		{
			IsMenuOpen = true;
			MasterPage.IsPresented = IsMenuOpen;
		}
	}
}
