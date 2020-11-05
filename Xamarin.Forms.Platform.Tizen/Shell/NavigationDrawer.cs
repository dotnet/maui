using System;
using ElmSharp;
using EBox = ElmSharp.Box;

namespace Xamarin.Forms.Platform.Tizen
{
	public class NavigationDrawer : EBox, INavigationDrawer
	{
		EvasObject _navigationView;
		EBox _mainContainer;
		EBox _dimArea;
		EvasObject _main;
		Panel _drawer;

		double _navigationViewRatio = 0.85;

		public NavigationDrawer(EvasObject parent) : base(parent)
		{
			Initialize(parent);
		}

		public event EventHandler Toggled;

		public EvasObject TargetView => this;

		public EvasObject NavigationView
		{
			get => _navigationView;
			set => UpdateNavigationView(value);
		}

		public EvasObject Main
		{
			get
			{
				return _main;
			}
			set
			{
				UpdateMain(value);
			}
		}

		public bool IsOpen
		{
			get
			{
				return _drawer.IsOpen;
			}
			set
			{
				if (value)
					ShowDrawer();

				_drawer.IsOpen = value;
			}
		}

		void Initialize(EvasObject parent)
		{
			SetLayoutCallback(OnLayout);

			_mainContainer = new EBox(parent);
			_mainContainer.Show();

			_dimArea = new EBox(parent)
			{
				BackgroundColor = ThemeConstants.Shell.ColorClass.DefaultDrawerDimBackgroundColor
			};

			_drawer = new Panel(parent);

			_drawer.SetScrollable(true);
			_drawer.SetScrollableArea(_navigationViewRatio);
			_drawer.Direction = PanelDirection.Left;

			_drawer.Toggled += (s, e) =>
			{
				if (!_drawer.IsOpen)
				{
					HideDrawer();
				}
				Toggled?.Invoke(this, e);
			};
			_drawer.IsOpen = false;
			_drawer.Show();

			PackEnd(_dimArea);
			PackEnd(_drawer);
			PackEnd(_mainContainer);
		}

		void HideDrawer()
		{
			/**
			*  /----------------/
			*  /     main       /
			*  /----------------/
			*  /----------------/
			*  /     drawer     /
			*  / /------------/ /
			*  / /   content  / /
			*  / /------------/ /
			*  /----------------/
			*  /----------------/
			*  /      dim       /
			*  /----------------/
			*/
			_dimArea.Hide();
			_drawer.Hide();
			_mainContainer.RaiseTop();
		}

		void ShowDrawer()
		{
			/**
			*  /----------------/
			*  /     drawer     /
			*  / /------------/ /
			*  / /   content  / /
			*  / /------------/ /
			*  /----------------/
			*  /----------------/
			*  /      dim       /
			*  /----------------/
			*  /----------------/
			*  /     main       /
			*  /----------------/
			*/
			_dimArea.RaiseTop();
			_dimArea.Show();
			_drawer.RaiseTop();
			_drawer.Show();
		}

		void UpdateNavigationView(EvasObject navigationView)
		{
			_navigationView?.Hide();
			_navigationView = navigationView;

			if (_navigationView != null)
			{
				_navigationView.SetAlignment(-1, -1);
				_navigationView.SetWeight(1, 1);
				_navigationView.Show();
			}
			_drawer.SetContent(_navigationView);
		}

		void UpdateMain(EvasObject main)
		{
			if (_main != null)
			{
				_mainContainer.UnPack(_main);
				_main.Hide();
			}

			_main = main;

			if (_main != null)
			{
				_main.SetAlignment(-1, -1);
				_main.SetWeight(1, 1);
				_main.Show();
				_mainContainer.PackEnd(_main);
			}
		}

		void OnLayout()
		{
			if (Geometry.Width == 0 || Geometry.Height == 0)
				return;

			_mainContainer.Geometry = Geometry;
			_dimArea.Geometry = Geometry;
			_drawer.Geometry = Geometry;
		}
	}
}
