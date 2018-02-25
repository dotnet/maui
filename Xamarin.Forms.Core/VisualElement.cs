using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public partial class VisualElement : Element, IAnimatable, IVisualElementController, IResourcesProvider, IFlowDirectionController
	{
		internal static readonly BindablePropertyKey NavigationPropertyKey = BindableProperty.CreateReadOnly("Navigation", typeof(INavigation), typeof(VisualElement), default(INavigation));

		public static readonly BindableProperty NavigationProperty = NavigationPropertyKey.BindableProperty;

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

		public static readonly BindableProperty ScaleProperty = BindableProperty.Create("Scale", typeof(double), typeof(VisualElement), 1d);

		public static readonly BindableProperty IsVisibleProperty = BindableProperty.Create("IsVisible", typeof(bool), typeof(VisualElement), true,
			propertyChanged: (bindable, oldvalue, newvalue) => ((VisualElement)bindable).OnIsVisibleChanged((bool)oldvalue, (bool)newvalue));

		public static readonly BindableProperty OpacityProperty = BindableProperty.Create("Opacity", typeof(double), typeof(VisualElement), 1d, coerceValue: (bindable, value) => ((double)value).Clamp(0, 1));

		public static readonly BindableProperty BackgroundColorProperty = BindableProperty.Create("BackgroundColor", typeof(Color), typeof(VisualElement), Color.Default);

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

		public static readonly BindableProperty StyleProperty = BindableProperty.Create("Style", typeof(Style), typeof(VisualElement), default(Style),
			propertyChanged: (bindable, oldvalue, newvalue) => ((VisualElement)bindable)._mergedStyle.Style = (Style)newvalue);

		public static readonly BindableProperty WidthRequestProperty = BindableProperty.Create("WidthRequest", typeof(double), typeof(VisualElement), -1d, propertyChanged: OnRequestChanged);

		public static readonly BindableProperty HeightRequestProperty = BindableProperty.Create("HeightRequest", typeof(double), typeof(VisualElement), -1d, propertyChanged: OnRequestChanged);

		public static readonly BindableProperty MinimumWidthRequestProperty = BindableProperty.Create("MinimumWidthRequest", typeof(double), typeof(VisualElement), -1d, propertyChanged: OnRequestChanged);

		public static readonly BindableProperty MinimumHeightRequestProperty = BindableProperty.Create("MinimumHeightRequest", typeof(double), typeof(VisualElement), -1d, propertyChanged: OnRequestChanged);

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly BindablePropertyKey IsFocusedPropertyKey = BindableProperty.CreateReadOnly("IsFocused", 
			typeof(bool), typeof(VisualElement), default(bool), propertyChanged: OnIsFocusedPropertyChanged);

		public static readonly BindableProperty IsFocusedProperty = IsFocusedPropertyKey.BindableProperty;

		public static readonly BindableProperty FlowDirectionProperty = BindableProperty.Create(nameof(FlowDirection), typeof(FlowDirection), typeof(VisualElement), FlowDirection.MatchParent, propertyChanged: FlowDirectionChanged);

		IFlowDirectionController FlowController => this;

		public FlowDirection FlowDirection
		{
			get { return (FlowDirection)GetValue(FlowDirectionProperty); }
			set { SetValue(FlowDirectionProperty, value); }
		}

		EffectiveFlowDirection _effectiveFlowDirection = default(EffectiveFlowDirection);
		EffectiveFlowDirection IFlowDirectionController.EffectiveFlowDirection
		{
			get { return _effectiveFlowDirection; }
			set
			{
				if (value == _effectiveFlowDirection)
					return;

				_effectiveFlowDirection = value;
				InvalidateMeasureInternal(InvalidationTrigger.Undefined);
				OnPropertyChanged(FlowDirectionProperty.PropertyName);
			}
		}

		EffectiveFlowDirection IVisualElementController.EffectiveFlowDirection => FlowController.EffectiveFlowDirection;

		readonly Dictionary<Size, SizeRequest> _measureCache = new Dictionary<Size, SizeRequest>();

		readonly MergedStyle _mergedStyle;

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

		internal VisualElement()
		{
			Navigation = new NavigationProxy();
			_mergedStyle = new MergedStyle(GetType(), this);
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

		public bool IsFocused
		{
			get { return (bool)GetValue(IsFocusedProperty); }
		}

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

		public INavigation Navigation
		{
			get { return (INavigation)GetValue(NavigationProperty); }
			internal set { SetValue(NavigationPropertyKey, value); }
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
			get { return (double)GetValue(ScaleProperty); }
			set { SetValue(ScaleProperty, value); }
		}

		public Style Style
		{
			get { return (Style)GetValue(StyleProperty); }
			set { SetValue(StyleProperty, value); }
		}

		[TypeConverter(typeof(ListStringTypeConverter))]
		public IList<string> StyleClass
		{
			get { return _mergedStyle.StyleClass; }
			set { _mergedStyle.StyleClass = value; }
		}

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

		public IList<TriggerBase> Triggers
		{
			get { return (IList<TriggerBase>)GetValue(TriggersProperty); }
		}

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

		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool Batched
		{
			get { return _batched > 0; }
		}

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

		internal LayoutConstraint Constraint
		{
			get { return ComputedConstraint | SelfConstraint; }
		}

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

				OnIsPlatformEnabledChanged();
			}
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public NavigationProxy NavigationProxy
		{
			get { return Navigation as NavigationProxy; }
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

		public void BatchBegin()
		{
			_batched++;
		}

		public void BatchCommit()
		{
			_batched = Math.Max(0, _batched - 1);
			if (!Batched && BatchCommitted != null)
				BatchCommitted(this, new EventArg<VisualElement>(this));
		}

		ResourceDictionary _resources;
		bool IResourcesProvider.IsResourcesCreated => _resources != null;

		public ResourceDictionary Resources
		{
			get {
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
		public void NativeSizeChanged()
		{
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

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
		public virtual SizeRequest GetSizeRequest(double widthConstraint, double heightConstraint)
		{
			SizeRequest cachedResult;
			var constraintSize = new Size(widthConstraint, heightConstraint);
			if (_measureCache.TryGetValue(constraintSize, out cachedResult))
			{
				return cachedResult;
			}

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
			{
				_measureCache[constraintSize] = r;
			}

			return r;
		}

		public void Layout(Rectangle bounds)
		{
			Bounds = bounds;
		}

		public SizeRequest Measure(double widthConstraint, double heightConstraint, MeasureFlags flags = MeasureFlags.None)
		{
			bool includeMargins = (flags & MeasureFlags.IncludeMargins) != 0;
			Thickness margin = default(Thickness);
			if (includeMargins)
			{
				var view = this as View;
				if (view != null)
					margin = view.Margin;
				widthConstraint = Math.Max(0, widthConstraint - margin.HorizontalThickness);
				heightConstraint = Math.Max(0, heightConstraint - margin.VerticalThickness);
			}
#pragma warning disable 0618 // retain until GetSizeRequest removed
			SizeRequest result = GetSizeRequest(widthConstraint, heightConstraint);
#pragma warning restore 0618

			if (includeMargins)
			{
				if (!margin.IsDefault)
				{
					result.Minimum = new Size(result.Minimum.Width + margin.HorizontalThickness, result.Minimum.Height + margin.VerticalThickness);
					result.Request = new Size(result.Request.Width + margin.HorizontalThickness, result.Request.Height + margin.VerticalThickness);
				}
			}

			return result;
		}

		public event EventHandler MeasureInvalidated;

		public event EventHandler SizeChanged;

		public void Unfocus()
		{
			if (!IsFocused)
				return;

			EventHandler<FocusRequestArgs> unfocus = FocusChangeRequested;
			if (unfocus != null)
			{
				unfocus(this, new FocusRequestArgs());
			}
		}

		public event EventHandler<FocusEventArgs> Unfocused;

		protected virtual void InvalidateMeasure()
		{
			InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
		}

		protected override void OnChildAdded(Element child)
		{
			base.OnChildAdded(child);
			var view = child as View;
			if (view != null)
				ComputeConstraintForView(view);
		}

		protected override void OnChildRemoved(Element child)
		{
			base.OnChildRemoved(child);
			var view = child as View;
			if (view != null)
				view.ComputedConstraint = LayoutConstraint.None;
		}

		protected void OnChildrenReordered()
		{
			if (ChildrenReordered != null)
				ChildrenReordered(this, EventArgs.Empty);
		}

		protected virtual SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
		{
#pragma warning disable 0618 // retain until OnSizeRequest removed
			return OnSizeRequest(widthConstraint, heightConstraint);
#pragma warning restore 0618
		}

		protected override void OnParentSet()
		{
#pragma warning disable 0618 // retain until ParentView removed
			base.OnParentSet();

			if (ParentView != null)
			{
				NavigationProxy.Inner = ParentView.NavigationProxy;
			}
			else
			{
				NavigationProxy.Inner = null;
			}
#pragma warning restore 0618

			FlowController.NotifyFlowDirectionChanged();
		}

		protected virtual void OnSizeAllocated(double width, double height)
		{
		}

		[Obsolete("OnSizeRequest is obsolete as of version 2.2.0. Please use OnMeasure instead.")]
		protected virtual SizeRequest OnSizeRequest(double widthConstraint, double heightConstraint)
		{
			if (Platform == null || !IsPlatformEnabled)
			{
				return new SizeRequest(new Size(-1, -1));
			}
			return Platform.GetNativeSize(this, widthConstraint, heightConstraint);
		}

		protected void SizeAllocated(double width, double height)
		{
			OnSizeAllocated(width, height);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public event EventHandler<EventArg<VisualElement>> BatchCommitted;

		internal void ComputeConstrainsForChildren()
		{
			for (var i = 0; i < LogicalChildrenInternal.Count; i++)
			{
				var child = LogicalChildrenInternal[i] as View;
				if (child != null)
					ComputeConstraintForView(child);
			}
		}

		internal virtual void ComputeConstraintForView(View view)
		{
			view.ComputedConstraint = LayoutConstraint.None;
		}

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

		void IVisualElementController.InvalidateMeasure(InvalidationTrigger trigger)
		{
			InvalidateMeasureInternal(trigger);
		}

		internal void MockBounds(Rectangle bounds)
		{
#if NETSTANDARD2_0
			(_mockX, _mockY, _mockWidth, _mockHeight) = bounds;
#else
			_mockX = bounds.X;
			_mockY = bounds.Y;
			_mockWidth = bounds.Width;
			_mockHeight = bounds.Height;
#endif
		}

		internal virtual void OnConstraintChanged(LayoutConstraint oldConstraint, LayoutConstraint newConstraint)
		{
			ComputeConstrainsForChildren();
		}

		internal virtual void OnIsPlatformEnabledChanged()
		{
		}

		internal virtual void OnIsVisibleChanged(bool oldValue, bool newValue)
		{
			InvalidateMeasureInternal(InvalidationTrigger.Undefined);
		}

		internal override void OnResourcesChanged(object sender, ResourcesChangedEventArgs e)
		{
			if (e == ResourcesChangedEventArgs.StyleSheets)
				ApplyStyleSheets();
			else
				base.OnResourcesChanged(sender, e);
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

		internal void UnmockBounds()
		{
			_mockX = _mockY = _mockWidth = _mockHeight = -1;
		}

		void OnFocused()
		{
			EventHandler<FocusEventArgs> focus = Focused;
			if (focus != null)
				focus(this, new FocusEventArgs(this, true));
		}

		static void FlowDirectionChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var self = bindable as IFlowDirectionController;

			if (self.EffectiveFlowDirection.IsExplicit() && oldValue == newValue)
				return;

			var newFlowDirection = (FlowDirection)newValue;

			self.EffectiveFlowDirection = newFlowDirection.ToEffectiveFlowDirection(isExplicit: true);

			self.NotifyFlowDirectionChanged();
		}

		static void OnIsEnabledPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			var element = (VisualElement)bindable;

			if (element == null)
			{
				return;
			}

			var isEnabled = (bool)newValue;

			VisualStateManager.GoToState(element, isEnabled 
				? VisualStateManager.CommonStates.Normal 
				: VisualStateManager.CommonStates.Disabled);
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

			VisualStateManager.GoToState(element, isFocused
				? VisualStateManager.CommonStates.Normal
				: VisualStateManager.CommonStates.Focused);
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
			((VisualElement)bindable).InvalidateMeasureInternal(InvalidationTrigger.SizeRequestChanged);
		}

		void OnUnfocus()
		{
			EventHandler<FocusEventArgs> unFocus = Unfocused;
			if (unFocus != null)
				unFocus(this, new FocusEventArgs(this, false));
		}

		void IFlowDirectionController.NotifyFlowDirectionChanged()
		{
			SetFlowDirectionFromParent(this);

			foreach (var element in LogicalChildren)
			{
				var view = element as IFlowDirectionController;
				if (view == null)
					continue;
				view.NotifyFlowDirectionChanged();
			}
		}

		void SetSize(double width, double height)
		{
			if (Width == width && Height == height)
				return;

			Width = width;
			Height = height;

			SizeAllocated(width, height);
			if (SizeChanged != null)
				SizeChanged(this, EventArgs.Empty);
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
				if (value != null) {
					if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
						return true;
					if (value.Equals("visible", StringComparison.OrdinalIgnoreCase))
						return true;
					if (value.Equals("false", StringComparison.OrdinalIgnoreCase))
						return false;
					if (value.Equals("hidden", StringComparison.OrdinalIgnoreCase))
						return false;
					if (value.Equals("collapse", StringComparison.OrdinalIgnoreCase))
						return false;
				}
				throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(bool)));

			}
		}
	}
}
