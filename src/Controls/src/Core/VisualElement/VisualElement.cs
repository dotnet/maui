#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Geometry = Microsoft.Maui.Controls.Shapes.Geometry;
using Rect = Microsoft.Maui.Graphics.Rect;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="Type[@FullName='Microsoft.Maui.Controls.VisualElement']/Docs/*" />
	public partial class VisualElement : NavigableElement, IAnimatable, IVisualElementController, IResourcesProvider, IStyleElement, IFlowDirectionController, IPropertyPropagationController, IVisualController, IWindowController, IView, IControlsVisualElement
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='NavigationProperty']/Docs/*" />
		public new static readonly BindableProperty NavigationProperty = NavigableElement.NavigationProperty;

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='StyleProperty']/Docs/*" />
		public new static readonly BindableProperty StyleProperty = NavigableElement.StyleProperty;

		bool _inputTransparentExplicit = (bool)InputTransparentProperty.DefaultValue;

		/// <summary>Bindable property for <see cref="InputTransparent"/>.</summary>
		public static readonly BindableProperty InputTransparentProperty = BindableProperty.Create(
			"InputTransparent", typeof(bool), typeof(VisualElement), default(bool),
			propertyChanged: OnInputTransparentPropertyChanged, coerceValue: CoerceInputTransparentProperty);

		bool _isEnabledExplicit = (bool)IsEnabledProperty.DefaultValue;

		/// <summary>Bindable property for <see cref="IsEnabled"/>.</summary>
		public static readonly BindableProperty IsEnabledProperty = BindableProperty.Create("IsEnabled", typeof(bool),
			typeof(VisualElement), true, propertyChanged: OnIsEnabledPropertyChanged, coerceValue: CoerceIsEnabledProperty);

		static readonly BindablePropertyKey XPropertyKey = BindableProperty.CreateReadOnly("X", typeof(double), typeof(VisualElement), default(double));

		/// <summary>Bindable property for <see cref="X"/>.</summary>
		public static readonly BindableProperty XProperty = XPropertyKey.BindableProperty;

		static readonly BindablePropertyKey YPropertyKey = BindableProperty.CreateReadOnly("Y", typeof(double), typeof(VisualElement), default(double));

		/// <summary>Bindable property for <see cref="Y"/>.</summary>
		public static readonly BindableProperty YProperty = YPropertyKey.BindableProperty;

		/// <summary>Bindable property for <see cref="AnchorX"/>.</summary>
		public static readonly BindableProperty AnchorXProperty = BindableProperty.Create("AnchorX", typeof(double), typeof(VisualElement), .5d);

		/// <summary>Bindable property for <see cref="AnchorY"/>.</summary>
		public static readonly BindableProperty AnchorYProperty = BindableProperty.Create("AnchorY", typeof(double), typeof(VisualElement), .5d);

		/// <summary>Bindable property for <see cref="TranslationX"/>.</summary>
		public static readonly BindableProperty TranslationXProperty = BindableProperty.Create("TranslationX", typeof(double), typeof(VisualElement), 0d);

		/// <summary>Bindable property for <see cref="TranslationY"/>.</summary>
		public static readonly BindableProperty TranslationYProperty = BindableProperty.Create("TranslationY", typeof(double), typeof(VisualElement), 0d);

		static readonly BindablePropertyKey WidthPropertyKey = BindableProperty.CreateReadOnly("Width", typeof(double), typeof(VisualElement), -1d,
			coerceValue: (bindable, value) => double.IsNaN((double)value) ? 0d : value);

		/// <summary>Bindable property for <see cref="Width"/>.</summary>
		public static readonly BindableProperty WidthProperty = WidthPropertyKey.BindableProperty;

		static readonly BindablePropertyKey HeightPropertyKey = BindableProperty.CreateReadOnly("Height", typeof(double), typeof(VisualElement), -1d,
			coerceValue: (bindable, value) => double.IsNaN((double)value) ? 0d : value);

		/// <summary>Bindable property for <see cref="Height"/>.</summary>
		public static readonly BindableProperty HeightProperty = HeightPropertyKey.BindableProperty;

		/// <summary>Bindable property for <see cref="Rotation"/>.</summary>
		public static readonly BindableProperty RotationProperty = BindableProperty.Create("Rotation", typeof(double), typeof(VisualElement), default(double));

		/// <summary>Bindable property for <see cref="RotationX"/>.</summary>
		public static readonly BindableProperty RotationXProperty = BindableProperty.Create("RotationX", typeof(double), typeof(VisualElement), default(double));

		/// <summary>Bindable property for <see cref="RotationY"/>.</summary>
		public static readonly BindableProperty RotationYProperty = BindableProperty.Create("RotationY", typeof(double), typeof(VisualElement), default(double));

		/// <summary>Bindable property for <see cref="Scale"/>.</summary>
		public static readonly BindableProperty ScaleProperty = BindableProperty.Create(nameof(Scale), typeof(double), typeof(VisualElement), 1d);

		/// <summary>Bindable property for <see cref="ScaleX"/>.</summary>
		public static readonly BindableProperty ScaleXProperty = BindableProperty.Create(nameof(ScaleX), typeof(double), typeof(VisualElement), 1d);

		/// <summary>Bindable property for <see cref="ScaleY"/>.</summary>
		public static readonly BindableProperty ScaleYProperty = BindableProperty.Create(nameof(ScaleY), typeof(double), typeof(VisualElement), 1d);

		internal static readonly BindableProperty TransformProperty = BindableProperty.Create("Transform", typeof(string), typeof(VisualElement), null, propertyChanged: OnTransformChanged);

		/// <summary>Bindable property for <see cref="Clip"/>.</summary>
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
			var clip = Clip;
			if (clip != null)
			{
				_clipChanged ??= (sender, e) => OnPropertyChanged(nameof(Clip));
				_clipProxy ??= new();
				_clipProxy.Subscribe(clip, _clipChanged);
			}
		}

		void StopNotifyingClipChanges()
		{
			_clipProxy?.Unsubscribe();
		}

		class WeakClipChangedProxy : WeakEventProxy<Geometry, EventHandler>
		{
			void OnClipChanged(object sender, EventArgs e)
			{
				if (TryGetHandler(out var handler))
				{
					handler(sender, e);
				}
				else
				{
					Unsubscribe();
				}
			}

			public override void Subscribe(Geometry source, EventHandler handler)
			{
				if (TryGetSource(out var s))
				{
					s.PropertyChanged -= OnClipChanged;

					if (s is GeometryGroup g)
						g.InvalidateGeometryRequested -= OnClipChanged;
				}

				source.PropertyChanged += OnClipChanged;
				if (source is GeometryGroup geometryGroup)
					geometryGroup.InvalidateGeometryRequested += OnClipChanged;

				base.Subscribe(source, handler);
			}

			public override void Unsubscribe()
			{
				if (TryGetSource(out var s))
				{
					s.PropertyChanged -= OnClipChanged;

					if (s is GeometryGroup g)
						g.InvalidateGeometryRequested -= OnClipChanged;
				}
				base.Unsubscribe();
			}
		}

		/// <summary>Bindable property for <see cref="Visual"/>.</summary>
		public static readonly BindableProperty VisualProperty =
			BindableProperty.Create(nameof(Visual), typeof(IVisual), typeof(VisualElement), Maui.Controls.VisualMarker.MatchParent,
									validateValue: (b, v) => v != null, propertyChanged: OnVisualChanged);

		static IVisual _defaultVisual = Microsoft.Maui.Controls.VisualMarker.Default;
		IVisual _effectiveVisual = _defaultVisual;

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Visual']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(VisualTypeConverter))]
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
				var openBracket = transform.IndexOf("(", StringComparison.Ordinal);
				var closeBracket = transform.IndexOf(")", StringComparison.Ordinal);
				if (string.IsNullOrEmpty(transform) || openBracket < 0 || closeBracket < 0)
					throw new FormatException("Format for transform is 'none | transform(value) [transform(value) ]*'");
				var transformName = transform.Substring(0, openBracket);
				var value = transform.Substring(openBracket + 1, closeBracket - openBracket - 1);
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

		/// <summary>Bindable property for <see cref="IsVisible"/>.</summary>
		public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create("IsVisible", typeof(bool), typeof(VisualElement), true,
			propertyChanged: (bindable, oldvalue, newvalue) => ((VisualElement)bindable).OnIsVisibleChanged((bool)oldvalue, (bool)newvalue));

		/// <summary>Bindable property for <see cref="Opacity"/>.</summary>
		public static readonly BindableProperty OpacityProperty = BindableProperty.Create("Opacity", typeof(double), typeof(VisualElement), 1d, coerceValue: (bindable, value) => ((double)value).Clamp(0, 1));

		/// <summary>Bindable property for <see cref="BackgroundColor"/>.</summary>
		public static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(nameof(BackgroundColor), typeof(Color), typeof(VisualElement), null);

		/// <summary>Bindable property for <see cref="Background"/>.</summary>
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

		WeakBackgroundChangedProxy _backgroundProxy;
		WeakClipChangedProxy _clipProxy;
		EventHandler _backgroundChanged, _clipChanged;
		WeakNotifyPropertyChangedProxy _shadowProxy = null;
		PropertyChangedEventHandler _shadowChanged;

		~VisualElement()
		{
			_clipProxy?.Unsubscribe();
			_backgroundProxy?.Unsubscribe();
			_shadowProxy?.Unsubscribe();
		}

		void NotifyBackgroundChanges()
		{
			var background = Background;
			if (background is ImmutableBrush)
				return;

			if (background != null)
			{
				SetInheritedBindingContext(background, BindingContext);
				_backgroundChanged ??= (sender, e) => OnPropertyChanged(nameof(Background));
				_backgroundProxy ??= new();
				_backgroundProxy.Subscribe(background, _backgroundChanged);
			}
		}

		void StopNotifyingBackgroundChanges()
		{
			var background = Background;
			if (background is ImmutableBrush)
				return;

			if (background != null)
			{
				SetInheritedBindingContext(background, null);
				_backgroundProxy?.Unsubscribe();
			}
		}

		class WeakBackgroundChangedProxy : WeakEventProxy<Brush, EventHandler>
		{
			void OnBackgroundChanged(object sender, EventArgs e)
			{
				if (TryGetHandler(out var handler))
				{
					handler(sender, e);
				}
				else
				{
					Unsubscribe();
				}
			}

			public override void Subscribe(Brush source, EventHandler handler)
			{
				if (TryGetSource(out var s))
				{
					s.PropertyChanged -= OnBackgroundChanged;

					if (s is GradientBrush g)
						g.InvalidateGradientBrushRequested -= OnBackgroundChanged;
				}

				source.PropertyChanged += OnBackgroundChanged;
				if (source is GradientBrush gradientBrush)
					gradientBrush.InvalidateGradientBrushRequested += OnBackgroundChanged;

				base.Subscribe(source, handler);
			}

			public override void Unsubscribe()
			{
				if (TryGetSource(out var s))
				{
					s.PropertyChanged -= OnBackgroundChanged;

					if (s is GradientBrush g)
						g.InvalidateGradientBrushRequested -= OnBackgroundChanged;
				}
				base.Unsubscribe();
			}
		}

		internal static readonly BindablePropertyKey BehaviorsPropertyKey = BindableProperty.CreateReadOnly("Behaviors", typeof(IList<Behavior>), typeof(VisualElement), default(IList<Behavior>),
			defaultValueCreator: bindable =>
			{
				var collection = new AttachedCollection<Behavior>();
				collection.AttachTo(bindable);
				return collection;
			});

		/// <summary>Bindable property for <see cref="Behaviors"/>.</summary>
		public static readonly BindableProperty BehaviorsProperty = BehaviorsPropertyKey.BindableProperty;

		internal static readonly BindablePropertyKey TriggersPropertyKey = BindableProperty.CreateReadOnly("Triggers", typeof(IList<TriggerBase>), typeof(VisualElement), default(IList<TriggerBase>),
			defaultValueCreator: bindable =>
			{
				var collection = new AttachedCollection<TriggerBase>();
				collection.AttachTo(bindable);
				return collection;
			});

		/// <summary>Bindable property for <see cref="Triggers"/>.</summary>
		public static readonly BindableProperty TriggersProperty = TriggersPropertyKey.BindableProperty;


		/// <summary>Bindable property for <see cref="WidthRequest"/>.</summary>

		public static readonly BindableProperty WidthRequestProperty = BindableProperty.Create(nameof(WidthRequest), typeof(double), typeof(VisualElement), -1d, propertyChanged: OnRequestChanged);

		/// <summary>Bindable property for <see cref="HeightRequest"/>.</summary>
		public static readonly BindableProperty HeightRequestProperty = BindableProperty.Create(nameof(HeightRequest), typeof(double), typeof(VisualElement), -1d, propertyChanged: OnRequestChanged);

		/// <summary>Bindable property for <see cref="MinimumWidthRequest"/>.</summary>
		public static readonly BindableProperty MinimumWidthRequestProperty = BindableProperty.Create(nameof(MinimumWidthRequest), typeof(double), typeof(VisualElement), -1d, propertyChanged: OnRequestChanged);

		/// <summary>Bindable property for <see cref="MinimumHeightRequest"/>.</summary>
		public static readonly BindableProperty MinimumHeightRequestProperty = BindableProperty.Create(nameof(MinimumHeightRequest), typeof(double), typeof(VisualElement), -1d, propertyChanged: OnRequestChanged);

		/// <summary>Bindable property for <see cref="MaximumWidthRequest"/>.</summary>
		public static readonly BindableProperty MaximumWidthRequestProperty = BindableProperty.Create(nameof(MaximumWidthRequest), typeof(double), typeof(VisualElement), double.PositiveInfinity, propertyChanged: OnRequestChanged);

		/// <summary>Bindable property for <see cref="MaximumHeightRequest"/>.</summary>
		public static readonly BindableProperty MaximumHeightRequestProperty = BindableProperty.Create(nameof(MaximumHeightRequest), typeof(double), typeof(VisualElement), double.PositiveInfinity, propertyChanged: OnRequestChanged);

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='IsFocusedPropertyKey']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly BindablePropertyKey IsFocusedPropertyKey = BindableProperty.CreateReadOnly("IsFocused",
			typeof(bool), typeof(VisualElement), default(bool), propertyChanged: OnIsFocusedPropertyChanged);

		/// <summary>Bindable property for <see cref="IsFocused"/>.</summary>
		public static readonly BindableProperty IsFocusedProperty = IsFocusedPropertyKey.BindableProperty;

		/// <summary>Bindable property for <see cref="FlowDirection"/>.</summary>
		public static readonly BindableProperty FlowDirectionProperty = BindableProperty.Create(nameof(FlowDirection), typeof(FlowDirection), typeof(VisualElement), FlowDirection.MatchParent, propertyChanging: FlowDirectionChanging, propertyChanged: FlowDirectionChanged);

		IFlowDirectionController FlowController => this;

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='FlowDirection']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(FlowDirectionConverter))]
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


		static readonly BindablePropertyKey WindowPropertyKey = BindableProperty.CreateReadOnly(
			nameof(Window), typeof(Window), typeof(VisualElement), null, propertyChanged: OnWindowChanged);

		/// <summary>Bindable property for <see cref="Window"/>.</summary>
		public static readonly BindableProperty WindowProperty = WindowPropertyKey.BindableProperty;

		public Window Window => (Window)GetValue(WindowProperty);

		Window IWindowController.Window
		{
			get => (Window)GetValue(WindowProperty);
			set => SetValue(WindowPropertyKey, value);
		}


		readonly Dictionary<Size, SizeRequest> _measureCache = new Dictionary<Size, SizeRequest>();



		int _batched;
		LayoutConstraint _computedConstraint;

		bool _isInPlatformLayout;

		bool _isPlatformStateConsistent = true;

		bool _isPlatformEnabled;

		double _mockHeight = -1;

		double _mockWidth = -1;

		double _mockX = -1;

		double _mockY = -1;

		LayoutConstraint _selfConstraint;

		protected internal VisualElement()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='AnchorX']/Docs/*" />
		public double AnchorX
		{
			get { return (double)GetValue(AnchorXProperty); }
			set { SetValue(AnchorXProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='AnchorY']/Docs/*" />
		public double AnchorY
		{
			get { return (double)GetValue(AnchorYProperty); }
			set { SetValue(AnchorYProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='BackgroundColor']/Docs/*" />
		public Color BackgroundColor
		{
			get { return (Color)GetValue(BackgroundColorProperty); }
			set { SetValue(BackgroundColorProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Background']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(BrushTypeConverter))]
		public Brush Background
		{
			get { return (Brush)GetValue(BackgroundProperty); }
			set { SetValue(BackgroundProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Behaviors']/Docs/*" />
		public IList<Behavior> Behaviors
		{
			get { return (IList<Behavior>)GetValue(BehaviorsProperty); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Bounds']/Docs/*" />
		public Rect Bounds
		{
			get { return IsMocked() ? new Rect(_mockX, _mockY, _mockWidth, _mockHeight) : _frame; }
			private set
			{
				Frame = value;
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Height']/Docs/*" />
		public double Height
		{
			get { return _mockHeight == -1 ? (double)GetValue(HeightProperty) : _mockHeight; }
			private set { SetValue(HeightPropertyKey, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='HeightRequest']/Docs/*" />
		public double HeightRequest
		{
			get { return (double)GetValue(HeightRequestProperty); }
			set { SetValue(HeightRequestProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='InputTransparent']/Docs/*" />
		public bool InputTransparent
		{
			get { return (bool)GetValue(InputTransparentProperty); }
			set { SetValue(InputTransparentProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='IsEnabled']/Docs/*" />
		public bool IsEnabled
		{
			get { return (bool)GetValue(IsEnabledProperty); }
			set { SetValue(IsEnabledProperty, value); }
		}

		/// <summary>
		/// This value represents the cumulative IsEnabled value.
		/// All types that override this property need to also invoke
		/// the RefreshIsEnabledProperty() method if the value will change.
		/// </summary>
		protected virtual bool IsEnabledCore
		{
			get
			{
				if (_isEnabledExplicit == false)
				{
					// If the explicitly set value is false, then nothing else matters
					// And we can save the effort of a Parent check
					return false;
				}

				var parent = Parent as VisualElement;
				if (parent is not null && !parent.IsEnabled)
					return false;

				return _isEnabledExplicit;
			}
		}

		/// <summary>
		/// This value represents the cumulative InputTransparent value.
		/// All types that override this property need to also invoke
		/// the RefreshInputTransparentProperty() method if the value will change.
		/// 
		/// This method is not virtual as none of the derived types actually need
		/// to change the calculation. If this ever needs to change, then the
		/// RefreshInputTransparentProperty() method should also call the
		/// RefreshPropertyValue() method - just like how the
		/// RefreshIsEnabledProperty() method does.
		/// </summary>
		private protected bool InputTransparentCore
		{
			get
			{
				if (_inputTransparentExplicit == true)
				{
					// If the explicitly set value is true, then nothing else matters
					// And we can save the effort of a Parent check
					return true;
				}

				var parent = Parent as IInputTransparentContainerElement;
				while (parent is not null)
				{
					if (parent.CascadeInputTransparent && parent.InputTransparent)
						return true;
					parent = parent.Parent as IInputTransparentContainerElement;
				}

				return _inputTransparentExplicit;
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='IsFocused']/Docs/*" />
		public bool IsFocused => (bool)GetValue(IsFocusedProperty);

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='IsVisible']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(VisibilityConverter))]
		public bool IsVisible
		{
			get { return (bool)GetValue(IsVisibleProperty); }
			set { SetValue(IsVisibleProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='MinimumHeightRequest']/Docs/*" />
		public double MinimumHeightRequest
		{
			get { return (double)GetValue(MinimumHeightRequestProperty); }
			set { SetValue(MinimumHeightRequestProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='MinimumWidthRequest']/Docs/*" />
		public double MinimumWidthRequest
		{
			get { return (double)GetValue(MinimumWidthRequestProperty); }
			set { SetValue(MinimumWidthRequestProperty, value); }
		}

		public double MaximumHeightRequest
		{
			get { return (double)GetValue(MaximumHeightRequestProperty); }
			set { SetValue(MaximumHeightRequestProperty, value); }
		}

		public double MaximumWidthRequest
		{
			get { return (double)GetValue(MaximumWidthRequestProperty); }
			set { SetValue(MaximumWidthRequestProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Opacity']/Docs/*" />
		public double Opacity
		{
			get { return (double)GetValue(OpacityProperty); }
			set { SetValue(OpacityProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Rotation']/Docs/*" />
		public double Rotation
		{
			get { return (double)GetValue(RotationProperty); }
			set { SetValue(RotationProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='RotationX']/Docs/*" />
		public double RotationX
		{
			get { return (double)GetValue(RotationXProperty); }
			set { SetValue(RotationXProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='RotationY']/Docs/*" />
		public double RotationY
		{
			get { return (double)GetValue(RotationYProperty); }
			set { SetValue(RotationYProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Scale']/Docs/*" />
		public double Scale
		{
			get => (double)GetValue(ScaleProperty);
			set => SetValue(ScaleProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='ScaleX']/Docs/*" />
		public double ScaleX
		{
			get => (double)GetValue(ScaleXProperty);
			set => SetValue(ScaleXProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='ScaleY']/Docs/*" />
		public double ScaleY
		{
			get => (double)GetValue(ScaleYProperty);
			set => SetValue(ScaleYProperty, value);
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='TranslationX']/Docs/*" />
		public double TranslationX
		{
			get { return (double)GetValue(TranslationXProperty); }
			set { SetValue(TranslationXProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='TranslationY']/Docs/*" />
		public double TranslationY
		{
			get { return (double)GetValue(TranslationYProperty); }
			set { SetValue(TranslationYProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Triggers']/Docs/*" />
		public IList<TriggerBase> Triggers => (IList<TriggerBase>)GetValue(TriggersProperty);

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Width']/Docs/*" />
		public double Width
		{
			get { return _mockWidth == -1 ? (double)GetValue(WidthProperty) : _mockWidth; }
			private set { SetValue(WidthPropertyKey, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='WidthRequest']/Docs/*" />
		public double WidthRequest
		{
			get { return (double)GetValue(WidthRequestProperty); }
			set { SetValue(WidthRequestProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='X']/Docs/*" />
		public double X
		{
			get { return _mockX == -1 ? (double)GetValue(XProperty) : _mockX; }
			private set { SetValue(XPropertyKey, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Y']/Docs/*" />
		public double Y
		{
			get { return _mockY == -1 ? (double)GetValue(YProperty) : _mockY; }
			private set { SetValue(YPropertyKey, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Clip']/Docs/*" />
		[System.ComponentModel.TypeConverter(typeof(PathGeometryConverter))]
		public Geometry Clip
		{
			get { return (Geometry)GetValue(ClipProperty); }
			set { SetValue(ClipProperty, value); }
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Batched']/Docs/*" />
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

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='DisableLayout']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool DisableLayout { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool IsInPlatformLayout
		{
			get
			{
				if (_isInPlatformLayout)
					return true;

				Element parent = RealParent;
				if (parent != null)
				{
					var visualElement = parent as VisualElement;
					if (visualElement != null && visualElement.IsInPlatformLayout)
						return true;
				}

				return false;
			}
			set { _isInPlatformLayout = value; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool IsPlatformStateConsistent
		{
			get { return _isPlatformStateConsistent; }
			set
			{
				if (_isPlatformStateConsistent == value)
					return;
				_isPlatformStateConsistent = value;
				if (value && IsPlatformEnabled)
					InvalidateMeasureInternal(InvalidationTrigger.RendererReady);
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		internal event EventHandler PlatformEnabledChanged;

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='IsPlatformEnabled']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool IsPlatformEnabled
		{
			get { return _isPlatformEnabled; }
			set
			{
				if (value == _isPlatformEnabled)
					return;

				_isPlatformEnabled = value;
				if (value && IsPlatformStateConsistent)
					InvalidateMeasureInternal(InvalidationTrigger.RendererReady);

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

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='BatchBegin']/Docs/*" />
		public void BatchBegin() => _batched++;

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='BatchCommit']/Docs/*" />
		public void BatchCommit()
		{
			_batched = Math.Max(0, _batched - 1);
			if (!Batched)
			{
				BatchCommitted?.Invoke(this, new EventArg<VisualElement>(this));
			}
		}

		ResourceDictionary _resources;
		bool IResourcesProvider.IsResourcesCreated => _resources != null;

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Resources']/Docs/*" />
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
		public void PlatformSizeChanged() => InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		public event EventHandler ChildrenReordered;

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Focus']/Docs/*" />
		public bool Focus() =>
			this.RequestFocus();

		public event EventHandler<FocusEventArgs> Focused;

		SizeRequest GetSizeRequest(double widthConstraint, double heightConstraint)
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

			if (heightRequest != -1 && !double.IsNaN(heightRequest))
			{
				request.Height = heightRequest;
				if (!hasMinimum)
					minimum.Height = heightRequest;
			}

			if (widthRequest != -1 && !double.IsNaN(widthRequest))
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

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Measure']/Docs/*" />
		public virtual SizeRequest Measure(double widthConstraint, double heightConstraint, MeasureFlags flags = MeasureFlags.None)
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

			SizeRequest result = GetSizeRequest(widthConstraint, heightConstraint);

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

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Unfocus']/Docs/*" />
		public void Unfocus()
		{
			if (!IsFocused)
				return;

			Handler?.Invoke(nameof(IView.Unfocus));
			FocusChangeRequested?.Invoke(this, new FocusRequestArgs());
		}

		public event EventHandler<FocusEventArgs> Unfocused;

		protected virtual void InvalidateMeasure() => InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);

		protected override void OnBindingContextChanged()
		{
			PropagateBindingContextToStateTriggers();
			PropagateBindingContextToBrush();
			PropagateBindingContextToShadow();

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

		IPlatformSizeService _platformSizeService;

		protected virtual SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
			if (!IsPlatformEnabled)
				return new SizeRequest(new Size(-1, -1));

			if (Handler != null)
				return new SizeRequest(Handler.GetDesiredSize(widthConstraint, heightConstraint));

			_platformSizeService ??= DependencyService.Get<IPlatformSizeService>();
			return _platformSizeService.GetPlatformSize(this, widthConstraint, heightConstraint);
		}

		protected virtual void OnSizeAllocated(double width, double height)
		{
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

		// TODO: Obsolete in favor of MapFocus https://github.com/dotnet/maui/issues/14299
		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler<FocusRequestArgs> FocusChangeRequested;
		internal void InvokeFocusChangeRequested(FocusRequestArgs args) =>
			FocusChangeRequested?.Invoke(this, args);
		internal bool HasFocusChangeRequestedEvent => FocusChangeRequested is not null;

		/// <include file="../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='InvalidateMeasureNonVirtual']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public void InvalidateMeasureNonVirtual(InvalidationTrigger trigger)
		{
			InvalidateMeasureInternal(trigger);
		}

		internal virtual void InvalidateMeasureInternal(InvalidationTrigger trigger)
		{
			_measureCache.Clear();

			// TODO ezhart Once we get InvalidateArrange sorted, HorizontalOptionsChanged and 
			// VerticalOptionsChanged will need to call ParentView.InvalidateArrange() instead

			switch (trigger)
			{
				case InvalidationTrigger.MarginChanged:
				case InvalidationTrigger.HorizontalOptionsChanged:
				case InvalidationTrigger.VerticalOptionsChanged:
					ParentView?.InvalidateMeasure();
					break;
				default:
					(this as IView)?.InvalidateMeasure();
					break;
			}

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

		internal void MockBounds(Rect bounds)
		{
#if NETSTANDARD2_0 || NET6_0_OR_GREATER
			(_mockX, _mockY, _mockWidth, _mockHeight) = bounds;
#else
			_mockX = bounds.X;
			_mockY = bounds.Y;
			_mockWidth = bounds.Width;
			_mockHeight = bounds.Height;
#endif
		}

		bool IsMocked()
		{
			return _mockX != -1 || _mockY != -1 || _mockWidth != -1 || _mockHeight != -1;
		}

		internal virtual void OnConstraintChanged(LayoutConstraint oldConstraint, LayoutConstraint newConstraint) => ComputeConstrainsForChildren();

		internal virtual void OnIsPlatformEnabledChanged()
		{
		}

		internal virtual void OnIsVisibleChanged(bool oldValue, bool newValue)
		{
			if (this is IView fe)
			{
				fe.Handler?.UpdateValue(nameof(IView.Visibility));
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

			var innerKeys = new HashSet<string>(StringComparer.Ordinal);
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

		bool _isPointerOver;

		internal bool IsPointerOver
		{
			get { return _isPointerOver; }
		}

		private protected void SetPointerOver(bool value, bool callChangeVisualState = true)
		{
			if (_isPointerOver == value)
				return;

			_isPointerOver = value;
			if (callChangeVisualState)
				ChangeVisualState();
		}

		protected internal virtual void ChangeVisualState()
		{
			if (!IsEnabled)
			{
				VisualStateManager.GoToState(this, VisualStateManager.CommonStates.Disabled);
			}
			else if (IsPointerOver)
			{
				VisualStateManager.GoToState(this, VisualStateManager.CommonStates.PointerOver);
			}
			else
			{
				VisualStateManager.GoToState(this, VisualStateManager.CommonStates.Normal);
			}

			if (IsEnabled)
			{
				// Focus needs to be handled independently; otherwise, if no actual Focus state is supplied
				// in the control's visual states, the state can end up stuck in PointerOver after the pointer
				// exits and the control still has focus.
				VisualStateManager.GoToState(this,
					IsFocused ? VisualStateManager.CommonStates.Focused : VisualStateManager.CommonStates.Unfocused);
			}
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

		static object CoerceIsEnabledProperty(BindableObject bindable, object value)
		{
			if (bindable is VisualElement visualElement)
			{
				visualElement._isEnabledExplicit = (bool)value;
				return visualElement.IsEnabledCore;
			}

			return false;
		}

		static void OnIsEnabledPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var element = (VisualElement)bindable;

			if (element == null)
				return;

			element.ChangeVisualState();

			(bindable as IPropertyPropagationController)?.PropagatePropertyChanged(VisualElement.IsEnabledProperty.PropertyName);
		}

		static object CoerceInputTransparentProperty(BindableObject bindable, object value)
		{
			if (bindable is VisualElement visualElement)
			{
				visualElement._inputTransparentExplicit = (bool)value;
				return visualElement.InputTransparentCore;
			}

			return false;
		}

		static void OnInputTransparentPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as IPropertyPropagationController)?.PropagatePropertyChanged(VisualElement.InputTransparentProperty.PropertyName);
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

			if (element is IView fe)
			{
				fe.Handler?.UpdateValue(nameof(IView.Width));
				fe.Handler?.UpdateValue(nameof(IView.Height));
				fe.Handler?.UpdateValue(nameof(IView.MinimumHeight));
				fe.Handler?.UpdateValue(nameof(IView.MinimumWidth));
				fe.Handler?.UpdateValue(nameof(IView.MaximumHeight));
				fe.Handler?.UpdateValue(nameof(IView.MaximumWidth));
			}

			((VisualElement)bindable).InvalidateMeasureInternal(InvalidationTrigger.SizeRequestChanged);
		}

		void OnUnfocus() => Unfocused?.Invoke(this, new FocusEventArgs(this, false));

		bool IFlowDirectionController.ApplyEffectiveFlowDirectionToChildContainer => true;

		void IPropertyPropagationController.PropagatePropertyChanged(string propertyName)
		{
			if (propertyName == null || propertyName == IsEnabledProperty.PropertyName)
				this.RefreshPropertyValue(IsEnabledProperty, _isEnabledExplicit);

			if (propertyName == null || propertyName == InputTransparentProperty.PropertyName)
				this.RefreshPropertyValue(InputTransparentProperty, _inputTransparentExplicit);

			PropertyPropagationExtensions.PropagatePropertyChanged(propertyName, this, ((IVisualTreeElement)this).GetVisualChildren());
		}

		/// <summary>
		/// This method must always be called if some event occurs and the value of
		/// the IsEnabledCore property will change.
		/// </summary>
		protected void RefreshIsEnabledProperty() =>
			this.RefreshPropertyValue(IsEnabledProperty, _isEnabledExplicit);

		/// <summary>
		/// This method must always be called if some event occurs and the value of
		/// the InputTransparentCore property will change.
		/// </summary>
		private protected void RefreshInputTransparentProperty()
		{
			// This method does not need to call the
			// this.RefreshPropertyValue(InputTransparentProperty, _inputTransparentExplicit);
			// method because none of the derived types will affect this view. All we
			// need to do is propagate the new value to all the children.

			(this as IPropertyPropagationController)?.PropagatePropertyChanged(VisualElement.InputTransparentProperty.PropertyName);
		}

		void UpdateBoundsComponents(Rect bounds)
		{
			_frame = bounds;

			BatchBegin();

			X = bounds.X;
			Y = bounds.Y;
			Width = bounds.Width;
			Height = bounds.Height;

			SizeAllocated(Width, Height);
			SizeChanged?.Invoke(this, EventArgs.Empty);

			BatchCommit();
		}

#nullable enable
		Semantics? _semantics;
		bool _isLoadedFired;
		EventHandler? _loaded;
		EventHandler? _unloaded;
		bool _watchingPlatformLoaded;
		Rect _frame = new Rect(0, 0, -1, -1);
		event EventHandler? _windowChanged;
		event EventHandler? _platformContainerViewChanged;

		public Rect Frame
		{
			get => _frame;
			set
			{
				if (_frame == value)
					return;

				UpdateBoundsComponents(value);
			}
		}

		new public IViewHandler? Handler
		{
			get => (IViewHandler?)base.Handler;
			set => base.Handler = value;
		}

		private protected override void OnHandlerChangedCore()
		{
			base.OnHandlerChangedCore();

			IsPlatformEnabled = Handler != null;
			UpdatePlatformUnloadedLoadedWiring(Window);
		}

		Paint? IView.Background
		{
			get
			{
				if (!Brush.IsNullOrEmpty(Background))
					return Background;
				if (BackgroundColor.IsNotDefault())
					return new SolidColorBrush(BackgroundColor);

				return null;
			}
		}

		IShape IView.Clip => Clip;

		IShadow IView.Shadow => Shadow;

		/// <summary>Bindable property for <see cref="Shadow"/>.</summary>
		public static readonly BindableProperty ShadowProperty =
 			BindableProperty.Create(nameof(Shadow), typeof(Shadow), typeof(VisualElement), defaultValue: null,
				propertyChanging: (bindable, oldvalue, newvalue) =>
				{
					if (oldvalue != null)
						(bindable as VisualElement)?.StopNotifyingShadowChanges();
				},
				propertyChanged: (bindable, oldvalue, newvalue) =>
				{
					if (newvalue != null)
						(bindable as VisualElement)?.NotifyShadowChanges();
				});

		public Shadow Shadow
		{
			get { return (Shadow)GetValue(ShadowProperty); }
			set { SetValue(ShadowProperty, value); }
		}

		/// <summary>Bindable property for <see cref="ZIndex"/>.</summary>
		public static readonly BindableProperty ZIndexProperty =
			BindableProperty.Create(nameof(ZIndex), typeof(int), typeof(VisualElement), default(int),
				propertyChanged: ZIndexPropertyChanged);

		static void ZIndexPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is IView view)
			{
				view.Handler?.Invoke(nameof(IView.ZIndex));
			}
		}

		public int ZIndex
		{
			get { return (int)GetValue(ZIndexProperty); }
			set { SetValue(ZIndexProperty, value); }
		}

		public Size DesiredSize { get; protected set; }

		public void Arrange(Rect bounds)
		{
			Layout(bounds);
		}

		Size IView.Arrange(Rect bounds)
		{
			return ArrangeOverride(bounds);
		}

		// ArrangeOverride provides a way to allow subclasses (e.g., ScrollView) to override Arrange even though
		// the interface has to be explicitly implemented to avoid conflict with the old Arrange method
		protected virtual Size ArrangeOverride(Rect bounds)
		{
			Frame = this.ComputeFrame(bounds);
			Handler?.PlatformArrange(Frame);
			return Frame.Size;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Layout']/Docs/*" />
		public void Layout(Rect bounds)
		{
			Bounds = bounds;
		}

		void IView.InvalidateMeasure()
		{
			InvalidateMeasureOverride();
		}

		// InvalidateMeasureOverride provides a way to allow subclasses (e.g., Layout) to override InvalidateMeasure even though
		// the interface has to be explicitly implemented to avoid conflict with the VisualElement.InvalidateMeasure method
		protected virtual void InvalidateMeasureOverride() => Handler?.Invoke(nameof(IView.InvalidateMeasure));

		void IView.InvalidateArrange()
		{
		}

		Size IView.Measure(double widthConstraint, double heightConstraint)
		{
			return MeasureOverride(widthConstraint, heightConstraint);
		}

		// MeasureOverride provides a way to allow subclasses (e.g., Layout) to override Measure even though
		// the interface has to be explicitly implemented to avoid conflict with the old Measure method
		protected virtual Size MeasureOverride(double widthConstraint, double heightConstraint)
		{
			DesiredSize = this.ComputeDesiredSize(widthConstraint, heightConstraint);
			return DesiredSize;
		}

		bool IView.IsFocused
		{
			get => (bool)GetValue(IsFocusedProperty);
			set => SetValueCore(IsFocusedPropertyKey, value);
		}

		FlowDirection IView.FlowDirection => FlowDirection;

		Primitives.LayoutAlignment IView.HorizontalLayoutAlignment => default;
		Primitives.LayoutAlignment IView.VerticalLayoutAlignment => default;

		Visibility IView.Visibility => IsVisible.ToVisibility();

		Semantics? IView.Semantics => UpdateSemantics();

		private protected virtual Semantics? UpdateSemantics()
		{
			if (!this.IsSet(SemanticProperties.HintProperty) &&
				!this.IsSet(SemanticProperties.DescriptionProperty) &&
				!this.IsSet(SemanticProperties.HeadingLevelProperty))
			{
				_semantics = null;
				return _semantics;
			}

			_semantics ??= new Semantics();
			_semantics.Description = SemanticProperties.GetDescription(this);
			_semantics.HeadingLevel = SemanticProperties.GetHeadingLevel(this);
			_semantics.Hint = SemanticProperties.GetHint(this);
			return _semantics;
		}

		static double EnsurePositive(double value)
		{
			if (value < 0)
			{
				return 0;
			}

			return value;
		}

		double IView.Width
		{
			get
			{
				if (!IsSet(WidthRequestProperty))
				{
					return Primitives.Dimension.Unset;
				}

				// Access once up front to avoid multiple GetValue calls
				var value = WidthRequest;

				if (value == -1)
				{
					return Primitives.Dimension.Unset;
				}

				value = EnsurePositive(value);
				return value;
			}
		}

		double IView.Height
		{
			get
			{
				if (!IsSet(HeightRequestProperty))
				{
					return Primitives.Dimension.Unset;
				}

				// Access once up front to avoid multiple GetValue calls
				var value = HeightRequest;

				if (value == -1)
				{
					return Primitives.Dimension.Unset;
				}

				value = EnsurePositive(value);
				return value;
			}
		}

		double IView.MinimumWidth
		{
			get
			{
				if (!IsSet(MinimumWidthRequestProperty))
				{
					return Primitives.Dimension.Unset;
				}

				// Access once up front to avoid multiple GetValue calls
				var value = MinimumWidthRequest;
				value = EnsurePositive(value);
				return value;
			}
		}

		double IView.MinimumHeight
		{
			get
			{
				if (!IsSet(MinimumHeightRequestProperty))
				{
					return Primitives.Dimension.Unset;
				}

				// Access once up front to avoid multiple GetValue calls
				var value = MinimumHeightRequest;
				value = EnsurePositive(value);
				return value;
			}
		}

		double IView.MaximumWidth
		{
			get
			{
				// Access once up front to avoid multiple GetValue calls
				var value = MaximumWidthRequest;
				value = EnsurePositive(value);
				return value;
			}
		}

		double IView.MaximumHeight
		{
			get
			{
				// Access once up front to avoid multiple GetValue calls
				var value = MaximumHeightRequest;
				value = EnsurePositive(value);
				return value;
			}
		}

		Thickness IView.Margin => Thickness.Zero;

		IElementHandler? Maui.IElement.Handler
		{
			get => base.Handler;
			set
			{
				if (value != null && value is not IViewHandler)
					throw new InvalidOperationException("Handler must be of type IViewHandler");

				base.Handler = value;
			}
		}

		void NotifyShadowChanges()
		{
			var shadow = Shadow;

			if (shadow is not null)
			{
				SetInheritedBindingContext(shadow, BindingContext);
				_shadowChanged ??= (sender, e) => OnPropertyChanged(nameof(Shadow));
				_shadowProxy ??= new();
				_shadowProxy.Subscribe(shadow, _shadowChanged);
			}
		}

		void StopNotifyingShadowChanges()
		{
			var shadow = Shadow;

			if (shadow is not null)
			{
				SetInheritedBindingContext(shadow, null);
				_shadowProxy?.Unsubscribe();
			}
		}

		void OnShadowChanged(object? sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(nameof(Shadow));
		}

		void PropagateBindingContextToBrush()
		{
			if (Background != null)
				SetInheritedBindingContext(Background, BindingContext);
		}

		void PropagateBindingContextToShadow()
		{
			if (Shadow != null)
				SetInheritedBindingContext(Shadow, BindingContext);
		}

		/// <summary>
		/// Indicates if a VisualElement is connected to the main object tree.
		/// </summary>
		public bool IsLoaded
		{
			get
			{
#if PLATFORM
				bool isLoaded = (Handler as IPlatformViewHandler)?.PlatformView?.IsLoaded() == true;
#else
				bool isLoaded = Window != null;
#endif

				return isLoaded;
			}
		}

		/// <summary>
		/// Occurs when a VisualElement has been constructed and added to the object tree.
		/// This event may occur before the VisualElement has been measured so should not be relied on for size information.  
		/// </summary>
		public event EventHandler? Loaded
		{
			add
			{
				_loaded += value;
				UpdatePlatformUnloadedLoadedWiring(Window);
				if (_isLoadedFired)
					_loaded?.Invoke(this, EventArgs.Empty);

			}
			remove
			{
				_loaded -= value;
				UpdatePlatformUnloadedLoadedWiring(Window);
			}
		}

		/// <summary>
		/// Occurs when this VisualElement is no longer connected to the main object tree.
		/// </summary>
		public event EventHandler? Unloaded
		{
			add
			{
				_unloaded += value;
				UpdatePlatformUnloadedLoadedWiring(Window);
			}
			remove
			{
				_unloaded -= value;
				UpdatePlatformUnloadedLoadedWiring(Window);
			}
		}

		event EventHandler? IControlsVisualElement.WindowChanged
		{
			add => _windowChanged += value;
			remove => _windowChanged -= value;
		}

		event EventHandler? IControlsVisualElement.PlatformContainerViewChanged
		{
			add => _platformContainerViewChanged += value;
			remove => _platformContainerViewChanged -= value;
		}

		void OnLoadedCore()
		{
			if (_isLoadedFired)
				return;

			_isLoadedFired = true;
			_loaded?.Invoke(this, EventArgs.Empty);
		}

		void OnUnloadedCore()
		{
			if (!_isLoadedFired)
				return;

			_isLoadedFired = false;
			_unloaded?.Invoke(this, EventArgs.Empty);
		}

		static void OnWindowChanged(BindableObject bindable, object? oldValue, object? newValue)
		{
			if (bindable is not VisualElement visualElement)
				return;

			if (visualElement._watchingPlatformLoaded && oldValue is Window oldWindow)
				oldWindow.HandlerChanged -= visualElement.OnWindowHandlerChanged;

			visualElement.UpdatePlatformUnloadedLoadedWiring(newValue as Window);
			visualElement.InvalidateStateTriggers(newValue != null);
			visualElement._windowChanged?.Invoke(visualElement, EventArgs.Empty);
		}

		void OnWindowHandlerChanged(object? sender, EventArgs e)
		{
			UpdatePlatformUnloadedLoadedWiring(Window);
		}

		// We only want to wire up to platform loaded events
		// if the user is watching for them. Otherwise
		// this will get wired up for every single VE that's on 
		// the screen
		void UpdatePlatformUnloadedLoadedWiring(Window? window)
		{
			// If I'm not attached to a window and I haven't started watching any platform events
			// then it's not useful to wire anything up. We will just wait until
			// This VE gets connected to the xplat Window before wiring up any events
			if (!_watchingPlatformLoaded && window == null)
				return;

			if (_unloaded == null && _loaded == null)
			{
				if (window is not null)
					window.HandlerChanged -= OnWindowHandlerChanged;

#if PLATFORM
				_loadedUnloadedToken?.Dispose();
				_loadedUnloadedToken = null;
#endif

				_watchingPlatformLoaded = false;
				return;
			}

			if (!_watchingPlatformLoaded)
			{
				if (window is not null)
					window.HandlerChanged += OnWindowHandlerChanged;

				_watchingPlatformLoaded = true;
			}

			HandlePlatformUnloadedLoaded();
		}

		partial void HandlePlatformUnloadedLoaded();

		internal IView? ParentView => ((this as IView)?.Parent as IView);
#nullable disable


		public class FocusRequestArgs : EventArgs
		{
			public bool Focus { get; set; }

			public bool Result { get; set; }
		}

		public class VisibilityConverter : TypeConverter
		{
			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
				=> sourceType == typeof(string);

			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
				=> true;

			public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
			{
				var strValue = value?.ToString()?.Trim();

				if (!string.IsNullOrEmpty(strValue))
				{
					if (strValue.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase))
						return true;
					if (strValue.Equals("visible", StringComparison.OrdinalIgnoreCase))
						return true;
					if (strValue.Equals(bool.FalseString, StringComparison.OrdinalIgnoreCase))
						return false;
					if (strValue.Equals("hidden", StringComparison.OrdinalIgnoreCase))
						return false;
					if (strValue.Equals("collapse", StringComparison.OrdinalIgnoreCase))
						return false;
				}
				throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}.", strValue, typeof(bool)));
			}

			public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
			{
				if (value is not bool visibility)
					throw new NotSupportedException();
				return visibility.ToString();
			}
		}
	}
}
