using System;
using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2740, "System.NotSupportedException: Unable to activate instance of type Xamarin.Forms.Platform.Android.PageContainer from native handle", PlatformAffected.Android)]
	public class Issue2740 : TestMasterDetailPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init()
		{
			var page = new AddressListView();

			// Initialize ui here instead of ctor
			Master = new ContentPage
			{
				Content = new Label
				{
					Text = "Click a item on the left then the toolbar item switch"
				},
				Title = "2740"
			};
			Detail = new NavigationPage(page);
		}

		public partial class AddressListView : ContentPage
		{

			public AddressListView()
			{
				var listview = new ListView();
				listview.ItemsSource = new List<string> { "1", "2" };
				listview.ItemTapped += OnItemTapped;
				Content = listview;
				Title = "Unit List";
			}

			public async void OnItemTapped(object sender, ItemTappedEventArgs e)
			{
				var p = new UnitViolationView();
				await Navigation.PushAsync(p);
			}
		}

		public partial class UnitViolationView : ContentPage
		{
			public UnitViolationView()
			{
				ToolbarItems.Add(new ToolbarItem("Switch", null, MapAddressSwitch) { AutomationId = "Switch" });
			}

			async void MapAddressSwitch()
			{
				await Navigation.PopAsync(false);
				(Application.Current.MainPage as MasterDetailPage).Detail = new NavigationPage(new AddressListView());
			}
		}

#if UITEST
		[Test]
		public void Issue2740Test ()
		{
			RunningApp.WaitForElement (q => q.Marked ("1"));
			RunningApp.Tap (q => q.Marked ("1"));
			RunningApp.WaitForElement (q => q.Marked ("Switch"));
			RunningApp.Tap (q => q.Marked ("Switch"));
			RunningApp.WaitForElement (q => q.Marked ("1"));
		}
#endif
	}
}