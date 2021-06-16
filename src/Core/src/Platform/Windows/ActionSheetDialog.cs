#nullable disable
using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public sealed class ActionSheetContent : UserControl
	{
		ActionSheetArguments _options;

		public event EventHandler OptionSelected;

		public ActionSheetContent(ActionSheetArguments sheetOptions)
		{
			Initialize();

			_options = sheetOptions;

			TitleBlock.Text = _options.Title ?? string.Empty;
			OptionsList.ItemsSource = _options.Buttons.ToList();

			if (_options.FlowDirection == Maui.FlowDirection.RightToLeft)
			{
				TitleBlock.FlowDirection = UI.Xaml.FlowDirection.RightToLeft;
				OptionsList.FlowDirection = UI.Xaml.FlowDirection.RightToLeft;
			}
			else if (_options.FlowDirection == Maui.FlowDirection.LeftToRight)
			{
				TitleBlock.FlowDirection = UI.Xaml.FlowDirection.LeftToRight;
				OptionsList.FlowDirection = UI.Xaml.FlowDirection.LeftToRight;
			}

			if (_options.FlowDirection == Maui.FlowDirection.RightToLeft)
			{
				if (_options.Cancel != null)
				{
					LeftBtn.Content = _options.Cancel;
					if (_options.Destruction != null)
						RightBtn.Content = _options.Destruction;
				}
				else if (_options.Destruction != null)
					LeftBtn.Content = _options.Destruction;
			}
			else
			{
				if (_options.Cancel != null)
				{
					RightBtn.Content = _options.Cancel;
					if (_options.Destruction != null)
						LeftBtn.Content = _options.Destruction;
				}
				else if (_options.Destruction != null)
					RightBtn.Content = _options.Destruction;
			}

			LeftBtn.Visibility = LeftBtn.Content == null ? UI.Xaml.Visibility.Collapsed : UI.Xaml.Visibility.Visible;
			RightBtn.Visibility = RightBtn.Content == null ? UI.Xaml.Visibility.Collapsed : UI.Xaml.Visibility.Visible;
		}

		internal TextBlock TitleBlock { get; private set; }

		internal ListView OptionsList { get; private  set; }

		internal Button LeftBtn { get; private set; }

		internal Button RightBtn { get; private set; }

		void Initialize()
		{
			var mainLayout = new Grid
			{
				Padding = new UI.Xaml.Thickness(10),
				RowDefinitions =
				{
					new RowDefinition { Height = new UI.Xaml.GridLength(0, UI.Xaml.GridUnitType.Star) },		
					new RowDefinition { Height = new UI.Xaml.GridLength(0, UI.Xaml.GridUnitType.Auto) }
				}
			};

			var firstLayout = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition { Height = new UI.Xaml.GridLength(0, UI.Xaml.GridUnitType.Auto) },
					new RowDefinition { Height = new UI.Xaml.GridLength(0, UI.Xaml.GridUnitType.Star) }
				}
			};

			TitleBlock = new TextBlock { FontSize = 18, MaxLines = 2 };
			firstLayout.Children.Add(TitleBlock);
			Grid.SetRow(TitleBlock, 0);

			OptionsList = new ListView { IsItemClickEnabled = true, Margin = new UI.Xaml.Thickness(0, 10, 0, 10), SelectionMode = ListViewSelectionMode.None };
			OptionsList.ItemClick += ListItemSelected;
			firstLayout.Children.Add(OptionsList);
			Grid.SetRow(OptionsList, 1);

			var secondLayout = new Grid
			{
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = new UI.Xaml.GridLength(0, UI.Xaml.GridUnitType.Star) },
					new ColumnDefinition { Width = new UI.Xaml.GridLength(0, UI.Xaml.GridUnitType.Star) }
				},
				VerticalAlignment = VerticalAlignment.Bottom
			};

			LeftBtn = new Button { Height = 32, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new UI.Xaml.Thickness(0, 0, 5, 0) };
			LeftBtn.Click += ActionButtonClicked;
			secondLayout.Children.Add(LeftBtn);
			Grid.SetColumn(LeftBtn, 0);

			RightBtn = new Button { Height = 32, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new UI.Xaml.Thickness(5, 0, 0, 0) };
			RightBtn.Click += ActionButtonClicked;
			secondLayout.Children.Add(RightBtn);
			Grid.SetColumn(RightBtn, 1);

			mainLayout.Children.Add(firstLayout);
			Grid.SetRow(firstLayout, 0);

			mainLayout.Children.Add(secondLayout);
			Grid.SetRow(secondLayout, 1);

			Content = mainLayout;
		}

		void ListItemSelected(object sender, ItemClickEventArgs e)
		{
			var selection = (string)e.ClickedItem;
			_options.SetResult(selection);

			OptionSelected?.Invoke(this, null);
		}

		void ActionButtonClicked(object sender, RoutedEventArgs e)
		{
			var button = (Button)sender;
			var selection = (string)button.Content;
			_options.SetResult(selection);

			OptionSelected?.Invoke(this, null);
		}
	}
}