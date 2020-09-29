using System;
using System.Collections.Generic;
using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen.Watch
{
	public class ShellRenderer : VisualElementRenderer<Shell>
	{
		NavigationDrawer _drawer;
		NavigationView _navigationView;

		Dictionary<BaseShellItem, IShellItemRenderer> _rendererCache = new Dictionary<BaseShellItem, IShellItemRenderer>();

		public ShellRenderer()
		{
			RegisterPropertyHandler(Shell.CurrentItemProperty, UpdateCurrentItem);
			RegisterPropertyHandler(Shell.FlyoutIsPresentedProperty, UpdateFlyoutIsPresented);
			RegisterPropertyHandler(Shell.FlyoutBehaviorProperty, UpdateFlyoutBehavior);
			RegisterPropertyHandler(Shell.FlyoutIconProperty, UpdateFlyoutIcon);
			RegisterPropertyHandler(Shell.FlyoutBackgroundColorProperty, UpdateFlyoutBackgroundColor);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Shell> e)
		{
			InitializeComponent();
			base.OnElementChanged(e);
		}

		protected override void OnElementReady()
		{
			base.OnElementReady();
			UpdateFlyoutMenu();
			(Element as IShellController).StructureChanged += OnNavigationStructureChanged;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				foreach (var renderer in _rendererCache.Values)
				{
					renderer.Dispose();
				}
				(Element as IShellController).StructureChanged -= OnNavigationStructureChanged;
			}
			base.Dispose(disposing);
		}

		protected virtual NavigationView CreateNavigationView(EvasObject parent)
		{
			return new NavigationView(parent);
		}

		protected virtual NavigationDrawer CreateNavigationDrawer(EvasObject parent)
		{
			return new NavigationDrawer(parent);
		}

		void InitializeComponent()
		{
			if (_drawer == null)
			{
				_drawer = CreateNavigationDrawer(Forms.NativeParent);
				_drawer.IsOpen = Element.FlyoutIsPresented;
				_drawer.Toggled += OnNavigationDrawerToggled;
				SetNativeView(_drawer);
			}
		}

		void OnNavigationStructureChanged(object sender, EventArgs e)
		{
			UpdateFlyoutMenu();
		}

		void UpdateFlyoutMenu()
		{
			if (Element.FlyoutBehavior == FlyoutBehavior.Disabled)
				return;

			var flyoutItems = (Element as IShellController).GenerateFlyoutGrouping();
			int itemCount = 0;
			foreach (var item in flyoutItems)
			{
				itemCount += item.Count;
			}

			if (itemCount > 1)
			{
				InitializeNavigationDrawer();
				_navigationView.Build(flyoutItems);
			}
			else
			{
				DeinitializeNavigationView();
			}
		}

		void InitializeNavigationDrawer()
		{
			if (_navigationView != null)
			{
				return;
			}

			_navigationView = CreateNavigationView(Forms.NativeParent);
			_navigationView.AlignmentX = -1;
			_navigationView.AlignmentY = -1;
			_navigationView.WeightX = 1;
			_navigationView.WeightY = 1;

			_navigationView.Show();
			_navigationView.ItemSelected += OnMenuItemSelected;

			_drawer.SetDrawerContent(_navigationView);
		}

		protected virtual void OnNavigationDrawerToggled(object sender, EventArgs e)
		{
			if (_drawer.IsOpen)
			{
				_navigationView.Activate();
			}
			else
			{
				_navigationView.Deactivate();
			}

			Element.SetValueFromRenderer(Shell.FlyoutIsPresentedProperty, _drawer.IsOpen);
		}

		void DeinitializeNavigationView()
		{
			if (_navigationView == null)
				return;
			_drawer.SetDrawerContent(null);
			_navigationView.Unrealize();
			_navigationView = null;
		}

		void OnMenuItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			((IShellController)Element).OnFlyoutItemSelected(e.SelectedItem as Element);
		}

		void UpdateCurrentItem()
		{
			ResetCurrentItem();
			if (Element.CurrentItem != null)
			{
				if (!_rendererCache.TryGetValue(Element.CurrentItem, out IShellItemRenderer renderer))
				{
					renderer = ShellRendererFactory.Default.CreateItemRenderer(Element.CurrentItem);
					_rendererCache[Element.CurrentItem] = renderer;
				}
				SetCurrentItem(renderer.NativeView);
			}
		}

		void UpdateFlyoutBehavior(bool init)
		{
			if (init)
				return;

			if (Element.FlyoutBehavior == FlyoutBehavior.Disabled)
			{
				DeinitializeNavigationView();
			}
			else if (Element.FlyoutBehavior == FlyoutBehavior.Flyout)
			{
				UpdateFlyoutMenu();
			}
			else if (Element.FlyoutBehavior == FlyoutBehavior.Locked)
			{
				// Locked behavior is not supported on circularshell
			}
		}

		void UpdateFlyoutIcon(bool init)
		{
			if (init && Element.FlyoutIcon == null)
				return;

			_drawer.UpdateDrawerIcon(Element.FlyoutIcon);
		}

		void UpdateFlyoutBackgroundColor(bool init)
		{
			if (init && Element.FlyoutBackgroundColor.IsDefault)
				return;

			if (_navigationView != null)
			{
				_navigationView.BackgroundColor = Element.FlyoutBackgroundColor.ToNative();
			}
		}

		void UpdateFlyoutIsPresented()
		{
			_drawer.IsOpen = Element.FlyoutIsPresented;
		}

		void SetCurrentItem(EvasObject item)
		{
			_drawer.SetMainContent(item);
		}

		void ResetCurrentItem()
		{
			_drawer.SetMainContent(null);
		}
	}
}