#nullable enable

using System;
using ElmSharp;
using Microsoft.Maui.Devices;
using Tizen.UIExtensions.Common.GraphicsView;
using Tizen.UIExtensions.ElmSharp;
using Tizen.UIExtensions.ElmSharp.GraphicsView;
using EBox = ElmSharp.Box;
using EColor = ElmSharp.Color;
using TButton = Tizen.UIExtensions.ElmSharp.Button;
using TDPExtensions = Tizen.UIExtensions.ElmSharp.DPExtensions;
using TLabel = Tizen.UIExtensions.ElmSharp.Label;
using TThemeConstants = Tizen.UIExtensions.ElmSharp.ThemeConstants;

namespace Microsoft.Maui.Controls.Platform
{
	public class ShellNavBar : EBox, IFlyoutBehaviorObserver, IDisposable
	{
		MaterialIcon _menuIcon;
		TButton _menuButton;
		TLabel _title;
		ShellSearchView? _searchView = null;
		EvasObject? _nativeTitleView = null;

		SearchHandler? _searchHandler = null;
		View? _titleView = null;
		Page? _page = null;

		FlyoutBehavior _flyoutBehavior = FlyoutBehavior.Flyout;

		EColor _backgroudColor = TThemeConstants.Shell.ColorClass.DefaultBackgroundColor;
		EColor _foregroundColor = TThemeConstants.Shell.ColorClass.DefaultForegroundColor;
		EColor _titleColor = TThemeConstants.Shell.ColorClass.DefaultTitleColor;

		bool _hasBackButton = false;
		bool _disposedValue;
		bool _isTV = DeviceInfo.Idiom == DeviceIdiom.TV;

		bool IsMenuIconVisible => _flyoutBehavior == FlyoutBehavior.Flyout || HasBackButton;

		public ShellNavBar(IMauiContext context) : base(context?.GetPlatformParent())
		{
			MauiContext = context;

			SetLayoutCallback(OnLayout);

			_menuButton = new TButton(PlatformParent);
			_menuButton.Clicked += OnMenuClicked;
			_menuIcon = new MaterialIcon(PlatformParent)
			{
				Color = _foregroundColor.ToCommon()
			};
			UpdateMenuIcon();

			_title = new TLabel(PlatformParent)
			{
				FontSize = this.GetDefaultTitleFontSize(),
				VerticalTextAlignment = (global::Tizen.UIExtensions.Common.TextAlignment)TextAlignment.Center,
				TextColor = _titleColor.ToCommon(),
				FontAttributes = Tizen.UIExtensions.Common.FontAttributes.Bold,
			};
			_title.Show();

			BackgroundColor = _backgroudColor;
			_menuButton.BackgroundColor = _backgroudColor;
			PackEnd(_menuButton);
			PackEnd(_title);
		}

		~ShellNavBar()
		{
			Dispose(false);
		}

		protected IMauiContext? MauiContext { get; private set; }

		protected EvasObject PlatformParent => MauiContext?.GetPlatformParent() ?? throw new InvalidOperationException($"PlatformParent cannot be null here");

		public IShellController ShellController => Shell.Current;

		public bool HasBackButton
		{
			get
			{
				return _hasBackButton;
			}
			set
			{
				_hasBackButton = value;
				UpdateMenuIcon();
			}
		}

		public FlyoutBehavior FlyoutBehavior
		{
			get => _flyoutBehavior;
			set
			{
				if (_flyoutBehavior != value)
				{
					_flyoutBehavior = value;
					UpdateMenuIcon();
				}
			}
		}

		public SearchHandler? SearchHandler
		{
			get
			{
				return _searchHandler;
			}
			set
			{
				_searchHandler = value;
				UpdateSearchHandler(_searchHandler);
				UpdateChildren();
			}
		}

		public View? TitleView
		{
			get
			{
				return _titleView;
			}
			set
			{
				_titleView = value;
				UpdateTitleView(_titleView);
				UpdateChildren();
			}
		}

		public string Title
		{
			get
			{
				return _title.Text;
			}
			set
			{
				_title.Text = value;
			}
		}

		public override EColor BackgroundColor
		{
			get
			{
				return _backgroudColor;
			}
			set
			{
				_backgroudColor = value;
				_menuButton.BackgroundColor = _backgroudColor;
				base.BackgroundColor = _backgroudColor;
			}
		}

		public EColor ForegroundColor
		{
			get
			{
				return _foregroundColor;
			}
			set
			{
				_foregroundColor = value;
			}
		}

		public EColor TitleColor
		{
			get
			{
				return _titleColor;
			}
			set
			{
				_titleColor = value;
				_title.TextColor = value.ToCommon();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void SetPage(Page page)
		{
			_page = page;
			Title = string.IsNullOrEmpty(page.Title) ? Shell.Current.CurrentContent.Title : page.Title;
			SearchHandler = Shell.GetSearchHandler(page);
			TitleView = Shell.GetTitleView(page);

			UpdateMenuIcon();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					Unrealize();
				}
				_disposedValue = true;
			}
		}

		void UpdateMenuIcon()
		{
			if (!IsMenuIconVisible)
			{
				_menuButton.Hide();
			}
			else
			{
				_menuIcon.IconType = HasBackButton ? MaterialIcons.ArrowBack : MaterialIcons.Menu;

				if (_isTV)
				{
					_menuButton.Style = TThemeConstants.Button.Styles.Default;
				}

				_menuIcon?.Show();
				_menuButton.SetIconPart(_menuIcon);
				_menuButton.Show();
			}

			OnLayout();
		}

		void OnMenuClicked(object? sender, EventArgs e)
		{
			var backButtonHandler = Shell.GetBackButtonBehavior(_page);
			if (backButtonHandler?.Command != null)
			{
				backButtonHandler.Command.Execute(backButtonHandler.CommandParameter);
			}
			else if (_hasBackButton)
			{
				Shell.Current.CurrentItem.Navigation.PopAsync();
			}
			else if (_flyoutBehavior == FlyoutBehavior.Flyout)
			{
				Shell.Current.FlyoutIsPresented = !Shell.Current.FlyoutIsPresented;
			}
		}

		void UpdateTitleView(View? titleView)
		{
			_ = MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");

			_nativeTitleView?.Unrealize();
			_nativeTitleView = null;

			if (titleView != null)
			{
				var _nativeTitleView = titleView.ToPlatform(MauiContext);
				_nativeTitleView.Show();
				PackEnd(_nativeTitleView);
			}
		}

		void UpdateSearchHandler(SearchHandler? handler)
		{
			if (_searchView != null)
			{
				UnPack(_searchView.PlatformView);
				_searchView.Dispose();
				_searchView = null;
			}

			if (handler != null)
			{
				_searchView = new ShellSearchView(handler, MauiContext);
				_searchView.PlatformView?.Show();
				PackEnd(_searchView.PlatformView);
			}
		}

		void UpdateChildren()
		{
			if (_searchHandler != null)
			{
				_searchView?.PlatformView?.Show();
				_title?.Hide();
				_nativeTitleView?.Hide();
			}
			else if (_titleView != null)
			{
				_nativeTitleView?.Show();
				_title?.Hide();
				_searchView?.PlatformView?.Hide();
			}
			else
			{
				_title.Show();
				_nativeTitleView?.Hide();
				_searchView?.PlatformView?.Hide();
			}
		}

		void OnLayout()
		{
			if (Geometry.Width == 0 || Geometry.Height == 0)
				return;

			int menuSize = TDPExtensions.ConvertToScaledPixel(this.GetDefaultMenuSize());
			int menuMargin = TDPExtensions.ConvertToScaledPixel(this.GetDefaultMargin());
			int titleHMargin = TDPExtensions.ConvertToScaledPixel(this.GetDefaultMargin());
			int titleVMargin = TDPExtensions.ConvertToScaledPixel(this.GetDefaultTitleVMargin());

			var bound = Geometry;
			var menuBound = new Rect(bound.X, bound.Y, 0, 0);

			if (IsMenuIconVisible)
			{
				menuBound.X += menuMargin;
				menuBound.Y += (bound.Height - menuSize) / 2;
				menuBound.Width = menuSize;
				menuBound.Height = menuSize;

				_menuButton.Geometry = menuBound;
			}

			var contentBound = Geometry;
			contentBound.X = menuBound.Right + titleHMargin;
			contentBound.Y += titleVMargin;
			contentBound.Width -= (menuBound.Width + menuMargin + titleHMargin * 2);
			contentBound.Height -= titleVMargin * 2;

			if (_searchView != null && _searchView.PlatformView != null)
			{
				_searchView.PlatformView.Geometry = contentBound;
			}
			else if (_titleView != null && _nativeTitleView != null)
			{
				_nativeTitleView.Geometry = contentBound;
			}
			else
			{
				_title.Geometry = contentBound;
			}
		}

		void IFlyoutBehaviorObserver.OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		{
			FlyoutBehavior = behavior;
		}
	}
}
