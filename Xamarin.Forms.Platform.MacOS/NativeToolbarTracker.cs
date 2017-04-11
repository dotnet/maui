using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppKit;
using CoreGraphics;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration.macOSSpecific;

namespace Xamarin.Forms.Platform.MacOS
{
	class NativeToolbarGroup
	{
		public class Item
		{
			public NSToolbarItem ToolbarItem;
			public NSButton Button;
			public ToolbarItem Element;
		}

		public NativeToolbarGroup(NSToolbarItemGroup itemGroup)
		{
			Group = itemGroup;
			Items = new List<Item>();
		}

		public NSToolbarItemGroup Group { get; }

		public List<Item> Items { get; }
	}

	internal class NativeToolbarTracker : NSToolbarDelegate
	{
		const string ToolBarId = "AwesomeBarToolbar";

		readonly string _defaultBackButtonTitle = "Back";
		readonly ToolbarTracker _toolbarTracker;

		NSToolbar _toolbar;
		NavigationPage _navigation;

		bool _hasTabs;

		const double BackButtonItemWidth = 36;
		const double ToolbarItemWidth = 44;
		const double ToolbarItemHeight = 25;
		const double ToolbarItemSpacing = 6;
		const double ToolbarHeight = 30;
		const double NavigationTitleMinSize = 300;

		const string NavigationGroupIdentifier = "NavigationGroup";
		const string TabbedGroupIdentifier = "TabbedGroup";
		const string ToolbarItemsGroupIdentifier = "ToolbarGroup";
		const string TitleGroupIdentifier = "TitleGroup";

		NativeToolbarGroup _navigationGroup;
		NativeToolbarGroup _tabbedGroup;
		NativeToolbarGroup _toolbarGroup;
		NativeToolbarGroup _titleGroup;

		NSView _nsToolbarItemViewer;

		public NativeToolbarTracker()
		{
			_toolbarTracker = new ToolbarTracker();
			_toolbarTracker.CollectionChanged += ToolbarTrackerOnCollectionChanged;
		}

		public NavigationPage Navigation
		{
			get { return _navigation; }
			set
			{
				if (_navigation == value)
					return;

				if (_navigation != null)
					_navigation.PropertyChanged -= NavigationPagePropertyChanged;

				_navigation = value;

				if (_navigation != null)
				{
					var parentTabbedPage = _navigation.Parent as TabbedPage;
					if (parentTabbedPage != null)
					{
						_hasTabs = parentTabbedPage.OnThisPlatform().GetTabsStyle() == TabsStyle.OnNavigation;
					}
					_toolbarTracker.Target = _navigation.CurrentPage;
					_navigation.PropertyChanged += NavigationPagePropertyChanged;
				}

				UpdateToolBar();
			}
		}

		public void TryHide(NavigationPage navPage = null)
		{
			if (navPage == null || navPage == _navigation)
			{
				Navigation = null;
			}
		}

		public override string[] AllowedItemIdentifiers(NSToolbar toolbar)
		{
			return new string[] { };
		}

		public override string[] DefaultItemIdentifiers(NSToolbar toolbar)
		{
			return new string[] { };
		}

		public override NSToolbarItem WillInsertItem(NSToolbar toolbar, string itemIdentifier, bool willBeInserted)
		{
			var group = new NSToolbarItemGroup(itemIdentifier);
			var view = new NSView();
			group.View = view;

			if (itemIdentifier == NavigationGroupIdentifier)
				_navigationGroup = new NativeToolbarGroup(group);
			else if (itemIdentifier == TitleGroupIdentifier)
				_titleGroup = new NativeToolbarGroup(group);
			else if (itemIdentifier == TabbedGroupIdentifier)
				_tabbedGroup = new NativeToolbarGroup(group);
			else if (itemIdentifier == ToolbarItemsGroupIdentifier)
				_toolbarGroup = new NativeToolbarGroup(group);

			return group;
		}

		protected virtual bool HasTabs => _hasTabs;

		protected virtual NSToolbar ConfigureToolbar()
		{
			var toolbar = new NSToolbar(ToolBarId)
			{
				DisplayMode = NSToolbarDisplayMode.Icon,
				AllowsUserCustomization = false,
				ShowsBaselineSeparator = true,
				SizeMode = NSToolbarSizeMode.Regular,
				Delegate = this
			};

			return toolbar;
		}

		internal void UpdateToolBar()
		{
			if (NSApplication.SharedApplication.MainWindow == null)
				return;

			if (_navigation == null)
			{
				if (_toolbar != null)
					_toolbar.Visible = false;
				_toolbar = null;
				return;
			}

			var currentPage = _navigation.Peek(0);

			if (NavigationPage.GetHasNavigationBar(currentPage))
			{
				if (_toolbar == null)
				{
					_toolbar = ConfigureToolbar();
					NSApplication.SharedApplication.MainWindow.Toolbar = _toolbar;

					_toolbar.InsertItem(NavigationGroupIdentifier, 0);
					_toolbar.InsertItem(
						HasTabs ? NSToolbar.NSToolbarSpaceItemIdentifier : NSToolbar.NSToolbarFlexibleSpaceItemIdentifier, 1);
					_toolbar.InsertItem(HasTabs ? TabbedGroupIdentifier : TitleGroupIdentifier, 2);
					_toolbar.InsertItem(NSToolbar.NSToolbarFlexibleSpaceItemIdentifier, 3);
					_toolbar.InsertItem(ToolbarItemsGroupIdentifier, 4);
				}

				_toolbar.Visible = true;
				UpdateToolbarItems();
				UpdateTitle();
				UpdateNavigationItems();
				if (HasTabs)
					UpdateTabbedItems();
				UpdateBarBackgroundColor();
			}
			else
			{
				if (_toolbar != null)
				{
					_toolbar.Visible = false;
				}
			}
		}

		void UpdateBarBackgroundColor()
		{
			var bgColor = GetBackgroundColor().CGColor;

			if (_nsToolbarItemViewer?.Superview?.Superview == null ||
				_nsToolbarItemViewer.Superview.Superview.Superview == null) return;
			// NSTitlebarView
			_nsToolbarItemViewer.Superview.Superview.Superview.WantsLayer = true;
			_nsToolbarItemViewer.Superview.Superview.Superview.Layer.BackgroundColor = bgColor;
		}

		void NavigationPagePropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName.Equals(NavigationPage.BarTextColorProperty.PropertyName) ||
				e.PropertyName.Equals(NavigationPage.BarBackgroundColorProperty.PropertyName))
				UpdateToolBar();
		}

		void ToolbarTrackerOnCollectionChanged(object sender, EventArgs eventArgs)
		{
			UpdateToolbarItems();
		}

		async Task NavigateBackFrombackButton()
		{
			var popAsyncInner = _navigation?.PopAsyncInner(true, true);
			if (popAsyncInner != null)
				await popAsyncInner;
		}

		bool ShowBackButton()
		{
			if (_navigation == null)
				return false;

			return NavigationPage.GetHasBackButton(_navigation.CurrentPage) && !IsRootPage();
		}

		bool IsRootPage()
		{
			if (_navigation == null)
				return true;
			return _navigation.StackDepth <= 1;
		}

		NSColor GetBackgroundColor()
		{
			var backgroundNSColor = NSColor.Clear;
			if (Navigation != null && Navigation.BarBackgroundColor != Color.Default)
				backgroundNSColor = Navigation.BarBackgroundColor.ToNSColor();
			return backgroundNSColor;
		}

		NSColor GetTitleColor()
		{
			var titleNSColor = NSColor.Black;
			if (Navigation != null && Navigation?.BarTextColor != Color.Default)
				titleNSColor = Navigation.BarTextColor.ToNSColor();

			return titleNSColor;
		}

		string GetCurrentPageTitle()
		{
			if (_navigation == null)
				return string.Empty;
			return _navigation.Peek(0).Title ?? "";
		}

		string GetPreviousPageTitle()
		{
			if (_navigation == null || _navigation.StackDepth <= 1)
				return string.Empty;

			return _navigation.Peek(1).Title ?? _defaultBackButtonTitle;
		}

		List<ToolbarItem> GetToolbarItems()
		{
			return _toolbarTracker.ToolbarItems.ToList();
		}

		void UpdateTitle()
		{
			if (_toolbar == null || _navigation == null || _titleGroup == null)
				return;

			var title = GetCurrentPageTitle();
			var item = new NSToolbarItem(title);
			var view = new NSView();
			var titleField = new NSTextField
			{
				AllowsEditingTextAttributes = true,
				Bordered = false,
				DrawsBackground = false,
				Bezeled = false,
				Editable = false,
				Selectable = false,
				Cell = new VerticallyCenteredTextFieldCell(0f, NSFont.TitleBarFontOfSize(18)),
				StringValue = title
			};
			titleField.Cell.TextColor = GetTitleColor();
			titleField.SizeToFit();
			_titleGroup.Group.MinSize = new CGSize(NavigationTitleMinSize, ToolbarHeight);
			_titleGroup.Group.Subitems = new NSToolbarItem[] { item };
			view.AddSubview(titleField);
			_titleGroup.Group.View = view;
			//save a reference so we can paint this for the background
			_nsToolbarItemViewer = _titleGroup.Group.View.Superview;
			//position is hard .. we manually set the title to be centered 
			var totalWidth = _titleGroup.Group.View.Superview.Superview.Frame.Width;
			var fieldWidth = titleField.Frame.Width;
			var x = ((totalWidth - fieldWidth) / 2) - _nsToolbarItemViewer.Frame.X;
			titleField.Frame = new CGRect(x, 0, fieldWidth, ToolbarHeight);
		}

		void UpdateToolbarItems()
		{
			if (_toolbar == null || _navigation == null || _toolbarGroup == null)
				return;

			var currentPage = _navigation.Peek(0);
			UpdateGroup(_toolbarGroup, currentPage.ToolbarItems, ToolbarItemWidth, ToolbarItemSpacing);
		}

		void UpdateNavigationItems()
		{
			if (_toolbar == null || _navigation == null || _navigationGroup == null)
				return;
			var items = new List<ToolbarItem>();
			if (ShowBackButton())
			{
				var backButtonItem = new ToolbarItem
				{
					Text = GetPreviousPageTitle(),
					Command = new Command(async () => await NavigateBackFrombackButton())
				};
				items.Add(backButtonItem);
			}

			UpdateGroup(_navigationGroup, items, BackButtonItemWidth, -1);

			var navItemBack = _navigationGroup.Items.FirstOrDefault();
			if (navItemBack != null)
			{
				navItemBack.Button.Image = NSImage.ImageNamed(NSImageName.GoLeftTemplate);
				navItemBack.Button.SizeToFit();
				navItemBack.Button.AccessibilityTitle = "NSBackButton";
			}
		}

		void UpdateTabbedItems()
		{
			if (_toolbar == null || _navigation == null || _tabbedGroup == null)
				return;

			var items = new List<ToolbarItem>();

			var tabbedPage = _navigation.Parent as TabbedPage;
			if (tabbedPage != null)
			{
				foreach (var item in tabbedPage.Children)
				{
					var tbI = new ToolbarItem
					{
						Text = item.Title,
						Icon = item.Icon,
						Command = new Command(() => tabbedPage.SelectedItem = item)
					};
					items.Add(tbI);
				}
			}

			UpdateGroup(_tabbedGroup, items, ToolbarItemWidth, ToolbarItemSpacing);
		}

		void UpdateGroup(NativeToolbarGroup group, IList<ToolbarItem> toolbarItems, double itemWidth,
		   double itemSpacing)
		{
			int count = toolbarItems.Count;
			group.Items.Clear();
			if (count > 0)
			{
				var subItems = new NSToolbarItem[count];
				var view = new NSView();
				nfloat totalWidth = 0;
				var currentX = 0.0;
				for (int i = 0; i < toolbarItems.Count; i++)
				{
					var element = toolbarItems[i];

					var item = new NSToolbarItem(element.Text ?? "");
					item.Activated += (sender, e) => element.Activate();

					var button = new NSButton();
					button.Title = element.Text ?? "";

					button.SizeToFit();
					var buttonWidth = itemWidth;
					if (button.FittingSize.Width > itemWidth)
					{
						buttonWidth = button.FittingSize.Width + 10;
					}
					button.Frame = new CGRect(currentX + i * itemSpacing, 0, buttonWidth, ToolbarItemHeight);
					currentX += buttonWidth;
					totalWidth += button.Frame.Width;
					button.Activated += (sender, e) => element.Activate();

					button.BezelStyle = NSBezelStyle.TexturedRounded;
					if (!string.IsNullOrEmpty(element.Icon))
						button.Image = new NSImage(element.Icon);

					button.SizeToFit();

					button.Enabled = item.Enabled = element.IsEnabled;
					element.PropertyChanged -= ToolBarItemPropertyChanged;
					element.PropertyChanged += ToolBarItemPropertyChanged;

					view.AddSubview(button);
					//item.Label = item.PaletteLabel = item.ToolTip = element.Text ?? "";

					subItems[i] = item;

					group.Items.Add(new NativeToolbarGroup.Item { ToolbarItem = item, Button = button, Element = element });
				}
				view.Frame = new CGRect(0, 0, totalWidth + (itemSpacing * (count - 1)), ToolbarItemHeight);

				group.Group.Subitems = subItems;
				group.Group.View = view;
			}
			else
			{
				group.Group.Subitems = new NSToolbarItem[] { };
				group.Group.View = new NSView();
			}
		}

		void ToolBarItemPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			var nativeToolbarItem = _toolbarGroup.Items.FirstOrDefault((NativeToolbarGroup.Item arg1) => arg1.Element == sender);
			if (nativeToolbarItem != null)
			{
				if (e.PropertyName.Equals(VisualElement.IsEnabledProperty.PropertyName))
				{
					nativeToolbarItem.Button.Enabled = nativeToolbarItem.ToolbarItem.Enabled = nativeToolbarItem.Element.IsEnabled;
				}

				if (e.PropertyName.Equals(ToolbarItem.TextProperty.PropertyName))
				{
					nativeToolbarItem.Button.Title = nativeToolbarItem.ToolbarItem.Label = nativeToolbarItem.Element.Text;
				}
			}
		}

		class ToolBarItemNSButton : NSView
		{
			public ToolBarItemNSButton(string automationID)
			{
				AccessibilityIdentifier = automationID;
			}
		}
	}
}