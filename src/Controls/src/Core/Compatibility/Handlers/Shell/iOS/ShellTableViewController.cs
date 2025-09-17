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
			TableView.ReloadData();
			ShellFlyoutContentManager.UpdateVerticalScrollMode();
		}

		public void LayoutParallax() =>
			ShellFlyoutContentManager.LayoutParallax();

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			if (OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11)
#if TVOS
				|| OperatingSystem.IsTvOSVersionAtLeast(11)
#endif
			)
			{
				TableView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
			}

			TableView.Source = _source;
			ShellFlyoutContentManager.ViewDidLoad();
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
