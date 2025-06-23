namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 3333, "[UWP] with ListView on page, Navigation.PopAsync() throws exception",
		PlatformAffected.UWP)]
	public class Issue3333 : NavigationPage
	{
		public Issue3333() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			const string kSuccess = "If you are reading this the test has passed";

			public MainPage()
			{
				var testPage = new TestPage();
				this.Navigation.PushAsync(testPage);
			}


			public class TestPage : ContentPage
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

#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
					Device.BeginInvokeOnMainThread(async () =>
					{
						BindingContext = null;
						await Navigation.PopAsync();
					});
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
				}
			}
		}
	}
}