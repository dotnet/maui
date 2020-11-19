using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ElmSharp;
using ElmSharp.Accessible;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Tizen.Native;
using EFocusDirection = ElmSharp.FocusDirection;
using ERect = ElmSharp.Rect;
using ESize = ElmSharp.Size;
using Specific = Xamarin.Forms.PlatformConfiguration.TizenSpecific.VisualElement;
using XFocusDirection = Xamarin.Forms.PlatformConfiguration.TizenSpecific.FocusDirection;

namespace Xamarin.Forms.Platform.Tizen
{
	/// <summary>
	/// Base class for rendering of a Xamarin element.
	/// </summary>
	public abstract class VisualElementRenderer<TElement> : IVisualElementRenderer, IEffectControlProvider where TElement : VisualElement
	{
		readonly List<EventHandler<VisualElementChangedEventArgs>> _elementChangedHandlers = new List<EventHandler<VisualElementChangedEventArgs>>();

		readonly Dictionary<string, Action<bool>> _propertyHandlersWithInit = new Dictionary<string, Action<bool>>();

		readonly Dictionary<string, Action> _propertyHandlers = new Dictionary<string, Action>();

		readonly HashSet<string> _batchedProperties = new HashSet<string>();

		VisualElementRendererFlags _flags = VisualElementRendererFlags.None;

		bool _movedCallbackEnabled = false;
		string _defaultAccessibilityName;
		string _defaultAccessibilityDescription;
		bool? _defaultIsAccessibilityElement;

		Lazy<CustomFocusManager> _customFocusManager;

		/// <summary>
		/// Default constructor.
		/// </summary>
		protected VisualElementRenderer()
		{
			RegisterPropertyHandler(VisualElement.IsVisibleProperty, UpdateIsVisible);
			RegisterPropertyHandler(VisualElement.OpacityProperty, UpdateOpacity);
			RegisterPropertyHandler(VisualElement.IsEnabledProperty, UpdateIsEnabled);
			RegisterPropertyHandler(VisualElement.InputTransparentProperty, UpdateInputTransparent);
			RegisterPropertyHandler(VisualElement.BackgroundColorProperty, UpdateBackgroundColor);
			RegisterPropertyHandler(VisualElement.BackgroundProperty, UpdateBackground);
			RegisterPropertyHandler(VisualElement.ClipProperty, UpdateClip);

			RegisterPropertyHandler(Specific.StyleProperty, UpdateThemeStyle);
			RegisterPropertyHandler(Specific.IsFocusAllowedProperty, UpdateFocusAllowed);
			RegisterPropertyHandler(Specific.NextFocusDirectionProperty, UpdateFocusDirection);
			RegisterPropertyHandler(Specific.NextFocusUpViewProperty, UpdateFocusUpView);
			RegisterPropertyHandler(Specific.NextFocusDownViewProperty, UpdateFocusDownView);
			RegisterPropertyHandler(Specific.NextFocusLeftViewProperty, UpdateFocusLeftView);
			RegisterPropertyHandler(Specific.NextFocusRightViewProperty, UpdateFocusRightView);
			RegisterPropertyHandler(Specific.NextFocusBackViewProperty, UpdateFocusBackView);
			RegisterPropertyHandler(Specific.NextFocusForwardViewProperty, UpdateFocusForwardView);
			RegisterPropertyHandler(Specific.ToolTipProperty, UpdateToolTip);

			RegisterPropertyHandler(VisualElement.AnchorXProperty, UpdateTransformation);
			RegisterPropertyHandler(VisualElement.AnchorYProperty, UpdateTransformation);
			RegisterPropertyHandler(VisualElement.ScaleProperty, UpdateTransformation);
			RegisterPropertyHandler(VisualElement.ScaleXProperty, UpdateTransformation);
			RegisterPropertyHandler(VisualElement.ScaleYProperty, UpdateTransformation);
			RegisterPropertyHandler(VisualElement.RotationProperty, UpdateTransformation);
			RegisterPropertyHandler(VisualElement.RotationXProperty, UpdateTransformation);
			RegisterPropertyHandler(VisualElement.RotationYProperty, UpdateTransformation);
			RegisterPropertyHandler(VisualElement.TranslationXProperty, UpdateTransformation);
			RegisterPropertyHandler(VisualElement.TranslationYProperty, UpdateTransformation);
			RegisterPropertyHandler(VisualElement.TabIndexProperty, UpdateTabIndex);
			RegisterPropertyHandler(VisualElement.IsTabStopProperty, UpdateIsTabStop);

			RegisterPropertyHandler(AutomationProperties.NameProperty, SetAccessibilityName);
			RegisterPropertyHandler(AutomationProperties.HelpTextProperty, SetAccessibilityDescription);
			RegisterPropertyHandler(AutomationProperties.IsInAccessibleTreeProperty, SetIsAccessibilityElement);
			RegisterPropertyHandler(AutomationProperties.LabeledByProperty, SetLabeledBy);

			_customFocusManager = new Lazy<CustomFocusManager>(() =>
			{
				return new CustomFocusManager(Element, NativeView as Widget);
			});
		}

		~VisualElementRenderer()
		{
			Dispose(false);
		}

		event EventHandler<VisualElementChangedEventArgs> ElementChanged
		{
			add
			{
				_elementChangedHandlers.Add(value);
			}
			remove
			{
				_elementChangedHandlers.Remove(value);
			}
		}

		/// <summary>
		/// Gets the Xamarin element associated with this renderer.
		/// </summary>
		public TElement Element
		{
			get;
			private set;
		}

		VisualElement IVisualElementRenderer.Element
		{
			get
			{
				return Element;
			}
		}

		public EvasObject NativeView { get; private set; }

		protected bool IsDisposed => _flags.HasFlag(VisualElementRendererFlags.Disposed);

		/// <summary>
		/// Releases all resource used by the <see cref="Xamarin.Forms.Platform.Tizen.VisualElementRenderer"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the
		/// <see cref="Xamarin.Forms.Platform.Tizen.VisualElementRenderer"/>. The <see cref="Dispose"/> method
		/// leaves the <see cref="Xamarin.Forms.Platform.Tizen.VisualElementRenderer"/> in an unusable state.
		/// After calling <see cref="Dispose"/>, you must release all references to the
		/// <see cref="Xamarin.Forms.Platform.Tizen.VisualElementRenderer"/> so the garbage collector can reclaim
		/// the memory that the <see cref="Xamarin.Forms.Platform.Tizen.VisualElementRenderer"/> was occupying.</remarks>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			if (null == NativeView)
			{
				return new SizeRequest(new Size(0, 0));
			}
			else
			{
				int availableWidth = Forms.ConvertToScaledPixel(widthConstraint);
				int availableHeight = Forms.ConvertToScaledPixel(heightConstraint);

				if (availableWidth < 0)
					availableWidth = int.MaxValue;
				if (availableHeight < 0)
					availableHeight = int.MaxValue;

				Size measured;
				var nativeViewMeasurable = NativeView as Native.IMeasurable;
				if (nativeViewMeasurable != null)
				{
					measured = nativeViewMeasurable.Measure(availableWidth, availableHeight).ToDP();
				}
				else
				{
					measured = Measure(availableWidth, availableHeight).ToDP();
				}

				return new SizeRequest(measured, MinimumSize());
			}
		}

		/// <summary>
		/// Sets the element associated with this renderer.
		/// </summary>
		public void SetElement(TElement newElement)
		{
			if (newElement == null)
			{
				throw new ArgumentNullException(nameof(newElement));
			}

			TElement oldElement = Element;

			Element = newElement;

			if (oldElement != null)
			{
				Platform.SetRenderer(oldElement, null);
			}

			// send notification
			OnElementChanged(new ElementChangedEventArgs<TElement>(oldElement, newElement));

			// store renderer for the new element
			Platform.SetRenderer(newElement, this);

			// add children
			var logicalChildren = (newElement as IElementController).LogicalChildren;
			foreach (Element child in logicalChildren)
			{
				AddChild(child);
			}

			newElement.IsPlatformEnabled = true;

			OnElementReady();

			SendVisualElementInitialized(newElement, NativeView);
		}

		void IVisualElementRenderer.UpdateLayout()
		{
			UpdateLayout();
		}

		void IVisualElementRenderer.SetElement(VisualElement element)
		{
			TElement tElement = element as TElement;
			if (tElement == null)
			{
				throw new ArgumentException("Element is not of type " + typeof(TElement), nameof(element));
			}
			SetElement(tElement);
		}

		/// <summary>
		/// Registers the effect with the element by establishing the parent-child relations needed for rendering on the specific platform.
		/// </summary>
		/// <param name="effect">The effect to register.</param>
		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			RegisterEffect(effect);
		}

		/// <summary>
		/// Registers the effect with the element by establishing the parent-child relations needed for rendering on the specific platform.
		/// </summary>
		/// <param name="effect">The effect to register.</param>
		protected void RegisterEffect(Effect effect)
		{
			var platformEffect = effect as PlatformEffect;
			if (platformEffect != null)
			{
				OnRegisterEffect(platformEffect);
			}
		}

		protected virtual void UpdateLayout()
		{
			if (null != NativeView)
			{
				UpdateNativeGeometry();
			}
		}

		/// <summary>
		/// Disposes of underlying resources.
		/// </summary>
		/// <param name="disposing">True if the memory release was requested on demand.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (IsDisposed)
			{
				return;
			}

			_flags |= VisualElementRendererFlags.Disposed;

			if (disposing)
			{
				// This is the reason why I call SendDisappearing() here.
				// When OnChildRemove is called first like how it is called in Navigation.PopToRootAsync(),
				// you can not controll using SendDisappearing() on the lower class.
				(Element as IPageController)?.SendDisappearing();

				if (Element != null)
				{
					Element.PropertyChanged -= OnElementPropertyChanged;
					Element.BatchCommitted -= OnBatchCommitted;

					Element.ChildAdded -= OnChildAdded;
					Element.ChildRemoved -= OnChildRemoved;
					Element.ChildrenReordered -= OnChildrenReordered;

					Element.FocusChangeRequested -= OnFocusChangeRequested;

					var logicalChildren = (Element as IElementController).LogicalChildren;
					foreach (var child in logicalChildren)
					{
						Platform.GetRenderer(child)?.Dispose();
					}

					if (Platform.GetRenderer(Element) == this)
					{
						Platform.SetRenderer(Element, null);
					}

					// Reset Element geometry, to re-calculate when Renderer was re-attached
					Element.Layout(new Rectangle(0, 0, -1, -1));

					Element = default(TElement);
				}

				if (NativeView != null)
				{
					NativeView.Deleted -= NativeViewDeleted;
					NativeView.Unrealize();
					NativeView = null;
				}

				if (_customFocusManager.IsValueCreated)
				{
					_customFocusManager.Value.Dispose();
				}
			}
		}

		/// <summary>
		/// Notification that the associated element has changed.
		/// </summary>
		/// <param name="e">Event parameters.</param>
		protected virtual void OnElementChanged(ElementChangedEventArgs<TElement> e)
		{
			if (null != e.OldElement)
			{
				e.OldElement.PropertyChanged -= OnElementPropertyChanged;
				e.OldElement.BatchCommitted -= OnBatchCommitted;

				e.OldElement.ChildAdded -= OnChildAdded;
				e.OldElement.ChildRemoved -= OnChildRemoved;
				e.OldElement.ChildrenReordered -= OnChildrenReordered;

				e.OldElement.FocusChangeRequested -= OnFocusChangeRequested;

				var controller = e.OldElement as IElementController;
				if (controller != null && controller.EffectControlProvider == this)
				{
					controller.EffectControlProvider = null;
				}
			}

			if (null != e.NewElement)
			{
				e.NewElement.PropertyChanged += OnElementPropertyChanged;
				e.NewElement.BatchCommitted += OnBatchCommitted;

				e.NewElement.ChildAdded += OnChildAdded;
				e.NewElement.ChildRemoved += OnChildRemoved;
				e.NewElement.ChildrenReordered += OnChildrenReordered;

				e.NewElement.FocusChangeRequested += OnFocusChangeRequested;

				(NativeView as IBatchable)?.BatchBegin();
				UpdateAllProperties(true);
				(NativeView as IBatchable)?.BatchCommit();

				var controller = e.NewElement as IElementController;
				if (controller != null)
				{
					controller.EffectControlProvider = this;
				}
			}
		}

		/// <summary>
		/// Notification that the property of the associated element has changed.
		/// </summary>
		/// <param name="sender">Object which sent the notification.</param>
		/// <param name="e">Event parameters.</param>
		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (Element.Batched)
			{
				if (e.PropertyName == VisualElement.XProperty.PropertyName ||
					e.PropertyName == VisualElement.YProperty.PropertyName ||
					e.PropertyName == VisualElement.WidthProperty.PropertyName ||
					e.PropertyName == VisualElement.HeightProperty.PropertyName)
				{
					_flags |= VisualElementRendererFlags.NeedsLayout;
				}
				else if (e.PropertyName == VisualElement.TranslationXProperty.PropertyName ||
						e.PropertyName == VisualElement.TranslationYProperty.PropertyName ||
						e.PropertyName == VisualElement.RotationProperty.PropertyName ||
						e.PropertyName == VisualElement.RotationXProperty.PropertyName ||
						e.PropertyName == VisualElement.RotationYProperty.PropertyName ||
						e.PropertyName == VisualElement.ScaleProperty.PropertyName ||
						e.PropertyName == VisualElement.AnchorXProperty.PropertyName ||
						e.PropertyName == VisualElement.AnchorYProperty.PropertyName)
				{
					_flags |= VisualElementRendererFlags.NeedsTransformation;
				}
				else
				{
					_batchedProperties.Add(e.PropertyName);
				}
				return;
			}

			Action<bool> init;
			if (_propertyHandlersWithInit.TryGetValue(e.PropertyName, out init))
			{
				init(false);
			}
			else
			{
				Action handler;
				if (_propertyHandlers.TryGetValue(e.PropertyName, out handler))
				{
					handler();
				}
			}
		}

		/// <summary>
		/// Updates the attached event handlers, sets the native control.
		/// </summary>
		protected void SetNativeView(EvasObject control)
		{
			if (NativeView != null)
			{
				if (_movedCallbackEnabled)
				{
					NativeView.Moved -= OnMoved;
				}
				NativeView.Deleted -= NativeViewDeleted;
			}

			Widget widget = NativeView as Widget;
			if (widget != null)
			{
				widget.Focused -= OnFocused;
				widget.Unfocused -= OnUnfocused;
			}
			NativeView = control;
			if (NativeView != null)
			{
				NativeView.Deleted += NativeViewDeleted;
				if (_movedCallbackEnabled)
				{
					NativeView.Moved += OnMoved;
				}
			}

			widget = NativeView as Widget;
			if (widget != null)
			{
				widget.Focused += OnFocused;
				widget.Unfocused += OnUnfocused;
			}
		}

		protected virtual void SetAccessibilityName(bool initialize)
		{
			if (initialize && (string)Element.GetValue(AutomationProperties.NameProperty) == (default(string)))
				return;

			var accessibleObject = NativeView as IAccessibleObject;
			if (accessibleObject != null)
			{
				_defaultAccessibilityName = accessibleObject.SetAccessibilityName(Element, _defaultAccessibilityName);
			}
		}

		protected virtual void SetAccessibilityDescription(bool initialize)
		{
			if (initialize && (string)Element.GetValue(AutomationProperties.HelpTextProperty) == (default(string)))
				return;

			var accessibleObject = NativeView as IAccessibleObject;
			if (accessibleObject != null)
			{
				_defaultAccessibilityDescription = accessibleObject.SetAccessibilityDescription(Element, _defaultAccessibilityDescription);
			}
		}

		protected virtual void SetIsAccessibilityElement(bool initialize)
		{
			if (initialize && (bool?)Element.GetValue(AutomationProperties.IsInAccessibleTreeProperty) == default(bool?))
				return;

			var accessibleObject = NativeView as IAccessibleObject;
			if (accessibleObject != null)
			{
				_defaultIsAccessibilityElement = accessibleObject.SetIsAccessibilityElement(Element, _defaultIsAccessibilityElement);
			}
		}

		protected virtual void SetLabeledBy(bool initialize)
		{
			if (initialize && (VisualElement)Element.GetValue(AutomationProperties.LabeledByProperty) == default(VisualElement))
				return;

			var accessibleObject = NativeView as IAccessibleObject;
			if (accessibleObject != null)
			{
				accessibleObject.SetLabeledBy(Element);
			}
		}

		protected Widget FocusSearch(bool forwardDirection)
		{
			VisualElement element = Element as VisualElement;
			int maxAttempts = 0;
			var tabIndexes = element?.GetTabIndexesOnParentPage(out maxAttempts);
			if (tabIndexes == null)
				return null;

			int tabIndex = Element.TabIndex;
			int attempt = 0;

			do
			{
				element = element.FindNextElement(forwardDirection, tabIndexes, ref tabIndex) as VisualElement;
				var renderer = Platform.GetRenderer(element);
				if (renderer?.NativeView is Widget widget && widget.IsFocusAllowed)
				{
					return widget;
				}
			} while (!(element.IsFocused || ++attempt >= maxAttempts));
			return null;
		}

		internal virtual void SendVisualElementInitialized(VisualElement element, EvasObject nativeView)
		{
			element.SendViewInitialized(nativeView);
		}

		void UpdateNativeGeometry()
		{
			var updatedGeometry = new Rectangle(ComputeAbsolutePoint(Element), new Size(Element.Width, Element.Height)).ToPixel();

			if (NativeView.Geometry != updatedGeometry)
			{
				NativeView.Geometry = updatedGeometry;
				ApplyTransformation();
			}
		}

		void NativeViewDeleted(object sender, EventArgs e)
		{
			Dispose();
		}

		void OnBatchCommitted(object sender, EventArg<VisualElement> e)
		{
			if (_flags.HasFlag(VisualElementRendererFlags.NeedsLayout))
			{
				UpdateLayout();
				// UpdateLayout already updates transformation, clear NeedsTranformation flag then
				_flags &= ~VisualElementRendererFlags.NeedsTransformation;

				_flags ^= VisualElementRendererFlags.NeedsLayout;
			}
			if (_flags.HasFlag(VisualElementRendererFlags.NeedsTransformation))
			{
				ApplyTransformation();
				_flags ^= VisualElementRendererFlags.NeedsTransformation;
			}

			foreach (string property in _batchedProperties)
			{
				OnElementPropertyChanged(this, new PropertyChangedEventArgs(property));
			}
			_batchedProperties.Clear();
		}

		/// <summary>
		/// Registers a handler which is executed when specified property changes.
		/// </summary>
		/// <param name="property">Handled property.</param>
		/// <param name="handler">Action to be executed when property changes.</param>
		protected void RegisterPropertyHandler(BindableProperty property, Action<bool> handler)
		{
			RegisterPropertyHandler(property.PropertyName, handler);
		}

		/// <summary>
		/// Registers a handler which is executed when specified property changes.
		/// </summary>
		/// <param name="name">Name of the handled property.</param>
		/// <param name="handler">Action to be executed when property changes.</param>
		protected void RegisterPropertyHandler(string name, Action<bool> handler)
		{
			_propertyHandlersWithInit.Add(name, handler);
		}

		/// <summary>
		/// Registers a handler which is executed when specified property changes.
		/// </summary>
		/// <param name="property">Handled property.</param>
		/// <param name="handler">Action to be executed when property changes.</param>
		protected void RegisterPropertyHandler(BindableProperty property, Action handler)
		{
			RegisterPropertyHandler(property.PropertyName, handler);
		}

		/// <summary>
		/// Registers a handler which is executed when specified property changes.
		/// </summary>
		/// <param name="name">Name of the handled property.</param>
		/// <param name="handler">Action to be executed when property changes.</param>
		protected void RegisterPropertyHandler(string name, Action handler)
		{
			_propertyHandlers.Add(name, handler);
		}

		/// <summary>
		/// Updates all registered properties.
		/// </summary>
		/// <param name="initialization">If set to <c>true</c> the method is called for an uninitialized object.</param>
		protected void UpdateAllProperties(bool initialization)
		{
			foreach (var action in _propertyHandlersWithInit.Values.Distinct())
			{
				action(initialization);
			}

			foreach (var action in _propertyHandlers.Values.Distinct())
			{
				action();
			}
		}

		/// <summary>
		/// Called when Element has been set and its native counterpart
		/// is properly initialized.
		/// </summary>
		protected virtual void OnElementReady()
		{
		}

		protected virtual Size MinimumSize()
		{
			return new ESize(NativeView.MinimumWidth, NativeView.MinimumHeight).ToDP();
		}

		/// <summary>
		/// Calculates how much space this element should take, given how much room there is.
		/// </summary>
		/// <returns>a desired dimensions of the element</returns>
		protected virtual ESize Measure(int availableWidth, int availableHeight)
		{
			return new ESize(NativeView.MinimumWidth, NativeView.MinimumHeight);
		}

		protected virtual void UpdateBackgroundColor(bool initialize)
		{
			if (initialize && Element.BackgroundColor.IsDefault)
				return;

			if (NativeView is Widget)
			{
				(NativeView as Widget).BackgroundColor = Element.BackgroundColor.ToNative();
			}
			else
			{
				Log.Warn("{0} uses {1} which does not support background color", this, NativeView);
			}
		}

		protected virtual void UpdateBackground(bool initialize)
		{
			if (!Forms.UseSkiaSharp || (initialize && Element.Background.Equals(Brush.Default)))
				return;

			if (this is SkiaSharp.IBackgroundCanvas canvasRenderer)
			{
				canvasRenderer.BackgroundCanvas.Invalidate();
			}
		}

		protected virtual void UpdateClip(bool initialize)
		{
			if (!Forms.UseSkiaSharp || (initialize && Element.Clip == null))
				return;

			if (this is SkiaSharp.IClipperCanvas canvasRenderer)
			{
				canvasRenderer.ClipperCanvas.Invalidate();
			}
		}

		protected virtual void UpdateOpacity(bool initialize)
		{
			if (initialize && Element.Opacity == 1d)
				return;

			if (NativeView is Widget)
			{
				(NativeView as Widget).Opacity = (int)(Element.Opacity * 255.0);
			}
			else
			{
				Log.Warn("{0} uses {1} which does not support opacity", this, NativeView);
			}
		}

		public virtual ERect GetNativeContentGeometry()
		{
			return NativeView.Geometry;
		}

		static double ComputeAbsoluteX(VisualElement e)
		{
			return e.X + ((e.RealParent is VisualElement) ? Forms.ConvertToScaledDP(Platform.GetRenderer(e.RealParent).GetNativeContentGeometry().X) : 0.0);
		}

		static double ComputeAbsoluteY(VisualElement e)
		{
			return e.Y + ((e.RealParent is VisualElement) ? Forms.ConvertToScaledDP(Platform.GetRenderer(e.RealParent).GetNativeContentGeometry().Y) : 0.0);
		}

		static Point ComputeAbsolutePoint(VisualElement e)
		{
			return new Point(ComputeAbsoluteX(e), ComputeAbsoluteY(e));
		}

		/// <summary>
		/// Handles focus events.
		/// </summary>
		protected virtual void OnFocused(object sender, EventArgs e)
		{
			if (null != Element)
			{
				Element.SetValue(VisualElement.IsFocusedPropertyKey, true);
			}
		}

		/// <summary>
		/// Handles unfocus events.
		/// </summary>
		protected virtual void OnUnfocused(object sender, EventArgs e)
		{
			if (null != Element)
			{
				Element.SetValue(VisualElement.IsFocusedPropertyKey, false);
			}
		}

		/// <summary>
		/// Adds a new child if it's derived from the VisualElement class. Otherwise this method does nothing.
		/// </summary>
		/// <param name="child">Child to be added.</param>
		protected virtual void AddChild(Element child)
		{
			VisualElement vElement = child as VisualElement;
			if (vElement != null)
			{
				var childRenderer = Platform.GetOrCreateRenderer(vElement);

				// if the native view can have children, attach the new child
				if (NativeView is Native.IContainable<EvasObject>)
				{
					(NativeView as Native.IContainable<EvasObject>).Children.Add(childRenderer.NativeView);
				}
			}
		}

		protected virtual void RemoveChild(VisualElement view)
		{
			var renderer = Platform.GetRenderer(view);
			var containerObject = NativeView as Native.IContainable<EvasObject>;
			if (containerObject != null)
			{
				containerObject.Children.Remove(renderer.NativeView);
			}

			renderer.Dispose();
		}

		void OnChildAdded(object sender, ElementEventArgs e)
		{
			var view = e.Element as VisualElement;
			if (view != null)
			{
				AddChild(view);
			}

			// changing the order makes sense only in case of Layouts
			if (Element is Layout)
			{
				IElementController controller = Element as IElementController;
				if (controller.LogicalChildren[controller.LogicalChildren.Count - 1] != view)
				{
					EnsureChildOrder();
				}
			}
		}

		void OnChildRemoved(object sender, ElementEventArgs e)
		{
			var view = e.Element as VisualElement;
			if (view != null)
			{
				RemoveChild(view);
			}
		}

		void OnChildrenReordered(object sender, EventArgs e)
		{
			EnsureChildOrder();
			Layout layout = Element as Layout;
			if (layout != null)
			{
				layout.InvalidateMeasureNonVirtual(InvalidationTrigger.MeasureChanged);
				layout.ForceLayout();
			}
		}

		void OnFocusChangeRequested(object sender, VisualElement.FocusRequestArgs e)
		{
			Widget widget = NativeView as Widget;
			if (widget == null)
			{
				Log.Warn("{0} is not a widget, it cannot receive focus", NativeView);
				return;
			}

			widget.SetFocus(e.Focus);
			e.Result = true;
		}

		/// <summary>
		/// On register the effect
		/// </summary>
		/// <param name="effect">The effect to register.</param>
		void OnRegisterEffect(PlatformEffect effect)
		{
			effect.SetContainer(Element.Parent == null ? null : Platform.GetRenderer(Element.Parent)?.NativeView);
			effect.SetControl(NativeView);
		}

		void OnMoved(object sender, EventArgs e)
		{
			ApplyTransformation();
		}

		void EnsureChildOrder()
		{
			var logicalChildren = (Element as IElementController).LogicalChildren;
			for (var i = logicalChildren.Count - 1; i >= 0; --i)
			{
				var element = logicalChildren[i] as VisualElement;
				if (element != null)
				{
					Platform.GetRenderer(element).NativeView?.Lower();
				}
			}
		}

		void UpdateIsVisible()
		{
			if (null != NativeView)
			{
				if (Element.IsVisible)
				{
					NativeView.Show();
				}
				else
				{
					NativeView.Hide();
				}
			}
		}

		/// <summary>
		/// Updates the IsEnabled property.
		/// </summary>
		protected virtual void UpdateIsEnabled(bool initialize)
		{
			if (initialize && Element.IsEnabled)
				return;

			var widget = NativeView as Widget;
			if (widget != null)
			{
				widget.IsEnabled = Element.IsEnabled;
			}
		}

		/// <summary>
		/// Updates the InputTransparent property.
		/// </summary>
		protected virtual void UpdateInputTransparent(bool initialize)
		{
			if (initialize && Element.InputTransparent == default(bool))
				return;

			NativeView.PassEvents = Element.InputTransparent;
		}

		protected virtual void UpdateThemeStyle()
		{
		}

		void UpdateTransformation(bool initialize)
		{
			if (!initialize)
				ApplyTransformation();
		}

		void UpdateFocusAllowed(bool initialize)
		{
			bool? isFocusAllowed = Specific.IsFocusAllowed(Element);
			if (initialize && isFocusAllowed == null)
				return;

			var widget = NativeView as Widget;
			if (widget != null && isFocusAllowed != null)
			{
				widget.AllowFocus((bool)Specific.IsFocusAllowed(Element));
			}
			else
			{
				Log.Warn("{0} uses {1} which does not support Focus management", this, NativeView);
			}
		}

		void UpdateFocusDirection(bool initialize)
		{
			var direction = Specific.GetNextFocusDirection(Element);
			if (!initialize && direction != XFocusDirection.None)
			{
				var widget = NativeView as Widget;
				if (widget != null)
				{
					widget.FocusNext(ConvertToNativeFocusDirection(direction));
				}
				else
				{
					Log.Warn("{0} uses {1} which does not support Focus management", this, NativeView);
				}
			}
		}

		void UpdateToolTip(bool initialize)
		{
			var tooltip = Specific.GetToolTip(Element);
			if (tooltip != null)
			{
				NativeView.SetTooltipText(tooltip);
			}
			else if (!initialize)
			{
				NativeView.UnsetTooltip();
			}
		}

		void SetNextFocusViewInternal(string direction)
		{
			var widget = NativeView as Widget;
			if (widget != null)
			{
				switch (direction)
				{
					case XFocusDirection.Back:
						_customFocusManager.Value.NextBackward = Specific.GetNextFocusBackView(Element);
						break;
					case XFocusDirection.Forward:
						_customFocusManager.Value.NextForward = Specific.GetNextFocusForwardView(Element);
						break;
					case XFocusDirection.Up:
						_customFocusManager.Value.NextUp = Specific.GetNextFocusUpView(Element);
						break;
					case XFocusDirection.Down:
						_customFocusManager.Value.NextDown = Specific.GetNextFocusDownView(Element);
						break;
					case XFocusDirection.Right:
						_customFocusManager.Value.NextRight = Specific.GetNextFocusRightView(Element);
						break;
					case XFocusDirection.Left:
						_customFocusManager.Value.NextLeft = Specific.GetNextFocusLeftView(Element);
						break;
					default:
						break;
				}
			}
			else
			{
				Log.Warn("{0} uses {1} which does not support Focus management", this, NativeView);
			}
		}

		void UpdateFocusUpView()
		{
			if (Specific.GetNextFocusUpView(Element) != null)
			{
				SetNextFocusViewInternal(XFocusDirection.Up);
			}
		}

		void UpdateFocusDownView()
		{
			if (Specific.GetNextFocusDownView(Element) != null)
			{
				SetNextFocusViewInternal(XFocusDirection.Down);
			}
		}

		void UpdateFocusLeftView()
		{
			if (Specific.GetNextFocusLeftView(Element) != null)
			{
				SetNextFocusViewInternal(XFocusDirection.Left);
			}
		}

		void UpdateFocusRightView()
		{
			if (Specific.GetNextFocusRightView(Element) != null)
			{
				SetNextFocusViewInternal(XFocusDirection.Right);
			}
		}

		void UpdateFocusBackView()
		{
			if (Specific.GetNextFocusBackView(Element) != null)
			{
				SetNextFocusViewInternal(XFocusDirection.Back);
			}
		}

		void UpdateFocusForwardView()
		{
			if (Specific.GetNextFocusForwardView(Element) != null)
			{
				SetNextFocusViewInternal(XFocusDirection.Forward);
			}
		}

		void ApplyRotation(EvasMap map, ERect geometry, ref bool changed)
		{
			var rotationX = Element.RotationX;
			var rotationY = Element.RotationY;
			var rotationZ = Element.Rotation;
			var anchorX = Element.AnchorX;
			var anchorY = Element.AnchorY;

			// apply rotations
			if (rotationX != 0 || rotationY != 0 || rotationZ != 0)
			{
				map.Rotate3D(rotationX, rotationY, rotationZ, (int)(geometry.X + geometry.Width * anchorX),
															  (int)(geometry.Y + geometry.Height * anchorY), 0);
				// the last argument is focal length, it determine the strength of distortion. We compared it with the Android implementation
				map.Perspective3D(geometry.X + geometry.Width / 2, geometry.Y + geometry.Height / 2, 0, (int)(1.3 * Math.Max(geometry.Height, geometry.Width)));
				// Need to unset clip because perspective 3d rotation is going beyond the container bound
				NativeView.SetClip(null);
				changed = true;
			}
		}

		void ApplyScale(EvasMap map, ERect geometry, ref bool changed)
		{
			var scale = Element.Scale;
			var scaleX = Element.ScaleX * scale;
			var scaleY = Element.ScaleY * scale;

			// apply scale factor
			if (scaleX != 1.0 || scaleY != 1.0)
			{
				map.Zoom(scaleX, scaleY,
					geometry.X + (int)(geometry.Width * Element.AnchorX),
					geometry.Y + (int)(geometry.Height * Element.AnchorY));
				changed = true;
			}
		}

		void ApplyTranslation(EvasMap map, ERect geometry, ref bool changed)
		{
			var shiftX = Forms.ConvertToScaledPixel(Element.TranslationX);
			var shiftY = Forms.ConvertToScaledPixel(Element.TranslationY);

			// apply translation, i.e. move/shift the object a little
			if (shiftX != 0 || shiftY != 0)
			{
				if (changed)
				{
					// special care is taken to apply the translation last
					Point3D p;
					for (int i = 0; i < 4; i++)
					{
						p = map.GetPointCoordinate(i);
						p.X += shiftX;
						p.Y += shiftY;
						map.SetPointCoordinate(i, p);
					}
				}
				else
				{
					// in case when we only need translation, then construct the map in a simpler way
					geometry.X += shiftX;
					geometry.Y += shiftY;
					map.PopulatePoints(geometry, 0);

					changed = true;
				}
			}
		}

		protected virtual void ApplyTransformation()
		{
			if (null == NativeView)
			{
				Log.Error("Trying to apply transformation to the non-existent native control");
				return;
			}

			// prepare the EFL effect structure
			ERect geometry = NativeView.Geometry;
			EvasMap map = new EvasMap(4);
			map.PopulatePoints(geometry, 0);

			bool changed = false;
			ApplyScale(map, geometry, ref changed);
			ApplyRotation(map, geometry, ref changed);
			ApplyTranslation(map, geometry, ref changed);

			NativeView.IsMapEnabled = changed;
			if (changed)
			{
				NativeView.EvasMap = map;
				if (!_movedCallbackEnabled)
				{
					_movedCallbackEnabled = true;
					NativeView.Moved += OnMoved;
				}
			}
			else
			{
				if (_movedCallbackEnabled)
				{
					_movedCallbackEnabled = false;
					NativeView.Moved -= OnMoved;
				}
			}
		}

		void UpdateTabIndex()
		{
			if (!Forms.Flags.Contains(Flags.DisableTabIndex))
			{
				if (Element is View && NativeView is Widget widget && widget.IsFocusAllowed)
				{
					_customFocusManager.Value.TabIndex = Element.TabIndex;
				}
			}
		}

		void UpdateIsTabStop(bool init)
		{
			if (init && Element.IsTabStop)
			{
				return;
			}
			if (!Forms.Flags.Contains(Flags.DisableTabIndex))
			{
				if (Element is View && NativeView is Widget widget && widget.IsFocusAllowed)
				{
					_customFocusManager.Value.IsTabStop = Element.IsTabStop;
				}
			}
		}

		EFocusDirection ConvertToNativeFocusDirection(string direction)
		{
			if (direction == XFocusDirection.Back)
				return EFocusDirection.Previous;
			if (direction == XFocusDirection.Forward)
				return EFocusDirection.Next;
			if (direction == XFocusDirection.Up)
				return EFocusDirection.Up;
			if (direction == XFocusDirection.Down)
				return EFocusDirection.Down;
			if (direction == XFocusDirection.Right)
				return EFocusDirection.Right;
			if (direction == XFocusDirection.Left)
				return EFocusDirection.Left;

			return EFocusDirection.Next;
		}
	}
}
