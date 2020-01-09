using System;
using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen
{
	public class ShellRenderer : VisualElementRenderer<Shell>, IFlyoutController
	{
		INavigationDrawer _native;
		INavigationView _navigationView;
		ShellItemRenderer _shellItem;

		public static readonly Color DefaultBackgroundColor = Color.FromRgb(33, 150, 243);
		public static readonly Color DefaultForegroundColor = Color.White;
		public static readonly Color DefaultTitleColor = Color.White;

		public ShellRenderer()
		{
			RegisterPropertyHandler(Shell.CurrentItemProperty, UpdateCurrentItem);
			RegisterPropertyHandler(Shell.FlyoutBackgroundColorProperty, UpdateFlyoutBackgroundColor);
			RegisterPropertyHandler(Shell.FlyoutBackgroundImageProperty, UpdateFlyoutBackgroundImage);
			RegisterPropertyHandler(Shell.FlyoutBackgroundImageAspectProperty, UpdateFlyoutBackgroundImageAspect);
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
				_native = CreateNavigationDrawer();
				_navigationView = CreateNavigationView();
				_native.NavigationView = _navigationView as ElmSharp.EvasObject;
				_native.Toggled += OnFlyoutIsPresentedChanged;
				SetNativeView(_native as ElmSharp.EvasObject);

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
					_navigationView.SelectedItemChanged -= OnItemSelected;
				}
			}
			base.Dispose(disposing);
		}

		protected void InitializeFlyout()
		{
			((IShellController)Element).StructureChanged += OnShellStructureChanged;

			View flyoutHeader = ((IShellController)Element).FlyoutHeader;
			if (flyoutHeader != null)
			{
				var headerView = Platform.GetOrCreateRenderer(flyoutHeader);
				(headerView as LayoutRenderer)?.RegisterOnLayoutUpdated();

				Size request = flyoutHeader.Measure(Forms.ConvertToScaledDP(_native.NavigationView.MinimumWidth),
													Forms.ConvertToScaledDP(_native.NavigationView.MinimumHeight)).Request;
				headerView.NativeView.MinimumHeight = Forms.ConvertToScaledPixel(request.Height);

				_navigationView.Header = headerView.NativeView;
			}
			_navigationView.BuildMenu(((IShellController)Element).GenerateFlyoutGrouping());
			_navigationView.SelectedItemChanged += OnItemSelected;
		}

		protected void OnFlyoutIsPresentedChanged(object sender, EventArgs e)
		{
			Element.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, _native.IsOpen);
		}

		protected virtual ShellItemRenderer CreateShellItem(ShellItem item)
		{
			return new ShellItemRenderer(this, item);
		}

		protected virtual INavigationDrawer CreateNavigationDrawer()
		{
			return new NavigationDrawer(Forms.NativeParent);
		}

		protected virtual INavigationView CreateNavigationView()
		{
			return new NavigationView(Forms.NativeParent);
		}

		void UpdateCurrentItem()
		{
			_shellItem?.Dispose();
			if (Element.CurrentItem != null)
			{
				_shellItem = CreateShellItem(Element.CurrentItem);
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
			_navigationView.BackgroundColor = Element.FlyoutBackgroundColor.ToNative();
		}

		void UpdateFlyoutBackgroundImageAspect()
		{
			_navigationView.BackgroundImageAspect = Element.FlyoutBackgroundImageAspect;
		}

		void UpdateFlyoutBackgroundImage()
		{
			_navigationView.BackgroundImageSource = Element.FlyoutBackgroundImage;
		}

		void UpdateFlyoutIsPresented()
		{
			_native.IsOpen = Element.FlyoutIsPresented;
		}

		void OnShellStructureChanged(object sender, EventArgs e)
		{
			_navigationView.BuildMenu(((IShellController)Element).GenerateFlyoutGrouping());
		}

		void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			((IShellController)Element).OnFlyoutItemSelected(e.SelectedItem as Element);
		}

		void IFlyoutController.Open()
		{
			_native.IsOpen = true;
		}
	}
}