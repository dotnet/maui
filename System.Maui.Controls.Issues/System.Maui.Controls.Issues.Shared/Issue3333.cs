using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3333, "[UWP] with ListView on page, Navigation.PopAsync() throws exception",
		PlatformAffected.UWP)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.ListView)]
	[NUnit.Framework.Category(UITestCategories.Navigation)]
#endif
	public class Issue3333 : TestNavigationPage
	{
		const string kSuccess = "If you're reading this the test has passed";
		protected override void Init()
		{
			var testPage = new TestPage();
			this.Navigation.PushAsync(testPage);
		}

		[Preserve(AllMembers = true)]
		public partial class TestPage : ContentPage
		{
			Label content = new Label();
			public TestPage()
			{
				Title = "Page 1";
				Navigation.PushAsync(new TestPage2());
				Content = content;
			}

			protected override void OnAppearing()
			{
				if (content.Text == string.Empty)
				{
					content.Text = "Hold Please";
				}
				else
				{
					content.Text = kSuccess;
				}
			}
		}

		[Preserve(AllMembers = true)]
		public class TestPage2 : ContentPage
		{
			public List<string> Items
			{
				get { return new List<string> { "Test1", "Test2", "Test3" }; }
			}

			public TestPage2()
			{
				BindingContext = this;
				ListView listView = new ListView();
				listView.SetBinding(ListView.ItemsSourceProperty, "Items");

				Content =
					new StackLayout()
					{
						Children =
						{
							new ScrollView()
							{
								Content = listView
							}
						}
					};

				Device.BeginInvokeOnMainThread(async () =>
				{
					BindingContext = null;
					await Navigation.PopAsync();
				});
			}
		}

#if UITEST
		[Test]
		public void SettingBindingContextToNullBeforingPoppingPageCrashes()
		{
			RunningApp.WaitForElement(kSuccess);
		}
#endif
	}
}
