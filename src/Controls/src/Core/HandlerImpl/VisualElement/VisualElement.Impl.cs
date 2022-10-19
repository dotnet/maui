#nullable enable
using System;
using System.ComponentModel;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="Type[@FullName='Microsoft.Maui.Controls.VisualElement']/Docs/*" />
	public partial class VisualElement : IView
	{
		Semantics _semantics;
		bool _isLoadedFired;
		EventHandler? _loaded;
		EventHandler? _unloaded;
		bool _watchingPlatformLoaded;

		Rect _frame = new Rect(0, 0, -1, -1);

		/// <include file="../../../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Frame']/Docs/*" />
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

		/// <include file="../../../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Handler']/Docs/*" />
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

		/// <include file="../../../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='ShadowProperty']/Docs/*" />
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

		/// <include file="../../../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Shadow']/Docs/*" />
		public Shadow Shadow
		{
			get { return (Shadow)GetValue(ShadowProperty); }
			set { SetValue(ShadowProperty, value); }
		}

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

		/// <include file="../../../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='ZIndex']/Docs/*" />
		public int ZIndex
		{
			get { return (int)GetValue(ZIndexProperty); }
			set { SetValue(ZIndexProperty, value); }
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='DesiredSize']/Docs/*" />
		public Size DesiredSize { get; protected set; }

		/// <include file="../../../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Arrange']/Docs/*" />
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

		/// <include file="../../../../docs/Microsoft.Maui.Controls/VisualElement.xml" path="//Member[@MemberName='Layout']/Docs/*" />
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

		Semantics IView.Semantics
		{
			get
			{
				UpdateSemantics();
				return _semantics;
			}
		}

		void UpdateSemantics()
		{
			if (!this.IsSet(SemanticProperties.HintProperty) &&
				!this.IsSet(SemanticProperties.DescriptionProperty) &&
				!this.IsSet(SemanticProperties.HeadingLevelProperty))
			{
				return;
			}

			_semantics ??= new Semantics();
			_semantics.Description = SemanticProperties.GetDescription(this);
			_semantics.HeadingLevel = SemanticProperties.GetHeadingLevel(this);
			_semantics.Hint = SemanticProperties.GetHint(this);
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
			if (Shadow != null)
			{
				Shadow.Parent = this;
				Shadow.PropertyChanged += OnShadowChanged;
			}
		}

		void StopNotifyingShadowChanges()
		{
			if (Shadow != null)
			{
				Shadow.Parent = null;
				Shadow.PropertyChanged -= OnShadowChanged;

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
	}
}