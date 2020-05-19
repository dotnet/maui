using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.DualScreen
{
	internal class TwoPaneViewLayoutGuide : INotifyPropertyChanged
	{
		public static TwoPaneViewLayoutGuide Instance => _twoPaneViewLayoutGuide.Value;
		static Lazy<TwoPaneViewLayoutGuide> _twoPaneViewLayoutGuide = new Lazy<TwoPaneViewLayoutGuide>(() => new TwoPaneViewLayoutGuide());

		IDualScreenService DualScreenService =>
			_dualScreenService ?? DependencyService.Get<IDualScreenService>() ?? NoDualScreenServiceImpl.Instance;

		Rectangle _hinge;
		Rectangle _leftPage;
		Rectangle _rightPane;
		TwoPaneViewMode _mode;
		VisualElement _layout;
		readonly IDualScreenService _dualScreenService;
		bool _isLandscape;
		public event PropertyChangedEventHandler PropertyChanged;
		List<string> _pendingPropertyChanges = new List<string>();
		Rectangle _absoluteLayoutPosition;
		object _watchHandle = null;

		TwoPaneViewLayoutGuide()
		{
			
		}

		public TwoPaneViewLayoutGuide(VisualElement layout) : this(layout, null)
		{
		}


		internal TwoPaneViewLayoutGuide(VisualElement layout, IDualScreenService dualScreenService)
		{
			_layout = layout;
			_dualScreenService = dualScreenService;

			if(_layout != null)
			{
				UpdateLayouts();
				_layout.PropertyChanged += OnLayoutPropertyChanged;
				_layout.PropertyChanging += OnLayoutPropertyChanging;
				WatchForChanges();
			}
		}

		void OnLayoutPropertyChanging(object sender, PropertyChangingEventArgs e)
		{
			if (e.PropertyName == "Renderer")
				StopWatchingForChanges();
		}

		void OnLayoutPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Renderer")
				WatchForChanges();
		}

		public void WatchForChanges()
		{
			StopWatchingForChanges();

			if (_layout != null)
			{
				_watchHandle = DualScreenService.WatchForChangesOnLayout(_layout, () => OnScreenChanged(DualScreenService, EventArgs.Empty));

				if (_watchHandle == null)
					return;
			}

			DualScreenService.OnScreenChanged += OnScreenChanged;
		}

		public void StopWatchingForChanges()
		{
			DualScreenService.OnScreenChanged -= OnScreenChanged;

			if (_layout != null)
			{
				DualScreenService.StopWatchingForChangesOnLayout(_layout, _watchHandle);
				_watchHandle = null;
			}
		}

		void OnScreenChanged(object sender, EventArgs e)
		{
			if (_layout == null)
			{
				UpdateLayouts();
				return;
			}

			var screenPosition = DualScreenService.GetLocationOnScreen(_layout);
			if (screenPosition == null)
				return;

			var newPosition = new Rectangle(screenPosition.Value, _layout.Bounds.Size);

			if (newPosition != _absoluteLayoutPosition)
			{
				_absoluteLayoutPosition = newPosition;
				UpdateLayouts();
			}
		}

		public bool IsLandscape
		{
			get => DualScreenService.IsLandscape;
			set => SetProperty(ref _isLandscape, value);
		}

		public TwoPaneViewMode Mode
		{
			get
			{
				return GetTwoPaneViewMode();
			}
			private set
			{
				SetProperty(ref _mode, value);
			}
		}

		public Rectangle Pane1
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

		public Rectangle Pane2
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

		public Rectangle Hinge
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

		Rectangle GetContainerArea()
		{
			Rectangle containerArea;
			if (_layout == null)
			{
				containerArea = new Rectangle(Point.Zero, DualScreenService.ScaledScreenSize);
			}
			else
			{
				containerArea = _layout.Bounds;
			}

			return containerArea;
		}

		Rectangle GetScreenRelativeBounds()
		{
			Rectangle containerArea;
			if (_layout == null)
			{
				containerArea = new Rectangle(Point.Zero, DualScreenService.ScaledScreenSize);
			}
			else
			{
				var locationOnScreen = DualScreenService.GetLocationOnScreen(_layout);
				if (!locationOnScreen.HasValue)
					return Rectangle.Zero;

				containerArea = new Rectangle(locationOnScreen.Value, _layout.Bounds.Size);
			}

			return containerArea;

		}

		internal void UpdateLayouts()
		{
			Rectangle containerArea = GetContainerArea();
			if (containerArea.Width <= 0)
			{
				return;
			}

			// Pane dimensions are calculated relative to the layout container
			Rectangle _newPane1 = Pane1;
			Rectangle _newPane2 = Pane2;
			var locationOnScreen = GetScreenRelativeBounds();
			if (locationOnScreen == Rectangle.Zero && Hinge == Rectangle.Zero)
				locationOnScreen = containerArea;

			bool isSpanned = IsInMultipleRegions(locationOnScreen);

			if (!DualScreenService.IsLandscape)
			{
				if (isSpanned)
				{
					var pane2X = Hinge.X + Hinge.Width;
					var containerRightX = locationOnScreen.X + locationOnScreen.Width;
					var pane2Width = containerRightX - pane2X;

					_newPane1 = new Rectangle(0, 0, Hinge.X - locationOnScreen.X, locationOnScreen.Height);
					_newPane2 = new Rectangle(_newPane1.Width + Hinge.Width, 0, pane2Width, locationOnScreen.Height);
				}
				else
				{
					// Check if part of the layout is underneath the hinge
					var containerRightX = locationOnScreen.X + locationOnScreen.Width;
					var hingeRightX = Hinge.X + Hinge.Width;

					// Right side under hinge
					if (containerRightX > Hinge.X && containerRightX < hingeRightX)
					{
						_newPane1 = new Rectangle(0, 0, Hinge.X - locationOnScreen.X, locationOnScreen.Height);
					}
					// left side under hinge
					else if (Hinge.X < locationOnScreen.X && hingeRightX > locationOnScreen.X)
					{
						var amountObscured = hingeRightX - locationOnScreen.X;
						_newPane1 = new Rectangle(amountObscured, 0, locationOnScreen.Width - amountObscured, locationOnScreen.Height);
					}
					else
						_newPane1 = new Rectangle(0, 0, locationOnScreen.Width, locationOnScreen.Height);

					_newPane2 = Rectangle.Zero;
				}
			}
			else
			{
				if (isSpanned)
				{
					var pane2Y = Hinge.Y + Hinge.Height;
					var containerBottomY = locationOnScreen.Y + locationOnScreen.Height;
					var pane2Height = containerBottomY - pane2Y;

					_newPane1 = new Rectangle(0, 0, locationOnScreen.Width, Hinge.Y - locationOnScreen.Y);
					_newPane2 = new Rectangle(0, _newPane1.Height + Hinge.Height, locationOnScreen.Width, pane2Height);
				}
				else
				{
					// Check if part of the layout is underneath the hinge
					var containerBottomY = locationOnScreen.Y + locationOnScreen.Height;
					var hingeBottomY = Hinge.Y + Hinge.Height;

					// bottom under hinge
					if (containerBottomY > Hinge.Y && containerBottomY < hingeBottomY)
					{
						_newPane1 = new Rectangle(0, 0, locationOnScreen.Width, Hinge.Y - locationOnScreen.Y);
					}
					// top under hinge
					else if (Hinge.Y < locationOnScreen.Y && hingeBottomY > locationOnScreen.Y)
					{
						var amountObscured = hingeBottomY - locationOnScreen.Y;
						_newPane1 = new Rectangle(0, amountObscured, locationOnScreen.Width, locationOnScreen.Height - amountObscured);
					}
					else
						_newPane1 = new Rectangle(0, 0, locationOnScreen.Width, locationOnScreen.Height);

					_newPane2 = Rectangle.Zero;
				}
			}

			if (_newPane2.Height < 0 || _newPane2.Width < 0)
				_newPane2 = Rectangle.Zero;

			if (_newPane1.Height < 0 || _newPane1.Width < 0)
				_newPane1 = Rectangle.Zero;

			Pane1 = _newPane1;
			Pane2 = _newPane2;
			Mode = GetTwoPaneViewMode();
			Hinge = DualScreenService.GetHinge();
			IsLandscape = DualScreenService.IsLandscape;

			var properties = _pendingPropertyChanges.ToList();
			_pendingPropertyChanges.Clear();

			foreach (var property in properties)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
			}
		}


		bool IsInMultipleRegions(Rectangle layoutBounds)
		{
			bool isInMultipleRegions = false;
			var hinge = DualScreenService.GetHinge();

			// Portrait
			if (!DualScreenService.IsLandscape)
			{
				// Check that the control is over the split
				if (layoutBounds.X < hinge.X && layoutBounds.X + layoutBounds.Width > (hinge.X + hinge.Width))
				{
					isInMultipleRegions = true;
				}
			}
			else
			{
				// Check that the control is over the split
				if (layoutBounds.Y < hinge.Y && layoutBounds.Y + layoutBounds.Height > (hinge.Y + hinge.Height))
				{
					isInMultipleRegions = true;
				}
			}

			return isInMultipleRegions;
		}

		TwoPaneViewMode GetTwoPaneViewMode()
		{
			if (!IsInMultipleRegions(GetScreenRelativeBounds()))
				return TwoPaneViewMode.SinglePane;

			if (DualScreenService.IsLandscape)
				return TwoPaneViewMode.Tall;

			return TwoPaneViewMode.Wide;
		}

		protected bool SetProperty<T>(ref T backingStore, T value,
			[CallerMemberName]string propertyName = "",
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