using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;
//using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Geometry = Microsoft.Maui.Controls.Shapes.Geometry;
using Rectangle = Microsoft.Maui.Graphics.Rectangle;

namespace Microsoft.Maui.Controls
{
	public partial class VisualElement : NavigableElement, IAnimatable, IVisualElementController, IResourcesProvider, IStyleElement, IFlowDirectionController, IPropertyPropagationController, IVisualController, ITabStopElement
	{
		public new static readonly BindableProperty NavigationProperty = NavigableElement.NavigationProperty;

		public new static readonly BindableProperty StyleProperty = NavigableElement.StyleProperty;

		public static readonly BindableProperty InputTransparentProperty = BindableProperty.Create("InputTransparent", typeof(bool), typeof(VisualElement), default(bool));

		public static readonly BindableProperty IsEnabledProperty = BindableProperty.Create("IsEnabled", typeof(bool),
			typeof(VisualElement), true, propertyChanged: OnIsEnabledPropertyChanged);

		static readonly BindablePropertyKey XPropertyKey = BindableProperty.CreateReadOnly("X", typeof(double), typeof(VisualElement), default(double));

		public static readonly BindableProperty XProperty = XPropertyKey.BindableProperty;

		static readonly BindablePropertyKey YPropertyKey = BindableProperty.CreateReadOnly("Y", typeof(double), typeof(VisualElement), default(double));

		public static readonly BindableProperty YProperty = YPropertyKey.BindableProperty;

		public static readonly BindableProperty AnchorXProperty = BindableProperty.Create("AnchorX", typeof(double), typeof(VisualElement), .5d);

		public static readonly BindableProperty AnchorYProperty = BindableProperty.Create("AnchorY", typeof(double), typeof(VisualElement), .5d);

		public static readonly BindableProperty TranslationXProperty = BindableProperty.Create("TranslationX", typeof(double), typeof(VisualElement), 0d);

		public static readonly BindableProperty TranslationYProperty = BindableProperty.Create("TranslationY", typeof(double), typeof(VisualElement), 0d);

		static readonly BindablePropertyKey WidthPropertyKey = BindableProperty.CreateReadOnly("Width", typeof(double), typeof(VisualElement), -1d,
			coerceValue: (bindable, value) => double.IsNaN((double)value) ? 0d : value);

		public static readonly BindableProperty WidthProperty = WidthPropertyKey.BindableProperty;

		static readonly BindablePropertyKey HeightPropertyKey = BindableProperty.CreateReadOnly("Height", typeof(double), typeof(VisualElement), -1d,
			coerceValue: (bindable, value) => double.IsNaN((double)value) ? 0d : value);

		public static readonly BindableProperty HeightProperty = HeightPropertyKey.BindableProperty;

		public static readonly BindableProperty RotationProperty = BindableProperty.Create("Rotation", typeof(double), typeof(VisualElement), default(double));

		public static readonly BindableProperty RotationXProperty = BindableProperty.Create("RotationX", typeof(double), typeof(VisualElement), default(double));

		public static readonly BindableProperty RotationYProperty = BindableProperty.Create("RotationY", typeof(double), typeof(VisualElement), default(double));

		public static readonly BindableProperty ScaleProperty = BindableProperty.Create(nameof(Scale), typeof(double), typeof(VisualElement), 1d);

		public static readonly BindableProperty ScaleXProperty = BindableProperty.Create(nameof(ScaleX), typeof(double), typeof(VisualElement), 1d);

		public static readonly BindableProperty ScaleYProperty = BindableProperty.Create(nameof(ScaleY), typeof(double), typeof(VisualElement), 1d);

		internal static readonly BindableProperty TransformProperty = BindableProperty.Create("Transform", typeof(string), typeof(VisualElement), null, propertyChanged: OnTransformChanged);

		public static readonly BindableProperty ClipProperty = BindableProperty.Create(nameof(Clip), typeof(Geometry), typeof(VisualElement), null,
			propertyChanging: (bindable, oldvalue, newvalue) =>
			{
				if (oldvalue != null)
					(bindable as VisualElement)?.StopNotifyingClipChanges();
			},
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				if (newvalue != null)
					(bindable as VisualElement)?.NotifyClipChanges();
			});

		void NotifyClipChanges()
		{
			if (Clip != null)
			{
				Clip.PropertyChanged += OnClipChanged;

				if (Clip is GeometryGroup geometryGroup)
					geometryGroup.InvalidateGeometryRequested += InvalidateGeometryRequested;
			}
		}

		void StopNotifyingClipChanges()
		{
			if (Clip != null)
			{
				Clip.PropertyChanged -= OnClipChanged;

				if (Clip is GeometryGroup geometryGroup)
					geometryGroup.InvalidateGeometryRequested -= InvalidateGeometryRequested;
			}
		}

		void OnClipChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(nameof(Clip));
		}

		void InvalidateGeometryRequested(object sender, EventArgs e)
		{
			OnPropertyChanged(nameof(Clip));
		}

		public static readonly BindableProperty VisualProperty =
			BindableProperty.Create(nameof(Visual), typeof(IVisual), typeof(VisualElement), Maui.Controls.VisualMarker.MatchParent,
									validateValue: (b, v) => v != null, propertyChanged: OnVisualChanged);

		static IVisual _defaultVisual = Microsoft.Maui.Controls.VisualMarker.Default;
		IVisual _effectiveVisual = _defaultVisual;

		public IVisual Visual
		{
			get { return (IVisual)GetValue(VisualProperty); }
			set { SetValue(VisualProperty, value); }
		}

		internal static void SetDefaultVisual(IVisual visual) => _defaultVisual = visual;

		IVisual IVisualController.EffectiveVisual
		{
			get { return _effectiveVisual; }
			set
			{
				if (value == _effectiveVisual)
					return;

				_effectiveVisual = value;
				OnPropertyChanged(VisualProperty.PropertyName);
			}
		}

		static void OnTransformChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if ((string)newValue == "none")
			{
				bindable.ClearValue(TranslationXProperty);
				bindable.ClearValue(TranslationYProperty);
				bindable.ClearValue(RotationProperty);
				bindable.ClearValue(RotationXProperty);
				bindable.ClearValue(RotationYProperty);
				bindable.ClearValue(ScaleProperty);
				bindable.ClearValue(ScaleXProperty);
				bindable.ClearValue(ScaleYProperty);
				return;
			}
			var transforms = ((string)newValue).Split(' ');
			foreach (var transform in transforms)
			{
				if (string.IsNullOrEmpty(transform) || transform.IndexOf('(') < 0 || transform.IndexOf(')') < 0)
					throw new FormatException("Format for transform is 'none | transform(value) [transform(value) ]*'");
				var transformName = transform.Substring(0, transform.IndexOf('('));
				var value = transform.Substring(transform.IndexOf('(') + 1, transform.IndexOf(')') - transform.IndexOf('(') - 1);
				double translationX, translationY, scaleX, scaleY, rotateX, rotateY, rotate;
				if (transformName.StartsWith("translateX", StringComparison.OrdinalIgnoreCase) && double.TryParse(value, out translationX))
					bindable.SetValue(TranslationXProperty, translationX);
				else if (transformName.StartsWith("translateY", StringComparison.OrdinalIgnoreCase) && double.TryParse(value, out translationY))
					bindable.SetValue(TranslationYProperty, translationY);
				else if (transformName.StartsWith("translate", StringComparison.OrdinalIgnoreCase))
				{
					var translate = value.Split(',');
					if (double.TryParse(translate[0], out translationX) && double.TryParse(translate[1], out translationY))
					{
						bindable.SetValue(TranslationXProperty, translationX);
						bindable.SetValue(TranslationYProperty, translationY);
					}
				}
				else if (transformName.StartsWith("scaleX", StringComparison.OrdinalIgnoreCase) && double.TryParse(value, out scaleX))
					bindable.SetValue(ScaleXProperty, scaleX);
				else if (transformName.StartsWith("scaleY", StringComparison.OrdinalIgnoreCase) && double.TryParse(value, out scaleY))
					bindable.SetValue(ScaleYProperty, scaleY);
				else if (transformName.StartsWith("scale", StringComparison.OrdinalIgnoreCase))
				{
					var scale = value.Split(',');
					if (double.TryParse(scale[0], out scaleX) && double.TryParse(scale[1], out scaleY))
					{
						bindable.SetValue(ScaleXProperty, scaleX);
						bindable.SetValue(ScaleYProperty, scaleY);
					}
				}
				else if (transformName.StartsWith("rotateX", StringComparison.OrdinalIgnoreCase) && double.TryParse(value, out rotateX))
					bindable.SetValue(RotationXProperty, rotateX);
				else if (transformName.StartsWith("rotateY", StringComparison.OrdinalIgnoreCase) && double.TryParse(value, out rotateY))
					bindable.SetValue(RotationYProperty, rotateY);
				else if (transformName.StartsWith("rotate", StringComparison.OrdinalIgnoreCase) && double.TryParse(value, out rotate))
					bindable.SetValue(RotationProperty, rotate);
				else
					throw new FormatException("Invalid transform name");
			}
		}

		internal static readonly BindableProperty TransformOriginProperty =
			BindableProperty.Create("TransformOrigin", typeof(Point), typeof(VisualElement), new Point(.5d, .5d),
									propertyChanged: (b, o, n) => { (((VisualElement)b).AnchorX, ((VisualElement)b).AnchorY) = (Point)n; });

		public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create("IsVisible", typeof(bool), typeof(VisualElement), true,
			propertyChanged: (bindable, oldvalue, newvalue) => ((VisualElement)bindable).OnIsVisibleChanged((bool)oldvalue, (bool)newvalue));

		public static readonly BindableProperty OpacityProperty = BindableProperty.Create("Opacity", typeof(double), typeof(VisualElement), 1d, coerceValue: (bindable, value) => ((double)value).Clamp(0, 1));

		public static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create("BackgroundColor", typeof(Color), typeof(VisualElement), null);

		public static readonly BindableProperty BackgroundProperty = BindableProperty.Create(nameof(Background), typeof(Brush), typeof(VisualElement), Brush.Default,
			propertyChanging: (bindable, oldvalue, newvalue) =>
			{
				if (oldvalue != null)
					(bindable as VisualElement)?.StopNotifyingBackgroundChanges();
			},
			propertyChanged: (bindable, oldvalue, newvalue) =>
			{
				if (newvalue != null)
					(bindable as VisualElement)?.NotifyBackgroundChanges();
			});

		void NotifyBackgroundChanges()
		{
			if (Background != null)
			{
				Background.Parent = this;
				Background.PropertyChanged += OnBackgroundChanged;

				if (Background is GradientBrush gradientBrush)
					gradientBrush.InvalidateGradientBrushRequested += InvalidateGradientBrushRequested;
			}
		}

		void StopNotifyingBackgroundChanges()
		{
			if (Background != null)
			{
				Background.Parent = null;
				Background.PropertyChanged -= OnBackgroundChanged;

				if (Background is GradientBrush gradientBrush)
					gradientBrush.InvalidateGradientBrushRequested -= InvalidateGradientBrushRequested;
			}
		}

		void OnBackgroundChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(nameof(Background));
		}

		void InvalidateGradientBrushRequested(object sender, EventArgs e)
		{
			OnPropertyChanged(nameof(Background));
		}

		internal static readonly BindablePropertyKey BehaviorsPropertyKey = BindableProperty.CreateReadOnly("Behaviors", typeof(IList<Behavior>), typeof(VisualElement), default(IList<Behavior>),
			defaultValueCreator: bindable =>
			{
				var collection = new AttachedCollection<Behavior>();
				collection.AttachTo(bindable);
				return collection;
			});

		public static readonly BindableProperty BehaviorsProperty = BehaviorsPropertyKey.BindableProperty;

		internal static readonly BindablePropertyKey TriggersPropertyKey = BindableProperty.CreateReadOnly("Triggers", typeof(IList<TriggerBase>), typeof(VisualElement), default(IList<TriggerBase>),
			defaultValueCreator: bindable =>
			{
				var collection = new AttachedCollection<TriggerBase>();
				collection.AttachTo(bindable);
				return collection;
			});

		public static readonly BindableProperty TriggersProperty = TriggersPropertyKey.BindableProperty;


		public static readonly BindableProperty WidthRequestProperty = BindableProperty.Create("WidthRequest", typeof(double), typeof(VisualElement), -1d, propertyChanged: OnRequestChanged);

		public static readonly BindableProperty HeightRequestProperty = BindableProperty.Create("HeightRequest", typeof(double), typeof(VisualElement), -1d, propertyChanged: OnRequestChanged);

		public static readonly BindableProperty MinimumWidthRequestProperty = BindableProperty.Create("MinimumWidthRequest", typeof(double), typeof(VisualElement), -1d, propertyChanged: OnRequestChanged);

		public static readonly BindableProperty MinimumHeightRequestProperty = BindableProperty.Create("MinimumHeightRequest", typeof(double), typeof(VisualElement), -1d, propertyChanged: OnRequestChanged);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly BindablePropertyKey IsFocusedPropertyKey = BindableProperty.CreateReadOnly("IsFocused",
			typeof(bool), typeof(VisualElement), default(bool), propertyChanged: OnIsFocusedPropertyChanged);

		public static readonly BindableProperty IsFocusedProperty = IsFocusedPropertyKey.BindableProperty;

		public static readonly BindableProperty FlowDirectionProperty = BindableProperty.Create(nameof(FlowDirection), typeof(FlowDirection), typeof(VisualElement), FlowDirection.MatchParent, propertyChanging: FlowDirectionChanging, propertyChanged: FlowDirectionChanged);

		public static readonly BindableProperty TabIndexProperty =
			BindableProperty.Create(nameof(TabIndex),
									typeof(int),
									typeof(VisualElement),
									defaultValue: 0,
									propertyChanged: OnTabIndexPropertyChanged,
									defaultValueCreator: TabIndexDefaultValueCreator);

		public static readonly BindableProperty IsTabStopProperty =
			BindableProperty.Create(nameof(IsTabStop),
									typeof(bool),
									typeof(VisualElement),
									defaultValue: true,
									propertyChanged: OnTabStopPropertyChanged,
									defaultValueCreator: TabStopDefaultValueCreator);

		static void OnTabIndexPropertyChanged(BindableObject bindable, object oldValue, object newValue) =>
			((VisualElement)bindable).OnTabIndexPropertyChanged((int)oldValue, (int)newValue);

		static object TabIndexDefaultValueCreator(BindableObject bindable) =>
			((VisualElement)bindable).TabIndexDefaultValueCreator();

		static void OnTabStopPropertyChanged(BindableObject bindable, object oldValue, object newValue) =>
			((VisualElement)bindable).OnTabStopPropertyChanged((bool)oldValue, (bool)newValue);

		static object TabStopDefaultValueCreator(BindableObject bindable) =>
			((VisualElement)bindable).TabStopDefaultValueCreator();

		IFlowDirectionController FlowController => this;

		public FlowDirection FlowDirection
		{
			get { return (FlowDirection)GetValue(FlowDirectionProperty); }
			set { SetValue(FlowDirectionProperty, value); }
		}

		EffectiveFlowDirection _effectiveFlowDirection = default(EffectiveFlowDirection);
		EffectiveFlowDirection IFlowDirectionController.EffectiveFlowDirection
		{
			get => _effectiveFlowDirection;
			set => SetEffectiveFlowDirection(value, true);
		}

		void SetEffectiveFlowDirection(EffectiveFlowDirection value, bool fireFlowDirectionPropertyChanged)
		{
			if (value == _effectiveFlowDirection)
				return;

			_effectiveFlowDirection = value;
			InvalidateMeasureInternal(InvalidationTrigger.Undefined);

			if (fireFlowDirectionPropertyChanged)
				OnPropertyChanged(FlowDirectionProperty.PropertyName);

		}

		EffectiveFlowDirection IVisualElementController.EffectiveFlowDirection => FlowController.EffectiveFlowDirection;

		readonly Dictionary<Size, SizeRequest> _measureCache = new Dictionary<Size, SizeRequest>();



		int _batched;
		LayoutConstraint _computedConstraint;

		bool _isInNativeLayout;

		bool _isNativeStateConsistent = true;

		bool _isPlatformEnabled;

		double _mockHeight = -1;

		double _mockWidth = -1;

		double _mockX = -1;

		double _mockY = -1;

		LayoutConstraint _selfConstraint;

		protected internal VisualElement()
		{
		}

		public double AnchorX
		{
			get { return (double)GetValue(AnchorXProperty); }
			set { SetValue(AnchorXProperty, value); }
		}

		public double AnchorY
		{
			get { return (double)GetValue(AnchorYProperty); }
			set { SetValue(AnchorYProperty, value); }
		}

		public Color BackgroundColor
		{
			get { return (Color)GetValue(BackgroundColorProperty); }
			set { SetValue(BackgroundColorProperty, value); }
		}

		[TypeConverter(typeof(BrushTypeConverter))]
		public Brush Background
		{
			get { return (Brush)GetValue(BackgroundProperty); }
			set { SetValue(BackgroundProperty, value); }
		}

		public IList<Behavior> Behaviors
		{
			get { return (IList<Behavior>)GetValue(BehaviorsProperty); }
		}

		public Rectangle Bounds
		{
			get { return new Rectangle(X, Y, Width, Height); }
			private set
			{
				if (value.X == X && value.Y == Y && value.Height == Height && value.Width == Width)
					return;
				BatchBegin();
				X = value.X;
				Y = value.Y;
				SetSize(value.Width, value.Height);
				BatchCommit();
			}
		}

		public double Height
		{
			get { return _mockHeight == -1 ? (double)GetValue(HeightProperty) : _mockHeight; }
			private set { SetValue(HeightPropertyKey, value); }
		}

		public double HeightRequest
		{
			get { return (double)GetValue(HeightRequestProperty); }
			set { SetValue(HeightRequestProperty, value); }
		}

		public bool InputTransparent
		{
			get { return (bool)GetValue(InputTransparentProperty); }
			set { SetValue(InputTransparentProperty, value); }
		}

		public bool IsEnabled
		{
			get { return (bool)GetValue(IsEnabledProperty); }
			set { SetValue(IsEnabledProperty, value); }
		}

		public bool IsFocused => (bool)GetValue(IsFocusedProperty);

		[TypeConverter(typeof(VisibilityConverter))]
		public bool IsVisible
		{
			get { return (bool)GetValue(IsVisibleProperty); }
			set { SetValue(IsVisibleProperty, value); }
		}

		public double MinimumHeightRequest
		{
			get { return (double)GetValue(MinimumHeightRequestProperty); }
			set { SetValue(MinimumHeightRequestProperty, value); }
		}

		public double MinimumWidthRequest
		{
			get { return (double)GetValue(MinimumWidthRequestProperty); }
			set { SetValue(MinimumWidthRequestProperty, value); }
		}

		public double Opacity
		{
			get { return (double)GetValue(OpacityProperty); }
			set { SetValue(OpacityProperty, value); }
		}

		public double Rotation
		{
			get { return (double)GetValue(RotationProperty); }
			set { SetValue(RotationProperty, value); }
		}

		public double RotationX
		{
			get { return (double)GetValue(RotationXProperty); }
			set { SetValue(RotationXProperty, value); }
		}

		public double RotationY
		{
			get { return (double)GetValue(RotationYProperty); }
			set { SetValue(RotationYProperty, value); }
		}

		public double Scale
		{
			get => (double)GetValue(ScaleProperty);
			set => SetValue(ScaleProperty, value);
		}

		public double ScaleX
		{
			get => (double)GetValue(ScaleXProperty);
			set => SetValue(ScaleXProperty, value);
		}

		public double ScaleY
		{
			get => (double)GetValue(ScaleYProperty);
			set => SetValue(ScaleYProperty, value);
		}

		public int TabIndex
		{
			get => (int)GetValue(TabIndexProperty);
			set => SetValue(TabIndexProperty, value);
		}

		protected virtual void OnTabIndexPropertyChanged(int oldValue, int newValue) { }

		protected virtual int TabIndexDefaultValueCreator() => 0;

		public bool IsTabStop
		{
			get => (bool)GetValue(IsTabStopProperty);
			set => SetValue(IsTabStopProperty, value);
		}

		protected virtual void OnTabStopPropertyChanged(bool oldValue, bool newValue) { }

		protected virtual bool TabStopDefaultValueCreator() => true;

		public double TranslationX
		{
			get { return (double)GetValue(TranslationXProperty); }
			set { SetValue(TranslationXProperty, value); }
		}

		public double TranslationY
		{
			get { return (double)GetValue(TranslationYProperty); }
			set { SetValue(TranslationYProperty, value); }
		}

		public IList<TriggerBase> Triggers => (IList<TriggerBase>)GetValue(TriggersProperty);

		public double Width
		{
			get { return _mockWidth == -1 ? (double)GetValue(WidthProperty) : _mockWidth; }
			private set { SetValue(WidthPropertyKey, value); }
		}

		public double WidthRequest
		{
			get { return (double)GetValue(WidthRequestProperty); }
			set { SetValue(WidthRequestProperty, value); }
		}

		public double X
		{
			get { return _mockX == -1 ? (double)GetValue(XProperty) : _mockX; }
			private set { SetValue(XPropertyKey, value); }
		}

		public double Y
		{
			get { return _mockY == -1 ? (double)GetValue(YProperty) : _mockY; }
			private set { SetValue(YPropertyKey, value); }
		}

		[TypeConverter(typeof(PathGeometryConverter))]
		public Geometry Clip
		{
			get { return (Geometry)GetValue(ClipProperty); }
			set { SetValue(ClipProperty, value); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool Batched => _batched > 0;

		internal LayoutConstraint ComputedConstraint
		{
			get { return _computedConstraint; }
			set
			{
				if (_computedConstraint == value)
					return;

				LayoutConstraint oldConstraint = Constraint;
				_computedConstraint = value;
				LayoutConstraint newConstraint = Constraint;
				if (oldConstraint != newConstraint)
					OnConstraintChanged(oldConstraint, newConstraint);
			}
		}

		internal LayoutConstraint Constraint => ComputedConstraint | SelfConstraint;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool DisableLayout { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool IsInNativeLayout
		{
			get
			{
				if (_isInNativeLayout)
					return true;

				Element parent = RealParent;
				if (parent != null)
				{
					var visualElement = parent as VisualElement;
					if (visualElement != null && visualElement.IsInNativeLayout)
						return true;
				}

				return false;
			}
			set { _isInNativeLayout = value; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool IsNativeStateConsistent
		{
			get { return _isNativeStateConsistent; }
			set
			{
				if (_isNativeStateConsistent == value)
					return;
				_isNativeStateConsistent = value;
				if (value && IsPlatformEnabled)
					InvalidateMeasureInternal(InvalidationTrigger.RendererReady);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		internal event EventHandler PlatformEnabledChanged;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool IsPlatformEnabled
		{
			get { return _isPlatformEnabled; }
			set
			{
				if (value == _isPlatformEnabled)
					return;

				_isPlatformEnabled = value;
				if (value && IsNativeStateConsistent)
					InvalidateMeasureInternal(InvalidationTrigger.RendererReady);

				InvalidateStateTriggers(IsPlatformEnabled);

				OnIsPlatformEnabledChanged();
				PlatformEnabledChanged?.Invoke(this, EventArgs.Empty);
			}
		}

		internal LayoutConstraint SelfConstraint
		{
			get { return _selfConstraint; }
			set
			{
				if (_selfConstraint == value)
					return;

				LayoutConstraint oldConstraint = Constraint;
				_selfConstraint = value;
				LayoutConstraint newConstraint = Constraint;
				if (oldConstraint != newConstraint)
				{
					OnConstraintChanged(oldConstraint, newConstraint);
				}
			}
		}

		public void BatchBegin() => _batched++;

		public void BatchCommit()
		{
			_batched = Math.Max(0, _batched - 1);
			if (!Batched)
			{
				BatchCommitted?.Invoke(this, new EventArg<VisualElement>(this));
				Device.Invalidate(this);
			}
		}

		ResourceDictionary _resources;
		bool IResourcesProvider.IsResourcesCreated => _resources != null;

		public ResourceDictionary Resources
		{
			get
			{
				if (_resources != null)
					return _resources;
				_resources = new ResourceDictionary();
				((IResourceDictionary)_resources).ValuesChanged += OnResourcesChanged;
				return _resources;
			}
			set
			{
				if (_resources == value)
					return;
				OnPropertyChanging();
				if (_resources != null)
					((IResourceDictionary)_resources).ValuesChanged -= OnResourcesChanged;
				_resources = value;
				OnResourcesChanged(value);
				if (_resources != null)
					((IResourceDictionary)_resources).ValuesChanged += OnResourcesChanged;
				OnPropertyChanged();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void NativeSizeChanged() => InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		public event EventHandler ChildrenReordered;

		public bool Focus()
		{
			if (IsFocused)
				return true;

			if (FocusChangeRequested == null)
				return false;

			var arg = new FocusRequestArgs { Focus = true };
			FocusChangeRequested(this, arg);
			return arg.Result;
		}

		public event EventHandler<FocusEventArgs> Focused;

		[Obsolete("OnSizeRequest is obsolete as of version 2.2.0. Please use OnMeasure instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public virtual SizeRequest GetSizeRequest(double widthConstraint, double heightConstraint)
		{
			var constraintSize = new Size(widthConstraint, heightConstraint);
			if (_measureCache.TryGetValue(constraintSize, out SizeRequest cachedResult))
				return cachedResult;

			double widthRequest = WidthRequest;
			double heightRequest = HeightRequest;
			if (widthRequest >= 0)
				widthConstraint = Math.Min(widthConstraint, widthRequest);
			if (heightRequest >= 0)
				heightConstraint = Math.Min(heightConstraint, heightRequest);

			SizeRequest result = OnMeasure(widthConstraint, heightConstraint);
			bool hasMinimum = result.Minimum != result.Request;
			Size request = result.Request;
			Size minimum = result.Minimum;

			if (heightRequest != -1)
			{
				request.Height = heightRequest;
				if (!hasMinimum)
					minimum.Height = heightRequest;
			}

			if (widthRequest != -1)
			{
				request.Width = widthRequest;
				if (!hasMinimum)
					minimum.Width = widthRequest;
			}

			double minimumHeightRequest = MinimumHeightRequest;
			double minimumWidthRequest = MinimumWidthRequest;

			if (minimumHeightRequest != -1)
				minimum.Height = minimumHeightRequest;
			if (minimumWidthRequest != -1)
				minimum.Width = minimumWidthRequest;

			minimum.Height = Math.Min(request.Height, minimum.Height);
			minimum.Width = Math.Min(request.Width, minimum.Width);

			var r = new SizeRequest(request, minimum);

			if (r.Request.Width > 0 && r.Request.Height > 0)
				_measureCache[constraintSize] = r;

			return r;
		}

		public SizeRequest Measure(double widthConstraint, double heightConstraint, MeasureFlags flags = MeasureFlags.None)
		{
			bool includeMargins = (flags & MeasureFlags.IncludeMargins) != 0;
			Thickness margin = default(Thickness);
			if (includeMargins)
			{
				if (this is View view)
					margin = view.Margin;
				widthConstraint = Math.Max(0, widthConstraint - margin.HorizontalThickness);
				heightConstraint = Math.Max(0, heightConstraint - margin.VerticalThickness);
			}
#pragma warning disable 0618 // retain until GetSizeRequest removed
			SizeRequest result = GetSizeRequest(widthConstraint, heightConstraint);
#pragma warning restore 0618

			if (includeMargins && !margin.IsEmpty)
			{
				result.Minimum = new Size(result.Minimum.Width + margin.HorizontalThickness, result.Minimum.Height + margin.VerticalThickness);
				result.Request = new Size(result.Request.Width + margin.HorizontalThickness, result.Request.Height + margin.VerticalThickness);
			}

			DesiredSize = result.Request;
			return result;
		}

		public event EventHandler MeasureInvalidated;

		public event EventHandler SizeChanged;

		public void Unfocus()
		{
			if (!IsFocused)
				return;

			FocusChangeRequested?.Invoke(this, new FocusRequestArgs());
		}

		public event EventHandler<FocusEventArgs> Unfocused;

		protected virtual void InvalidateMeasure() => InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		protected override void OnBindingContextChanged()
		{
			PropagateBindingContextToStateTriggers();

			base.OnBindingContextChanged();
		}

		protected override void OnChildAdded(Element child)
		{
			base.OnChildAdded(child);
			var view = child as View;
			if (view != null)
				ComputeConstraintForView(view);
		}

		protected override void OnChildRemoved(Element child, int oldLogicalIndex)
		{
			base.OnChildRemoved(child, oldLogicalIndex);
			if (child is View view)
				view.ComputedConstraint = LayoutConstraint.None;
		}

		protected void OnChildrenReordered()
			=> ChildrenReordered?.Invoke(this, EventArgs.Empty);

		protected virtual SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
#pragma warning disable 0618 // retain until OnSizeRequest removed
			return OnSizeRequest(widthConstraint, heightConstraint);
#pragma warning restore 0618
		}

		protected virtual void OnSizeAllocated(double width, double height)
		{
		}

		[Obsolete("OnSizeRequest is obsolete as of version 2.2.0. Please use OnMeasure instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		protected virtual SizeRequest OnSizeRequest(double widthConstraint, double heightConstraint)
		{
			if (!IsPlatformEnabled)
				return new SizeRequest(new Size(-1, -1));

			return Device.PlatformServices.GetNativeSize(this, widthConstraint, heightConstraint);
		}

		protected void SizeAllocated(double width, double height) => OnSizeAllocated(width, height);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler<EventArg<VisualElement>> BatchCommitted;

		internal void ComputeConstrainsForChildren()
		{
			for (var i = 0; i < LogicalChildrenInternal.Count; i++)
			{
				if (LogicalChildrenInternal[i] is View child)
					ComputeConstraintForView(child);
			}
		}

		internal virtual void ComputeConstraintForView(View view) => view.ComputedConstraint = LayoutConstraint.None;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler<FocusRequestArgs> FocusChangeRequested;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public void InvalidateMeasureNonVirtual(InvalidationTrigger trigger)
		{
			InvalidateMeasureInternal(trigger);
		}

		internal virtual void InvalidateMeasureInternal(InvalidationTrigger trigger)
		{
			_measureCache.Clear();
			MeasureInvalidated?.Invoke(this, new InvalidationEventArgs(trigger));
		}

		void IVisualElementController.InvalidateMeasure(InvalidationTrigger trigger) => InvalidateMeasureInternal(trigger);

		internal void InvalidateStateTriggers(bool attach)
		{
			if (!this.HasVisualStateGroups())
				return;

			var groups = (IList<VisualStateGroup>)GetValue(VisualStateManager.VisualStateGroupsProperty);

			if (groups.Count == 0)
				return;

			foreach (var group in groups)
				foreach (var state in group.States)
					foreach (var stateTrigger in state.StateTriggers)
					{
						if (attach)
							stateTrigger.SendAttached();
						else
							stateTrigger.SendDetached();
					}
		}

		internal void MockBounds(Rectangle bounds)
		{
#if NETSTANDARD2_0 || NET6_0
			(_mockX, _mockY, _mockWidth, _mockHeight) = bounds;
#else
			_mockX = bounds.X;
			_mockY = bounds.Y;
			_mockWidth = bounds.Width;
			_mockHeight = bounds.Height;
#endif
		}

		internal virtual void OnConstraintChanged(LayoutConstraint oldConstraint, LayoutConstraint newConstraint) => ComputeConstrainsForChildren();

		internal virtual void OnIsPlatformEnabledChanged()
		{
		}

		internal virtual void OnIsVisibleChanged(bool oldValue, bool newValue)
		{
			if (this is IFrameworkElement fe)
			{
				fe.Handler?.UpdateValue(nameof(IFrameworkElement.Visibility));
			}

			InvalidateMeasureInternal(InvalidationTrigger.Undefined);
		}

		internal override void OnParentResourcesChanged(IEnumerable<KeyValuePair<string, object>> values)
		{
			if (values == null)
				return;

			if (!((IResourcesProvider)this).IsResourcesCreated || Resources.Count == 0)
			{
				base.OnParentResourcesChanged(values);
				return;
			}

			var innerKeys = new HashSet<string>();
			var changedResources = new List<KeyValuePair<string, object>>();
			foreach (KeyValuePair<string, object> c in Resources)
				innerKeys.Add(c.Key);
			foreach (KeyValuePair<string, object> value in values)
			{
				if (innerKeys.Add(value.Key))
					changedResources.Add(value);
				else if (value.Key.StartsWith(Style.StyleClassPrefix, StringComparison.Ordinal))
				{
					var mergedClassStyles = new List<Style>(Resources[value.Key] as List<Style>);
					mergedClassStyles.AddRange(value.Value as List<Style>);
					changedResources.Add(new KeyValuePair<string, object>(value.Key, mergedClassStyles));
				}
			}
			if (changedResources.Count != 0)
				OnResourcesChanged(changedResources);
		}

		internal void UnmockBounds() => _mockX = _mockY = _mockWidth = _mockHeight = -1;

		void PropagateBindingContextToStateTriggers()
		{
			var groups = (IList<VisualStateGroup>)GetValue(VisualStateManager.VisualStateGroupsProperty);

			if (groups.Count == 0)
				return;

			foreach (var group in groups)
				foreach (var state in group.States)
					foreach (var stateTrigger in state.StateTriggers)
						SetInheritedBindingContext(stateTrigger, BindingContext);
		}

		void OnFocused() => Focused?.Invoke(this, new FocusEventArgs(this, true));

		internal void ChangeVisualStateInternal() => ChangeVisualState();

		protected internal virtual void ChangeVisualState()
		{
			if (!IsEnabled)
				VisualStateManager.GoToState(this, VisualStateManager.CommonStates.Disabled);
			else if (IsFocused)
				VisualStateManager.GoToState(this, VisualStateManager.CommonStates.Focused);
			else
				VisualStateManager.GoToState(this, VisualStateManager.CommonStates.Normal);
		}

		static void OnVisualChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var self = bindable as IVisualController;
			var newVisual = (IVisual)newValue;

			if (newVisual.IsMatchParent())
				self.EffectiveVisual = Microsoft.Maui.Controls.VisualMarker.Default;
			else
				self.EffectiveVisual = (IVisual)newValue;

			(self as IPropertyPropagationController)?.PropagatePropertyChanged(VisualElement.VisualProperty.PropertyName);
		}

		static void FlowDirectionChanging(BindableObject bindable, object oldValue, object newValue)
		{
			var self = bindable as IFlowDirectionController;

			if (self.EffectiveFlowDirection.IsExplicit() && oldValue == newValue)
				return;

			var newFlowDirection = ((FlowDirection)newValue).ToEffectiveFlowDirection(isExplicit: true);

			if (self is VisualElement ve)
				ve.SetEffectiveFlowDirection(newFlowDirection, false);
			else
				self.EffectiveFlowDirection = newFlowDirection;
		}

		static void FlowDirectionChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as IPropertyPropagationController)?.PropagatePropertyChanged(VisualElement.FlowDirectionProperty.PropertyName);
		}


		static void OnIsEnabledPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var element = (VisualElement)bindable;

			if (element == null)
				return;

			element.ChangeVisualState();
		}

		static void OnIsFocusedPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			var element = (VisualElement)bindable;

			if (element == null)
			{
				return;
			}

			var isFocused = (bool)newvalue;
			if (isFocused)
			{
				element.OnFocused();
			}
			else
			{
				element.OnUnfocus();
			}

			element.ChangeVisualState();
		}

		static void OnRequestChanged(BindableObject bindable, object oldvalue, object newvalue)
		{
			var constraint = LayoutConstraint.None;
			var element = (VisualElement)bindable;
			if (element.WidthRequest >= 0 && element.MinimumWidthRequest >= 0)
			{
				constraint |= LayoutConstraint.HorizontallyFixed;
			}
			if (element.HeightRequest >= 0 && element.MinimumHeightRequest >= 0)
			{
				constraint |= LayoutConstraint.VerticallyFixed;
			}

			element.SelfConstraint = constraint;

			if (element is IFrameworkElement fe)
			{
				fe.Handler?.UpdateValue(nameof(IFrameworkElement.Width));
				fe.Handler?.UpdateValue(nameof(IFrameworkElement.Height));
			}

			((VisualElement)bindable).InvalidateMeasureInternal(InvalidationTrigger.SizeRequestChanged);
		}

		void OnUnfocus() => Unfocused?.Invoke(this, new FocusEventArgs(this, false));

		bool IFlowDirectionController.ApplyEffectiveFlowDirectionToChildContainer => true;

		void IPropertyPropagationController.PropagatePropertyChanged(string propertyName)
		{
			PropertyPropagationExtensions.PropagatePropertyChanged(propertyName, this, LogicalChildren);
		}

		void SetSize(double width, double height)
		{
			if (Width == width && Height == height)
				return;

			Width = width;
			Height = height;

			SizeAllocated(width, height);
			SizeChanged?.Invoke(this, EventArgs.Empty);
		}

		public class FocusRequestArgs : EventArgs
		{
			public bool Focus { get; set; }

			public bool Result { get; set; }
		}

		public class VisibilityConverter : TypeConverter
		{
			public override object ConvertFromInvariantString(string value)
			{
				value = value?.Trim();
				if (!string.IsNullOrEmpty(value))
				{
					if (value.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase))
						return true;
					if (value.Equals("visible", StringComparison.OrdinalIgnoreCase))
						return true;
					if (value.Equals(bool.FalseString, StringComparison.OrdinalIgnoreCase))
						return false;
					if (value.Equals("hidden", StringComparison.OrdinalIgnoreCase))
						return false;
					if (value.Equals("collapse", StringComparison.OrdinalIgnoreCase))
						return false;
				}
				throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}.", value, typeof(bool)));
			}

			public override string ConvertToInvariantString(object value)
			{
				if (!(value is bool visibility))
					throw new NotSupportedException();
				return visibility.ToString();
			}
		}
	}
}
