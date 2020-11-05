using System;
using System.Threading.Tasks;
using ElmSharp;
using EBox = ElmSharp.Box;
using ERect = ElmSharp.Rect;
using EGestureType = ElmSharp.GestureLayer.GestureType;

namespace Xamarin.Forms.Platform.Tizen
{
	public class NavigationDrawer : EBox, INavigationDrawer, IAnimatable
	{
		EvasObject _navigationView;
		EBox _mainContainer;
		EBox _dimArea;
		EvasObject _main;
		EBox _drawerBox;

		GestureLayer _gestureOnDimArea;

		bool _isOpen;

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
				return _isOpen;
			}
			set
			{
				if (_isOpen != value)
				{
					if (value)
					{
						ShowDrawer();
					}
					else
					{
						HideDrawer();
					}
				}
			}
		}

		void Initialize(EvasObject parent)
		{
			SetLayoutCallback(OnLayout);
			_mainContainer = new EBox(parent);
			_mainContainer.Show();
			PackEnd(_mainContainer);

			_dimArea = new EBox(parent)
			{
				BackgroundColor = ThemeConstants.Shell.ColorClass.DefaultDrawerDimBackgroundColor
			};
			PackEnd(_dimArea);

			_gestureOnDimArea = new GestureLayer(_dimArea);
			_gestureOnDimArea.SetTapCallback(EGestureType.Tap, GestureLayer.GestureState.Start, OnTapped);
			_gestureOnDimArea.Attach(_dimArea);

			_drawerBox = new EBox(parent);
			PackEnd(_drawerBox);
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
			_drawerBox.PackEnd(_navigationView);
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
			var drawerWidth = (int)(Geometry.Width * this.GetFlyoutRatio(Geometry.Width, Geometry.Height));
			var bound = Geometry;
			bound.Width = drawerWidth;
			bound.X = _isOpen ? bound.X :  bound.X - drawerWidth;
			_drawerBox.Geometry = bound;
		}

		async void HideDrawer()
		{
			var dest = _drawerBox.Geometry;
			dest.X = Geometry.X - dest.Width;

			await MoveDrawerAsync(_drawerBox, dest);

			_drawerBox.Hide();
			_dimArea.Hide();
			_mainContainer.IsEnabled = true;
			if (_isOpen)
			{
				_isOpen = false;
				Toggled?.Invoke(this, EventArgs.Empty);
			}
		}

		async void ShowDrawer()
		{
			_dimArea.Show();
			_mainContainer.IsEnabled = false;
			_drawerBox.Show();

			var dest = _drawerBox.Geometry;
			dest.X = Geometry.X;

			await MoveDrawerAsync(_drawerBox, dest);

			if(!_isOpen)
			{
				_isOpen = true;
				Toggled?.Invoke(this, EventArgs.Empty);
			}
		}

		Task MoveDrawerAsync(EvasObject target, ERect dest, uint length = 200)
		{
			var tcs = new TaskCompletionSource<bool>();

			var dx = target.Geometry.X - dest.X;

			new Animation((progress) =>
			{
				var toMove = dest;
				toMove.X += (int)(dx * (1 - progress));
				target.Geometry = toMove;

			}, easing: Easing.Linear).Commit(this, "Move", length: length, finished:(s, e)=>
			{
				target.Geometry = dest;
				tcs.SetResult(true);
			});
			return tcs.Task;
		}

		void OnTapped(GestureLayer.TapData data)
		{
			if(_isOpen)
			{
				HideDrawer();
			}
		}

		void IAnimatable.BatchBegin()
		{
		}

		void IAnimatable.BatchCommit()
		{
		}
	}
}
