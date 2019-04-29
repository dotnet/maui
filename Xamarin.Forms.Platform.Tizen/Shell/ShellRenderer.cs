using System;
using System.Collections.Generic;
using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen
{
	public class ShellRenderer : VisualElementRenderer<Shell>, IFlyoutController
	{
		NavigationDrawer _native;
		ShellItemRenderer _shellItem;

		IDictionary<int, Element> _flyoutMenu = new Dictionary<int, Element>();

		public static readonly Color DefaultBackgroundColor = Color.FromRgb(33, 150, 243);
		public static readonly Color DefaultForegroundColor = Color.White;
		public static readonly Color DefaultTitleColor = Color.White;

		public ShellRenderer()
		{
			RegisterPropertyHandler(Shell.CurrentItemProperty, UpdateCurrentItem);
			RegisterPropertyHandler(Shell.FlyoutBackgroundColorProperty, UpdateFlyoutBackgroundColor);
			RegisterPropertyHandler(Shell.FlyoutIsPresentedProperty, UpdateFlyoutIsPresented);
		}

		public override Rect GetNativeContentGeometry()
		{
			var rect = base.GetNativeContentGeometry();
			rect.X = 0;
			rect.Y = 0;
			return rect;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Shell> e)
		{
			if (_native == null)
			{
				_native = new NavigationDrawer(Forms.NativeParent)
				{
					NavigationView = new NavigationView(Forms.NativeParent)
				};
				SetNativeView(_native);

				_native.Toggled += OnFlyoutIsPresentedChanged;

				InitializeFlyout();
			}

			base.OnElementChanged(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				((IShellController)Element).StructureChanged -= OnShellStructureChanged;
				if (_native != null)
				{
					_native.Toggled -= OnFlyoutIsPresentedChanged;
					_native.NavigationView.MenuItemSelected -= OnItemSelected;
				}
			}
			base.Dispose(disposing);
		}

		void InitializeFlyout()
		{
			((IShellController)Element).StructureChanged += OnShellStructureChanged;

			View flyoutHeader = ((IShellController)Element).FlyoutHeader;
			if (flyoutHeader != null)
			{
				var headerView = Platform.GetOrCreateRenderer(flyoutHeader);
				(headerView as LayoutRenderer)?.RegisterOnLayoutUpdated();

				Size request = flyoutHeader.Measure(Forms.ConvertToScaledDP(_native.NavigationView.MinimumWidth), Forms.ConvertToScaledDP(_native.NavigationView.MinimumHeight)).Request;
				headerView.NativeView.MinimumHeight = Forms.ConvertToScaledPixel(request.Height);

				_native.NavigationView.Header = headerView.NativeView;
			}

			BuildMenu();
			_native.NavigationView.MenuItemSelected += OnItemSelected;
		}

		void UpdateCurrentItem()
		{
			_shellItem?.Dispose();
			if (Element.CurrentItem != null)
			{
				_shellItem = new ShellItemRenderer(this, Element.CurrentItem);
				_shellItem.Control.SetAlignment(-1, -1);
				_shellItem.Control.SetWeight(1, 1);
				_native.Main = _shellItem.Control;
			}
			else
			{
				_native.Main = null;
			}
		}

		void UpdateFlyoutBackgroundColor()
		{
			_native.NavigationView.BackgroundColor = Element.FlyoutBackgroundColor.ToNative();
		}

		void UpdateFlyoutIsPresented()
		{
			_native.IsOpen = Element.FlyoutIsPresented;
		}

		void OnFlyoutIsPresentedChanged(object sender, EventArgs e)
		{
			Element.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, _native.IsOpen);
		}

		void OnItemSelected(object sender, GenListItemEventArgs e)
		{
			_flyoutMenu.TryGetValue(e.Item.Index - 1, out Element element);

			if (element != null)
			{
				((IShellController)Element).OnFlyoutItemSelected(element);
			}
		}

		void OnShellStructureChanged(object sender, EventArgs e)
		{
			BuildMenu();
		}

		void BuildMenu()
		{
			var groups = new List<Group>();
			var flyoutGroups = ((IShellController)Element).GenerateFlyoutGrouping();

			_flyoutMenu.Clear();

			int index = 0;
			for (int i = 0; i < flyoutGroups.Count; i++)
			{
				var flyoutGroup = flyoutGroups[i];
				var items = new List<Item>();
				for (int j = 0; j < flyoutGroup.Count; j++)
				{
					string title = null;
					string icon = null;
					if (flyoutGroup[j] is BaseShellItem shellItem)
					{
						title = shellItem.Title;

						if (shellItem.FlyoutIcon is FileImageSource flyoutIcon)
						{
							icon = flyoutIcon.File;
						}
					}
					else if (flyoutGroup[j] is MenuItem menuItem)
					{
						title = menuItem.Text;
						if (menuItem.IconImageSource is FileImageSource source)
						{
							icon = source.File;
						}
					}

					items.Add(new Item(title, icon));

					_flyoutMenu.Add(index, flyoutGroup[j]);
					index++;
				}

				var group = new Group(items);
				groups.Add(group);
			}

			_native.NavigationView.Menu = groups;
		}

		void IFlyoutController.Open()
		{
			_native.IsOpen = true;
		}
	}
}
