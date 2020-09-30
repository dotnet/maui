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
				if (!_drawer.IsOpen && value)
				{
					_drawer.SetScrollableArea(_navigationViewRatio);
				}

				_drawer.IsOpen = value;

				if (!_drawer.IsOpen)
				{
					_drawer.SetScrollableArea(0);
				}
			}
		}

		void Initialize(EvasObject parent)
		{
			/**
			 *  /----------------/
			 *  /     drawer     /
			 *  / /------------/ /
			 *  / /   content  / /
			 *  / /------------/ /
			 *  /----------------/
			 *  /----------------/
			 *  /     dim        /
			 *  /----------------/
			 *  /----------------/
			 *  /      main      /
			 *  /-------------- -/
			 * 
			 */

			SetLayoutCallback(OnLayout);

			_mainContainer = new EBox(parent);
			_mainContainer.Show();
			PackEnd(_mainContainer);

			_dimArea = new EBox(parent)
			{
				BackgroundColor = ThemeConstants.Shell.ColorClass.DefaultDrawerDimBackgroundColor
			};
			PackEnd(_dimArea);

			_drawer = new Panel(parent);

			_drawer.SetScrollable(true);
			_drawer.SetScrollableArea(0);
			_drawer.Direction = PanelDirection.Left;

			_drawer.Toggled += (s, e) =>
			{
				UpdateDimArea();
				Toggled?.Invoke(this, e);
				if (!_drawer.IsOpen)
				{
					Device.StartTimer(TimeSpan.FromSeconds(1), () =>
					{
						if (!_drawer.IsOpen)
							_drawer.SetScrollableArea(0);
						return false;
					});
				}
			};
			_drawer.IsOpen = false;
			_drawer.Show();
			PackEnd(_drawer);
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
