using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 57910, "ObjectDisposedException in Microsoft.Maui.Controls.Platform.Android.Renderers.ProgressBarRenderer", PlatformAffected.Android)]
	public class Bugzilla57910 : NavigationPage
	{
		const string ButtonId = "btnPush";
		const string Button2Id = "btnPop";
		const string Instructions = "Tap Push. Then quickly tap Pop on the subsequent screen. Do this several times. If there is no crash, then this test has passed.";
		const string Instructions2 = "Tap Pop. Then quickly tap Push on the subsequent screen. Do this several times. If there is no crash, then this test has passed.";

		public Bugzilla57910() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				Navigation.PushAsync(new HomePage());
			}


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
					GarbageCollectionHelper.Collect();
				}
			}


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


			class ListHeaderView : ContentView
			{
				public ListHeaderView()
				{
					Label newLabel = new Label();
					newLabel.SetBinding(Label.TextProperty, nameof(ListPageViewModel.Header));
					Content = newLabel;
				}
			}


			class ListFooterView : ContentView
			{
				public ListFooterView()
				{
					Label newLabel = new Label();
					newLabel.SetBinding(Label.TextProperty, nameof(ListPageViewModel.Footer));

					var stack = new StackLayout { Children = { newLabel } };
					Content = stack;
				}
			}


			class ListPageViewModel : INotifyPropertyChanged
			{
				ObservableCollection<ListItemViewModel> _items;
				string _footer;
				string _header;

				int _counter;
				public ListPageViewModel()
				{
					_header = "Header!";
					_footer = "Footer!";
					_counter = 0;
					_items = new ObservableCollection<ListItemViewModel>(Enumerable.Range(0, 100).Select(c => new ListItemViewModel()));

					// Need an asynchronous action that happens sometime between creation of the Element and Pop of the containing page
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
					Device.StartTimer(TimeSpan.FromMilliseconds((100)), () =>
					{
						Header = $"Header! {_counter++}";
						Footer = $"Footer! {_counter++}";

						return true;
					});
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
				}

				public event PropertyChangedEventHandler PropertyChanged;

				public ObservableCollection<ListItemViewModel> Items
				{
					get { return _items; }
					set
					{
						_items = value;
						OnPropertyChanged(nameof(Items));
					}
				}

				public string Header
				{
					get { return _header; }
					set
					{
						_header = value;
						OnPropertyChanged(nameof(Header));
					}
				}

				public string Footer
				{
					get { return _footer; }
					set
					{
						_footer = value;
						OnPropertyChanged(nameof(Footer));
					}
				}

				protected virtual void OnPropertyChanged(string propertyName)
				{
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
				}
			}


			class ListItemViewModel : INotifyPropertyChanged
			{
				double _downloadProgressPercentage;

				InstallableState _state;

				public ListItemViewModel()
				{
					DownloadProgressPercentage = 0d;
					State = InstallableState.Downloading;

					// Need an asynchronous action that happens sometime between creation of the Element and Pop of the containing page
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
					Device.StartTimer(TimeSpan.FromMilliseconds((1000)), () =>
					{
						DownloadProgressPercentage += 0.05d;
						if (DownloadProgressPercentage > 0.99d)
						{
							State = InstallableState.Local;
						}

						return DownloadProgressPercentage != 1.0d;
					});
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
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


			class ListPage : ContentPage
			{
				public ListPage()
				{
					ListView listView = new ListView(ListViewCachingStrategy.RecycleElement)
					{
						RowHeight = 70,
						ItemTemplate = new DataTemplate(typeof(ListItemView)),
						HeaderTemplate = new DataTemplate(typeof(ListHeaderView)),
						FooterTemplate = new DataTemplate(typeof(ListFooterView)),
					};

					listView.SetBinding(ListView.ItemsSourceProperty, nameof(ListPageViewModel.Items));
					listView.SetBinding(ListView.HeaderProperty, ".");
					listView.SetBinding(ListView.FooterProperty, ".");

					Button newButton = new Button { Text = "Pop", AutomationId = Button2Id };
					newButton.Clicked += NewButton_Clicked;
					Content = new StackLayout { Children = { new Label { Text = Instructions2 }, newButton, listView } };

					BindingContext = new ListPageViewModel();
				}

				void NewButton_Clicked(object sender, EventArgs e)
				{
					Navigation.PopAsync();
				}
			}
		}
	}
}