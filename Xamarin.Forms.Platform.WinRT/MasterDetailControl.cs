using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace Xamarin.Forms.Platform.WinRT
{
	public class MasterDetailControl : Control
	{
		public static readonly DependencyProperty MasterContentProperty = DependencyProperty.Register("MasterContent", typeof(object), typeof(MasterDetailControl),
			new PropertyMetadata(null, (d, e) => ((MasterDetailControl)d).UpdateMaster()));

		public static readonly DependencyProperty DetailContentProperty = DependencyProperty.Register("DetailContent", typeof(object), typeof(MasterDetailControl),
			new PropertyMetadata(null, (d, e) => ((MasterDetailControl)d).UpdateDetail()));

		public static readonly DependencyProperty IsMasterVisibleProperty = DependencyProperty.Register("IsMasterVisible", typeof(bool), typeof(MasterDetailControl),
			new PropertyMetadata(false, (d, e) => ((MasterDetailControl)d).UpdateMaster()));

		public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(MasterDetailControl),
			new PropertyMetadata("", (d, e) => ((MasterDetailControl)d).UpdateTitle()));

		public static readonly DependencyProperty DetailTitleVisibilityProperty = DependencyProperty.Register("DetailTitleVisibility", typeof(Visibility), typeof(MasterDetailControl),
			new PropertyMetadata(Visibility.Collapsed, (d, e) => ((MasterDetailControl)d).UpdateToolbar()));

		public static readonly DependencyProperty ToolbarForegroundProperty = DependencyProperty.Register("ToolbarForeground", typeof(Brush), typeof(MasterDetailControl),
			new PropertyMetadata(default(Brush), (d, e) => ((MasterDetailControl)d).UpdateToolbar()));

		public static readonly DependencyProperty ToolbarBackgroundProperty = DependencyProperty.Register("ToolbarBackground", typeof(Brush), typeof(MasterDetailControl),
			new PropertyMetadata(default(Brush), (d, e) => ((MasterDetailControl)d).UpdateToolbar()));

		Windows.UI.Xaml.Controls.Grid _grd;

		Windows.UI.Xaml.Controls.ContentPresenter _masterPresenter;
		Windows.UI.Xaml.Controls.ContentPresenter _detailPresenter;
		Popup _popup;
		TextBlock _txbTitle;

		public object DetailContent
		{
			get { return GetValue(DetailContentProperty); }
			set { SetValue(DetailContentProperty, value); }
		}

		public Visibility DetailTitleVisibility
		{
			get { return (Visibility)GetValue(DetailTitleVisibilityProperty); }
			set { SetValue(DetailTitleVisibilityProperty, value); }
		}

		public bool IsMasterVisible
		{
			get { return (bool)GetValue(IsMasterVisibleProperty); }
			set { SetValue(IsMasterVisibleProperty, value); }
		}

		public object MasterContent
		{
			get { return GetValue(MasterContentProperty); }
			set { SetValue(MasterContentProperty, value); }
		}

		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}

		public Brush ToolbarBackground
		{
			get { return (Brush)GetValue(ToolbarBackgroundProperty); }
			set { SetValue(ToolbarBackgroundProperty, value); }
		}

		public Brush ToolbarForeground
		{
			get { return (Brush)GetValue(ToolbarForegroundProperty); }
			set { SetValue(ToolbarForegroundProperty, value); }
		}

		public event EventHandler UserClosedPopover;

		protected override void OnApplyTemplate()
		{
			if (_masterPresenter != null)
			{
				// Despite itself being unparented when the template changes, the presenters' children still think they're
				// parented and so the new presenters throw when the content is assigned to them.
				_masterPresenter.Content = null;
				_masterPresenter = null;
			}

			if (_detailPresenter != null)
			{
				_detailPresenter.Content = null;
				_detailPresenter = null;
			}

			if (_popup != null)
			{
				_popup.Closed -= OnPopupClosed;
				_popup = null;
			}

			base.OnApplyTemplate();

			_masterPresenter = GetTemplateChild("masterPresenter") as Windows.UI.Xaml.Controls.ContentPresenter;
			_detailPresenter = GetTemplateChild("detailPresenter") as Windows.UI.Xaml.Controls.ContentPresenter;
			_txbTitle = GetTemplateChild("txbTitle") as TextBlock;
			_grd = GetTemplateChild("grdToolbar") as Windows.UI.Xaml.Controls.Grid;

			_popup = GetTemplateChild("popup") as Popup;
			if (_popup != null)
				_popup.Closed += OnPopupClosed;

			UpdateMaster();
			UpdateDetail();
			UpdateTitle();
			UpdateToolbar();
		}

		void OnPopupClosed(object sender, object e)
		{
			EventHandler closed = UserClosedPopover;
			if (closed != null)
				closed(this, EventArgs.Empty);
		}

		void UpdateDetail()
		{
			if (_detailPresenter == null)
				return;

			_detailPresenter.Content = DetailContent;
		}

		void UpdateMaster()
		{
			if (_masterPresenter == null)
				return;

			bool visible = IsMasterVisible;
			_masterPresenter.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
			_masterPresenter.Content = MasterContent;

			if (_popup != null)
				_popup.IsOpen = visible;
		}

		void UpdateTitle()
		{
			if (_txbTitle == null)
				return;

			_txbTitle.Text = Title ?? "";
		}

		void UpdateToolbar()
		{
			if (_txbTitle == null)
				return;

			_grd.Visibility = DetailTitleVisibility;
			_grd.Background = ToolbarBackground;
			_txbTitle.Foreground = ToolbarForeground;
		}
	}
}