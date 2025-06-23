#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomNavigation;
using Google.Android.Material.BottomSheet;
using Google.Android.Material.Navigation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Graphics;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;
using IMenu = Android.Views.IMenu;
using LP = Android.Views.ViewGroup.LayoutParams;
using Orientation = Android.Widget.Orientation;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ShellItemRenderer : ShellItemRendererBase, NavigationBarView.IOnItemSelectedListener, IAppearanceObserver
	{
		#region IOnItemSelectedListener

		bool NavigationBarView.IOnItemSelectedListener.OnNavigationItemSelected(IMenuItem item)
		{
			return OnItemSelected(item);
		}

		#endregion IOnItemSelectedListener

		#region IAppearanceObserver

		void IAppearanceObserver.OnAppearanceChanged(ShellAppearance appearance)
		{
			_shellAppearance = appearance;

			if (appearance is not null)
				SetAppearance(appearance);
			else
				ResetAppearance();
		}

		#endregion IAppearanceObserver

		protected const int MoreTabId = 99;
		BottomNavigationView _bottomView;
		FrameLayout _navigationArea;
		LinearLayout _outerLayout;
		IShellBottomNavViewAppearanceTracker _appearanceTracker;
		BottomNavigationViewTracker _bottomNavigationTracker;
		BottomSheetDialog _bottomSheetDialog;
		bool _menuSetup;
		ShellAppearance _shellAppearance;
		bool _appearanceSet;
		public IShellItemController ShellItemController => ShellItem;
		IMauiContext MauiContext => ShellContext.Shell.Handler.MauiContext;
		IMenuItem _updateMenuItemTitle;
		IMenuItem _updateMenuItemIcon;
		ShellSection _updateMenuItemSource;

		public ShellItemRenderer(IShellContext shellContext) : base(shellContext)
		{
		}

		public override AView OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView(inflater, container, savedInstanceState);

			var context = MauiContext.Context;
			_outerLayout = PlatformInterop.CreateNavigationBarOuterLayout(context);
			_navigationArea = PlatformInterop.CreateNavigationBarArea(context, _outerLayout);
			_bottomView = PlatformInterop.CreateNavigationBar(context, Resource.Attribute.bottomNavigationViewStyle, _outerLayout, this);

			if (ShellItem is null)
				throw new InvalidOperationException("Active Shell Item not set. Have you added any Shell Items to your Shell?");

			if (ShellItem.CurrentItem is null)
				throw new InvalidOperationException($"Content not found for active {ShellItem}. Title: {ShellItem.Title}. Route: {ShellItem.Route}.");

			HookEvents(ShellItem);
			SetupMenu();

			_appearanceTracker = ShellContext.CreateBottomNavViewAppearanceTracker(ShellItem);
			_bottomNavigationTracker = new BottomNavigationViewTracker();
			((IShellController)ShellContext.Shell).AddAppearanceObserver(this, ShellItem);

			return _outerLayout;
		}


		void Destroy()
		{
			if (ShellItem is not null)
				UnhookEvents(ShellItem);

			((IShellController)ShellContext?.Shell)?.RemoveAppearanceObserver(this);

			if (_bottomSheetDialog is not null)
			{
				_bottomSheetDialog.DismissEvent -= OnMoreSheetDismissed;
				_bottomSheetDialog?.Dispose();
				_bottomSheetDialog = null;
			}

			_navigationArea?.Dispose();
			_appearanceTracker?.Dispose();
			_outerLayout?.Dispose();

			if (_bottomView is not null)
			{
				_bottomView?.SetOnItemSelectedListener(null);
				_bottomView?.Background?.Dispose();
				_bottomView?.Dispose();
			}

			_bottomView = null;
			_navigationArea = null;
			_appearanceTracker = null;
			_outerLayout = null;

		}

		// Use OnDestory become OnDestroyView may fire before events are completed.
		public override void OnDestroy()
		{
			Destroy();
			base.OnDestroy();
		}

		protected virtual void SetAppearance(ShellAppearance appearance)
		{
			if (_bottomView is null ||
				_bottomView.Visibility == ViewStates.Gone ||
				DisplayedPage is null)
			{
				return;
			}

			_appearanceSet = true;
			_appearanceTracker.SetAppearance(_bottomView, appearance);
		}

		protected virtual bool ChangeSection(ShellSection shellSection)
		{
			return ((IShellItemController)ShellItem).ProposeSection(shellSection);
		}

		protected virtual Drawable CreateItemBackgroundDrawable()
		{
			return BottomNavigationViewUtils.CreateItemBackgroundDrawable();
		}

		[Obsolete("Use CreateMoreBottomSheet(Action<int, BottomSheetDialog> selectCallback)")]
		protected virtual BottomSheetDialog CreateMoreBottomSheet(Action<ShellSection, BottomSheetDialog> selectCallback)
		{
			return CreateMoreBottomSheet((int index, BottomSheetDialog dialog) =>
			{
				selectCallback(ShellItemController.GetItems()[index], dialog);
			});
		}

		protected virtual BottomSheetDialog CreateMoreBottomSheet(Action<int, BottomSheetDialog> selectCallback)
		{
			var bottomSheetDialog = new BottomSheetDialog(Context);
			var bottomSheetLayout = new LinearLayout(Context);
			using (var bottomShellLP = new LP(LP.MatchParent, LP.WrapContent))
				bottomSheetLayout.LayoutParameters = bottomShellLP;
			bottomSheetLayout.Orientation = Orientation.Vertical;

			// handle the more tab
			var items = ((IShellItemController)ShellItem).GetItems();
			for (int i = _bottomView.MaxItemCount - 1; i < items.Count; i++)
			{
				var closure_i = i;
				var shellContent = items[i];

				using (var innerLayout = new LinearLayout(Context))
				{
					innerLayout.SetClipToOutline(true);
					innerLayout.SetBackground(CreateItemBackgroundDrawable());
					innerLayout.SetPadding(0, (int)Context.ToPixels(6), 0, (int)Context.ToPixels(6));
					innerLayout.Orientation = Orientation.Horizontal;
					using (var param = new LP(LP.MatchParent, LP.WrapContent))
						innerLayout.LayoutParameters = param;

					// technically the unhook isn't needed
					// we dont even unhook the events that dont fire
					void clickCallback(object s, EventArgs e)
					{
						selectCallback(closure_i, bottomSheetDialog);
						if (!innerLayout.IsDisposed())
							innerLayout.Click -= clickCallback;
					}

					innerLayout.Click += clickCallback;

					var image = new ImageView(Context);
					var lp = new LinearLayout.LayoutParams((int)Context.ToPixels(32), (int)Context.ToPixels(32))
					{
						LeftMargin = (int)Context.ToPixels(20),
						RightMargin = (int)Context.ToPixels(20),
						TopMargin = (int)Context.ToPixels(6),
						BottomMargin = (int)Context.ToPixels(6),
						Gravity = GravityFlags.Center
					};
					image.LayoutParameters = lp;
					lp.Dispose();

					var services = MauiContext.Services;
					var provider = services.GetRequiredService<IImageSourceServiceProvider>();
					var icon = shellContent.Icon;

					shellContent.Icon.LoadImage(
						MauiContext,
						(result) =>
						{
							image.SetImageDrawable(result?.Value);
							if (result?.Value is not null)
							{
								var color = Colors.Black.MultiplyAlpha(0.6f).ToPlatform();
								result.Value.SetTint(color);
							}
						});

					innerLayout.AddView(image);

					using (var text = new TextView(Context))
					{
						text.Typeface = services.GetRequiredService<IFontManager>()
							.GetTypeface(Font.OfSize("sans-serif-medium", 0.0));

						text.SetTextColor(AColor.Black);
						text.Text = shellContent.Title;
						lp = new LinearLayout.LayoutParams(0, LP.WrapContent)
						{
							Gravity = GravityFlags.Center,
							Weight = 1
						};
						text.LayoutParameters = lp;
						lp.Dispose();

						innerLayout.AddView(text);
					}

					bottomSheetLayout.AddView(innerLayout);
				}
			}

			bottomSheetDialog.SetContentView(bottomSheetLayout);
			bottomSheetLayout.Dispose();

			return bottomSheetDialog;
		}

		protected override ViewGroup GetNavigationTarget() => _navigationArea;

		protected override void OnShellSectionChanged()
		{
			base.OnShellSectionChanged();

			var index = ((IShellItemController)ShellItem).GetItems().IndexOf(ShellSection);
			using (var menu = _bottomView.Menu)
			{
				index = Math.Min(index, menu.Size() - 1);
				if (index < 0)
					return;
				using (var menuItem = menu.GetItem(index))
					menuItem.SetChecked(true);
			}
		}

		protected override void OnDisplayedPageChanged(Page newPage, Page oldPage)
		{
			base.OnDisplayedPageChanged(newPage, oldPage);

			if (oldPage is not null)
				oldPage.PropertyChanged -= OnDisplayedElementPropertyChanged;

			if (newPage is not null)
				newPage.PropertyChanged += OnDisplayedElementPropertyChanged;

			if (newPage is not null && !_menuSetup)
			{
				SetupMenu();
			}

			UpdateTabBarVisibility();
		}

		protected virtual bool OnItemSelected(IMenuItem item)
		{
			var id = item.ItemId;
			if (id == MoreTabId)
			{
				var items = CreateTabList(ShellItem);
				_bottomSheetDialog = BottomNavigationViewUtils.CreateMoreBottomSheet(OnMoreItemSelected, MauiContext, items, _bottomView.MaxItemCount);
				_bottomSheetDialog.Show();
				_bottomSheetDialog.DismissEvent += OnMoreSheetDismissed;
			}
			else
			{
				var shellSection = ((IShellItemController)ShellItem).GetItems()[id];
				if (item.IsChecked)
				{
					OnTabReselected(shellSection);
				}
				else
				{
					return ChangeSection(shellSection);
				}
			}

			return true;
		}

		void OnMoreItemSelected(int shellSectionIndex, BottomSheetDialog dialog)
		{
			OnMoreItemSelected(ShellItemController.GetItems()[shellSectionIndex], dialog);
		}

		protected virtual void OnMoreItemSelected(ShellSection shellSection, BottomSheetDialog dialog)
		{
			ChangeSection(shellSection);

			dialog.Dismiss(); //should trigger OnMoreSheetDismissed, which will clean up the dialog
			if (dialog != _bottomSheetDialog) //should never be true, but just in case, prevent a leak
				dialog.Dispose();
		}

		List<(string title, ImageSource icon, bool tabEnabled)> CreateTabList(ShellItem shellItem)
		{
			var items = new List<(string title, ImageSource icon, bool tabEnabled)>();
			var shellItems = ((IShellItemController)shellItem).GetItems();

			for (int i = 0; i < shellItems.Count; i++)
			{
				var item = shellItems[i];
				items.Add((item.Title, item.Icon, item.IsEnabled));
			}
			return items;
		}

		protected virtual void OnMoreSheetDismissed(object sender, EventArgs e)
		{
			OnShellSectionChanged();

			if (_bottomSheetDialog is not null)
			{
				_bottomSheetDialog.DismissEvent -= OnMoreSheetDismissed;
				_bottomSheetDialog.Dispose();
				_bottomSheetDialog = null;
			}
		}

		protected override void OnShellItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			base.OnShellItemsChanged(sender, e);

			SetupMenu();
		}

		protected override void OnShellSectionPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnShellSectionPropertyChanged(sender, e);

			if (e.IsOneOf(BaseShellItem.TitleProperty, BaseShellItem.IconProperty, BaseShellItem.IsEnabledProperty))
			{
				var shellSection = (ShellSection)sender;
				var index = ((IShellItemController)ShellItem).GetItems().IndexOf(shellSection);

				var itemCount = ((IShellItemController)ShellItem).GetItems().Count;
				var maxItems = _bottomView.MaxItemCount;
				IMenuItem menuItem = null;

				if (!(itemCount > maxItems && index > maxItems - 2))
				{
					menuItem = _bottomView.Menu.FindItem(index);
				}

				if (menuItem is not null)
				{
					if (e.Is(BaseShellItem.IsEnabledProperty))
					{
						UpdateShellSectionEnabled(shellSection, menuItem);
					}
					else if (e.Is(BaseShellItem.IconProperty))
					{
						_updateMenuItemIcon = menuItem;
					}
					else if (e.Is(BaseShellItem.TitleProperty))
					{
						_updateMenuItemTitle = menuItem;
					}
				}

				// This is primarily used so that `SetupMenu` is still called when the
				// title and icon property change calls happen. We don't want to break users
				// that are dependent on that behavior
				if (e.IsOneOf(BaseShellItem.IconProperty, BaseShellItem.TitleProperty))
				{
					try
					{
						_updateMenuItemSource = shellSection;
						SetupMenu();
					}
					finally
					{
						_updateMenuItemIcon = null;
						_updateMenuItemTitle = null;
						_updateMenuItemSource = null;
					}
				}
			}
		}

		protected virtual void OnTabReselected(ShellSection shellSection)
		{
		}

		protected virtual void ResetAppearance() => _appearanceTracker.ResetAppearance(_bottomView);

		protected virtual void SetupMenu(IMenu menu, int maxBottomItems, ShellItem shellItem)
		{
			if ((_updateMenuItemIcon is not null || _updateMenuItemTitle is not null) &&
				_menuSetup &&
				_updateMenuItemSource is not null &&
				_bottomView.IsAlive() &&
				_bottomView.IsAttachedToWindow)
			{
				if (_updateMenuItemIcon is not null)
				{
					var menuItem = _updateMenuItemIcon;
					var shellSection = _updateMenuItemSource;
					_updateMenuItemSource = null;
					_updateMenuItemIcon = null;

					UpdateShellSectionIcon(shellSection, menuItem);
					return;
				}
				else if (_updateMenuItemTitle is not null)
				{
					var menuItem = _updateMenuItemTitle;
					var shellSection = _updateMenuItemSource;
					_updateMenuItemSource = null;
					_updateMenuItemIcon = null;

					UpdateShellSectionTitle(shellSection, menuItem);
					return;
				}
			}

			if (DisplayedPage is null)
				return;

			if (ShellItemController.ShowTabs)
			{
				_menuSetup = true;
				var currentIndex = ((IShellItemController)ShellItem).GetItems().IndexOf(ShellSection);
				var items = CreateTabList(shellItem);

				BottomNavigationViewUtils.SetupMenu(
					menu,
					maxBottomItems,
					items,
					currentIndex,
					_bottomView,
					MauiContext);
			}

			UpdateTabBarVisibility();
		}

		protected virtual void UpdateShellSectionIcon(ShellSection shellSection, IMenuItem menuItem)
		{
			BottomNavigationViewUtils.SetMenuItemIcon(menuItem, shellSection.Icon, MauiContext)
				.FireAndForget(e => MauiContext?.CreateLogger<ShellItemRenderer>()?
						.LogWarning(e, "Failed to Update Shell Section Icon"));
		}

		protected virtual void UpdateShellSectionTitle(ShellSection shellSection, IMenuItem menuItem)
		{
			BottomNavigationViewUtils.SetMenuItemTitle(menuItem, shellSection.Title);
		}

		protected virtual void UpdateShellSectionEnabled(ShellSection shellSection, IMenuItem menuItem)
		{
			bool tabEnabled = shellSection.IsEnabled;
			if (menuItem.IsEnabled != tabEnabled)
				menuItem.SetEnabled(tabEnabled);
		}

		void OnDisplayedElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Shell.TabBarIsVisibleProperty.PropertyName)
			{
				if (!_menuSetup)
					SetupMenu();

				UpdateTabBarVisibility();
			}
		}

		void SetupMenu()
		{
			using (var menu = _bottomView.Menu)
				SetupMenu(menu, _bottomView.MaxItemCount, ShellItem);
		}

		protected virtual void UpdateTabBarVisibility()
		{
			if (DisplayedPage is null)
				return;

			_bottomView.Visibility = ShellItemController.ShowTabs ? ViewStates.Visible : ViewStates.Gone;

			if (_shellAppearance is not null && !_appearanceSet)
			{
				SetAppearance(_shellAppearance);
			}
		}
	}
}