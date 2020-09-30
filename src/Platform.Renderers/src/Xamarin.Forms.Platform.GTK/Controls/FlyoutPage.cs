using System;
using System.Windows.Markup;
using Gdk;
using Gtk;
using Xamarin.Forms.Platform.GTK.Animations;
using Xamarin.Forms.Platform.GTK.Extensions;

namespace Xamarin.Forms.Platform.GTK.Controls
{
	public enum FlyoutLayoutBehaviorType
	{
		Default = 0,
		Popover,
		Split
	}

	public class FlyoutPage : Fixed
	{
		private const int DefaultFlyoutWidth = 300;
		private const int IsPresentedAnimationMilliseconds = 300;

		private Gdk.Rectangle _lastAllocation;
		private bool _isPresented;
		private FlyoutPageFlyoutTitleContainer _titleContainer;
		private EventBox _flyoutContainerWrapper;
		private VBox _flyoutContainer;
		private Widget _flyout;
		private Widget _detail;
		private FlyoutLayoutBehaviorType _flyoutBehaviorType;
		private static Pixbuf _hamburgerPixBuf;
		private bool _displayTitle;
		private bool _animationsEnabled;

		public FlyoutPage()
		{
			_animationsEnabled = false;
			_flyoutBehaviorType = FlyoutLayoutBehaviorType.Default;

			// Flyout Stuff
			_flyoutContainerWrapper = new EventBox();
			_flyoutContainer = new VBox(false, 0);
			_titleContainer = new FlyoutPageFlyoutTitleContainer();
			_titleContainer.HamburguerClicked += OnHamburgerClicked;
			_titleContainer.HeightRequest = GtkToolbarConstants.ToolbarHeight;
			_flyoutContainer.PackStart(_titleContainer, false, true, 0);

			_flyout = new EventBox();
			_flyoutContainer.PackEnd(_flyout, false, true, 0);
			_flyoutContainerWrapper.Add(_flyoutContainer);

			// Detail Stuff
			_detail = new EventBox();

			Add(_detail);
			Add(_flyoutContainerWrapper);
		}

		public FlyoutLayoutBehaviorType FlyoutLayoutBehaviorType
		{
			get
			{
				return _flyoutBehaviorType;
			}

			set
			{
				if (_flyoutBehaviorType != value)
				{
					_flyoutBehaviorType = value;
					RefreshFlyoutLayoutBehavior(_flyoutBehaviorType);
				}
			}
		}

		public Widget Flyout
		{
			get
			{
				return _flyout;
			}

			set
			{
				RefreshFlyout(value);
			}
		}

		public Widget Detail
		{
			get
			{
				return _detail;
			}

			set
			{
				RefreshDetail(value);
			}
		}

		public bool IsPresented
		{
			get
			{
				return _isPresented;
			}

			set
			{
				RefreshPresented(value);
				NotifyIsPresentedChanged();
			}
		}

		public string FlyoutTitle
		{
			get
			{
				return _titleContainer.Title;
			}

			set
			{
				_titleContainer.Title = value ?? string.Empty;
			}
		}

		public bool DisplayTitle
		{
			get
			{
				return _displayTitle;
			}

			set
			{
				RefreshDisplayTitle(value);
			}
		}

		public static Pixbuf HamburgerPixBuf
		{
			get
			{
				try
				{
					if (_hamburgerPixBuf == null)
					{
						_hamburgerPixBuf = new Pixbuf("./Resources/hamburger.png");
					}

					return _hamburgerPixBuf;
				}
				catch
				{
					return null;
				}
			}
			set
			{
				_hamburgerPixBuf = value;
			}
		}

		public void UpdateBarTextColor(Gdk.Color? barTextColor)
		{
			if (_titleContainer != null)
			{
				_titleContainer.UpdateTitleColor(barTextColor);
			}
		}

		public void UpdateBarBackgroundColor(Gdk.Color? barBackgroundColor)
		{
			if (_titleContainer != null)
			{
				_titleContainer.UpdateBackgroundColor(barBackgroundColor);
			}
		}

		public void UpdateHamburguerIcon(Pixbuf hamburguerIcon)
		{
			HamburgerPixBuf = hamburguerIcon;

			if (_titleContainer != null)
			{
				_titleContainer.HamburgerPixBuf = HamburgerPixBuf;
			}
		}

		public event EventHandler IsPresentedChanged;

		protected override void OnSizeAllocated(Gdk.Rectangle allocation)
		{
			base.OnSizeAllocated(allocation);

			if (_lastAllocation != allocation)
			{
				_lastAllocation = allocation;
			}

			_flyout.WidthRequest = DefaultFlyoutWidth;
			_flyout.HeightRequest = _detail.HeightRequest = allocation.Height;
			RefreshFlyoutLayoutBehavior(_flyoutBehaviorType);
		}

		protected override void OnShown()
		{
			base.OnShown();

			_animationsEnabled = true;
		}


		private void RefreshFlyoutLayoutBehavior(FlyoutLayoutBehaviorType flyoutBehaviorType)
		{
			int detailWidthRequest = 0;
			Gdk.Point point = default(Gdk.Point);

			switch (_flyoutBehaviorType)
			{
				case FlyoutLayoutBehaviorType.Split:
					detailWidthRequest = _lastAllocation.Width - DefaultFlyoutWidth;
					point = new Gdk.Point(_flyout.WidthRequest, 0);
					break;
				case FlyoutLayoutBehaviorType.Default:
				case FlyoutLayoutBehaviorType.Popover:
					detailWidthRequest = _lastAllocation.Width;
					point = new Gdk.Point(0, 0);
					break;
			}

			if (detailWidthRequest >= 0)
			{
				_detail.WidthRequest = detailWidthRequest;
				_detail.MoveTo(point.X, point.Y);
			}
		}

		private void RefreshFlyout(Widget newFlyout)
		{
			if (_flyout != null)
			{
				_flyoutContainer.RemoveFromContainer(_flyout);
			}

			UpdateHamburguerIcon(HamburgerPixBuf);
			_flyout = newFlyout;
			_flyoutContainer.PackEnd(newFlyout, false, true, 0);
			_flyout.ShowAll();
		}

		private void RefreshDetail(Widget newDetail)
		{
			if (_detail != null)
			{
				this.RemoveFromContainer(_detail);
			}

			_detail = newDetail;

			Add(_detail);

			Remove(_flyoutContainerWrapper);
			Add(_flyoutContainerWrapper);

			_detail.ShowAll();
			_flyoutContainerWrapper.GdkWindow?.Raise(); // Forcing Flyout to be on top
		}

		private async void RefreshPresented(bool isPresented)
		{
			_isPresented = isPresented;

			if (_flyoutBehaviorType == FlyoutLayoutBehaviorType.Split)
				return;

			if (_animationsEnabled)
			{
				var from = (_isPresented) ? -DefaultFlyoutWidth : 0;
				var to = (_isPresented) ? 0 : -DefaultFlyoutWidth;

				await new FloatAnimation(from, to, TimeSpan.FromMilliseconds(IsPresentedAnimationMilliseconds), true, (f) =>
				{
					Gtk.Application.Invoke(delegate
					{
						_flyoutContainerWrapper.MoveTo(f, 0);
					});
				}).Run();
			}
			else
			{
				_flyoutContainerWrapper.MoveTo(_isPresented ? 0 : -DefaultFlyoutWidth, 0);
			}
		}

		private void RefreshDisplayTitle(bool value = true)
		{
			_displayTitle = value;

			_flyoutContainer.RemoveFromContainer(_titleContainer);

			if (_displayTitle)
			{
				_flyoutContainer.PackStart(_titleContainer, false, true, 0);
			}
		}

		private void OnHamburgerClicked(object sender, EventArgs e)
		{
			IsPresented = !IsPresented;
		}

		private void NotifyIsPresentedChanged()
		{
			IsPresentedChanged?.Invoke(this, EventArgs.Empty);
		}

		private class FlyoutPageFlyoutTitleContainer : EventBox
		{
			private HBox _root;
			private ToolButton _hamburguerButton;
			private Gtk.Label _titleLabel;
			private Gtk.Image _hamburguerIcon;
			private Gdk.Color _defaultTextColor;
			private Gdk.Color _defaultBackgroundColor;

			public FlyoutPageFlyoutTitleContainer()
			{
				_defaultBackgroundColor = Style.Backgrounds[(int)StateType.Normal];

				_root = new HBox();
				_hamburguerIcon = new Gtk.Image();

				try
				{
					_hamburguerIcon = new Gtk.Image(HamburgerPixBuf);
				}
				catch (Exception ex)
				{
					Internals.Log.Warning("FlyoutPage HamburguerIcon", "Could not load hamburguer icon: {0}", ex);
				}

				_hamburguerButton = new ToolButton(_hamburguerIcon, string.Empty);
				_hamburguerButton.HeightRequest = GtkToolbarConstants.ToolbarItemHeight;
				_hamburguerButton.WidthRequest = GtkToolbarConstants.ToolbarItemWidth;
				_hamburguerButton.Clicked += OnHamburguerButtonClicked;

				_titleLabel = new Gtk.Label();
				_defaultTextColor = _titleLabel.Style.Foregrounds[(int)StateType.Normal];

				_root.PackStart(_hamburguerButton, false, false, GtkToolbarConstants.ToolbarItemSpacing);
				_root.PackStart(_titleLabel, false, false, 25);

				Add(_root);
			}

			public string Title
			{
				get
				{
					return _titleLabel.Text;
				}

				set
				{
					_titleLabel.Text = value ?? string.Empty;
				}
			}

			public Pixbuf HamburgerPixBuf
			{
				get
				{
					return _hamburguerIcon.Pixbuf;
				}

				set
				{
					_hamburguerIcon.Pixbuf = value ?? null;
				}
			}

			public void UpdateTitleColor(Gdk.Color? titleColor)
			{
				if (_titleLabel != null)
				{
					if (titleColor.HasValue)
					{
						_titleLabel.ModifyFg(StateType.Normal, titleColor.Value);
					}
					else
					{
						_titleLabel.ModifyFg(StateType.Normal, _defaultTextColor);
					}
				}
			}

			public void UpdateBackgroundColor(Gdk.Color? backgroundColor)
			{
				if (_root == null)
				{
					return;
				}

				if (backgroundColor.HasValue)
				{
					ModifyBg(StateType.Normal, backgroundColor.Value);
					_root.ModifyBg(StateType.Normal, backgroundColor.Value);
				}
				else
				{
					ModifyBg(StateType.Normal, _defaultBackgroundColor);
					_root.ModifyBg(StateType.Normal, _defaultBackgroundColor);
				}
			}

			public event EventHandler HamburguerClicked;

			private void OnHamburguerButtonClicked(object sender, EventArgs e)
			{
				HamburguerClicked?.Invoke(this, EventArgs.Empty);
			}
		}
	}

	public class MasterDetailPage : FlyoutPage
	{

		public string MasterTitle
		{
			get => base.FlyoutTitle;

			set => base.FlyoutTitle = value;
		}
	}
}
