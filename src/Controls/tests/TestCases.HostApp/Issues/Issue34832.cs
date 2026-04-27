using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34832, "SwipeItem.IsVisible doesn't properly refresh native swipe items when binding value changes dynamically", PlatformAffected.All)]
public class Issue34832 : ContentPage
{
	readonly Issue34832ViewModel _viewModel = new() { IsDeleteVisible = true };
	SwipeView _swipeView;

	public Issue34832()
	{
		BindingContext = _viewModel;

		SwipeItem deleteSwipeItem = new SwipeItem
		{
			Text = "Delete",
			BackgroundColor = Colors.Green,
			AutomationId = "DeleteSwipeItem"
		};
		deleteSwipeItem.SetBinding(SwipeItem.IsVisibleProperty, new Binding(nameof(Issue34832ViewModel.IsDeleteVisible)));

		SwipeItem archiveSwipeItem = new SwipeItem
		{
			Text = "Archive",
			BackgroundColor = Colors.Blue,
			AutomationId = "ArchiveSwipeItem"
		};

		_swipeView = new SwipeView
		{
			AutomationId = "TestSwipeView",
			HeightRequest = 60,
			LeftItems = new SwipeItems { deleteSwipeItem, archiveSwipeItem },
			Content = new Grid
			{
				BackgroundColor = Colors.LightGray,
				Children =
				{
					new Label
					{
						Text = "Swipe left to reveal items",
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center
					}
				}
			}
		};

		_swipeView.Loaded += (s, e) => _swipeView = (SwipeView)s;

		Button toggleButton = new Button
		{
			Text = "Toggle Delete Visibility",
			AutomationId = "ToggleVisibilityButton"
		};
		toggleButton.Clicked += (s, e) => _viewModel.IsDeleteVisible = !_viewModel.IsDeleteVisible;

		Button openSwipeButton = new Button
		{
			Text = "Open Swipe",
			AutomationId = "OpenSwipeButton"
		};
		openSwipeButton.Clicked += (s, e) => _swipeView?.Open(OpenSwipeItem.LeftItems);

		Button closeSwipeButton = new Button
		{
			Text = "Close Swipe",
			AutomationId = "CloseSwipeButton"
		};
		closeSwipeButton.Clicked += (s, e) => _swipeView?.Close();

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 20,
			Children =
			{
				_swipeView,
				toggleButton,
				openSwipeButton,
				closeSwipeButton
			}
		};
	}
}

public class Issue34832ViewModel : INotifyPropertyChanged
{
	bool _isDeleteVisible;

	public bool IsDeleteVisible
	{
		get => _isDeleteVisible;
		set
		{
			if (_isDeleteVisible != value)
			{
				_isDeleteVisible = value;
				OnPropertyChanged();
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void OnPropertyChanged([CallerMemberName] string name = null) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
