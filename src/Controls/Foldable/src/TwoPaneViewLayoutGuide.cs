using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Foldable;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Controls.Foldable
{
	internal class TwoPaneViewLayoutGuide : INotifyPropertyChanged
	{
		public static TwoPaneViewLayoutGuide Instance => _twoPaneViewLayoutGuide.Value;
		static Lazy<TwoPaneViewLayoutGuide> _twoPaneViewLayoutGuide = new Lazy<TwoPaneViewLayoutGuide>(() => new TwoPaneViewLayoutGuide());

		internal IFoldableService DualScreenService
		{
			get
			{
				if (_dualScreenService == null)
				{
					_dualScreenService = _layout?.Handler?.MauiContext?.Services?.GetService<IFoldableService>();
				}

				return _dualScreenService ??
					NoPlatformFoldableService.Instance;
			}
		}

		Rect _hinge;
		Rect _leftPage;
		Rect _rightPane;
		TwoPaneViewMode _mode;
		VisualElement _layout;
		IFoldableService _dualScreenService;
		bool _isLandscape;
		public event PropertyChangedEventHandler PropertyChanged;
		List<string> _pendingPropertyChanges = new List<string>();

		TwoPaneViewLayoutGuide()
		{
		}

		public TwoPaneViewLayoutGuide(VisualElement layout) : this(layout, null)
		{
		}

		internal TwoPaneViewLayoutGuide(VisualElement layout, IFoldableService dualScreenService)
		{
			_layout = layout;

			if (_layout != null)
			{
				UpdateLayouts(layout.Width, layout.Height);
				_layout.HandlerChanged += OnLayoutHandlerChanged;
			}
		}

		void OnLayoutHandlerChanged(object sender, EventArgs e)
		{
			if (_dualScreenService != null)
			{
				_dualScreenService.OnLayoutChanged -= OnDualScreenServiceChanged;
			}

			if (_layout.Handler != null)
			{
				DualScreenService.OnLayoutChanged += OnDualScreenServiceChanged;
			}
		}

		void OnDualScreenServiceChanged(object sender, FoldEventArgs e)
		{
			UpdateLayouts();
		}

		internal void SetFoldableService(IFoldableService foldableService)
		{
			_dualScreenService = foldableService;
			UpdateLayouts();
		}

		public bool IsLandscape
		{
			get => DualScreenService.IsLandscape;
			set => SetProperty(ref _isLandscape, value);
		}

		/// <summary>
		/// Determine whether SinglePane, Tall, or Wide
		/// </summary>
		public TwoPaneViewMode Mode
		{
			get
			{
				if (_layoutWidth == -1)
					return TwoPaneViewMode.SinglePane;

				var mode = GetTwoPaneViewMode(_layoutWidth, _layoutHeight, _hinge);
				return mode;
			}
			private set
			{
				SetProperty(ref _mode, value);
			}
		}

		public Rect Pane1
		{
			get
			{
				return _leftPage;
			}
			private set
			{
				SetProperty(ref _leftPage, value);
			}
		}

		public Rect Pane2
		{
			get
			{
				return _rightPane;
			}
			private set
			{
				SetProperty(ref _rightPane, value);
			}
		}

		public Rect Hinge
		{
			get
			{
				return DualScreenService.GetHinge();
			}
			private set
			{
				SetProperty(ref _hinge, value);
			}
		}

		Rect GetContainerArea(double width, double height)
		{
			System.Diagnostics.Debug.Write($"TwoPaneViewLayoutGuide.GetContainerArea {width},{height}", "JWM");
			Rect containerArea;
			if (_layout == null)
			{
				containerArea = new Rect(Point.Zero, DualScreenService.ScaledScreenSize);
			}
			else
			{
				containerArea = new Rect(_layout.X, _layout.Y, width, height);
			}

			return containerArea;
		}

		Rect GetScreenRelativeBounds(double width, double height)
		{
			Rect containerArea;
			if (_layout == null)
			{
				containerArea = new Rect(Point.Zero, DualScreenService.ScaledScreenSize);
			}
			else
			{
				var locationOnScreen = DualScreenService.GetLocationOnScreen(_layout);
				if (!locationOnScreen.HasValue)
					return Rect.Zero;

				containerArea = new Rect(locationOnScreen.Value, new Size(width, height));
			}

			return containerArea;
		}

		double _layoutWidth = -1;
		double _layoutHeight = -1;


		void UpdateLayouts()
		{
			if (_layoutWidth > 0 && _layoutHeight > 0)
				UpdateLayouts(_layoutWidth, _layoutHeight);
		}

		internal void UpdateLayouts(double width, double height)
		{
			_layoutWidth = width;
			_layoutHeight = height;

			Rect containerArea = GetContainerArea(width, height);
			System.Diagnostics.Debug.Write($"TwoPaneViewLayoutGuide.UpdateLayouts containerArea {containerArea}", "JWM");
			if (containerArea.Width <= 0)
			{
				return;
			}

			// Pane dimensions are calculated relative to the layout container
			Rect _newPane1 = Pane1;
			Rect _newPane2 = Pane2;
			var locationOnScreen = GetScreenRelativeBounds(width, height);
			if (locationOnScreen == Rect.Zero && Hinge == Rect.Zero)
				locationOnScreen = containerArea;

			bool isSpanned = IsInMultipleRegions(locationOnScreen);
			bool hingeIsVertical = Hinge.Height > Hinge.Width;

			if (isSpanned)
			{
				if (hingeIsVertical)
				{
					var pane2X = Hinge.X + Hinge.Width;
					var containerRightX = locationOnScreen.X + locationOnScreen.Width;
					var pane2Width = containerRightX - pane2X;

					_newPane1 = new Rect(0, 0, Hinge.X - locationOnScreen.X, locationOnScreen.Height);
					_newPane2 = new Rect(_newPane1.Width + Hinge.Width, 0, pane2Width, locationOnScreen.Height);
				}
				else
				{
					var pane2Y = Hinge.Y + Hinge.Height;
					var containerBottomY = locationOnScreen.Y + locationOnScreen.Height;
					var pane2Height = containerBottomY - pane2Y;

					_newPane1 = new Rect(0, 0, locationOnScreen.Width, Hinge.Y - locationOnScreen.Y);
					_newPane2 = new Rect(0, _newPane1.Height + Hinge.Height, locationOnScreen.Width, pane2Height);
				}
			}

			else
			{   // NOT spanned
				if (DualScreenService.IsLandscape)
				{
					// Check if part of the layout is underneath the hinge
					var containerRightX = locationOnScreen.X + locationOnScreen.Width;
					var hingeRightX = Hinge.X + Hinge.Width;

					// Right side under hinge
					if (containerRightX > Hinge.X && containerRightX < hingeRightX)
					{
						_newPane1 = new Rect(0, 0, Hinge.X - locationOnScreen.X, locationOnScreen.Height);
					}
					// left side under hinge
					else if (Hinge.X < locationOnScreen.X && hingeRightX > locationOnScreen.X)
					{
						var amountObscured = hingeRightX - locationOnScreen.X;
						_newPane1 = new Rect(amountObscured, 0, locationOnScreen.Width - amountObscured, locationOnScreen.Height);
					}
					else
						_newPane1 = new Rect(0, 0, locationOnScreen.Width, locationOnScreen.Height);

					_newPane2 = Rect.Zero;
				}
				else // isPortrait
				{
					// Check if part of the layout is underneath the hinge
					var containerBottomY = locationOnScreen.Y + locationOnScreen.Height;
					var hingeBottomY = Hinge.Y + Hinge.Height;

					// bottom under hinge
					if (containerBottomY > Hinge.Y && containerBottomY < hingeBottomY)
					{
						_newPane1 = new Rect(0, 0, locationOnScreen.Width, Hinge.Y - locationOnScreen.Y);
					}
					// top under hinge
					else if (Hinge.Y < locationOnScreen.Y && hingeBottomY > locationOnScreen.Y)
					{
						var amountObscured = hingeBottomY - locationOnScreen.Y;
						_newPane1 = new Rect(0, amountObscured, locationOnScreen.Width, locationOnScreen.Height - amountObscured);
					}
					else
						_newPane1 = new Rect(0, 0, locationOnScreen.Width, locationOnScreen.Height);

					_newPane2 = Rect.Zero;
				}
			}

			if (_newPane2.Height < 0 || _newPane2.Width < 0)
				_newPane2 = Rect.Zero;

			if (_newPane1.Height < 0 || _newPane1.Width < 0)
				_newPane1 = Rect.Zero;

			Pane1 = _newPane1;
			Pane2 = _newPane2;
			Mode = GetTwoPaneViewMode(width, height, _hinge);
			Hinge = DualScreenService.GetHinge();
			IsLandscape = DualScreenService.IsLandscape;

			var properties = _pendingPropertyChanges.ToList();
			_pendingPropertyChanges.Clear();

			foreach (var property in properties)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
			}
		}

		/// <summary>
		/// Determines whether the layoutBounds describes an area on both sides of
		/// a hinge/fold
		/// </summary>
		/// <param name="layoutBounds">Coordinates of the view being tested</param>
		/// <returns>true if layoutBounds intersects the hinge/fold</returns>
		bool IsInMultipleRegions(Rect layoutBounds)
		{
			bool isInMultipleRegions = false;
			var hinge = DualScreenService.GetHinge();
			bool hingeIsVertical = Hinge.Height > Hinge.Width;

			if (hingeIsVertical)
			{
				// Check that the control is over the split
				if (layoutBounds.Y < hinge.Y && layoutBounds.Y + layoutBounds.Height > (hinge.Y + hinge.Height))
				{
					isInMultipleRegions = true; // TODO: investigate cases where this is hit
				}

				// Check that the control is over the split
				if (layoutBounds.X < hinge.X && layoutBounds.X + layoutBounds.Width > (hinge.X + hinge.Width))
				{
					isInMultipleRegions = true;
				}
			}
			else // Portrait
			{
				// Check that the control is over the split
				if (layoutBounds.Y < hinge.Y && layoutBounds.Y + layoutBounds.Height > (hinge.Y + hinge.Height))
				{
					isInMultipleRegions = true;
				}

				// Check that the control is over the split
				if (layoutBounds.X < hinge.X && layoutBounds.X + layoutBounds.Width > (hinge.X + hinge.Width))
				{
					isInMultipleRegions = true; // TODO: investigate cases where this is hit
				}
			}
			System.Diagnostics.Debug.Write($"TwoPaneViewLayoutGuide.IsInMultipleRegions:{isInMultipleRegions} layoutBounds:{layoutBounds} == ", "JWM");
			return isInMultipleRegions;
		}

		/// <summary>
		/// Determines SinglePane, Wide, or Tall; which is used to lay out the underlying
		/// Grid in a horizontal or vertical row.
		/// </summary>
		TwoPaneViewMode GetTwoPaneViewMode(double width, double height, Rect hinge)
		{
			// TODO: ideally this would also return SinglePane if isSeparating were false to mimic Samsung Flex Mode
			if (!IsInMultipleRegions(GetScreenRelativeBounds(width, height)))
				return TwoPaneViewMode.SinglePane;

			// Hinge/fold orientation determines the direction to stack the views,
			// NOT the portrait/landscape orientation of the screen's outer dimensions
			if (hinge.Height > hinge.Width)
			{
				/* Vertical hinge/fold
				  Surface Duo       Fold           Flip
				 +------------+    +-------+    +-----------+
				 |     ||     |    |   |   |    |     |     |
				 |     ||     |    |   |   |    +-----------+
				 |     ||     |	   |   |   |     
				 +------------+	   +-------+     
				  Landscape        Portrait     Landscape
				 */
				return TwoPaneViewMode.Wide;
			}
			/*   Horizontal hinge/fold
			     Surface Duo     Fold          Flip
			     +--------+    +--------+     +----+
				 |        |    |        |     |    |
			     |        |    |--------|     |    |
				 |========|    |        |     |----| 
				 |        |    +--------+     |    | 
				 |        |                   |    |
			     +--------+	                  +----+
			      Portrait     Landscape      Portrait
			 */
			return TwoPaneViewMode.Tall;
		}

		protected bool SetProperty<T>(ref T backingStore, T value,
			[CallerMemberName] string propertyName = "",
			Action onChanged = null)
		{
			if (EqualityComparer<T>.Default.Equals(backingStore, value))
				return false;

			backingStore = value;
			onChanged?.Invoke();
			_pendingPropertyChanges.Add(propertyName);
			return true;
		}
	}
}