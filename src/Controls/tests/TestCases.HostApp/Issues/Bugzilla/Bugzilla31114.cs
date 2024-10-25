using System.Collections.ObjectModel;
using AbsoluteLayoutFlags = Microsoft.Maui.Layouts.AbsoluteLayoutFlags;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 31114, "iOS ContextAction leaves blank line after swiping in ListView")]
public class Bugzilla31114 : TestContentPage
{
	ObservableCollection<ListItem> _items = new ObservableCollection<ListItem>();
	ListView _listView;
	Button _btbLoad;
	public Command RefreshListCommand;
	bool _isBusy = false;

	protected override void Init()
	{

		RefreshListCommand = new Command(LoadItemsFromCommand, CanRefreshList);
		_listView = new ListView();
		_listView.ItemsSource = _items;
		_listView.ItemTemplate = new DataTemplate(typeof(TaskItemTemplate));
		_listView.RowHeight = 64;
		_listView.RefreshCommand = RefreshListCommand;
		_listView.IsPullToRefreshEnabled = true;

		_btbLoad = new Button { Text = "Load", AutomationId = "btnLoad", Command = RefreshListCommand };
		TaskItemTemplate.RefreshFromQuickComplete += TaskListPageRefreshFromQuickComplete;

		LoadItems();

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		Content = new StackLayout
		{
			VerticalOptions = LayoutOptions.FillAndExpand,
			HorizontalOptions = LayoutOptions.FillAndExpand,
			Children = {
				_listView,_btbLoad
			}
		};
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
	}

	protected override void OnDisappearing()
	{
		TaskItemTemplate.RefreshFromQuickComplete -= TaskListPageRefreshFromQuickComplete;
		base.OnDisappearing();
	}

	bool CanRefreshList()
	{
		return !isBusy;
	}

	void LoadItemsFromCommand()
	{
		LoadItems();
	}

	void TaskListPageRefreshFromQuickComplete(object sender, ListItemEventArgs e)
	{
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
		Device.BeginInvokeOnMainThread(() =>
			{
				LoadItemsFromCommand();
			});
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
	}

	void LoadItems()
	{
		isBusy = true;

		Random random = new Random(DateTime.Now.Millisecond);

		int count = random.Next(20, 30);

		_items.Clear();

		for (int i = 0; i < count - 1; i++)
		{
			var newItem = new ListItem()
			{
				Id = Guid.NewGuid().ToString(),
				EntityTypeId = 1350,
				BackgroundColor = "00aa00",
				TextColor = "FFFFFF",
				PrimaryText = "PIPE #" + (i + 1000).ToString(),
				CircleColor = "0000aa",
				Icon = "",
				OtherText = random.Next(100, 200).ToString() + " ft",
				SecondaryText = "LENGTH " + i.ToString(),
				SupportsQuickComplete = true,
			};

			//Debug.WriteLine(newItem.PrimaryText);

			_items.Add(newItem);
		}


		isBusy = false;
	}

	bool isBusy
	{
		get { return _isBusy; }
		set
		{
			_isBusy = value;
			_listView.IsRefreshing = _isBusy;
		}
	}


	public class ListItem
	{
		public string Id { get; set; }

		public string PrimaryText { get; set; }

		public string SecondaryText { get; set; }

		public string TertiaryText { get; set; }

		public string OtherText { get; set; }

		public string ListControl { get; set; }

		public string Icon { get; set; }

		public string Params { get; set; }

		public string BackgroundColor { get; set; }

		public string TextColor { get; set; }

		public string CircleColor { get; set; }

		public long EntityTypeId { get; set; }

		public bool SupportsQuickComplete { get; set; }

		public ListItem()
		{
		}

		public string BackgroundColorColor
		{
			get
			{
				return BackgroundColor;
			}
			set
			{
				if (BackgroundColor != value)
				{
					BackgroundColor = value;
				}
			}
		}

		public string PrimaryLabelText
		{
			get
			{
				return PrimaryText;
			}
		}

		public string SecondaryLabelText
		{
			get
			{
				return SecondaryText;
			}
		}

		public string OtherLabelText
		{
			get
			{
				return OtherText;
			}
		}
	}

	public class ListItemEventArgs : EventArgs
	{
		public ListItem ListItem { get; set; }

		public ListItemEventArgs(ListItem item)
		{
			ListItem = item;
		}
	}


	public class TaskItemTemplate : ViewCell
	{
		Image _photo;
		Label _mainLabel;
		Label _secondaryLabel;
		Label _distanceLabel;
		Label _statusCircle;
		StackLayout _stackLayout;
		StackLayout _primaryContent;
		StackLayout _secondaryContent;
		AbsoluteLayout _masterLayout;

		MenuItem _quickCompleteMenu;

		public static event EventHandler<ListItemEventArgs> RefreshFromQuickComplete;

		public TaskItemTemplate()
		{
			Init(true);
		}

		public TaskItemTemplate(bool fast = true)
		{
			Init(fast);
		}

		void Init(bool fast)
		{
			_photo = new Image
			{
				HeightRequest = 52,
				WidthRequest = 52,
			};


			_mainLabel = new Label() { HeightRequest = 40, FontSize = 24, TranslationY = 5, LineBreakMode = LineBreakMode.TailTruncation };
			_mainLabel.SetBinding(Label.TextProperty, "PrimaryLabelText");

			_secondaryLabel = new Label() { HeightRequest = 40, FontSize = 16, TranslationY = -5, LineBreakMode = LineBreakMode.TailTruncation };
			_secondaryLabel.SetBinding(Label.TextProperty, "SecondaryLabelText");

#pragma warning disable CS0618 // Type or member is obsolete
			_distanceLabel = new Label() { HorizontalTextAlignment = TextAlignment.End, HorizontalOptions = LayoutOptions.EndAndExpand, FontSize = 11, LineBreakMode = LineBreakMode.NoWrap };
#pragma warning restore CS0618 // Type or member is obsolete
			_distanceLabel.SetBinding(Label.TextProperty, "OtherLabelText");

#pragma warning disable CS0618 // Type or member is obsolete
			_statusCircle = new Label()
			{
				HorizontalOptions = LayoutOptions.EndAndExpand,
				FontSize = 30,
				TranslationY = 0,
			};
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
			_primaryContent = new StackLayout()
			{
				HorizontalOptions = LayoutOptions.StartAndExpand,
				Orientation = StackOrientation.Vertical,
				Children =
				{
					_mainLabel,
					_secondaryLabel,
				},
				Padding = new Thickness(12, 0, 0, 0),
			};
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
			_secondaryContent = new StackLayout()
			{
				MinimumWidthRequest = 50,
				HorizontalOptions = LayoutOptions.EndAndExpand,
				Children =
				{
					_distanceLabel,
					_statusCircle,
				},
				Padding = new Thickness(0, 5, 5, 0),
			};
#pragma warning restore CS0618 // Type or member is obsolete

			_stackLayout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				Children =
				{
					_photo,
					_primaryContent,
					_secondaryContent,
				},
				Padding = new Thickness(5, 0, 0, 0)
			};

			if (!fast)
			{
				View = _stackLayout;
			}
			else
			{
				_quickCompleteMenu = new MenuItem { Text = "Complete", IsDestructive = false };
				_quickCompleteMenu.SetBinding(MenuItem.CommandParameterProperty, new Binding("."));

				// Delete context menu action
				_quickCompleteMenu.Clicked += (sender, e) =>
				{
					FastCompleteForCmd(sender);
				};

				// Add this action to the cell
				ContextActions.Add(_quickCompleteMenu);

				_masterLayout = new AbsoluteLayout();

				_masterLayout.Children.Add(_stackLayout);

				AbsoluteLayout.SetLayoutFlags(_stackLayout, AbsoluteLayoutFlags.All);
				AbsoluteLayout.SetLayoutBounds(_stackLayout, new Rect(0.0, 0.0, 1.0f, 1.0f));

				View = _masterLayout;
			}
		}


		protected override void OnPropertyChanged(string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);
			if (propertyName == "BackgroundColor")
			{
				var item = BindingContext as ListItem;
				if (item != null && !string.IsNullOrEmpty(item.BackgroundColor))
					View.BackgroundColor = Color.FromArgb(item.BackgroundColor);
			}
		}

		protected override void OnBindingContextChanged()
		{
			try
			{
				base.OnBindingContextChanged();
				var item = BindingContext as ListItem;
				if (item != null)
				{
					Color transformedColor;

					if (!string.IsNullOrWhiteSpace(item.TextColor))
					{
						transformedColor = Color.FromArgb(item.TextColor);

						_mainLabel.TextColor = transformedColor;
						_secondaryLabel.TextColor = transformedColor;
						_distanceLabel.TextColor = transformedColor;
					}

					if (string.IsNullOrEmpty(item.Icon))
						item.Icon = "https://beehive.blob.core.windows.net/staticimages/FeatureImages/MutantLizard01.png";

					_photo.Source = new UriImageSource()
					{
						Uri = new Uri(item.Icon),
						CachingEnabled = true,
						CacheValidity = new TimeSpan(30, 0, 0, 0),
					};

					if (!string.IsNullOrWhiteSpace(item.BackgroundColor))
					{
						transformedColor = Color.FromArgb(item.BackgroundColor);
						View.BackgroundColor = transformedColor;
					}

					if (!string.IsNullOrWhiteSpace(item.CircleColor))
					{
						_statusCircle.Text = "\u25CF "; // ascii circle
						_statusCircle.TextColor = Color.FromArgb(item.CircleColor);
						_statusCircle.FontSize = 30;
					}
				}
			}
			catch
			{
			}
		}

#pragma warning disable 1998 // considered for removal
		async void FastCompleteForCmd(object sender)
#pragma warning restore 1998
		{
			try
			{
				{
					var item = BindingContext as ListItem;
					bool success = true; // await _taskListManager.FastComplete(item);

					if (success)
						RefreshFromQuickComplete(this, new ListItemEventArgs(item));
				}
			}
			catch
			{

			}
		}

	}
}
