using System;
using System.Reflection;
using ElmSharp;
using EBox = ElmSharp.Box;
using EButton = ElmSharp.Button;
using EColor = ElmSharp.Color;
using EImage = ElmSharp.Image;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	public class ShellNavBar : EBox, IFlyoutBehaviorObserver, IDisposable
	{
		EImage _menuIcon = null;
		EButton _menuButton = null;
		Native.Label _title = null;
		SearchHandlerRenderer _searchRenderer = null;
		EvasObject _nativeTitleView = null;

		SearchHandler _searchHandler = null;
		View _titleView = null;
		Page _page = null;

		FlyoutBehavior _flyoutBehavior = FlyoutBehavior.Flyout;

		EColor _backgroudColor = ShellRenderer.DefaultBackgroundColor.ToNative();
		EColor _foregroudColor = ShellRenderer.DefaultForegroundColor.ToNative();
		EColor _titleColor = ShellRenderer.DefaultTitleColor.ToNative();

		// The source of icon resources is https://materialdesignicons.com/
		const string _menuIconRes = ThemeConstants.Shell.Resources.MenuIcon;
		const string _backIconRes = ThemeConstants.Shell.Resources.BackIcon;

		bool _hasBackButton = false;
		private bool disposedValue;
		bool _isTV = Device.Idiom == TargetIdiom.TV;

		public ShellNavBar() : base(Forms.NativeParent)
		{
			SetLayoutCallback(OnLayout);

			_menuButton = new EButton(Forms.NativeParent);
			_menuButton.Clicked += OnMenuClicked;

			_menuIcon = new EImage(Forms.NativeParent);
			UpdateMenuIcon();

			_title = new Native.Label(Forms.NativeParent)
			{
				FontSize = this.GetDefaultTitleFontSize(),
				VerticalTextAlignment = Native.TextAlignment.Center,
				TextColor = _titleColor,
				FontAttributes = FontAttributes.Bold,
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

		public SearchHandler SearchHandler
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

		public View TitleView
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
				return _title?.Text;
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
				return _foregroudColor;
			}
			set
			{
				_foregroudColor = value;
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
				_title.TextColor = value;
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
			Title = page.Title;
			SearchHandler = Shell.GetSearchHandler(page);
			TitleView = Shell.GetTitleView(page);
			UpdateMenuIcon();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					Unrealize();
				}
				disposedValue = true;
			}
		}

		void UpdateMenuIcon()
		{
			ImageSource source = null;
			if (HasBackButton)
			{
				if (_isTV)
				{
					_menuButton.Style = ThemeConstants.Button.Styles.Default;
					_menuButton.Text = ThemeConstants.Shell.Resources.TV.BackIconCode;
					_menuIcon = null;
				}
				else
				{
					var assembly = typeof(ShellNavBar).GetTypeInfo().Assembly;
					var assemblyName = assembly.GetName().Name;
					source = ImageSource.FromResource(assemblyName + "." + _backIconRes, assembly);
				}
			}
			else if (_flyoutBehavior != FlyoutBehavior.Flyout)
			{
				_menuButton.Hide();
			}
			else if (ShellController.FlyoutIcon != null)
			{
				if (_isTV)
				{
					_menuButton.Style = ThemeConstants.Button.Styles.Circle;
					_menuIcon = new EImage(Forms.NativeParent);
				}
				source = Shell.Current.FlyoutIcon;
			}
			else
			{
				if (_isTV)
				{
					_menuButton.Style = ThemeConstants.Button.Styles.Default;
					_menuButton.Text = ThemeConstants.Shell.Resources.TV.MenuIconCode;
					_menuIcon = null;
				}
				else
				{
					var assembly = typeof(ShellNavBar).GetTypeInfo().Assembly;
					var assemblyName = assembly.GetName().Name;
					source = ImageSource.FromResource(assemblyName + "." + _menuIconRes, assembly);
				}
			}

			if (source != null && _menuIcon != null)
			{
				_menuIcon.Show();
				_ = _menuIcon.LoadFromImageSourceAsync(source);
			}
			_menuButton.SetIconPart(_menuIcon);
			_menuButton.Show();
		}

		void OnMenuClicked(object sender, EventArgs e)
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
				Shell.Current.FlyoutIsPresented = true;
			}
		}

		void UpdateTitleView(View titleView)
		{
			_nativeTitleView?.Unrealize();
			_nativeTitleView = null;

			if (titleView != null)
			{
				var renderer = Platform.GetOrCreateRenderer(titleView);
				(renderer as ILayoutRenderer)?.RegisterOnLayoutUpdated();
				_nativeTitleView = renderer.NativeView;
				_nativeTitleView.Show();
				PackEnd(_nativeTitleView);
			}
		}

		void UpdateSearchHandler(SearchHandler handler)
		{
			if (_searchRenderer != null)
			{
				_searchRenderer.Dispose();
				_searchRenderer = null;
			}

			if (handler != null)
			{
				_searchRenderer = new SearchHandlerRenderer(handler);
				_searchRenderer.NativeView.Show();
				PackEnd(_searchRenderer.NativeView);
			}
		}

		void UpdateChildren()
		{
			if (_searchHandler != null)
			{
				_searchRenderer.NativeView.Show();
				_title?.Hide();
				_nativeTitleView?.Hide();
			}
			else if (_titleView != null)
			{
				_nativeTitleView.Show();
				_title?.Hide();
				_searchRenderer?.NativeView?.Hide();
			}
			else
			{
				_title.Show();
				_nativeTitleView?.Hide();
				_searchRenderer?.NativeView?.Hide();
			}
		}

		void OnLayout()
		{
			if (Geometry.Width == 0 || Geometry.Height == 0)
				return;

			int menuSize = Forms.ConvertToScaledPixel(this.GetDefaultMenuSize());
			int menuMargin = Forms.ConvertToScaledPixel(this.GetDefaultMargin());
			int titleHMargin = Forms.ConvertToScaledPixel(this.GetDefaultMargin());
			int titleVMargin = Forms.ConvertToScaledPixel(this.GetDefaultMargin());

			var bound = Geometry;

			var menuBound = bound;
			menuBound.X += menuMargin;
			menuBound.Y += (menuBound.Height - menuSize) / 2;
			menuBound.Width = menuSize;
			menuBound.Height = menuSize;

			_menuButton.Geometry = menuBound;

			var contentBound = Geometry;
			contentBound.X = menuBound.Right + titleHMargin;
			contentBound.Y += titleVMargin;
			contentBound.Width -= (menuBound.Width + menuMargin + titleHMargin * 2);
			contentBound.Height -= titleVMargin * 2;

			if (_searchRenderer != null)
			{
				_searchRenderer.NativeView.Geometry = contentBound;
			}
			else if (_titleView != null)
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
