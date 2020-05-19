using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	public abstract class Bugzilla43313_Template : ContentPage
	{
		public static int ItemCount = 20;
		readonly ListView _listView;
		protected abstract DataTemplate CellTemplate();

		_43313ViewModel ViewModel => BindingContext as _43313ViewModel;

		protected Bugzilla43313_Template()
		{
			BindingContext = new _43313ViewModel();

			var btnAdd = new Button
			{
				Text = "Add item",
				WidthRequest = 100
			};
			btnAdd.Clicked += BtnAddOnClicked;

			var btnBottom = new Button
			{
				Text = "Scroll to end",
				WidthRequest = 100
			};
			btnBottom.Clicked += BtnBottomOnClicked;

			var btnPanel = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Center,
				Children = { btnAdd, btnBottom }
			};

			_listView = new ListView
			{
				HasUnevenRows = true,
				BackgroundColor = Color.Transparent,
				VerticalOptions = LayoutOptions.FillAndExpand,
				ItemTemplate = CellTemplate()
			};

			_listView.SetBinding(ListView.ItemsSourceProperty, new Binding(nameof(_43313ViewModel.ListViewContent)));
			_listView.ItemTapped += (sender, e) => ((ListView)sender).SelectedItem = null;

			var instructions = new Label() { FontSize = 12, Text = "Tap the 'Add Item' button; a new item should be added to the bottom of the list and the list should scroll smoothly to display it. If the list scrolls back to the top before scrolling down to the new item, the test has failed." };

			Content = new StackLayout
			{
				Padding = new Thickness(0, 40, 0, 0),
				Children =
				{
					instructions,
					btnPanel,
					_listView
				}
			};
		}

		void BtnAddOnClicked(object sender, EventArgs eventArgs)
		{
			string str = $"Item {ItemCount++}";
			var item = new _43313Model { Name = str };
			ViewModel.ListViewContent.Add(item);

			_listView.ScrollTo(item, ScrollToPosition.End, true);
		}

		void BtnBottomOnClicked(object sender, EventArgs e)
		{
			_43313Model item = ViewModel.ListViewContent.Last();
			_listView.ScrollTo(item, ScrollToPosition.End, true);
		}

		[Preserve(AllMembers = true)]
		public class _43313Model
		{
			public string Name { get; set; }
		}

		[Preserve(AllMembers = true)]
		public class _43313ViewModel : INotifyPropertyChanged
		{
			ObservableCollection<_43313Model> _listViewContent;

			public _43313ViewModel()
			{
				ListViewContent = new ObservableCollection<_43313Model>();

				for (int n = 0; n < ItemCount; n++)
				{
					_listViewContent.Add(new _43313Model { Name = $"Item {n}" });
				}
			}

			public ObservableCollection<_43313Model> ListViewContent
			{
				get { return _listViewContent; }
				set
				{
					_listViewContent = value;
					OnPropertyChanged();
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;

			protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
			{
				PropertyChangedEventHandler handler = PropertyChanged;
				handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	[Preserve(AllMembers = true)]
	public class Bugzilla43313_KnownHeight : Bugzilla43313_Template
	{
		protected override DataTemplate CellTemplate()
		{
			return new DataTemplate(() =>
			{
				var label = new Label { FontSize = 16, VerticalOptions = LayoutOptions.Center };
				label.SetBinding(Label.TextProperty, nameof(_43313Model.Name));
				int height = 60 + new Random().Next(10, 100);

				return new ViewCell
				{
					Height = height,
					View = new StackLayout
					{
						Padding = new Thickness(0, 5, 0, 5),
						BackgroundColor = Color.Transparent,
						Children =
							{
								label
							}
					}
				};
			});
		}
	}

	[Preserve(AllMembers = true)]
	public class Bugzilla43313_EstimatedHeight : Bugzilla43313_Template
	{
		protected override DataTemplate CellTemplate()
		{
			return new DataTemplate(() =>
			{
				var label = new Label { FontSize = 16, VerticalOptions = LayoutOptions.Center };
				label.SetBinding(Label.TextProperty, nameof(_43313Model.Name));

				label.FontSize = 12 + new Random().Next(1, 6);

				return new ViewCell
				{
					View = new StackLayout
					{
						Padding = new Thickness(0, 5, 0, 5),
						BackgroundColor = Color.Transparent,
						Children =
							{
								label
							}
					}
				};
			});
		}
	}

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 43313, "Adding an item to ListView ItemSource has unexpected animation with different height rows and HasUnevenRows is true", PlatformAffected.iOS)]
	public class Bugzilla43313 : TestNavigationPage
	{
		protected override void Init()
		{
			var root = new ContentPage();
			
			var layout = new StackLayout();

			var knownHeightButton = new Button() { Text = "Known Height (original bug report test case)" };
			knownHeightButton.Clicked += (sender, args) => PushAsync(new Bugzilla43313_KnownHeight());

			var estimatedHeightButton = new Button() { Text = "Estimated Height" };
			estimatedHeightButton.Clicked += (sender, args) => PushAsync(new Bugzilla43313_EstimatedHeight());

			layout.Children.Add(knownHeightButton);
			layout.Children.Add(estimatedHeightButton);

			root.Content = layout;

			PushAsync(root);
		}
	}
}
