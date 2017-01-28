using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Xamarin.Forms.Platform.WinRT
{
	public abstract partial class Platform
	{
		CommandBar AddOpenMasterButton(CommandBar commandBar)
		{
			if (!_toolbarTracker.HaveMasterDetail)
			{
				return commandBar;
			}

			if (commandBar == null)
			{
				commandBar = CreateCommandBar();
			}

			Page target = _toolbarTracker.Target;
			var mdp = target as MasterDetailPage;
			while (mdp == null)
			{
				var container = target as IPageContainer<Page>;
				if (container == null)
				{
					break;
				}

				target = container.CurrentPage;
				mdp = container.CurrentPage as MasterDetailPage;
			}

			if (mdp == null || !mdp.ShouldShowToolbarButton())
			{
				return commandBar;
			}

			var openMaster = new AppBarButton { DataContext = mdp };
			openMaster.SetBinding(AppBarButton.LabelProperty, "Master.Title");
			openMaster.SetBinding(AppBarButton.IconProperty, "Master.Icon", _fileImageSourcePathConverter);
			openMaster.Click += (s, a) => { mdp.IsPresented = !mdp.IsPresented; };

			commandBar.PrimaryCommands.Add(openMaster);

			return commandBar;
		}

		CommandBar CreateCommandBar()
		{
			var commandBar = new CommandBar();
			_page.BottomAppBar = commandBar;
			return commandBar;
		}

		void UpdateBounds()
		{
			_bounds = new Rectangle(0, 0, _page.ActualWidth, _page.ActualHeight);
		}

		Task<CommandBar> GetCommandBarAsync()
		{
			return Task.FromResult(_page.BottomAppBar as CommandBar);
		}

		void ClearCommandBar()
		{
			_page.BottomAppBar = null;
		}

		void OnPageActionSheet(Page sender, ActionSheetArguments options)
		{
			var finalArguments = new List<string>();
			if (options.Destruction != null)
				finalArguments.Add(options.Destruction);
			if (options.Buttons != null)
				finalArguments.AddRange(options.Buttons);
			if (options.Cancel != null)
				finalArguments.Add(options.Cancel);

			var list = new Windows.UI.Xaml.Controls.ListView
			{
				Style = (Windows.UI.Xaml.Style)Windows.UI.Xaml.Application.Current.Resources["ActionSheetList"],
				ItemsSource = finalArguments,
				IsItemClickEnabled = true
			};

			list.ItemClick += (s, e) =>
			{
				_currentActionSheet.IsOpen = false;
				options.SetResult((string)e.ClickedItem);
			};

			_actionSheetOptions = options;

			Size size = Device.Info.ScaledScreenSize;

			var stack = new StackPanel
			{
				MinWidth = 100,
				Children =
				{
					new TextBlock
					{
						Text = options.Title ?? string.Empty,
						Style = (Windows.UI.Xaml.Style)Windows.UI.Xaml.Application.Current.Resources["TitleTextBlockStyle"],
						Margin = new Windows.UI.Xaml.Thickness(0, 0, 0, 10),
						Visibility = options.Title != null ? Visibility.Visible : Visibility.Collapsed
					},
					list
				}
			};

			var border = new Border
			{
				Child = stack,
				BorderBrush = new SolidColorBrush(Colors.White),
				BorderThickness = new Windows.UI.Xaml.Thickness(1),
				Padding = new Windows.UI.Xaml.Thickness(15),
				Background = (Brush)Windows.UI.Xaml.Application.Current.Resources["AppBarBackgroundThemeBrush"]
			};

			Windows.UI.Xaml.Controls.Grid.SetRow(border, 1);
			Windows.UI.Xaml.Controls.Grid.SetColumn(border, 1);

			var container = new Windows.UI.Xaml.Controls.Grid
			{
				RowDefinitions =
				{
					new Windows.UI.Xaml.Controls.RowDefinition { Height = new Windows.UI.Xaml.GridLength(1, Windows.UI.Xaml.GridUnitType.Star) },
					new Windows.UI.Xaml.Controls.RowDefinition { Height = new Windows.UI.Xaml.GridLength(0, Windows.UI.Xaml.GridUnitType.Auto) },
					new Windows.UI.Xaml.Controls.RowDefinition { Height = new Windows.UI.Xaml.GridLength(1, Windows.UI.Xaml.GridUnitType.Star) }
				},
				ColumnDefinitions =
				{
					new Windows.UI.Xaml.Controls.ColumnDefinition { Width = new Windows.UI.Xaml.GridLength(1, Windows.UI.Xaml.GridUnitType.Star) },
					new Windows.UI.Xaml.Controls.ColumnDefinition { Width = new Windows.UI.Xaml.GridLength(0, Windows.UI.Xaml.GridUnitType.Auto) },
					new Windows.UI.Xaml.Controls.ColumnDefinition { Width = new Windows.UI.Xaml.GridLength(1, Windows.UI.Xaml.GridUnitType.Star) }
				},
				Height = size.Height,
				Width = size.Width,
				Children = { border }
			};

			var bgPopup = new Popup { Child = new Canvas { Width = size.Width, Height = size.Height, Background = new SolidColorBrush(new Windows.UI.Color { A = 128, R = 0, G = 0, B = 0 }) } };

			bgPopup.IsOpen = true;

			_currentActionSheet = new Popup { ChildTransitions = new TransitionCollection { new PopupThemeTransition() }, IsLightDismissEnabled = true, Child = container };

			_currentActionSheet.Closed += (s, e) =>
			{
				bgPopup.IsOpen = false;
				CancelActionSheet();
			};

			if (Device.Idiom == TargetIdiom.Phone)
			{
				double height = _page.ActualHeight;
				stack.Height = height;
				stack.Width = size.Width;
				border.BorderThickness = new Windows.UI.Xaml.Thickness(0);

				_currentActionSheet.Height = height;
				_currentActionSheet.VerticalOffset = size.Height - height;
			}

			_currentActionSheet.IsOpen = true;
		}

		internal async Task UpdateToolbarItems()
		{
			CommandBar commandBar = await GetCommandBarAsync();
			if (commandBar != null)
			{
				commandBar.PrimaryCommands.Clear();
				commandBar.SecondaryCommands.Clear();
			}

			commandBar = AddOpenMasterButton(commandBar);

			foreach (ToolbarItem item in _toolbarTracker.ToolbarItems.OrderBy(ti => ti.Priority))
			{
				if (commandBar == null)
					commandBar = CreateCommandBar();

				var button = new AppBarButton();
				button.SetBinding(AppBarButton.LabelProperty, "Text");
				button.SetBinding(AppBarButton.IconProperty, "Icon", _fileImageSourcePathConverter);
				button.Command = new MenuItemCommand(item);
				button.DataContext = item;

				ToolbarItemOrder order = item.Order == ToolbarItemOrder.Default ? ToolbarItemOrder.Primary : item.Order;
				if (order == ToolbarItemOrder.Primary)
					commandBar.PrimaryCommands.Add(button);
				else
					commandBar.SecondaryCommands.Add(button);
			}

			if (commandBar?.PrimaryCommands.Count + commandBar?.SecondaryCommands.Count == 0)
				ClearCommandBar();
		}
	}
}
