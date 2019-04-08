using System;
using System.Reflection;
using ElmSharp;
using EColor = ElmSharp.Color;
using Xamarin.Forms.Platform.Tizen.Native;

namespace Xamarin.Forms.Platform.Tizen
{
	public class ShellNavBar : Native.Box
	{
		Native.Image _menu = null;
		Native.Label _title = null;
		Native.SearchBar _nativeSearchHandler = null;
		EvasObject _nativeTitleView = null;
		ShellSectionNavigation _shellSectionNavigation = null;

		SearchHandler _searchHandler = null;
		View _titleView = null;
		Page _page = null;

		IFlyoutController _flyoutController = null;

		EColor _backgroudColor = ShellRenderer.DefaultBackgroundColor.ToNative();
		EColor _foregroudColor = ShellRenderer.DefaultForegroundColor.ToNative();

		// The source of icon resources is https://materialdesignicons.com/
		const string _menuIcon = "Xamarin.Forms.Platform.Tizen.Resource.menu.png";
		const string _backIcon = "Xamarin.Forms.Platform.Tizen.Resource.arrow_left.png";

		bool _hasBackButton = false;

		public ShellNavBar(IFlyoutController flyoutController, ShellSectionNavigation shellSectionNavigation) : base(Forms.NativeParent)
		{
			_flyoutController = flyoutController;
			_shellSectionNavigation = shellSectionNavigation;

			_menu = new Native.Image(Forms.NativeParent);
			_menu.Clicked += OnMenuClicked;
			UpdateMenuIcon();
			_menu.Show();

			_title = new Native.Label(Forms.NativeParent)
			{
				FontSize = Device.Idiom == TargetIdiom.TV ? 60 : 23,
				VerticalTextAlignment = Native.TextAlignment.Center,
				TextColor = _backgroudColor,
				FontAttributes = FontAttributes.Bold,
			};
			_title.Show();

			BackgroundColor = _backgroudColor;
			PackEnd(_menu);
			PackEnd(_title);
			LayoutUpdated += OnLayoutUpdated;
		}

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
				if (string.IsNullOrEmpty(value))
				{
					_title?.Hide();
				}
				else
				{
					_title.Text = value;
				}
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
				_menu.Color = value;
			}
		}

		public EColor TitleColor
		{
			get
			{
				return _title.TextColor;
			}
			set
			{
				_title.TextColor = value;
			}
		}

		internal Page CurrentPage
		{
			get
			{
				return _page;
			}
			set
			{
				_page = value;
			}
		}

		async void UpdateMenuIcon()
		{
			string file = _hasBackButton ? _backIcon : _menuIcon;
			ImageSource source = ImageSource.FromResource(file, typeof(ShellNavBar).GetTypeInfo().Assembly);
			bool ret = await _menu.LoadFromImageSourceAsync(source);
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
				_shellSectionNavigation.PopRequest(this, new Internals.NavigationRequestedEventArgs(_page, false));
			}
			else
			{
				_flyoutController.Open();
			}
		}

		void UpdateTitleView(View titleView)
		{
			_nativeTitleView?.Unrealize();
			_nativeTitleView = null;

			if (titleView != null)
			{
				var renderer = Platform.GetOrCreateRenderer(titleView);
				(renderer as LayoutRenderer)?.RegisterOnLayoutUpdated();
				_nativeTitleView = renderer.NativeView;
				PackEnd(_nativeTitleView);
			}
		}

		void UpdateSearchHandler(SearchHandler handler)
		{
			_nativeSearchHandler?.Unrealize();
			_nativeSearchHandler = null;

			if (handler != null)
			{
				_nativeSearchHandler = new Native.SearchBar(Forms.NativeParent);
				_nativeSearchHandler.IsSingleLine = true;
				_nativeSearchHandler.BackgroundColor = ElmSharp.Color.White;
				_nativeSearchHandler.Placeholder = handler.Placeholder;
				_nativeSearchHandler.Show();
				PackEnd(_nativeSearchHandler);
			}
		}

		Native.Image GetSearchHandlerIcon(ImageSource source)
		{
			Native.Image _icon = new Native.Image(Forms.NativeParent);
			if (source != null)
			{
				var task = _icon.LoadFromImageSourceAsync(source);
			}
			return _icon;
		}

		void UpdateChildren()
		{
			if (_searchHandler != null)
			{
				_nativeSearchHandler.Show();
				_title?.Hide();
				_nativeTitleView?.Hide();
			}
			else if (_titleView != null)
			{
				_nativeTitleView.Show();
				_title?.Hide();
				_nativeSearchHandler?.Hide();
			}
			else
			{
				_title.Show();
				_nativeTitleView?.Hide();
				_nativeSearchHandler?.Hide();
			}
			UpdatPageLayout();
		}

		void OnLayoutUpdated(object sender, LayoutEventArgs e)
		{
			int menuSize = 50;
			int menuMargin = 20;
			int titleLeftMargin = 40;
			int titleViewTopMargin = 40;

			_menu.Move(e.Geometry.X + menuMargin, e.Geometry.Y + (e.Geometry.Height - menuSize) / 2);
			_menu.Resize(menuSize, menuSize);

			if (_searchHandler != null)
			{
				_nativeSearchHandler.Move(e.Geometry.X + (menuSize + menuMargin * 2) + titleLeftMargin, e.Geometry.Y + (titleViewTopMargin / 2));
				_nativeSearchHandler.Resize(e.Geometry.Width - (menuSize + menuMargin * 2) - titleLeftMargin - (titleViewTopMargin / 2), e.Geometry.Height - titleViewTopMargin);
			}
			else if (_titleView != null)
			{
				_nativeTitleView.Move(e.Geometry.X + (menuSize + menuMargin * 2) + titleLeftMargin, e.Geometry.Y + (titleViewTopMargin / 2));
				_nativeTitleView.Resize(e.Geometry.Width - (menuSize + menuMargin * 2) - titleLeftMargin - (titleViewTopMargin / 2), e.Geometry.Height - titleViewTopMargin);
			}
			else
			{
				_title.Move(e.Geometry.X + (menuSize + menuMargin * 2) + titleLeftMargin, e.Geometry.Y);
				_title.Resize(e.Geometry.Width - (menuSize + menuMargin) - titleLeftMargin, e.Geometry.Height);
			}
		}

		void UpdatPageLayout()
		{
			OnLayoutUpdated(this, new LayoutEventArgs() { Geometry = Geometry });
		}
	}
}
