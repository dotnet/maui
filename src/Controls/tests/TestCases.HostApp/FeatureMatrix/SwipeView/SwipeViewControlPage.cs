using Microsoft.Maui.Controls;
using System;

namespace Maui.Controls.Sample;

public class SwipeViewControlPage : NavigationPage
{
	public SwipeViewControlPage()
	{
		var vm = new SwipeViewViewModel();
		var mainPage = new SwipeViewControlMainPage(vm);
		PushAsync(mainPage);
	}
}

public class SwipeViewControlMainPage : ContentPage
{
	private SwipeViewViewModel _viewModel;
	private VerticalStackLayout layout;
	private Action<OpenSwipeItem> _requestOpenHandler;
	private Action _requestCloseHandler;

	public SwipeViewControlMainPage(SwipeViewViewModel viewModel)
	{
		Title = "SwipeView Control";
		_viewModel = viewModel;
		BindingContext = _viewModel;

		ToolbarItems.Add(new ToolbarItem
		{
			Text = "Options",
			Command = new Command(async () =>
			{
				_viewModel.IsEnabled = true;
				_viewModel.IsVisible = true;
				_viewModel.BackgroundColor = Color.FromArgb("#F0F0F0");
				_viewModel.SwipeItemsBackgroundColor = Color.FromArgb("#6A5ACD");
				_viewModel.FlowDirection = FlowDirection.LeftToRight;
				_viewModel.Threshold = 100;
				_viewModel.HasShadow = false;
				_viewModel.SwipeMode = SwipeMode.Reveal;
				_viewModel.SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.Auto;
				_viewModel.EventInvokedText = "Event not invoked yet";
				_viewModel.SwipeStartedText = "Swipe Started: ";
				_viewModel.SwipeChangingText = "Swipe Changing: ";
				_viewModel.SwipeEndedText = "Swipe Ended: ";
				_viewModel.SelectedSwipeItemType = "Label";
				_viewModel.SelectedContentType = "Label";
				var optionsPage = new SwipeViewOptionsPage(viewModel, this);
				optionsPage.SwipeViewOptionsApplied += (content, swipeItem) =>
				{
					UpdateSwipeViewContent(content, swipeItem);
				};
				await Navigation.PushAsync(optionsPage);

			})
		});

		layout = new VerticalStackLayout
		{
			Padding = 15,
			Spacing = 8,
			Children =
		{
			new Label
			{
				Text = "SwipeView Control Label",
				FontSize = 20,
				HorizontalOptions = LayoutOptions.Center,
				Margin = new Thickness(0, 0, 0, 10),
				AutomationId = "SwipeViewControlLabel"
			},

			ApplyContentWithSwipeItems("Label", "Label"),

			new BoxView
			{
				HeightRequest = 100,
				Color = Colors.Transparent
			},

			CreateProgrammaticActionButtons(),


			new Label { Text = "Events:", FontSize = 11, FontAttributes = FontAttributes.Bold },

			new Label { FontSize = 11, TextColor = Colors.Red, AutomationId = "EventInvokedLabel" }
				.ApplyBinding(Label.TextProperty, nameof(_viewModel.EventInvokedText)),

			new Label { FontSize = 11, TextColor = Colors.Red, AutomationId = "SwipeStartedLabel" }
				.ApplyBinding(Label.TextProperty, nameof(_viewModel.SwipeStartedText)),

			new Label { FontSize = 11, TextColor = Colors.Red, AutomationId = "SwipeChangingLabel" }
				.ApplyBinding(Label.TextProperty, nameof(_viewModel.SwipeChangingText)),

			new Label { FontSize = 11, TextColor = Colors.Red, AutomationId = "SwipeEndedLabel" }
				.ApplyBinding(Label.TextProperty, nameof(_viewModel.SwipeEndedText))
		}
		};

		Content = layout;
	}

	private View ApplyContentWithSwipeItems(string contentType, string swipeItemType)
	{
		View finalContent;

		switch (contentType)
		{
			case "Label":
				var label = new Label
				{
					Text = "The .NET MAUI SwipeView offers flexible swipe gestures with customizable actions via LeftItems, RightItems, TopItems, and BottomItems. Properties like SwipeMode and SwipeBehaviorOnInvoked control whether actions reveal or execute immediately. Verifying these ensures consistent and expected behavior across platforms.",
					FontSize = 16,
					HeightRequest = 150,
					AutomationId = "SwipeViewLabel",
				};

				finalContent = CreateSwipeView(label, swipeItemType);
				break;

			case "Image":
				var image = new Image
				{
					Source = "dotnet_bot.png",
					HeightRequest = 150,
					WidthRequest = 150,
					AutomationId = "SwipeViewImage"
				};

				finalContent = CreateSwipeView(image, swipeItemType);
				break;

			case "CollectionView":
				var collectionView = new CollectionView
				{
					ItemsSource = _viewModel.Items,
					ItemTemplate = new DataTemplate(() =>
					{
						var itemLabel = new Label { FontSize = 18, HeightRequest = 30 };
						itemLabel.SetBinding(Label.TextProperty, nameof(SwipeViewViewModel.ItemModel.Title));

						var leftSwipeItems = CreateSwipeItems(swipeItemType);
						var rightSwipeItems = CreateSwipeItems(swipeItemType);
						var topSwipeItems = CreateSwipeItems(swipeItemType);
						var bottomSwipeItems = CreateSwipeItems(swipeItemType);
						var swipeView = new SwipeView
						{
							Content = itemLabel,
							LeftItems = leftSwipeItems,
							RightItems = rightSwipeItems,
							TopItems = topSwipeItems,
							BottomItems = bottomSwipeItems,
							Threshold = _viewModel.Threshold,
							FlowDirection = _viewModel.FlowDirection,
							BackgroundColor = _viewModel.BackgroundColor,
							IsEnabled = _viewModel.IsEnabled,
							IsVisible = _viewModel.IsVisible,
							Shadow = _viewModel.SwipeViewShadow,
							AutomationId = "SwipeViewCollectionItem"
						};

						swipeView.SetBinding(BindingContextProperty, ".");
						AttachSwipeEvents(swipeView);
						return swipeView;
					})
				};
				finalContent = collectionView;
				break;

			default:
				finalContent = new Label { Text = "Default Content" };
				break;
		}

		if (finalContent is SwipeView swipe)
		{
			WireUpOpenCloseHandlers(swipe);
		}
		return finalContent;
	}

	private SwipeItems CreateSwipeItems(string type)
	{
		var swipeItems = new SwipeItems
		{
			Mode = _viewModel.SwipeMode,
			SwipeBehaviorOnInvoked = _viewModel.SwipeBehaviorOnInvoked
		};

		switch (type)
		{
			case "Label":
				var labelItem = new SwipeItem
				{
					Text = "Label",
					AutomationId = "SwipeLabelItem",
					BackgroundColor = _viewModel.SwipeItemsBackgroundColor
				};
				labelItem.Invoked += (s, e) => _viewModel.EventInvokedText = "Label Invoked";
				swipeItems.Add(labelItem);
				break;

			case "IconImageSource":
				var iconItem = new SwipeItem
				{
					Text = "Icon",
					AutomationId = "SwipeIconItem",
					IconImageSource = "groceries.png",
					BackgroundColor = _viewModel.SwipeItemsBackgroundColor
				};
				iconItem.Invoked += (s, e) => _viewModel.EventInvokedText = "Icon Invoked";
				swipeItems.Add(iconItem);
				break;

			case "Button":
				var button = new Button
				{
					Text = "Click Me",
					TextColor = Colors.Black,
					AutomationId = "SwipeButtonItem",
					BackgroundColor = _viewModel.SwipeItemsBackgroundColor,
					Padding = new Thickness(5)
				};
				button.Clicked += (s, e) => _viewModel.EventInvokedText = "Button Clicked";

				swipeItems.Add(new SwipeItemView { Content = button });
				break;
		}

		return swipeItems;
	}

	private SwipeView CreateSwipeView(View contentView, string swipeItemType)
	{
		var leftItems = CreateSwipeItems(swipeItemType);
		var rightItems = CreateSwipeItems(swipeItemType);
		var topItems = CreateSwipeItems(swipeItemType);
		var bottomItems = CreateSwipeItems(swipeItemType);
		var swipeView = new SwipeView
		{
			Content = contentView,
			LeftItems = leftItems,
			RightItems = rightItems,
			TopItems = topItems,
			BottomItems = bottomItems,
			Threshold = _viewModel.Threshold,
			FlowDirection = _viewModel.FlowDirection,
			BackgroundColor = _viewModel.BackgroundColor,
			IsEnabled = _viewModel.IsEnabled,
			IsVisible = _viewModel.IsVisible,
			Shadow = _viewModel.SwipeViewShadow,
			AutomationId = "SwipeViewControl"
		};

		AttachSwipeEvents(swipeView);
		return swipeView;
	}



	private void AttachSwipeEvents(SwipeView swipeView)
	{
		swipeView.SwipeStarted += (s, e) =>
			_viewModel.SwipeStartedText = $"Swipe Started: {e.SwipeDirection}";

		swipeView.SwipeChanging += (s, e) =>
			_viewModel.SwipeChangingText = $"Swipe Changing: {e.SwipeDirection}";

		swipeView.SwipeEnded += (s, e) =>
			_viewModel.SwipeEndedText = $"Swipe Ended: {e.SwipeDirection}, IsOpen: {(e.IsOpen ? "Open" : "Closed")}";
	}

	private View CreateProgrammaticActionButtons()
	{
		return new Grid
		{
			ColumnSpacing = 10,
			RowSpacing = 10,
			Margin = new Thickness(0, 10, 0, 10),
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto }
			},
			ColumnDefinitions =
			{
				new ColumnDefinition { Width = GridLength.Star },
				new ColumnDefinition { Width = GridLength.Star }
			},
			Children =
			{
				new Label { Text = "Programmatic Actions:", FontSize = 11 }
					.AssignToGrid(0, 0, 1, 2),

				new HorizontalStackLayout
				{
					Spacing = 5,
					Children =
					{
						new Button { Text = "Open Left", FontSize = 11, Command = _viewModel.OpenLeftCommand, AutomationId = "OpenLeft" },
						new Button { Text = "Open Right", FontSize = 11, Command = _viewModel.OpenRightCommand, AutomationId = "OpenRight" }
					}
				}.AssignToGrid(1, 0),

				new HorizontalStackLayout
				{
					Spacing = 5,
					Children =
					{
						new Button { Text = "Open Top", FontSize = 11, Command = _viewModel.OpenTopCommand, AutomationId = "OpenTop" },
						new Button { Text = "Open Bottom", FontSize = 11, Command = _viewModel.OpenBottomCommand, AutomationId = "OpenBottom" }
					}
				}.AssignToGrid(1, 1),

			new Button { Text ="Close",FontSize=11, Command = _viewModel.CloseCommand, AutomationId = "CloseSwipeViewButton" }.AssignToGrid(2, 0, 1, 2)
			}
		};
	}

	private void WireUpOpenCloseHandlers(SwipeView swipeView)
	{
		// Unsubscribe previous handlers if they exist
		if (_requestOpenHandler != null)
		{
			_viewModel.RequestOpen -= _requestOpenHandler;
		}
		if (_requestCloseHandler != null)
		{
			_viewModel.RequestClose -= _requestCloseHandler;
		}

		// Create new handlers
		_requestOpenHandler = dir =>
		{
			Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() => swipeView.Open(dir));
		};
		_requestCloseHandler = () =>
		{
			Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() => swipeView.Close());
		};

		// Subscribe new handlers
		_viewModel.RequestOpen += _requestOpenHandler;
		_viewModel.RequestClose += _requestCloseHandler;
	}

	public void UpdateSwipeViewContent(string contentType, string swipeItemType)
	{
		_viewModel.SelectedContentType = contentType;
		_viewModel.SelectedSwipeItemType = swipeItemType;
		var newContent = ApplyContentWithSwipeItems(contentType, swipeItemType);
		layout.Children[1] = newContent;
		if (newContent is SwipeView swipeView)
		{
			WireUpOpenCloseHandlers(swipeView);
		}
	}
}

public static class ViewExtensions
{
	public static TView AssignToGrid<TView>(this TView view, int row, int column, int rowSpan = 1, int columnSpan = 1) where TView : View
	{
		Grid.SetRow(view, row);
		Grid.SetColumn(view, column);
		if (rowSpan > 1)
			Grid.SetRowSpan(view, rowSpan);
		if (columnSpan > 1)
			Grid.SetColumnSpan(view, columnSpan);
		return view;
	}

	public static TView ApplyBinding<TView>(this TView view, BindableProperty property, string path) where TView : BindableObject
	{
		view.SetBinding(property, path);
		return view;
	}
}