using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace Maui.Controls.Sample.Issues
{
	// Issue12574 (src\ControlGallery\src\Issues.Shared\Issue12574.cs
	[Issue(IssueTracker.None, 12574, "CarouselView Loop=True default freezes iOS app", PlatformAffected.Default)]
	public class CarouselViewLoopNoFreeze : ContentPage
	{
		readonly string _carouselAutomationId = "carouselView";
		readonly string _btnRemoveAutomationId = "btnRemove";
		readonly string _btnRemoveAllAutomationId = "btnRemoveAll";
		readonly string _btnSwipeAutomationId = "btnSwipe";

		readonly ViewModelIssue12574 _viewModel;
		readonly CarouselView2 _carouselView;
		readonly Button _btn;
		readonly Button _btn2;
		readonly Button _btn3;

		public CarouselViewLoopNoFreeze()
		{
			_btn = new Button
			{
				Text = "Remove Last",
				AutomationId = _btnRemoveAutomationId
			};
			_btn.SetBinding(Button.CommandProperty, "RemoveLastItemCommand");

			_btn2 = new Button
			{
				Text = "Remove All",
				AutomationId = _btnRemoveAllAutomationId
			};
			_btn2.SetBinding(Button.CommandProperty, "RemoveAllItemsCommand");

			_btn3 = new Button
			{
				Text = "Swipe",
				AutomationId = _btnSwipeAutomationId
			};
			_btn3.Clicked += (s, e) =>
			{
				if (_viewModel.Items.Count == 0)
					return;
				_viewModel.CurrentPosition = (_viewModel.CurrentPosition + 1) % _viewModel.Items.Count;
				_carouselView.ScrollTo(_viewModel.CurrentPosition);
			};

			_carouselView = new CarouselView2
			{
				AutomationId = _carouselAutomationId,
				Margin = new Thickness(30),
				BackgroundColor = Colors.Yellow,
				ItemTemplate = new DataTemplate(() =>
				{

					var stacklayout = new StackLayout();
					var labelId = new Label { TextColor = Colors.Black };
					var labelText = new Label { TextColor = Colors.Black };
					var labelDescription = new Label { TextColor = Colors.Black };
					labelId.SetBinding(Label.TextProperty, "Id");
					labelText.SetBinding(Label.TextProperty, "Text");
					labelDescription.SetBinding(Label.TextProperty, "Description");

					stacklayout.Children.Add(labelId);
					stacklayout.Children.Add(labelText);
					stacklayout.Children.Add(labelDescription);
					return stacklayout;
				})
			};
			_carouselView.SetBinding(CarouselView.ItemsSourceProperty, "Items");
			this.SetBinding(Page.TitleProperty, "Title");
			_carouselView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;

			var layout = new Grid();
			layout.RowDefinitions.Add(new RowDefinition { Height = 100 });
			layout.RowDefinitions.Add(new RowDefinition { Height = 100 });
			layout.RowDefinitions.Add(new RowDefinition { Height = 100 });
			layout.RowDefinitions.Add(new RowDefinition());
			Grid.SetRow(_btn2, 1);
			Grid.SetRow(_btn3, 2);
			Grid.SetRow(_carouselView, 3);
			layout.Children.Add(_btn);
			layout.Children.Add(_btn2);
			layout.Children.Add(_btn3);
			layout.Children.Add(_carouselView);

			BindingContext = _viewModel = new ViewModelIssue12574();
			Content = layout;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			_viewModel.OnAppearing();
		}
	}
	class ViewModelIssue12574 : BaseViewModel1
	{
		public ObservableCollection<ModelIssue12574> Items { get; set; }
		public Command LoadItemsCommand { get; set; }
		public Command RemoveAllItemsCommand { get; set; }
		public Command RemoveLastItemCommand { get; set; }
		private int _currentPosition = 0;
		public int CurrentPosition
		{
			get => _currentPosition;
			set
			{
				_currentPosition = value;
				OnPropertyChanged(nameof(CurrentPosition));
			}
		}

		public ViewModelIssue12574()
		{
			Title = "CarouselView Looping";
			Items = [];
			LoadItemsCommand = new Command(() => ExecuteLoadItemsCommand());
			RemoveAllItemsCommand = new Command(() => ExecuteRemoveItemsCommand(), () => Items.Count > 0);
			RemoveLastItemCommand = new Command(() => ExecuteRemoveLastItemCommand(), () => Items.Count > 0);
		}

		void ExecuteRemoveItemsCommand()
		{
			while (Items.Count > 0)
			{
				Items.Remove(Items.Last());
			}
			RemoveAllItemsCommand.ChangeCanExecute();
			RemoveLastItemCommand.ChangeCanExecute();
		}

		void ExecuteRemoveLastItemCommand()
		{
			Items.Remove(Items.Last());
			RemoveAllItemsCommand.ChangeCanExecute();
			RemoveLastItemCommand.ChangeCanExecute();
			if (CurrentPosition > 0)
			{
				CurrentPosition--;
			}
		}

		void ExecuteLoadItemsCommand()
		{
			IsBusy = true;

			try
			{
				Items.Clear();
				for (int i = 0; i < 3; i++)
				{
					Items.Add(new ModelIssue12574 { Id = Guid.NewGuid().ToString(), Text = $"{i} item", Description = "This is an item description." });
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
			finally
			{
				IsBusy = false;
				RemoveAllItemsCommand.ChangeCanExecute();
				RemoveLastItemCommand.ChangeCanExecute();
			}
		}

		public void OnAppearing()
		{
			IsBusy = true;
			LoadItemsCommand.Execute(null);
		}
	}

	class ModelIssue12574
	{
		public string Id { get; set; }
		public string Text { get; set; }
		public string Description { get; set; }
	}

	class BaseViewModel1 : INotifyPropertyChanged
	{
		public string Title { get; set; }
		public bool IsInitialized { get; set; }

		bool _isBusy;

		/// <summary>
		/// Gets or sets if VM is busy working
		/// </summary>
		public bool IsBusy
		{
			get { return _isBusy; }
			set { _isBusy = value; OnPropertyChanged("IsBusy"); }
		}

		//INotifyPropertyChanged Implementation
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged == null)
				return;

			PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}