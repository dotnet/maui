using System;
using ElmSharp;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	/// <summary>
	/// The native widget which provides Xamarin.MasterDetailPage features.
	/// </summary>
	public class MasterDetailPage : Box
	{
		/// <summary>
		/// The default master behavior (a.k.a mode).
		/// </summary>
		static readonly MasterBehavior s_defaultMasterBehavior = (Device.Idiom == TargetIdiom.Phone) ? MasterBehavior.Popover : MasterBehavior.SplitOnLandscape;

		/// <summary>
		/// The MasterPage native container.
		/// </summary>
		readonly Canvas _masterCanvas;

		/// <summary>
		/// The DetailPage native container.
		/// </summary>
		readonly Canvas _detailCanvas;

		/// <summary>
		/// The container for <c>_masterCanvas</c> and <c>_detailCanvas</c> used in split mode.
		/// </summary>
		readonly Panes _splitPane;

		/// <summary>
		/// The container for <c>_masterCanvas</c> used in popover mode.
		/// </summary>
		readonly Panel _drawer;

		/// <summary>
		/// The <see cref="MasterBehavior"/> property value.
		/// </summary>
		MasterBehavior _masterBehavior = s_defaultMasterBehavior;

		/// <summary>
		/// The actual MasterDetailPage mode - either split or popover. It depends on <c>_masterBehavior</c> and screen orientation.
		/// </summary>
		MasterBehavior _internalMasterBehavior = MasterBehavior.Popover;

		/// <summary>
		/// The <see cref="Master"/> property value.
		/// </summary>
		EvasObject _master;

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
		/// The portion of the screen that the MasterPage takes in Split mode.
		/// </summary>
		double _splitRatio = 0.35;

		/// <summary>
		/// The portion of the screen that the MasterPage takes in Popover mode.
		/// </summary>
		double _popoverRatio = 0.8;

		/// <summary>
		/// Initializes a new instance of the <see cref="Xamarin.Forms.Platform.Tizen.Native.MasterDetailPage"/> class.
		/// </summary>
		/// <param name="parent">Parent evas object.</param>
		public MasterDetailPage(EvasObject parent) : base(parent)
		{
			LayoutUpdated += (s, e) =>
			{
				UpdateChildCanvasGeometry();
			};

			// create the controls which will hold the master and detail pages
			_masterCanvas = new Canvas(this);
			_masterCanvas.SetAlignment(-1.0, -1.0);  // fill
			_masterCanvas.SetWeight(1.0, 1.0);  // expand
			_masterCanvas.LayoutUpdated += (sender, e) =>
			{
				UpdatePageGeometry(_master);
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
					UpdateMasterBehavior();
				}
			};
		}

		/// <summary>
		/// Occurs when the MasterPage is shown or hidden.
		/// </summary>
		public event EventHandler<IsPresentedChangedEventArgs> IsPresentedChanged;

		/// <summary>
		/// Occurs when the IsPresentChangeable was changed.
		/// </summary>
		public event EventHandler<UpdateIsPresentChangeableEventArgs> UpdateIsPresentChangeable;

		/// <summary>
		/// Gets or sets the MasterDetailPage behavior.
		/// </summary>
		/// <value>The behavior of the <c>MasterDetailPage</c> requested by the user.</value>
		public MasterBehavior MasterBehavior
		{
			get
			{
				return _masterBehavior;
			}

			set
			{
				_masterBehavior = value;
				UpdateMasterBehavior();
			}
		}

		/// <summary>
		/// Gets the MasterDEtailPage was splited
		/// </summary>
		public bool IsSplit => _internalMasterBehavior == MasterBehavior.Split;

		/// <summary>
		/// Gets or sets the content of the MasterPage.
		/// </summary>
		/// <value>The MasterPage.</value>
		public EvasObject Master
		{
			get
			{
				return _master;
			}

			set
			{
				if (_master != value)
				{
					_master = value;
					UpdatePageGeometry(_master);
					_masterCanvas.Children.Clear();
					_masterCanvas.Children.Add(_master);
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
		/// Gets or sets a value indicating whether the MasterPage is shown.
		/// </summary>
		/// <value><c>true</c> if the MasterPage is presented.</value>
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
		/// Gets or sets a value indicating whether a MasterDetailPage allows showing MasterPage with swipe gesture.
		/// </summary>
		/// <value><c>true</c> if the MasterPage can be revealed with a gesture.</value>
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
		/// Gets or Sets the portion of the screen that the MasterPage takes in split mode.
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
		/// Gets or sets the portion of the screen that the MasterPage takes in Popover mode.
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
		/// <param name="page">Master or Detail page to be updated.</param>
		void UpdatePageGeometry(EvasObject page)
		{
			if (page != null)
			{
				if (_master == page)
				{
					// update the geometry of the master page
					page.Geometry = _masterCanvas.Geometry;
				}
				else if (_detail == page)
				{
					// update the geometry of the detail page
					page.Geometry = _detailCanvas.Geometry;
				}
			}
		}

		/// <summary>
		/// Updates <see cref="_internalMasterBehavior"/> according to <see cref="MasterDetailBehavior"/> set by the user and current screen orientation.
		/// </summary>
		void UpdateMasterBehavior()
		{
			var behavior = (_masterBehavior == MasterBehavior.Default) ? s_defaultMasterBehavior : _masterBehavior;

			// Screen orientation affects those 2 behaviors
			if (behavior == MasterBehavior.SplitOnLandscape ||
				behavior == MasterBehavior.SplitOnPortrait)
			{
				var orientation = Device.Info.CurrentOrientation;

				if ((orientation.IsLandscape() && behavior == MasterBehavior.SplitOnLandscape) ||
					(orientation.IsPortrait() && behavior == MasterBehavior.SplitOnPortrait))
				{
					behavior = MasterBehavior.Split;
				}
				else
				{
					behavior = MasterBehavior.Popover;
				}
			}

			if (behavior != _internalMasterBehavior)
			{
				_internalMasterBehavior = behavior;
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

			_splitPane.SetPartContent("left", null, true);
			_splitPane.SetPartContent("right", null, true);
			_splitPane.Hide();

			UnPackAll();

			// the structure for split mode and for popover mode looks differently
			if (IsSplit)
			{
				_splitPane.SetPartContent("left", _masterCanvas, true);
				_splitPane.SetPartContent("right", _detailCanvas, true);
				_splitPane.Show();
				_mainWidget = _splitPane;
				PackEnd(_splitPane);

				IsPresented = true;
				UpdateIsPresentChangeable?.Invoke(this, new UpdateIsPresentChangeableEventArgs(false));
				UpdateFocusPolicy(true);
			}
			else
			{
				_drawer.SetContent(_masterCanvas, true);
				_drawer.Show();
				_mainWidget = _detailCanvas;
				PackEnd(_detailCanvas);
				PackEnd(_drawer);

				_drawer.IsOpen = IsPresented;
				UpdateIsPresentChangeable?.Invoke(this, new UpdateIsPresentChangeableEventArgs(true));
				UpdateFocusPolicy();
			}

			_masterCanvas.Show();
			_detailCanvas.Show();

			// even though child was changed, Layout callback was not called, so i manually call layout function.
			// Layout callback was filter out when geometry was not changed in Native.Box
			UpdateChildCanvasGeometry();
		}

		void UpdateChildCanvasGeometry()
		{
			var bound = Geometry;
			// main widget should fill the area of the MasterDetailPage
			if (_mainWidget != null)
			{
				_mainWidget.Geometry = bound;
			}

			bound.Width = (int)((_popoverRatio * bound.Width));
			_drawer.Geometry = bound;
		}

		/// <summary>
		/// Force update the focus management
		/// </summary>
		void UpdateFocusPolicy(bool forceAllowFocusAll=false)
		{
			var master = _master as Widget;
			var detail = _detail as Widget;

			if(forceAllowFocusAll)
			{
				if (master != null)
					master.AllowTreeFocus = true;
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
				if (master != null)
				{
					master.AllowTreeFocus = true;
					master.SetFocus(true);
				}
			}
			else
			{
				if (master != null)
				{
					master.AllowTreeFocus = false;
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
