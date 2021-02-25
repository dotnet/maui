using System;
using ElmSharp;
using EBox = ElmSharp.Box;
using EButton = ElmSharp.Button;
using EColor = ElmSharp.Color;

namespace Xamarin.Forms.Platform.Tizen.TV
{
	public class TVNavigationDrawer : EBox, INavigationDrawer, IAnimatable, IFlyoutBehaviorObserver
	{
		EBox _drawerBox;
		EBox _mainBox;
		EvasObject _main;
		TVNavigationView _drawer;
		EButton _focusControlArea;

		FlyoutBehavior _behavior;
		bool _isOpen;

		double _openRatio;

		public TVNavigationDrawer(EvasObject parent) : base(parent)
		{
			Initialize(parent);
		}

		public event EventHandler Toggled;

		public EvasObject TargetView => this;

		public EvasObject NavigationView
		{
			get => _drawer;
			set => UpdateNavigationView(value);
		}

		public EvasObject Main
		{
			get => _main;
			set => UpdateMain(value);
		}

		public bool IsOpen
		{
			get => _isOpen;
			set => UpdateOpenState(value);
		}

		void Initialize(EvasObject parent)
		{
			SetLayoutCallback(OnLayout);

			_drawerBox = new EBox(parent);
			_drawerBox.Show();
			PackEnd(_drawerBox);

			_mainBox = new EBox(parent);
			_mainBox.SetLayoutCallback(OnMainBoxLayout);
			_mainBox.Show();
			PackEnd(_mainBox);

			_focusControlArea = new EButton(parent);
			_focusControlArea.Color = EColor.Transparent;
			_focusControlArea.BackgroundColor = EColor.Transparent;
			_focusControlArea.SetEffectColor(EColor.Transparent);
			_focusControlArea.Show();
			_mainBox.PackEnd(_focusControlArea);

			_drawerBox.KeyUp += (s, e) =>
			{
				if (e.KeyName == "Return" || e.KeyName == "Right")
				{
					IsOpen = false;
				}
			};

			_mainBox.KeyUp += (s, e) =>
			{
				if (e.KeyName == "Left")
				{
					if (_focusControlArea.IsFocused)
						IsOpen = true;
				}
				else
				{
					// Workaround to prevent unexpected movement of the focus to drawer during page pushing.
					if (_behavior == FlyoutBehavior.Locked)
						_drawerBox.AllowTreeFocus = true;
				}
			};

			_mainBox.KeyDown += (s, e) =>
			{
				if (e.KeyName != "Left")
				{
					// Workaround to prevent unexpected movement of the focus to drawer during page pushing.
					if (_behavior == FlyoutBehavior.Locked)
						_drawerBox.AllowTreeFocus = false;
				}
			};

			UpdateFocusPolicy();
		}

		void UpdateNavigationView(EvasObject navigationView)
		{
			if (_drawer != null)
			{
				_drawer.ContentFocused -= OnNavigationViewItemFocused;
				_drawer.ContentUnfocused -= OnNavigationViewItemUnfocused;
				_drawerBox.UnPack(_drawer);
				_drawer.Hide();
			}

			_drawer = navigationView as TVNavigationView;

			if (_drawer != null)
			{
				_drawer.SetAlignment(-1, -1);
				_drawer.SetWeight(1, 1);
				_drawer.Show();
				_drawerBox.PackEnd(_drawer);

				_drawer.ContentFocused += OnNavigationViewItemFocused;
				_drawer.ContentUnfocused += OnNavigationViewItemUnfocused;
			}
		}

		void OnNavigationViewItemFocused(object sender, EventArgs args)
		{
			IsOpen = true;
		}

		void OnNavigationViewItemUnfocused(object sender, EventArgs args)
		{
			IsOpen = false;
		}

		void UpdateMain(EvasObject main)
		{
			if (_main != null)
			{
				_mainBox.UnPack(_main);
				_main.Hide();
			}
			_main = main;

			if (_main != null)
			{
				_main.SetAlignment(-1, -1);
				_main.SetWeight(1, 1);
				_main.Show();
				_mainBox.PackStart(_main);
			}
		}

		void UpdateBehavior(FlyoutBehavior behavior)
		{
			_behavior = behavior;
			_focusControlArea.IsEnabled = _behavior == FlyoutBehavior.Flyout;

			var open = false;

			if (_behavior == FlyoutBehavior.Locked)
				open = true;
			else if (_behavior == FlyoutBehavior.Disabled)
				open = false;
			else
				open = _drawerBox.IsFocused;

			UpdateOpenState(open);
		}

		void OnMainBoxLayout()
		{
			if(_main != null)
			{
				_main.Geometry = _mainBox.Geometry;
			}

			var focusedButtonGeometry = _mainBox.Geometry;
			focusedButtonGeometry.X = focusedButtonGeometry.X - 100;
			focusedButtonGeometry.Width = 0;
			_focusControlArea.Geometry = focusedButtonGeometry;
		}

		void OnLayout()
		{
			if (Geometry.Width == 0 || Geometry.Height == 0)
				return;

			var bound = Geometry;

			var ratio = this.GetFlyoutRatio(Geometry.Width, Geometry.Height);
			var collapseRatio = (_behavior == FlyoutBehavior.Disabled) ? 0 : this.GetFlyoutCollapseRatio();
			var drawerWidthMax = (int)(bound.Width * ratio);
			var drawerWidthMin = (int)(bound.Width * collapseRatio);

			var drawerWidthOutBound = (int)((drawerWidthMax - drawerWidthMin) * (1 - _openRatio));
			var drawerWidthInBound = drawerWidthMax - drawerWidthOutBound;

			var drawerGeometry = bound;
			drawerGeometry.Width = drawerWidthInBound;
			_drawerBox.Geometry = drawerGeometry;

			var containerGeometry = bound;
			containerGeometry.X = drawerWidthInBound;
			containerGeometry.Width = (_behavior == FlyoutBehavior.Locked) ? (bound.Width - drawerWidthInBound) : (bound.Width - drawerWidthMin);
			_mainBox.Geometry = containerGeometry;
		}

		void UpdateOpenState(bool isOpen)
		{
			if (_behavior == FlyoutBehavior.Locked && !isOpen)
				return;

			double endState = ((_behavior != FlyoutBehavior.Disabled) && isOpen) ? 1 : 0;
			new Animation((r) =>
			{
				_openRatio = r;
				OnLayout();
			}, _openRatio, endState, Easing.SinOut).Commit(this, "DrawerMove", finished: (f, aborted) =>
			{
				if (!aborted)
				{
					if (_isOpen != isOpen)
					{
						_isOpen = isOpen;
						UpdateFocusPolicy();
						Toggled?.Invoke(this, EventArgs.Empty);
					}
				}
			});
		}

		void UpdateFocusPolicy()
		{
			if (_isOpen)
			{
				if(_behavior == FlyoutBehavior.Locked)
				{
					_drawerBox.AllowTreeFocus = true;
					_mainBox.AllowTreeFocus = true;
				}
				else
				{
					_mainBox.AllowTreeFocus = false;
					_drawerBox.AllowTreeFocus = true;
					_drawerBox.SetFocus(true);
				}
			}
			else
			{
				_mainBox.AllowTreeFocus = true;
				_drawerBox.AllowTreeFocus = false;
				_mainBox.SetFocus(true);
			}
		}

		void IFlyoutBehaviorObserver.OnFlyoutBehaviorChanged(FlyoutBehavior behavior)
		{
			UpdateBehavior(behavior);
		}

		void IAnimatable.BatchBegin()
		{
		}

		void IAnimatable.BatchCommit()
		{
		}
	}
}