using System;
using Xamarin.Forms.CustomAttributes;
using System.ComponentModel;
using System.Diagnostics;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.UITest.iOS;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Ignore("This test is looking for an invalid behavior; the second tap *should* keep the drawer open.")] 
#endif
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 2961, "MasterDetail NavigationDrawer Does Not Hide On DoubleTap of Item", PlatformAffected.Android)]
	public class Issue2961 : TestMasterDetailPage
	{
		static MasterDetailPage s_mdp;

		SliderMenuItem _selectedMenuItem;
		SliderMenuPage _slidingPage;
		ContentPage _displayPage;

		protected override void Init ()
		{
			s_mdp = this;
			
			_slidingPage = new SliderMenuPage {
				Title = "Menu",
				BackgroundColor = Color.FromHex ("1e1e1e")
			};
			_slidingPage.MenuListView.ItemTapped += (sender, e) => OnMenuSelected (e.Item as SliderMenuItem);
			Padding = new Thickness (0);

			Master = _slidingPage;
			OnMenuSelected (_slidingPage.MenuListView.SelectedItem as SliderMenuItem);
		}

		void OnMenuSelected (SliderMenuItem menu)
		{
			Debug.WriteLine (IsPresented);

			IsPresented = false;	

			if (menu == null || menu == _selectedMenuItem) {
				return;
			}
			_displayPage = null;

			if (menu.TargetType.Equals (typeof(SignOutPage))) {
				HandleSignOut ();
				return;
			}
			_displayPage = (ContentPage)Activator.CreateInstance (menu.TargetType);
			Detail = new NavigationPage (_displayPage);
		
			if (_selectedMenuItem != null) {
				_selectedMenuItem.IsSelected = false;
			}

			_selectedMenuItem = menu;
			_selectedMenuItem.IsSelected = true;
		}

		void HandleSignOut ()
		{
			DisplayAlert (
				"",
				"Do you want to sign out?", 
				"OK", 
				"Cancel"
			);
		}

		[Preserve (AllMembers = true)]
		public class SliderMenuItem: INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			void NotifyPropertyChanged (string info)
			{
				if (PropertyChanged != null) {
					PropertyChanged (this, new PropertyChangedEventArgs (info));
				}
			}

			public SliderMenuItem (string title, Type targetType)
			{
				Title = title;
				TargetType = targetType;

			}

			public string Title { get; set; }

			bool _isSelected;

			public bool IsSelected { 
				get{ return _isSelected; }
				set { 
					if (_isSelected != value) {
						_isSelected = value;
						Background = _isSelected ? Color.FromHex ("101010") : Color.Transparent;
						NotifyPropertyChanged ("Background");
					}
				}
			}

			public Type TargetType { get; set; }

			public Color Background { get; private set; }
		}

		[Preserve (AllMembers = true)]
		public class SliderMenuPage: ContentPage
		{
			public ListView MenuListView { get; set; }

			public SliderMenuPage ()
			{
				var data = GetData ();
				MenuListView = new ListView {
					HorizontalOptions = LayoutOptions.StartAndExpand,
					ItemTemplate = new DataTemplate (typeof(MenuCell)),
					ItemsSource = data,
					BackgroundColor = Color.FromHex ("1e1e1e"),
				};

				MenuListView.SelectedItem = data [0];
				data [0].IsSelected = true;

				var logoImg = new Image {
					Source = ImageSource.FromFile ("bank.png"),
					HorizontalOptions = LayoutOptions.Start,
					VerticalOptions = LayoutOptions.EndAndExpand
				};
				var logoImgWrapper = new StackLayout {
					Padding = new Thickness (12, 24),
					HorizontalOptions = LayoutOptions.FillAndExpand,
					Children = { logoImg }
				};

				var paddingTop = Device.RuntimePlatform == Device.iOS ? 40 : 2;
				Content = new StackLayout {
					Spacing = 0, 
					BackgroundColor = Color.FromHex ("1e1e1e"),
					Padding = new Thickness (0, paddingTop, 0, 10),
					VerticalOptions = LayoutOptions.FillAndExpand,
					HorizontalOptions = LayoutOptions.FillAndExpand,
					Children = {
						MenuListView,
						logoImgWrapper
					}
				};
			}

			SliderMenuItem[] GetData ()
			{
				return new [] {
					new SliderMenuItem ("Home", typeof(HomePage)),
					new SliderMenuItem ("About", typeof(AboutPage)),
					new SliderMenuItem ("Sign Out", typeof(SignOutPage))
				};
			}
		}

		[Preserve (AllMembers = true)]
		public class HomePage : ContentPage
		{
			

			public HomePage ()
			{
				var showMasterButton = new Button {
					AutomationId = "ShowMasterBtnHome",
					Text = "Show Master"
				};
				showMasterButton.Clicked += (sender, e) => {
					s_mdp.IsPresented = true;
				};

				Content = new StackLayout {
					
					VerticalOptions = LayoutOptions.CenterAndExpand,
					HorizontalOptions = LayoutOptions.CenterAndExpand,
					Children = {
						showMasterButton,
						new Label {
							AutomationId = "lblHome",
							Text = "Sample Home page",
#pragma warning disable 618
							XAlign = TextAlignment.Center,
#pragma warning restore 618

#pragma warning disable 618
							YAlign = TextAlignment.Center
#pragma warning restore 618
						}
					}
				};
			}
		}

		[Preserve (AllMembers = true)]
		public class AboutPage: ContentPage
		{
			public AboutPage ()
			{
				var showMasterButton = new Button {
					AutomationId = "ShowMasterBtnAbout",
					Text = "Show Master"
				};
				showMasterButton.Clicked += (sender, e) => {
					s_mdp.IsPresented = true;
				};

				Content = new StackLayout {
					VerticalOptions = LayoutOptions.CenterAndExpand,
					HorizontalOptions = LayoutOptions.CenterAndExpand,
					Children = {
						showMasterButton,
						new Label {
							AutomationId = "lblAbout",
							Text = "Sample About page",
#pragma warning disable 618
							XAlign = TextAlignment.Center,
#pragma warning restore 618

#pragma warning disable 618
							YAlign = TextAlignment.Center
#pragma warning restore 618
						}
					}
				};
			}
		}

		[Preserve (AllMembers = true)]
		public class SignOutPage
		{
			public SignOutPage ()
			{
			}
		}

		[Preserve (AllMembers = true)]
		public class MenuCell: ViewCell
		{
			public MenuCell ()
			{
				Label textLabel = new Label {
					FontSize = 18,
				};

				textLabel.SetBinding (Label.TextProperty, "Title");
				var root = new StackLayout {
					Padding = new Thickness (12, 8),
					Children = { textLabel }
				};
				root.SetBinding (BackgroundColorProperty, "Background");
				View = root;
			}
		}

			
		#if UITEST
		[Test]
		public void Issue2961Test ()
		{
			RunningApp.Screenshot ("I am at Issue 2961");
			OpenMDP ("ShowMasterBtnHome");
			RunningApp.Tap (c => c.Marked ("Home"));
			RunningApp.WaitForElement (c => c.Marked ("lblHome"));
			OpenMDP ("ShowMasterBtnHome");
			RunningApp.Tap (c => c.Marked ("About"));
			RunningApp.WaitForElement (c => c.Marked ("lblAbout"));
			OpenMDP ("ShowMasterBtnAbout");
#if __IOS__
			return;
#else
			RunningApp.DoubleTap (c => c.Marked ("Home"));
			RunningApp.WaitForElement (c => c.Marked ("lblHome"));
			RunningApp.Tap (c => c.Marked ("About"));
			RunningApp.WaitForNoElement (c => c.Marked ("Home"));
#endif
		}

		public void OpenMDP(string masterBtnId) {
#if __IOS__
			RunningApp.Tap (q => q.Marked("Menu"));
#else
			RunningApp.Tap (masterBtnId);
#endif
		}
#endif
	}
}
