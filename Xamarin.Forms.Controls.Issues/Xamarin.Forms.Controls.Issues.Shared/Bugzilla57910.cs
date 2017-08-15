using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System;
using System.Linq;
using System.ComponentModel;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 57910, "ObjectDisposedException in Xamarin.Forms.Platform.Android.Renderers.ProgressBarRenderer", PlatformAffected.Android)]
	public class Bugzilla57910 : Bugzilla57910QuickCollectNavigationPage
	{
		const string ButtonId = "btnPush";
		const string Button2Id = "btnPop";
		const string Instructions = "Tap Push. Then quickly tap Pop on the subsequent screen. Do this several times. If there is no crash, then this test has passed.";
		const string Instructions2 = "Tap Pop. Then quickly tap Push on the subsequent screen. Do this several times. If there is no crash, then this test has passed.";

		protected override void Init()
		{
			Navigation.PushAsync(new HomePage());
		}

		[Preserve(AllMembers = true)]
		class HomePage : ContentPage
		{
			public HomePage()
			{
				Button button = new Button { Text = "Push", AutomationId = ButtonId };

				button.Clicked += Button_Clicked;

				Content = new StackLayout { Children = { new Label { Text = Instructions }, button } };
			}

			async void Button_Clicked(object sender, EventArgs e)
			{
				await Navigation.PushAsync(new ListPage());
				GC.Collect();
			}
		}

		[Preserve(AllMembers = true)]
		class ListItemView : ViewCell
		{
			public ListItemView()
			{
				ProgressBar progressBar = new ProgressBar { IsVisible = false };
				progressBar.SetBinding(ProgressBar.ProgressProperty, nameof(ListItemViewModel.DownloadProgressPercentage));

				// Need a trigger to set a property on a VisualElement. Not actually specific to ProgressBar.
				DataTrigger newDataTrigger = new DataTrigger(typeof(ProgressBar)) { Binding = new Binding(nameof(ListItemViewModel.State)), Value = ListItemViewModel.InstallableState.Downloading };
				newDataTrigger.Setters.Add(new Setter { Property = ProgressBar.IsVisibleProperty, Value = true });
				progressBar.Triggers.Add(newDataTrigger);

				View = new ContentView { Content = new StackLayout { Children = { progressBar } } };
			}
		}

		[Preserve(AllMembers = true)]
		class ListItemViewModel : INotifyPropertyChanged
		{
			double _downloadProgressPercentage;

			InstallableState _state;

			public ListItemViewModel()
			{
				DownloadProgressPercentage = 0d;
				State = InstallableState.Downloading;

				// Need an asynchronous action that happens sometime between creation of the Element and Pop of the containing page
				Device.StartTimer(TimeSpan.FromMilliseconds((1000)), () =>
				{
					DownloadProgressPercentage += 0.05d;
					if (DownloadProgressPercentage > 0.99d)
					{
						State = InstallableState.Local;
					}

					return DownloadProgressPercentage != 1.0d;
				});
			}

			public event PropertyChangedEventHandler PropertyChanged;

			public enum InstallableState
			{
				Local,
				Downloading
			}

			public double DownloadProgressPercentage
			{
				get { return _downloadProgressPercentage; }
				set
				{
					_downloadProgressPercentage = value;
					OnPropertyChanged(nameof(DownloadProgressPercentage));
				}
			}

			public InstallableState State
			{
				get { return _state; }
				set
				{
					_state = value;
					OnPropertyChanged(nameof(State));
				}
			}

			protected virtual void OnPropertyChanged(string propertyName)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		[Preserve(AllMembers = true)]
		class ListPage : ContentPage
		{
			public ListPage()
			{
				ListView listView = new ListView(ListViewCachingStrategy.RecycleElement)
				{
					RowHeight = 70,
					ItemsSource = Enumerable.Range(0, 100).Select(c => new ListItemViewModel()),
					ItemTemplate = new DataTemplate(typeof(ListItemView))
				};
				Button newButton = new Button { Text = "Pop", AutomationId = Button2Id };
				newButton.Clicked += NewButton_Clicked;
				Content = new StackLayout { Children = { new Label { Text = Instructions2 }, newButton, listView } };
			}

			void NewButton_Clicked(object sender, EventArgs e)
			{
				Navigation.PopAsync();
			}
		}

#if UITEST
		[Test]
		public void Bugzilla57910Test()
		{
			for (int i = 0; i < 10; i++)
			{
				RunningApp.WaitForElement(q => q.Marked(ButtonId));
				RunningApp.Tap(q => q.Marked(ButtonId));
				RunningApp.WaitForElement(q => q.Marked(Button2Id));
				RunningApp.Tap(q => q.Marked(Button2Id));
			}
		}
#endif
	}

	[Preserve(AllMembers = true)]
	public class Bugzilla57910QuickCollectNavigationPage : TestNavigationPage
	{
		protected override void Init()
		{
		}
	}
}