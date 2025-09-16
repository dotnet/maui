#nullable disable
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using CoreAnimation;
using CoreGraphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ShellTableViewController : UITableViewController
	{
		readonly IShellContext _context;
		readonly ShellTableViewSource _source;
		bool _isDisposed;
		Action<Element> _onElementSelected;
		IShellController ShellController => _context.Shell;

		public ShellTableViewController(IShellContext context, UIContainerView headerView, Action<Element> onElementSelected) : this(context, onElementSelected)
		{
			ShellFlyoutContentManager = new ShellFlyoutLayoutManager(context);
			HeaderView = headerView;
		}

		public ShellTableViewController(IShellContext context, Action<Element> onElementSelected)
		{
			ShellFlyoutContentManager = ShellFlyoutContentManager ?? new ShellFlyoutLayoutManager(context);
			_context = context;
			_onElementSelected = onElementSelected;
			_source = CreateShellTableViewSource();

			ShellController.FlyoutItemsChanged += OnFlyoutItemsChanged;
			_source.ScrolledEvent += OnScrolled;
		}

		internal ShellFlyoutLayoutManager ShellFlyoutContentManager
		{
			get;
			set;
		}

		void OnScrolled(object sender, UIScrollView e)
		{
			ShellFlyoutContentManager.OnScrolled(e.ContentOffset.Y);
		}

		public virtual UIContainerView HeaderView
		{
			get => ShellFlyoutContentManager.HeaderView;
			set => ShellFlyoutContentManager.HeaderView = value;
		}

		public virtual UIView FooterView
		{
			get => ShellFlyoutContentManager.FooterView;
			set => ShellFlyoutContentManager.FooterView = value;
		}

		protected ShellTableViewSource CreateShellTableViewSource()
		{
			return new ShellTableViewSource(_context, _onElementSelected);
		}

		void OnFlyoutItemsChanged(object sender, EventArgs e)
		{
			_source.ReSyncCache();
#pragma warning disable CS0618 // Type or member is obsolete
			TableView.ReloadData();
#pragma warning restore CS0618 // Type or member is obsolete
			ShellFlyoutContentManager.UpdateVerticalScrollMode();
		}

		public void LayoutParallax() =>
			ShellFlyoutContentManager.LayoutParallax();

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

#pragma warning disable CS0618 // Type or member is obsolete
			TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
#pragma warning restore CS0618 // Type or member is obsolete
			if (OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11)
#if TVOS
				|| OperatingSystem.IsTvOSVersionAtLeast(11)
#endif
			)
			{
#pragma warning disable CS0618 // Type or member is obsolete
				TableView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
#pragma warning restore CS0618 // Type or member is obsolete
			}

			TableView.AccessibilityTraits = UIAccessibilityTrait.None;

			TableView.AccessibilityLabel = "Navigation Flyout Menu";
			TableView.AccessibilityValue = "Expanded";
			TableView.AccessibilityHint = "Swipe right to navigate to menu items.";
			TableView.AccessibilityIdentifier = "FlyoutNavigationMenu";

			TableView.IsAccessibilityElement = false;
			TableView.AccessibilityElementsHidden = false;

			TableView.Source = _source;
#pragma warning restore CS0618 // Type or member is obsolete
			ShellFlyoutContentManager.ViewDidLoad();
		}

		public override void LoadView()
		{
			base.LoadView();
			View = new AccessibilityNeutralTableView();
		}

		internal class AccessibilityNeutralTableView : UITableView,IUIAccessibilityContainer
		{
			public AccessibilityNeutralTableView()
			{
				this.SetAccessibilityContainerType(UIAccessibilityContainerType.None);
			}
		}

		[System.Runtime.Versioning.SupportedOSPlatform("ios11.0")]
		[System.Runtime.Versioning.SupportedOSPlatform("tvos11.0")]
		public override void ViewSafeAreaInsetsDidChange()
		{
			ShellFlyoutContentManager.SetHeaderContentInset();
			base.ViewSafeAreaInsetsDidChange();
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			if (disposing)
			{
				if (ShellController != null)
					ShellController.FlyoutItemsChanged -= OnFlyoutItemsChanged;

				if (_source != null)
					_source.ScrolledEvent -= OnScrolled;

				ShellFlyoutContentManager.TearDown();
				_onElementSelected = null;
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}
	}
}
