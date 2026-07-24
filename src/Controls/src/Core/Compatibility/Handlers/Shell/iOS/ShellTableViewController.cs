#nullable disable
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using CoreAnimation;
using CoreGraphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public class ShellTableViewController : UITableViewController
	{
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Shell context is required for the table controller lifetime and event subscriptions are removed in Dispose(bool).")]
		readonly IShellContext _context;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Table source is owned by the controller and detached from events in Dispose(bool).")]
		readonly ShellTableViewSource _source;
		bool _isDisposed;
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Selection callback is cleared in Dispose(bool) when the controller is released.")]
		Action<Element> _onElementSelected;
		IShellController ShellController => _context.Shell;

		public ShellTableViewController(IShellContext context, UIContainerView headerView, Action<Element> onElementSelected) : this(context, onElementSelected)
		{
			ShellFlyoutContentManager = new ShellFlyoutLayoutManager(context);
			HeaderView = headerView;
		}

		[UnconditionalSuppressMessage("Memory", "MEM0003", Justification = "FlyoutItemsChanged and ScrolledEvent subscriptions are removed in Dispose(bool).")]
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

#pragma warning disable CS0618 // Type or member is obsolete
			TableView.Source = _source;
#pragma warning restore CS0618 // Type or member is obsolete
			ShellFlyoutContentManager.ViewDidLoad();
		}

		public override void LoadView()
		{
			base.LoadView();
			View = new AccessibilityNeutralTableView();
		}

		internal class AccessibilityNeutralTableView : UITableView, IUIAccessibilityContainer
		{
			public AccessibilityNeutralTableView()
			{
				this.SetAccessibilityContainerType(UIAccessibilityContainerType.None);
				ScrollsToTop = false;
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
				{
					_source.ScrolledEvent -= OnScrolled;
					_source.Disconnect();
				}

				ShellFlyoutContentManager.TearDown();
				_onElementSelected = null;
			}

			_isDisposed = true;
			base.Dispose(disposing);
		}
	}
}
