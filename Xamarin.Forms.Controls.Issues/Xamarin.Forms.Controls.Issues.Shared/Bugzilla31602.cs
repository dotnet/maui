using System;
using Xamarin.Forms.CustomAttributes;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest.iOS;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.MasterDetailPage)]
#endif

	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 31602, "not possible to programmatically open master page after iPad landscape -> portrait rotation, also tests 31664")]
	public class Bugzilla31602 : TestMasterDetailPage
	{
		protected override void Init ()
		{
			BindingContext = new MasterViewModel1 ();
			((MasterViewModel1)BindingContext).MasterPage = this;

			Master = new SidemenuPage ();
			Detail = new NavigationPage (new DetailPage1 (Master as SidemenuPage));
		}

		public class SidemenuPage : ContentPage
		{
			public SidemenuPage ()
			{
				Title = "Side";
				Icon = "menuIcon.png";
				var lbl = new Label { Text = "SideMenu" };
				var btn = new Button { Text = "Menu Opener"  };

				btn.SetBinding (Button.CommandProperty, new Binding ("OpenSideMenuCommand"));

				var stackpanel = new StackLayout { VerticalOptions = LayoutOptions.Center };

				stackpanel.Children.Add (btn);
				stackpanel.Children.Add (lbl);
				Content = stackpanel;
			}

			public void ChangeIcon() {
				Icon = "bank.png";
			}
		}

		public class DetailPage1 : ContentPage
		{
			SidemenuPage _sideMenu;

			public DetailPage1 (SidemenuPage menu)
			{
				_sideMenu = menu;
				Title = "Detail";
				var btns = new Button { Text = "Change Icon" };
				btns.Clicked += (object sender, EventArgs e) => {
					_sideMenu.ChangeIcon();
				};
				var btn = new Button { Text = "Sidemenu Opener"  };
				btn.SetBinding (Button.CommandProperty, new Binding ("OpenSideMenuCommand"));
				Content = new StackLayout { Children = { btns, btn} };
			}
		}

		[Preserve (AllMembers = true)]
		public class MasterViewModel1 : INotifyPropertyChanged
		{
			public MasterDetailPage MasterPage;

			public event PropertyChangedEventHandler PropertyChanged = delegate { };

			protected void OnPropertyChanged ([CallerMemberName] string propertyName = null)
			{
				if (PropertyChanged != null) {
					PropertyChanged (this, new PropertyChangedEventArgs (propertyName));
				}
			}

			bool _isMenuOpen;

			public bool IsMenuOpen {
				get { return _isMenuOpen; }
				private set {
					if (_isMenuOpen != value) {
						System.Diagnostics.Debug.WriteLine ("setting new sidemenu visibility flag to: " + value);
						_isMenuOpen = value;
						OnPropertyChanged ();
					}
				}
			}

			public ICommand OpenSideMenuCommand { get; private set; }

			public MasterViewModel1 ()
			{
				IsMenuOpen = true;
				OpenSideMenuCommand = new Command (OpenSideMenu);
			}

			void OpenSideMenu ()
			{
				IsMenuOpen = true;
				MasterPage.IsPresented = IsMenuOpen;
			}
		}

		#if UITEST
		[Test]
		public void Bugzilla31602Test ()
		{
			var appAs = RunningApp as iOSApp;
			if (appAs != null && appAs.Device.IsTablet) {
				RunningApp.Tap (q => q.Marked ("Sidemenu Opener"));
				RunningApp.WaitForElement (q => q.Marked ("SideMenu"));
				RunningApp.SetOrientationLandscape ();
				RunningApp.WaitForElement (q => q.Marked ("SideMenu"));
				RunningApp.SetOrientationPortrait ();
				RunningApp.WaitForNoElement (q => q.Marked ("SideMenu"));
				RunningApp.Tap (q => q.Marked ("Sidemenu Opener"));
				RunningApp.WaitForElement (q => q.Marked ("SideMenu"));
			}
		}

		[TearDown]
		public void TearDown() 
		{
			RunningApp.SetOrientationPortrait ();
		}
#endif
	}
}
