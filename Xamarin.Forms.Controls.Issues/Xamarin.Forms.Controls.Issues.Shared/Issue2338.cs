using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Linq;
using System.Collections.Generic;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{

#if UITEST
	[NUnit.Framework.Category(UITestCategories.LifeCycle)]
	[NUnit.Framework.Category(UITestCategories.Navigation)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 2338, "Test Various Paths for Changing Main Page In Constructor")]
	public class Issue2338 : TestNavigationPage
	{
		Dictionary<string, Type> tests = new Dictionary<string, Type>
		{
			{ "Swap Main Page right after settings Details to Navigation Page", typeof(Issue2338_MasterDetailsPage_NavigationPage)},
			{ "Main Page right after settings Details to Content Page", typeof(Issue2338_MasterDetailsPage_ContentPage)},
			{ "Change Page in Constructor of Page Currently being set to Main Page", typeof(Issue2338_Ctor)},
			{ "Change Page in Constructor with some added additional changes", typeof(Issue2338_Ctor_MultipleChanges)},
			{ "Basic change Main Page when previous page is Master Details", typeof(Issue2338_MasterDetailsPage)},
			{ "Swap Main Page during OnAppearing", typeof(Issue2338_SwapMainPageDuringAppearing)},
			{ "Swap out Tabbed Page", typeof(Issue2338_TabbedPage)},
		};

		protected override void Init()
		{
			StackLayout layout = new StackLayout();
			layout.Children.Add(new Label() { Text = "Click each button to test a variation of Main Page Swapping. If you don't see a success page the test has failed" });

			foreach (var test in tests)
			{
				Button testButton = new Button();
				testButton.Text = test.Key;
				testButton.Command = new Command(() => Navigation.PushModalAsync((Page)Activator.CreateInstance(test.Value)));
				layout.Children.Add(testButton);
			}

			ContentPage page = new ContentPage();
			page.Content = layout;
			PushAsync(page);
		}

#if UITEST

		public async Task TestForSuccess(IApp RunningApp, Type type)
		{
			var test = tests.FirstOrDefault(x => x.Value == type);
			RunningApp.WaitForElement(test.Key);
			RunningApp.Tap(test.Key);
			//It takes a second for everything to settle
			await Task.Delay(2500);
			RunningApp.WaitForElement($"Success: {type.Name.Replace("_", " ")}");
			RunningApp.Tap("Start Over");
		}

		// Various tests are commented out on certain platforms because
		// https://github.com/xamarin/Xamarin.Forms/issues/3188
		[Test]
		public async Task SwapMainPageOut()
		{
			await TestForSuccess(RunningApp, typeof(Issue2338_SwapMainPageDuringAppearing));
			await TestForSuccess(RunningApp, typeof(Issue2338_MasterDetailsPage_ContentPage));
			await TestForSuccess(RunningApp, typeof(Issue2338_MasterDetailsPage_NavigationPage));

#if !__IOS__
			await TestForSuccess(RunningApp, typeof(Issue2338_Ctor));
#endif
			await TestForSuccess(RunningApp, typeof(Issue2338_Ctor_MultipleChanges));
			await TestForSuccess(RunningApp, typeof(Issue2338_TabbedPage));

#if !__IOS__
			await TestForSuccess(RunningApp, typeof(Issue2338_MasterDetailsPage));
#endif

		}
#endif

		[Preserve(AllMembers = true)]
		public class Issue2338_Ctor : TestNavigationPage
		{
			public Issue2338_Ctor()
			{
			}

			protected override void Init()
			{
			}

			protected override void OnAppearing()
			{
				Navigation.PushAsync(new InternalPage());
			}

			[Preserve(AllMembers = true)]
			public class InternalPage : ContentPage
			{
				public InternalPage()
				{
					Application.Current.MainPage = Issue2338TestHelper.CreateSuccessPage(nameof(Issue2338_Ctor));
				}
			}
		}

		[Preserve(AllMembers = true)]
		public class Issue2338_Ctor_MultipleChanges : TestNavigationPage
		{
			public Issue2338_Ctor_MultipleChanges()
			{
			}

			protected override void Init()
			{
				PushAsync(new ContentPage());
			}

			protected override void OnAppearing()
			{
				Navigation.PushAsync(new InternalPage(0));
			}

			[Preserve(AllMembers = true)]
			public class InternalPage : ContentPage
			{
				private readonly int _permutations;
				public InternalPage(int permutations)
				{
					_permutations = permutations;
					if (permutations > 5)
					{
						Device.BeginInvokeOnMainThread(() =>
						{
							Application.Current.MainPage = Issue2338TestHelper.CreateSuccessPage(nameof(Issue2338_Ctor_MultipleChanges));
						});
					}
					else
					{
						Device.BeginInvokeOnMainThread(() =>
						{

							Application.Current.MainPage =
								new NavigationPage(new InternalPage(permutations + 1) { Title = "Title 1" });
						});
					}
				}

				protected override void OnAppearing() => Debug.WriteLine($"OnAppearing: {_permutations}");
				protected override void OnDisappearing() => Debug.WriteLine($"OnDisappearing: {_permutations}");
			}

		}

		[Preserve(AllMembers = true)]
		public class Issue2338_SwapMainPageDuringAppearing : TestNavigationPage
		{
			protected override void Init()
			{
				PushAsync(new InternalPage(10));
				PushAsync(new InternalPage(20));
				PushAsync(new InternalPage(30));

				var otherPage = new InternalPage(40);
				PushAsync(otherPage);

				otherPage.Appearing += async (object sender, EventArgs e) =>
				{
					await Task.Delay(1000);
					Application.Current.MainPage = new InternalTabbedPage(this);

					// this is here just to mimic the issue the user reported
					// where additional behavior was occuring during the bindingcontext change
					Application.Current.MainPage.BindingContext = new object();
				};

			}
			protected override void OnAppearing() => Debug.WriteLine($"OnAppearing: Issue2338");
			protected override void OnDisappearing() => Debug.WriteLine($"OnDisappearing: Issue2338");

			[Preserve(AllMembers = true)]
			public class InternalTabbedPage : TabbedPage
			{
				private readonly NavigationPage _navigationPage;
				public InternalTabbedPage(NavigationPage navigationPage)
				{
					_navigationPage = navigationPage;
					Children.Add(Issue2338TestHelper.CreateSuccessPage(nameof(Issue2338_SwapMainPageDuringAppearing)));
				}

				protected override void OnBindingContextChanged()
				{
					_navigationPage.PushAsync(new InternalPage(50));
					base.OnBindingContextChanged();
					_navigationPage.PushAsync(new InternalPage(60));
				}


				protected override void OnAppearing() => Debug.WriteLine($"OnAppearing: InternalTabbedPage");
				protected override void OnDisappearing() => Debug.WriteLine($"OnDisappearing: InternalTabbedPage");
			}

			[Preserve(AllMembers = true)]
			class InternalPage : ContentPage
			{
				private int v;

				public InternalPage(int v)
				{
					this.v = v;
					this.Content = new Label { Text = v.ToString() };
				}

				protected override void OnAppearing() => Debug.WriteLine($"OnAppearing: {v}");
				protected override void OnDisappearing() => Debug.WriteLine($"OnDisappearing: {v}");
			}
		}

		[Preserve(AllMembers = true)]
		public class Issue2338_TabbedPage : TestTabbedPage
		{
			public Issue2338_TabbedPage() : base()
			{
			}


			protected override void Init()
			{
				Children.Add(new ContentPage());
				Children.Add(new ContentPage());
				Children.Add(new ContentPage());
				Children.Add(new InternalPage(this));
			}
			protected override void OnAppearing()
			{
				base.OnAppearing();
				SelectedItem = Children.Last();
			}


			[Preserve(AllMembers = true)]
			class InternalPage : ContentPage
			{
				private readonly TabbedPage _tabbedPage;

				public InternalPage(TabbedPage tabbedPage)
				{
					_tabbedPage = tabbedPage;
				}

				protected override void OnAppearing()
				{
					base.OnAppearing();
					_tabbedPage.Children.Add(new ContentPage());

					Application.Current.MainPage = Issue2338TestHelper.CreateSuccessPage(nameof(Issue2338_TabbedPage));
					_tabbedPage.Children.Add(new ContentPage());
				}
			}
		}

		[Preserve(AllMembers = true)]
		public class Issue2338_MasterDetailsPage : TestContentPage
		{

			protected override void Init()
			{
			}

			protected override void OnAppearing()
			{
				Application.Current.MainPage = new InternalMasterDetailsPage();
			}

			[Preserve(AllMembers = true)]
			class InternalMasterDetailsPage : MasterDetailPage
			{
				public InternalMasterDetailsPage()
				{
					Detail = new NavigationPage(new ContentPage() { Title = "Details" });
					Master = new ContentPage() { Title = "Master" };
				}

				protected override async void OnAppearing()
				{
					base.OnAppearing();
					await Task.Delay(500);
					Detail.Navigation.PushAsync(new ContentPage());
					Detail.Navigation.PushModalAsync(new NavigationPage(new ContentPage() { Title = "Details 2" }));

					var navPage = new NavigationPage(new ContentPage() { Title = "Details" });
					Detail = navPage;
					Application.Current.MainPage = Issue2338TestHelper.CreateSuccessPage(nameof(Issue2338_MasterDetailsPage));
					navPage.PushAsync(new ContentPage() { Title = "Details 2" });
				}
			}
		}

		[Preserve(AllMembers = true)]
		public class Issue2338_MasterDetailsPage_NavigationPage : TestContentPage
		{

			protected override void Init()
			{
			}

			protected override void OnAppearing()
			{
				Application.Current.MainPage = new InternalMasterDetailsPage();
			}

			[Preserve(AllMembers = true)]
			class InternalMasterDetailsPage : MasterDetailPage
			{
				public InternalMasterDetailsPage()
				{
					Detail = new NavigationPage(new ContentPage() { Title = "Details" });
					Master = new ContentPage() { Title = "Master" };
				}

				protected override async void OnAppearing()
				{
					base.OnAppearing();
					await Task.Delay(500);
					var contentPage = new ContentPage();
					Detail.Navigation.PushAsync(contentPage);

					contentPage.Appearing += (_, __) =>
					{
						var navPage = new NavigationPage(new ContentPage() { Title = "Details" });
						Detail = navPage;
						Master = new ContentPage() { Title = "Master" };

						Application.Current.MainPage = Issue2338TestHelper.CreateSuccessPage(nameof(Issue2338_MasterDetailsPage_NavigationPage));

						navPage.PushAsync(new ContentPage() { Title = "Details 2" });
					};
				}
			}
		}

		[Preserve(AllMembers = true)]
		public class Issue2338_MasterDetailsPage_ContentPage : TestContentPage
		{

			protected override void Init()
			{
			}

			protected override void OnAppearing()
			{
				Application.Current.MainPage = new InternalMasterDetailsPage();
			}

			[Preserve(AllMembers = true)]
			class InternalMasterDetailsPage : MasterDetailPage
			{
				public InternalMasterDetailsPage()
				{
					Detail = new ContentPage() { Title = "Details" };
					Master = new ContentPage() { Title = "Master" };
					Detail.Appearing += DetailAppearing;
				}

				private void DetailAppearing(object sender, EventArgs e)
				{
					Detail = new ContentPage() { Title = "Details" };
					Master = new ContentPage() { Title = "Master" };

					Application.Current.MainPage = Issue2338TestHelper.CreateSuccessPage(nameof(Issue2338_MasterDetailsPage_ContentPage));
				}
			}
		}
	}

	public static class Issue2338TestHelper
	{
		public static Page CreateSuccessPage(string name)
		{
			return new NavigationPage(new ContentPage()
			{
				Title = "Title 1",
				Content = new StackLayout()
				{
					Children = {
						new Label() { Text = $"Success: {name.Replace("_", " ")}" },
						new Button() { Text = "Start Over", Command = new Command(() =>
						{
							Application.Current.MainPage = new Issue2338();
						})}
					}
				}
			});
		}
	}
}
