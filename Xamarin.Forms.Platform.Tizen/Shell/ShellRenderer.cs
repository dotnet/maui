using System;

namespace Xamarin.Forms.Platform.Tizen
{
	public class ShellRenderer : VisualElementRenderer<Shell>, IFlyoutController
	{
		INavigationDrawer _drawer;
		INavigationView _navigationView;
		ShellItemRenderer _currentShellItem;

		public static readonly Color DefaultBackgroundColor = ThemeConstants.Shell.ColorClass.DefaultBackgroundColor;
		public static readonly Color DefaultForegroundColor = ThemeConstants.Shell.ColorClass.DefaultForegroundColor;
		public static readonly Color DefaultTitleColor = ThemeConstants.Shell.ColorClass.DefaultTitleColor;

		public ShellRenderer()
		{
			RegisterPropertyHandler(Shell.CurrentItemProperty, UpdateCurrentItem);
			RegisterPropertyHandler(Shell.FlyoutBackgroundColorProperty, UpdateFlyoutBackgroundColor);
			RegisterPropertyHandler(Shell.FlyoutBackgroundImageProperty, UpdateFlyoutBackgroundImage);
			RegisterPropertyHandler(Shell.FlyoutBackgroundImageAspectProperty, UpdateFlyoutBackgroundImageAspect);
			RegisterPropertyHandler(Shell.FlyoutIsPresentedProperty, UpdateFlyoutIsPresented);
			RegisterPropertyHandler(Shell.FlyoutHeaderProperty, UpdateFlyoutHeader);
			RegisterPropertyHandler(Shell.FlyoutHeaderTemplateProperty, UpdateFlyoutHeader);
			RegisterPropertyHandler(Shell.FlyoutHeaderBehaviorProperty, UpdateFlyoutHeaderBehavior);
		}

		protected INavigationDrawer NavigationDrawer => _drawer;

		protected override void OnElementChanged(ElementChangedEventArgs<Shell> e)
		{
			if (_drawer == null)
			{
				_drawer = CreateNavigationDrawer();
				_navigationView = CreateNavigationView();
				_drawer.NavigationView = _navigationView.NativeView;
				_drawer.Toggled += OnFlyoutIsPresentedChanged;
				SetNativeView(_drawer.TargetView);

				InitializeFlyout();
			}
			base.OnElementChanged(e);
			UpdateFlyoutHeader(false);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				((IShellController)Element).StructureChanged -= OnShellStructureChanged;
				if (_drawer != null)
				{
					_drawer.Toggled -= OnFlyoutIsPresentedChanged;
					_navigationView.SelectedItemChanged -= OnItemSelected;
				}
			}
			base.Dispose(disposing);
		}

		protected void InitializeFlyout()
		{
			((IShellController)Element).StructureChanged += OnShellStructureChanged;
			_navigationView.BuildMenu(((IShellController)Element).GenerateFlyoutGrouping());
			_navigationView.SelectedItemChanged += OnItemSelected;
		}

		protected void OnFlyoutIsPresentedChanged(object sender, EventArgs e)
		{
			Element.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, _drawer.IsOpen);
		}

		protected virtual ShellItemRenderer CreateShellItemRenderer(ShellItem item)
		{
			return new ShellItemRenderer(item);
		}

		protected virtual INavigationDrawer CreateNavigationDrawer()
		{
			return new NavigationDrawer(Forms.NativeParent);
		}

		protected virtual INavigationView CreateNavigationView()
		{
			return new NavigationView(Forms.NativeParent, Element);
		}

		void UpdateFlyoutHeader(bool init)
		{
			if (init)
				return;

			if ((Element as IShellController).FlyoutHeader != null)
			{
				_navigationView.Header = (Element as IShellController).FlyoutHeader;
			}
			else
			{
				_navigationView.Header = null;
			}
		}

		void UpdateFlyoutHeaderBehavior()
		{
			_navigationView.HeaderBehavior = Element.FlyoutHeaderBehavior;
		}

		void UpdateCurrentItem()
		{
			_currentShellItem?.Dispose();
			if (Element.CurrentItem != null)
			{
				_currentShellItem = CreateShellItemRenderer(Element.CurrentItem);
				_drawer.Main = _currentShellItem.NativeView;
			}
			else
			{
				_drawer.Main = null;
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

		protected virtual void UpdateFlyoutIsPresented()
		{
			// It is workaround of Panel.IsOpen bug, Panel.IsOpen property is not working when layouting was triggered
			Device.BeginInvokeOnMainThread(() =>
			{
				_drawer.IsOpen = Element.FlyoutIsPresented;
			});
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
			_drawer.IsOpen = true;
		}
	}
}