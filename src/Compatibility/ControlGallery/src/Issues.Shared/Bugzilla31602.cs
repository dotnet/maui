using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.FlyoutPage)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif

	[Preserve(AllMembers = true)]
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
				IconImageSource = "menuIcon.png";
				var lbl = new Label { Text = "SideMenu" };
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
				var btn = new Button { Text = "Sidemenu Opener" };
				btn.SetBinding(Button.CommandProperty, new Binding("OpenSideMenuCommand"));
				Content = new StackLayout { Children = { btns, btn } };
			}
		}

		[Preserve(AllMembers = true)]
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

#if UITEST && __IOS__
		[Test]
		public void Bugzilla31602Test()
		{
			if (RunningApp.IsTablet())
			{
				RunningApp.Tap(q => q.Marked("Sidemenu Opener"));
				RunningApp.WaitForElement(q => q.Marked("SideMenu"));
				RunningApp.SetOrientationLandscape();
				RunningApp.WaitForElement(q => q.Marked("SideMenu"));
				RunningApp.SetOrientationPortrait();
				RunningApp.WaitForNoElement(q => q.Marked("SideMenu"));
				RunningApp.Tap(q => q.Marked("Sidemenu Opener"));
				RunningApp.WaitForElement(q => q.Marked("SideMenu"));
			}
		}

		[TearDown]
		public override void TearDown()
		{
			RunningApp.SetOrientationPortrait();

			base.TearDown();
		}
#endif
	}
}
