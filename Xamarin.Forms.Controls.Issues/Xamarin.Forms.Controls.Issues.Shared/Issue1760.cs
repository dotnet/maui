using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1760, "Content set after an await is not visible", PlatformAffected.Android)]
	public class Issue1760 : TestMasterDetailPage
	{
		const string Before = "Before";
		const string After = "After";
		const int Wait = 3;

		protected override void Init()
		{
			Master = new _1760Master();
			Detail = new _1760TestPage();
			IsPresented = true;
		}

		[Preserve(AllMembers = true)]
		public class _1760Master : ContentPage
		{
			public _1760Master()
			{
				var instructions = new Label { Text = $"Select one of the menu items. The detail page text should change to {Before}. After {Wait} seconds the text should change to {After}." };

				var menuView = new ListView(ListViewCachingStrategy.RetainElement)
				{
					ItemsSource = new List<string> { "Test Page 1", "Test Page 2" }
				};

				menuView.ItemSelected += OnMenuClicked;

				Content = new StackLayout{Children = { instructions, menuView }};
				Title = "GH 1760 Test App";
			}

			void OnMenuClicked(object sender, SelectedItemChangedEventArgs e)
			{
				var mainPage = (MasterDetailPage)Parent;
				mainPage.Detail = new _1760TestPage();
				mainPage.IsPresented = false;
			}
		}

		[Preserve(AllMembers = true)]
		public class _1760TestPage : ContentPage
		{
			public async Task DisplayPage()
			{
				IsBusy = true;
				HeaderPageContent = new Label {Text = Before, TextColor = Color.Black};

				await Task.Delay(Wait * 1000);

				HeaderPageContent = new Label { Text = After, TextColor = Color.Black};
				IsBusy = false;
			}

			ContentView _headerPageContent;
			public View HeaderPageContent
			{
				set => _headerPageContent.Content = value;
			}

			public _1760TestPage()
			{
				CreateHeaderPage();
				DisplayPage();
			}

			void CreateHeaderPage()
			{

				_headerPageContent = new ContentView
				{
					Content = new Label { Text = "_1760 Test Page Content" },
					BackgroundColor = Color.White,
					Margin = 40
				};

				Title = "_1760 Test Page";

				Content = new ScrollView
				{
					Content = _headerPageContent
				};
			}
		}
	}
}