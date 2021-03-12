using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1760, "Content set after an await is not visible", PlatformAffected.Android)]
	public class Issue1760 : TestFlyoutPage
	{
		public const string Before = "Before";
		public const string After = "After";
		const int Wait = 3;

		protected override void Init()
		{
			Flyout = new _1760Master(true);
			Detail = new _1760TestPage(true);
			IsPresented = true;
		}

		[Preserve(AllMembers = true)]
		public class _1760Master : ContentPage
		{
			readonly bool _scrollEnabled;

			public _1760Master(bool scrollEnabled)
			{
				var instructions = new Label { Text = $"Select one of the menu items. The detail page text should change to {Before}. After {Wait} seconds the text should change to {After}." };

				var menuView = new ListView(ListViewCachingStrategy.RetainElement)
				{
					ItemsSource = new List<string> { "Test Page 1", "Test Page 2" }
				};

				menuView.ItemSelected += OnMenuClicked;

				Content = new StackLayout { Children = { instructions, menuView } };
				Title = "GH 1760 Test App";

				_scrollEnabled = scrollEnabled;
			}

			void OnMenuClicked(object sender, SelectedItemChangedEventArgs e)
			{
				var mainPage = (FlyoutPage)Parent;
				mainPage.Detail = new _1760TestPage(_scrollEnabled);
				mainPage.IsPresented = false;
			}
		}

		[Preserve(AllMembers = true)]
		public class _1760TestPage : ContentPage
		{
			readonly bool _scrollEnabled;

			public async Task DisplayPage()
			{
				IsBusy = true;
				HeaderPageContent = new Label { Text = Before, TextColor = Color.Black };

				await Task.Delay(Wait * 1000);

				HeaderPageContent = new Label { Text = After, TextColor = Color.Black };
				IsBusy = false;
			}

			ContentView _headerPageContent;
			public View HeaderPageContent
			{
				set => _headerPageContent.Content = value;
			}

			public _1760TestPage(bool scrollEnabled)
			{
				_scrollEnabled = scrollEnabled;
				CreateHeaderPage();
#pragma warning disable 4014
				// The lack of `await` here is from the original repro code and is intentional
				DisplayPage();
#pragma warning restore 4014
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

				if (_scrollEnabled)
				{
					Content = new ScrollView
					{
						Content = _headerPageContent
					};
				}
				else
				{
					var _headerLabel = new Label
					{
						Text = Title,
						TextColor = Color.FromHex("333333"),
						HeightRequest = 25,
					};

					var headerLayout = new RelativeLayout
					{
						BackgroundColor = Color.White,
						HorizontalOptions = LayoutOptions.Start,
						VerticalOptions = LayoutOptions.Start,
					};

					headerLayout.Children.Add(_headerLabel,
						Microsoft.Maui.Controls.Constraint.Constant(0),
						Microsoft.Maui.Controls.Constraint.Constant(0),
						Microsoft.Maui.Controls.Constraint.RelativeToParent(parent => parent.Width));

					Content = new StackLayout
					{
						HorizontalOptions = LayoutOptions.FillAndExpand,
						VerticalOptions = LayoutOptions.FillAndExpand,
						Children = {
							_headerLabel, _headerPageContent
						}
					};
				}
			}
		}

#if UITEST && __ANDROID__
		[Test]
		public void Issue1760Test()
		{
			RunningApp.WaitForElement(Before);
			RunningApp.WaitForElement(After);

			RunningApp.Tap("Test Page 1");
			RunningApp.WaitForElement(Before);
			RunningApp.WaitForElement(After);
		}
#endif
	}
}