namespace Maui.Controls.Sample
{

	public class ContentViewGalleryPage : ContentPage
	{
		int _currentIndex;
		Picker _picker;
		List<ContentView> _contentViews;
		Entry _targetEntry;
		StackLayout _layout;

		protected new StackLayout Layout { get; private set; }

		public ContentViewGalleryPage()
		{
			Layout = new StackLayout
			{
				Padding = new Thickness(20)
			};

			var modalDismissButton = new Button()
			{
				Text = "Dismiss Page",
				Command = new Command(async () =>
				{
					if (_picker.SelectedIndex == 0)
					{
						await Navigation.PopAsync();
					}
					else
					{
						_picker.SelectedIndex--;
					}
				})
			};

			Layout.Children.Add(modalDismissButton);

			Build(Layout);

			if (SupportsScroll)
				Content = new ScrollView { AutomationId = "GalleryScrollView", Content = Layout };
			else
			{
				var content = new Grid { AutomationId = "GalleryScrollView" };
				content.Children.Add(Layout);
				Content = content;
			}
		}

		protected virtual void Build(StackLayout stackLayout)
		{
			_contentViews = new List<ContentView>();
			_layout = new StackLayout();

			_targetEntry = new Entry { AutomationId = "TargetView", Placeholder = "Jump To View" };
			var goButton = new Button
			{
				Text = "Go",
				AutomationId = "GoButton"
			};
			goButton.Clicked += GoClicked;

			_picker = new Picker();
			foreach (var container in _contentViews)
			{
				_picker.Items.Add(container.AutomationId);
			}

			_picker.SelectedIndex = _currentIndex;

			_picker.SelectedIndexChanged += PickerSelectedIndexChanged;

			_layout.Children.Add(_picker);
			_layout.Children.Add(_targetEntry);
			_layout.Children.Add(goButton);
			if (_contentViews.Any())
			{
				_layout.Children.Add(_contentViews[_currentIndex]);
			}

			stackLayout.Children.Add(_layout);
		}


		void GoClicked(object sender, EventArgs e)
		{
			if (!_contentViews.Any())
			{
				return;
			}

			var target = _targetEntry.Text;
			_targetEntry.Text = "";
			var index = -1;

			if (string.IsNullOrEmpty(target))
			{
				return;
			}

			for (int n = 0; n < _contentViews.Count; n++)
			{
				if (_contentViews[n].AutomationId == target)
				{
					index = n;
					break;
				}
			}

			if (index < 0)
			{
				return;
			}

			var targetContainer = _contentViews[index];

			if (_layout.Children.Count == 4)
			{
				_layout.Children.RemoveAt(3);
			}
			_layout.Children.Add(targetContainer);

			_picker.SelectedIndexChanged -= PickerSelectedIndexChanged;
			_picker.SelectedIndex = index;
			_picker.SelectedIndexChanged += PickerSelectedIndexChanged;
		}

		void PickerSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			_currentIndex = _picker.SelectedIndex;
			_layout.Children.RemoveAt(3);
			_layout.Children.Add(_contentViews[_currentIndex]);
		}

		protected virtual bool SupportsScroll
		{
			get { return true; }
		}

		protected void Add(ContentView view)
		{
			_contentViews.Add(view);
			_picker.Items.Add(view.AutomationId);
		}
	}
}