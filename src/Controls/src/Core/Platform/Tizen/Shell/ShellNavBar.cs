using System;
using System.Reflection;
using ElmSharp;
using Microsoft.Extensions.DependencyInjection;
using Tizen.UIExtensions.ElmSharp;
using EBox = ElmSharp.Box;
using EColor = ElmSharp.Color;
using TButton = Tizen.UIExtensions.ElmSharp.Button;
using TImage = Tizen.UIExtensions.ElmSharp.Image;
using TLabel = Tizen.UIExtensions.ElmSharp.Label;
using TThemeConstants = Tizen.UIExtensions.Shell.ThemeConstants;

namespace Microsoft.Maui.Controls.Platform
{
	public class ShellNavBar : EBox, IFlyoutBehaviorObserver, IDisposable
	{
		TImage _menuIcon = null;
		TButton _menuButton = null;
		TLabel _title = null;
		EvasObject _nativeTitleView = null;
		View _titleView = null;
		Page _page = null;

		FlyoutBehavior _flyoutBehavior = FlyoutBehavior.Flyout;

		EColor _backgroudColor = ShellView.DefaultBackgroundColor;
		EColor _foregroudColor = ShellView.DefaultForegroundColor;
		EColor _titleColor = ShellView.DefaultTitleColor;

		// The source of icon resources is https://materialdesignicons.com/
		const string _menuIconRes = TThemeConstants.Shell.Resources.MenuIcon;
		const string _backIconRes = TThemeConstants.Shell.Resources.BackIcon;

		bool _hasBackButton = false;
		private bool disposedValue;
		bool _isTV = Device.Idiom == TargetIdiom.TV;

		public ShellNavBar(IMauiContext context) : base(context?.Context?.BaseLayout)
		{
			MauiContext = context;

			SetLayoutCallback(OnLayout);

			_menuButton = new TButton(NativeParent);
			_menuButton.Clicked += OnMenuClicked;

			_menuIcon = new TImage(NativeParent);
			UpdateMenuIcon();

			_title = new TLabel(NativeParent)
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

		protected IMauiContext MauiContext { get; private set; }

		protected EvasObject NativeParent
		{
			get => MauiContext?.Context?.BaseLayout;
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
			Title = page.Title;
			//SearchHandler = Shell.GetSearchHandler(page);
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

		async void UpdateMenuIcon()
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
					_menuIcon = new TImage(NativeParent);
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
				var provider = MauiContext.Services.GetRequiredService<IImageSourceServiceProvider>();
				var service = provider.GetRequiredImageSourceService(source);

				await service.LoadImageAsync(source, _menuIcon);
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
				Shell.Current.FlyoutIsPresented = !Shell.Current.FlyoutIsPresented;
			}
		}

		void UpdateTitleView(View titleView)
		{
			_nativeTitleView?.Unrealize();
			_nativeTitleView = null;

			if (titleView != null)
			{
				var _nativeTitleView = titleView.ToNative(MauiContext);
				_nativeTitleView.Show();
				PackEnd(_nativeTitleView);
			}
		}

		void UpdateChildren()
		{
			if (_titleView != null)
			{
				_nativeTitleView.Show();
				_title?.Hide();
			}
			else
			{
				_title.Show();
				_nativeTitleView?.Hide();
			}
		}

		void OnLayout()
		{
			if (Geometry.Width == 0 || Geometry.Height == 0)
				return;

			int menuSize = DPExtensions.ConvertToScaledPixel(this.GetDefaultMenuSize());
			int menuMargin = DPExtensions.ConvertToScaledPixel(this.GetDefaultMargin());
			int titleHMargin = DPExtensions.ConvertToScaledPixel(this.GetDefaultMargin());
			int titleVMargin = DPExtensions.ConvertToScaledPixel(this.GetDefaultTitleVMargin());

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

			if (_titleView != null)
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
