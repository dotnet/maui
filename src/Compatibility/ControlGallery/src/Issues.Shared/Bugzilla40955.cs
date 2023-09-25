using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif


namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 40955, "Memory leak with FormsAppCompatActivity and NavigationPage", PlatformAffected.Android)]
#if UITEST
	[Category(UITestCategories.Performance)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	public class Bugzilla40955 : TestFlyoutPage
	{
		const string DestructorMessage = "NavigationPageEx Destructor called";
		const string Page1Title = "Page1";
		const string Page2Title = "Page2";
		const string Page3Title = "Page3";
		const string LabelPage1 = "Open the drawer menu and select Page2";
		const string LabelPage2 = "Open the drawer menu and select Page3";
		static string LabelPage3 = $"The console should have displayed the text '{DestructorMessage}' at least once. If not, this test has failed.";
		static string Success = string.Empty;

		static FlyoutPage Reference;

		protected override void Init()
		{
			var masterPage = new MasterPage();
			Flyout = masterPage;
			masterPage.ListView.ItemSelected += (sender, e) =>
			{
				var item = e.SelectedItem as MasterPageItem;
				if (item != null)
				{
					Detail = new NavigationPageEx((Page)Activator.CreateInstance(item.TargetType));
					masterPage.ListView.SelectedItem = null;
					IsPresented = false;
				}
			};

			Detail = new NavigationPageEx(new _409555_Page1());
			Reference = this;
		}

		[Preserve(AllMembers = true)]
		public class MasterPageItem
		{
			public string IconSource { get; set; }

			public Type TargetType { get; set; }

			public string Title { get; set; }
		}

		[Preserve(AllMembers = true)]
		public class MasterPage : ContentPage
		{
			public MasterPage()
			{
				Title = "Menu";
				ListView = new ListView { VerticalOptions = LayoutOptions.FillAndExpand, SeparatorVisibility = SeparatorVisibility.None };

				ListView.ItemTemplate = new DataTemplate(() =>
				{
					var ic = new ImageCell();
					ic.SetBinding(TextCell.TextProperty, "Title");
					return ic;
				});

				Content = new StackLayout
				{
					Children = { ListView }
				};

				var masterPageItems = new List<MasterPageItem>();
				masterPageItems.Add(new MasterPageItem
				{
					Title = Page1Title,
					TargetType = typeof(_409555_Page1)
				});
				masterPageItems.Add(new MasterPageItem
				{
					Title = Page2Title,
					TargetType = typeof(_409555_Page2)
				});
				masterPageItems.Add(new MasterPageItem
				{
					Title = Page3Title,
					TargetType = typeof(_409555_Page3)
				});

				ListView.ItemsSource = masterPageItems;
			}

			public ListView ListView { get; }
		}

		[Preserve(AllMembers = true)]
		public class NavigationPageEx : NavigationPage
		{
			public NavigationPageEx(Page root) : base(root)
			{
			}

			~NavigationPageEx()
			{
				Debug.WriteLine(DestructorMessage);
				Success = DestructorMessage;
			}
		}

		[Preserve(AllMembers = true)]
		public class _409555_Page1 : ContentPage
		{
			public _409555_Page1()
			{
				Title = Page1Title;

				var lbl = new Label
				{
					Text = LabelPage1
				};

				lbl.GestureRecognizers.Add(new TapGestureRecognizer
				{
					Command = new Command(OpenMaster)
				});

				Content = new StackLayout
				{
					Children = { lbl }
				};
			}
		}

		static void OpenMaster()
		{
			Reference.IsPresented = true;
		}

		[Preserve(AllMembers = true)]
		public class _409555_Page2 : ContentPage
		{
			public _409555_Page2()
			{
				Title = Page2Title;
				var lbl = new Label
				{
					Text = LabelPage2
				};

				lbl.GestureRecognizers.Add(new TapGestureRecognizer
				{
					Command = new Command(OpenMaster)
				});
				Content = new StackLayout { Children = { lbl } };
			}
		}

		[Preserve(AllMembers = true)]
		public class _409555_Page3 : ContentPage
		{
			public _409555_Page3()
			{
				Title = Page3Title;

				var lbl = new Label
				{
					Text = LabelPage3
				};

				lbl.GestureRecognizers.Add(new TapGestureRecognizer
				{
					Command = new Command(async () => await DisplayAlert("Alert", Success, "Ok"))
				});

				var successLabel = new Label();
				Content = new StackLayout
				{
					Children =
					{
						lbl
					}
				};
			}

			protected override void OnAppearing()
			{
				base.OnAppearing();
				GarbageCollectionHelper.Collect();
			}
		}


#if UITEST && __ANDROID__
		[Test]
		public void MemoryLeakInFormsAppCompatActivity()
		{
			RunningApp.WaitForElement(Page1Title);
			RunningApp.Tap(LabelPage1);
			RunningApp.WaitForElement(Page1Title);
			RunningApp.Tap(Page2Title);
			RunningApp.WaitForElement(LabelPage2);
			RunningApp.Tap(LabelPage2);
			RunningApp.WaitForElement(Page2Title);
			RunningApp.Tap(Page3Title);
			RunningApp.WaitForElement(LabelPage3);
			RunningApp.Tap(LabelPage3);
			RunningApp.WaitForElement(Success);

		}
#endif
	}
}