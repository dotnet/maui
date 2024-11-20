namespace Maui.Controls.Sample
{
	internal abstract class CoreGalleryBasePage : CoreGalleryBasePage<View>
	{
	}

	internal abstract class CoreGalleryBasePage<TView> : ContentPage
		where TView : View
	{
		readonly List<IViewContainer<TView>> _viewContainers = new();
		int _currentIndex;

		Picker _picker;
		Entry _targetEntry;
		Button _goButton;
		Layout _layout;

		/// <summary>
		/// Gets a list of all the test view containers for this page.
		/// </summary>
		protected IReadOnlyList<IViewContainer<TView>> ViewContainers => _viewContainers;

		/// <summary>
		/// Gets the root layout for the page. This layout contains everything - including the test chrome.
		/// </summary>
		protected Layout Root { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this gallery supports being hosted in a ScrollView.
		/// </summary>
		protected virtual bool SupportsScroll => true;

		internal CoreGalleryBasePage()
		{
			Initialize();

			Root = new StackLayout
			{
				Padding = new Thickness(20),
				Children =
				{
					new Button()
					{
						Text = "Dismiss Page",
						Command = new Command(() =>
						{
							if (_picker.SelectedIndex == 0)
								Application.Current.Quit();
							else
								_picker.SelectedIndex--;
						})
					},
					BuildLayout(),
				}
			};

			Build();

			InitializeLayout();

			if (SupportsScroll)
			{
				Content = new ScrollView
				{
					AutomationId = "GalleryScrollView",
					Content = Root
				};
			}
			else
			{
				Content = new Grid
				{
					AutomationId = "GalleryScrollView",
					Children = { Root }
				};
			}
		}

		/// <summary>
		/// Code that needs to run before the page is created goes here.
		/// </summary>
		protected virtual void Initialize() { }

		/// <summary>
		/// Add a test case to the gallery page.
		/// </summary>
		/// <param name="viewContainer">The test view container to add.</param>
		protected TViewContainer Add<TViewContainer>(TViewContainer viewContainer)
			where TViewContainer : IViewContainer<TView>
		{
			_viewContainers.Add(viewContainer);
			_picker.Items.Add(viewContainer.TitleLabel.Text);
			return viewContainer;
		}

		/// <summary>
		/// Add all the test view containers to the gallery page.
		/// </summary>
		protected abstract void Build();

		Layout BuildLayout()
		{
			_picker = new Picker();

			_targetEntry = new Entry
			{
				AutomationId = "TargetViewContainer",
				Placeholder = "Jump To ViewContainer"
			};

			_goButton = new Button
			{
				Text = "Go",
				AutomationId = "GoButton",
				Command = new Command(GoClicked)
			};

			_layout = new StackLayout
			{
				Children =
				{
					_picker,
					_targetEntry,
					_goButton,
				}
			};

			return _layout;
		}

		void InitializeLayout()
		{
			_layout.Children.Add(_viewContainers[_currentIndex].ContainerLayout);

			_picker.SelectedIndex = _currentIndex;
			_picker.SelectedIndexChanged += PickerSelectedIndexChanged;
		}

		void GoClicked()
		{
			if (!_viewContainers.Any())
				return;

			var target = _targetEntry.Text;
			if (string.IsNullOrEmpty(target))
				return;

			_targetEntry.Text = "";

			var index = -1;
			for (int n = 0; n < _viewContainers.Count; n++)
			{
				if (_viewContainers[n].View.AutomationId == target)
				{
					index = n;
					break;
				}
			}
			if (index < 0)
				return;

			var targetContainer = _viewContainers[index];

			_layout.Children.RemoveAt(3);
			_layout.Children.Add(targetContainer.ContainerLayout);

			_picker.SelectedIndexChanged -= PickerSelectedIndexChanged;
			_picker.SelectedIndex = index;
			_picker.SelectedIndexChanged += PickerSelectedIndexChanged;
		}

		void PickerSelectedIndexChanged(object sender, EventArgs eventArgs)
		{
			_currentIndex = _picker.SelectedIndex;

			_layout.Children.RemoveAt(3);
			_layout.Children.Add(_viewContainers[_currentIndex].ContainerLayout);
		}
	}
}
