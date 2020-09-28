using System;
using ElmSharp;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	/// <summary>
	/// The native widget which provides Xamarin.FlyoutPage features.
	/// </summary>
	public class FlyoutPage : Box
	{
		/// <summary>
		/// The default flyout layout behavior (a.k.a mode).
		/// </summary>
		static readonly FlyoutLayoutBehavior s_defaultFlyoutLayoutBehavior = (Device.Idiom == TargetIdiom.Phone || Device.Idiom == TargetIdiom.Watch) ? FlyoutLayoutBehavior.Popover : FlyoutLayoutBehavior.SplitOnLandscape;

		/// <summary>
		/// The Flyout native container.
		/// </summary>
		readonly Canvas _flyoutCanvas;

		/// <summary>
		/// The Detail native container.
		/// </summary>
		readonly Canvas _detailCanvas;

		/// <summary>
		/// The container for <c>_flyoutCanvas</c> and <c>_detailCanvas</c> used in split mode.
		/// </summary>
		readonly Panes _splitPane;

		/// <summary>
		/// The container for <c>_flyoutCanvas</c> used in popover mode.
		/// </summary>
		readonly Panel _drawer;

		/// <summary>
		/// The <see cref="FlyoutLayoutBehavior"/> property value.
		/// </summary>
		FlyoutLayoutBehavior _flyoutLayoutBehavior = s_defaultFlyoutLayoutBehavior;

		/// <summary>
		/// The actual FlyoutPage mode - either split or popover. It depends on <c>_flyoutLayoutBehavior</c> and screen orientation.
		/// </summary>
		FlyoutLayoutBehavior _internalFlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;

		/// <summary>
		/// The <see cref="Flyout"/> property value.
		/// </summary>
		EvasObject _flyout;

		/// <summary>
		/// The <see cref="Detail"/> property value.
		/// </summary>
		EvasObject _detail;

		/// <summary>
		/// The main widget - either <see cref="_splitPlane"/> or <see cref="_detailPage"/>, depending on the mode.
		/// </summary>
		EvasObject _mainWidget;

		/// <summary>
		/// The <see cref="IsGestureEnabled"/> property value.
		/// </summary>
		bool _isGestureEnabled = true;

		/// <summary>
		/// The portion of the screen that the Flyout takes in Split mode.
		/// </summary>
		double _splitRatio = 0.35;

		/// <summary>
		/// The portion of the screen that the Flyout takes in Popover mode.
		/// </summary>
		double _popoverRatio = 0.8;

		/// <summary>
		/// Initializes a new instance of the <see cref="Xamarin.Forms.Platform.Tizen.Native.FlyoutPage"/> class.
		/// </summary>
		/// <param name="parent">Parent evas object.</param>
		public FlyoutPage(EvasObject parent) : base(parent)
		{
			LayoutUpdated += (s, e) =>
			{
				UpdateChildCanvasGeometry();
			};

			// create the controls which will hold the flyout and detail pages
			_flyoutCanvas = new Canvas(this);
			_flyoutCanvas.SetAlignment(-1.0, -1.0);  // fill
			_flyoutCanvas.SetWeight(1.0, 1.0);  // expand
			_flyoutCanvas.LayoutUpdated += (sender, e) =>
			{
				UpdatePageGeometry(_flyout);
			};

			_detailCanvas = new Canvas(this);
			_detailCanvas.SetAlignment(-1.0, -1.0);  // fill
			_detailCanvas.SetWeight(1.0, 1.0);  // expand
			_detailCanvas.LayoutUpdated += (sender, e) =>
			{
				UpdatePageGeometry(_detail);
			};

			_splitPane = new Panes(this)
			{
				AlignmentX = -1,
				AlignmentY = -1,
				WeightX = 1,
				WeightY = 1,
				IsFixed = true,
				IsHorizontal = false,
				Proportion = _splitRatio,
			};

			_drawer = new Panel(Forms.NativeParent);
			_drawer.SetScrollable(_isGestureEnabled);
			_drawer.SetScrollableArea(1.0);
			_drawer.Direction = PanelDirection.Left;
			_drawer.Toggled += (object sender, EventArgs e) =>
			{
				UpdateFocusPolicy();
				IsPresentedChanged?.Invoke(this, new IsPresentedChangedEventArgs(_drawer.IsOpen));
			};

			ConfigureLayout();

			// in case of the screen rotation we may need to update the choice between split
			// and popover behaviors and reconfigure the layout
			Device.Info.PropertyChanged += (s, e) =>
			{
				if (e.PropertyName == nameof(Device.Info.CurrentOrientation))
				{
					UpdateFlyoutLayoutBehavior();
				}
			};
		}

		/// <summary>
		/// Occurs when the Flyout is shown or hidden.
		/// </summary>
		public event EventHandler<IsPresentedChangedEventArgs> IsPresentedChanged;

		/// <summary>
		/// Occurs when the IsPresentChangeable was changed.
		/// </summary>
		public event EventHandler<UpdateIsPresentChangeableEventArgs> UpdateIsPresentChangeable;

		/// <summary>
		/// Gets or sets the FlyoutPage behavior.
		/// </summary>
		/// <value>The behavior of the <c>FlyoutPage</c> requested by the user.</value>
		public FlyoutLayoutBehavior FlyoutLayoutBehavior
		{
			get
			{
				return _flyoutLayoutBehavior;
			}

			set
			{
				_flyoutLayoutBehavior = value;
				UpdateFlyoutLayoutBehavior();
			}
		}

		/// <summary>
		/// Gets the FlyoutPage was splited
		/// </summary>
		public bool IsSplit => _internalFlyoutLayoutBehavior == FlyoutLayoutBehavior.Split;

		/// <summary>
		/// Gets or sets the content of the Flyout.
		/// </summary>
		/// <value>The Flyout.</value>
		public EvasObject Flyout
		{
			get
			{
				return _flyout;
			}

			set
			{
				if (_flyout != value)
				{
					_flyout = value;
					UpdatePageGeometry(_flyout);
					_flyoutCanvas.Children.Clear();
					_flyoutCanvas.Children.Add(_flyout);
					if (!IsSplit)
						UpdateFocusPolicy();
				}
			}
		}

		/// <summary>
		/// Gets or sets the content of the DetailPage.
		/// </summary>
		/// <value>The DetailPage.</value>
		public EvasObject Detail
		{
			get
			{
				return _detail;
			}

			set
			{
				if (_detail != value)
				{
					_detail = value;
					UpdatePageGeometry(_detail);
					_detailCanvas.Children.Clear();
					_detailCanvas.Children.Add(_detail);
					if (!IsSplit)
						UpdateFocusPolicy();
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether the Flyout is shown.
		/// </summary>
		/// <value><c>true</c> if the Flyout is presented.</value>
		public bool IsPresented
		{
			get
			{
				return _drawer.IsOpen;
			}

			set
			{
				if (_drawer.IsOpen != value)
				{
					_drawer.IsOpen = value;
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether a FlyoutPage allows showing FlyoutPage with swipe gesture.
		/// </summary>
		/// <value><c>true</c> if the FlyoutPage can be revealed with a gesture.</value>
		public bool IsGestureEnabled
		{
			get
			{
				return _isGestureEnabled;
			}

			set
			{
				if (_isGestureEnabled != value)
				{
					_isGestureEnabled = value;
					// Fixme
					// Elementary panel was not support to change scrollable property on runtime
					// Please uncomment when EFL was updated
					//_drawer.SetScrollable(_isGestureEnabled);
				}
			}
		}

		/// <summary>
		/// Gets or Sets the portion of the screen that the FlyoutPage takes in split mode.
		/// </summary>
		/// <value>The portion.</value>
		public double SplitRatio
		{
			get
			{
				return _splitRatio;
			}
			set
			{
				if (_splitRatio != value)
				{
					_splitRatio = value;
					_splitPane.Proportion = _splitRatio;
				}
			}

		}

		/// <summary>
		/// Gets or sets the portion of the screen that the FlyoutPage takes in Popover mode.
		/// </summary>
		/// <value>The portion.</value>
		public double PopoverRatio
		{
			get
			{
				return _popoverRatio;
			}
			set
			{
				if (_popoverRatio != value)
				{
					_popoverRatio = value;
					UpdateChildCanvasGeometry();
				}
			}
		}

		/// <summary>
		/// Provides destruction for native element and contained elements.
		/// </summary>
		protected override void OnUnrealize()
		{
			// Views that are not belong to view tree should be unrealized.
			if (IsSplit)
			{
				_drawer.Unrealize();
			}
			else
			{
				_splitPane.Unrealize();
			}
			base.OnUnrealize();
		}

		/// <summary>
		/// Updates the geometry of the selected page.
		/// </summary>
		/// <param name="page">Flyout or Detail page to be updated.</param>
		void UpdatePageGeometry(EvasObject page)
		{
			if (page != null)
			{
				if (_flyout == page)
				{
					// update the geometry of the flyout page
					page.Geometry = _flyoutCanvas.Geometry;
				}
				else if (_detail == page)
				{
					// update the geometry of the detail page
					page.Geometry = _detailCanvas.Geometry;
				}
			}
		}

		/// <summary>
		/// Updates <see cref="_internalFlyoutLayoutBehavior"/> according to <see cref="FlyoutLayoutBehavior"/> set by the user and current screen orientation.
		/// </summary>
		void UpdateFlyoutLayoutBehavior()
		{
			var behavior = (_flyoutLayoutBehavior == FlyoutLayoutBehavior.Default) ? s_defaultFlyoutLayoutBehavior : _flyoutLayoutBehavior;

			// Screen orientation affects those 2 behaviors
			if (behavior == FlyoutLayoutBehavior.SplitOnLandscape || behavior == FlyoutLayoutBehavior.SplitOnPortrait)
			{
				var orientation = Device.Info.CurrentOrientation;

				if ((orientation.IsLandscape() && behavior == FlyoutLayoutBehavior.SplitOnLandscape) || (orientation.IsPortrait() && behavior == FlyoutLayoutBehavior.SplitOnPortrait))
				{
					behavior = FlyoutLayoutBehavior.Split;
				}
				else
				{
					behavior = FlyoutLayoutBehavior.Popover;
				}
			}

			if (behavior != _internalFlyoutLayoutBehavior)
			{
				_internalFlyoutLayoutBehavior = behavior;
				ConfigureLayout();
			}
		}

		/// <summary>
		/// Composes the structure of all the necessary widgets.
		/// </summary>
		void ConfigureLayout()
		{
			_drawer.SetContent(null, true);
			_drawer.Hide();

			_splitPane.SetLeftPart(null, true);
			_splitPane.SetRightPart(null, true);
			_splitPane.Hide();

			UnPackAll();

			// the structure for split mode and for popover mode looks differently
			if (IsSplit)
			{
				_splitPane.SetLeftPart(_flyoutCanvas, true);
				_splitPane.SetRightPart(_detailCanvas, true);
				_splitPane.Show();
				_mainWidget = _splitPane;
				PackEnd(_splitPane);

				IsPresented = true;
				UpdateIsPresentChangeable?.Invoke(this, new UpdateIsPresentChangeableEventArgs(false));
				UpdateFocusPolicy(true);
			}
			else
			{
				_drawer.SetContent(_flyoutCanvas, true);
				_drawer.Show();
				_mainWidget = _detailCanvas;
				PackEnd(_detailCanvas);
				PackEnd(_drawer);

				_drawer.IsOpen = IsPresented;
				UpdateIsPresentChangeable?.Invoke(this, new UpdateIsPresentChangeableEventArgs(true));
				UpdateFocusPolicy();
			}

			_flyoutCanvas.Show();
			_detailCanvas.Show();

			// even though child was changed, Layout callback was not called, so i manually call layout function.
			// Layout callback was filter out when geometry was not changed in Native.Box
			UpdateChildCanvasGeometry();
		}

		void UpdateChildCanvasGeometry()
		{
			var bound = Geometry;
			// main widget should fill the area of the FlyoutPage
			if (_mainWidget != null)
			{
				_mainWidget.Geometry = bound;
			}

			bound.Width = (int)((_popoverRatio * bound.Width));
			_drawer.Geometry = bound;
			// When a _drawer.IsOpen was false, Content of _drawer area is not allocated. So, need to manaully set the content area
			if (!IsSplit)
				_flyoutCanvas.Geometry = bound;
		}

		/// <summary>
		/// Force update the focus management
		/// </summary>
		void UpdateFocusPolicy(bool forceAllowFocusAll=false)
		{
			var flyout = _flyout as Widget;
			var detail = _detail as Widget;

			if(forceAllowFocusAll)
			{
				if (flyout != null)
					flyout.AllowTreeFocus = true;
				if (detail != null)
					detail.AllowTreeFocus = true;
				return;
			}

			if (_drawer.IsOpen)
			{
				if (detail != null)
				{
					detail.AllowTreeFocus = false;
				}
				if (flyout != null)
				{
					flyout.AllowTreeFocus = true;
					flyout.SetFocus(true);
				}
			}
			else
			{
				if (flyout != null)
				{
					flyout.AllowTreeFocus = false;
				}
				if (detail != null)
				{
					detail.AllowTreeFocus = true;
					detail.SetFocus(true);
				}
			}
		}
	}

	public class IsPresentedChangedEventArgs : EventArgs
	{
		public IsPresentedChangedEventArgs (bool isPresent)
		{
			IsPresent = isPresent;
		}

		/// <summary>
		/// Value of IsPresent
		/// </summary>
		public bool IsPresent { get; private set; }
	}

	public class UpdateIsPresentChangeableEventArgs : EventArgs
	{
		public UpdateIsPresentChangeableEventArgs(bool canChange)
		{
			CanChange = canChange;
		}

		/// <summary>
		/// Value of changeable
		/// </summary>
		public bool CanChange { get; private set; }
	}
}
