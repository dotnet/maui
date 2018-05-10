using System;
using System.Collections.Generic;
using System.Timers;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2595, "ScrollView.Content is not re-layouted on Android", PlatformAffected.Android)]
	public class Issue2595 : TestMasterDetailPage
	{
		protected override void Init()
		{
			Master = new _2595Master();
			Detail = new _2595ScrollPage();
			IsPresented = true;
		}

		[Preserve(AllMembers = true)]
		public class _2595Master : ContentPage
		{
			public _2595Master()
			{
				var instructions = new Label { Text = $"Select one of the menu items. The detail page text should "
				                                      + $"display a label which disappears after 1 second and is"
				                                      + $" replaced by an updating list of labels which grows vertically." };

				var menuView = new ListView(ListViewCachingStrategy.RetainElement)
				{
					ItemsSource = new List<string> { "Test Page 1", "Test Page 2" }
				};

				menuView.ItemSelected += OnMenuClicked;

				Content = new StackLayout{Children = { instructions, menuView }};
				Title = "GH 2595 Test App";
			}

			void OnMenuClicked(object sender, SelectedItemChangedEventArgs e)
			{
				var mainPage = (MasterDetailPage)Parent;
				mainPage.Detail = new _2595ScrollPage ();
				mainPage.IsPresented = false;
			}
		}

		[Preserve(AllMembers = true)]
		public class _2595ScrollPage : ContentPage
		{
			readonly Timer _timer = new Timer(1000);
			protected Label Label;

			public _2595ScrollPage() {
				Content = new ScrollView {

					BackgroundColor = Color.Red,

					Content = new StackLayout {
						BackgroundColor = Color.BlueViolet,
						Children = {
							(Label = new Label {
								Text = "this text should disappear after 1 sec",
								BackgroundColor = Color.LightBlue,
								HorizontalOptions = LayoutOptions.StartAndExpand,
							})
						}
					}
				};
			}

			protected StackLayout ScrollContent {
				get => (Content as ScrollView).Content as StackLayout;
				set => (Content as ScrollView).Content = value;
			}

			protected override void OnAppearing() {
				base.OnAppearing();
				_timer.Elapsed += (s, e) => Device.BeginInvokeOnMainThread(OnTimerElapsed);

				_timer.Start();
			}

			void OnTimerElapsed() {
				Label.Text = $"{ DateTime.Now.ToString() }: expecting {ScrollContent?.Children.Count} dates to show up.";
				ScrollContent.Children.Add(new Label { Text = DateTime.Now.ToString() });
			}
		}
	}
}