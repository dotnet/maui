using System;
using ElmSharp;
using EBox = ElmSharp.Box;

namespace Xamarin.Forms.Platform.Tizen
{
	public class NavigationDrawer : Native.Box, INavigationDrawer
	{
		EvasObject _navigationView;
		Box _mainContainer;
		Box _dimArea;
		EvasObject _main;
		Panel _drawer;

		bool _isLock = false;
		double _navigationViewRatio = 0.85;

		public NavigationDrawer(EvasObject parent) : base(parent)
		{
			Initialize(parent);
			LayoutUpdated += (s, e) =>
			{
				UpdateChildGeometry();
			};
		}

		public event EventHandler Toggled;

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
				_drawer.IsOpen = value;
			}
		}

		public bool IsLock
		{
			get
			{
				return _isLock;
			}
			set
			{
				_isLock = value;
				_drawer.SetScrollable(!_isLock);
			}
		}

		void Initialize(EvasObject parent)
		{
			_mainContainer = new Box(parent)
			{
				AlignmentX = -1,
				AlignmentY = -1,
				WeightX = 1,
				WeightY = 1,
			};
			_mainContainer.Show();
			PackEnd(_mainContainer);

			_dimArea = new Box(parent)
			{
				AlignmentX = -1,
				AlignmentY = -1,
				WeightX = 1,
				WeightY = 1,
				BackgroundColor = new ElmSharp.Color(0, 0, 0, 82)
			};

			PackEnd(_dimArea);

			_drawer = new Panel(parent);

			_drawer.SetScrollable(!_isLock);
			_drawer.SetScrollableArea(_navigationViewRatio);
			_drawer.Direction = PanelDirection.Left;
			_drawer.Toggled += (s, e) =>
			{
				UpdateDimArea();

				Toggled?.Invoke(this, e);
			};

			_drawer.Show();
			PackEnd(_drawer);
		}

		void UpdateNavigationView(EvasObject navigationView)
		{
			if (_navigationView != null)
			{
				_navigationView.Hide();
			}

			_navigationView = navigationView;

			if (_navigationView != null)
			{
				_navigationView.SetAlignment(-1, -1);
				_navigationView.SetWeight(1, 1);
				if (!_navigationView.IsVisible)
				{
					_navigationView.Show();
				}
				_drawer.SetContent(_navigationView, true);
				UpdateDimArea();
				UpdateNavigationViewGeometry();
			}
			else
			{
				_drawer.SetContent(null, true);
			}
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
				if (!_main.IsVisible)
				{
					_main.Show();
				}
				_mainContainer.PackEnd(_main);
			}
		}

		void UpdateDimArea()
		{
			if (_drawer.IsOpen)
			{
				_dimArea.Show();
			}
			else
			{
				_dimArea.Hide();
			}
		}

		void UpdateChildGeometry()
		{
			if (_main != null)
			{
				_mainContainer.Geometry = Geometry;
			}

			UpdateNavigationViewGeometry();
		}

		void UpdateNavigationViewGeometry()
		{
			if (_navigationView != null)
			{
				_drawer.Geometry = Geometry;
				_dimArea.Geometry = Geometry;
			}
		}
	}
}
