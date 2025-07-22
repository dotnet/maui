using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Tizen.NUI;
using Tizen.UIExtensions.Common;
using Tizen.UIExtensions.NUI;
using NView = Tizen.NUI.BaseComponents.View;
using Point = Microsoft.Maui.Graphics.Point;
using Rect = Microsoft.Maui.Graphics.Rect;
using Size = Microsoft.Maui.Graphics.Size;
using Specific = Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement;
using TPoint = Tizen.UIExtensions.Common.Point;
using XFocusDirection = Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.FocusDirection;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	/// <summary>
	/// Base class for rendering of a Xamarin element.
	/// </summary>
	[Obsolete("Use Microsoft.Maui.Controls.Handlers.Compatibility.VisualElementRenderer instead")]
	public abstract class VisualElementRenderer<TElement> : IVisualElementRenderer, IEffectControlProvider where TElement : VisualElement
	{
		event EventHandler<VisualElementChangedEventArgs> _elementChanged;

		readonly Dictionary<string, Action<bool>> _propertyHandlersWithInit = new(StringComparer.Ordinal);

		readonly Dictionary<string, Action> _propertyHandlers = new(StringComparer.Ordinal);

		readonly HashSet<string> _batchedProperties = new HashSet<string>();

		VisualElementRendererFlags _flags = VisualElementRendererFlags.None;

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

			RegisterPropertyHandler(AutomationProperties.NameProperty, SetAccessibilityName);
			RegisterPropertyHandler(AutomationProperties.HelpTextProperty, SetAccessibilityDescription);
			RegisterPropertyHandler(AutomationProperties.IsInAccessibleTreeProperty, SetIsAccessibilityElement);
			RegisterPropertyHandler(AutomationProperties.LabeledByProperty, SetLabeledBy);
		}

		~VisualElementRenderer()
		{
			Dispose(false);
		}

		event EventHandler<VisualElementChangedEventArgs> IVisualElementRenderer.ElementChanged
		{
			add
			{
				_elementChanged += value;
			}
			remove
			{
				_elementChanged -= value;
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

		public NView NativeView { get; private set; }

		public event EventHandler<ElementChangedEventArgs<TElement>> ElementChanged;

		protected bool IsDisposed => _flags.HasFlag(VisualElementRendererFlags.Disposed);

		/// <summary>
		/// Releases all resource used by the <see cref="Microsoft.Maui.Controls.Compatibility.Platform.Tizen.VisualElementRenderer{TElement}"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose()"/> when you are finished using the
		/// <see cref="Microsoft.Maui.Controls.Compatibility.Platform.Tizen.VisualElementRenderer{TElement}"/>. The <see cref="Dispose()"/> method
		/// leaves the <see cref="Microsoft.Maui.Controls.Compatibility.Platform.Tizen.VisualElementRenderer{TElement}"/> in an unusable state.
		/// After calling <see cref="Dispose()"/>, you must release all references to the
		/// <see cref="Microsoft.Maui.Controls.Compatibility.Platform.Tizen.VisualElementRenderer{TElement}"/> so the garbage collector can reclaim
		/// the memory that the <see cref="Microsoft.Maui.Controls.Compatibility.Platform.Tizen.VisualElementRenderer{TElement}"/> was occupying.</remarks>
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
				if (NativeView is IMeasurable nativeViewMeasurable)
				{
					measured = nativeViewMeasurable.Measure(availableWidth, availableHeight).ToDP();
				}
				else
				{
					measured = Measure(widthConstraint, heightConstraint);
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
				// you cannot control using SendDisappearing() on the lower class.
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
					Element.Layout(new Rect(0, 0, -1, -1));

					Element = default(TElement);
				}

				if (NativeView != null)
				{
					NativeView.Unparent();
					NativeView.Dispose();
					NativeView = null;
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

			var args = new VisualElementChangedEventArgs(e.OldElement, e.NewElement);
			_elementChanged?.Invoke(this, args);

			ElementChanged?.Invoke(this, e);

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
		protected void SetNativeView(NView control)
		{
			if (NativeView != null)
			{
				NativeView.FocusGained -= OnFocused;
				NativeView.FocusLost -= OnUnfocused;
			}

			NativeView = control;

			if (NativeView != null)
			{
				NativeView.FocusGained += OnFocused;
				NativeView.FocusLost += OnUnfocused;
			}
		}

		protected virtual void SetAccessibilityName(bool initialize)
		{
			if (initialize && (string)Element.GetValue(AutomationProperties.NameProperty) == (default(string)))
				return;
			// TODO
		}

		protected virtual void SetAccessibilityDescription(bool initialize)
		{
			if (initialize && (string)Element.GetValue(AutomationProperties.HelpTextProperty) == (default(string)))
				return;

			// TODO
		}

		protected virtual void SetIsAccessibilityElement(bool initialize)
		{
			if (initialize && (bool?)Element.GetValue(AutomationProperties.IsInAccessibleTreeProperty) == default(bool?))
				return;

			// TODO
		}

		protected virtual void SetLabeledBy(bool initialize)
		{
			if (initialize && (VisualElement)Element.GetValue(AutomationProperties.LabeledByProperty) == default(VisualElement))
				return;

			// TODO
		}

		internal virtual void SendVisualElementInitialized(VisualElement element, NView nativeView)
		{
			element.SendViewInitialized(nativeView);
		}

		void UpdateNativeGeometry()
		{
			NativeView.UpdateBounds(Element.Bounds.ToPixel());
			ApplyTranslation(true);
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
			return NativeView.MinimumSize.ToDP();
		}

		/// <summary>
		/// Calculates how much space this element should take, given how much room there is.
		/// </summary>
		/// <returns>a desired dimensions of the element</returns>
		protected virtual Size Measure(double availableWidth, double availableHeight)
		{
			return MinimumSize();
		}

		protected virtual void UpdateBackgroundColor(bool initialize)
		{
			if (initialize && Element.BackgroundColor.IsDefault())
				return;

			NativeView.UpdateBackgroundColor(Element.BackgroundColor.ToNative());
		}

		protected virtual void UpdateBackground(bool initialize)
		{
			if (initialize && Element.Background.Equals(Brush.Default))
				return;

			// TODO
		}

		protected virtual void UpdateClip(bool initialize)
		{
			// TODO
		}

		protected virtual void UpdateOpacity(bool initialize)
		{
			if (initialize && Element.Opacity == 1d)
				return;

			NativeView.Opacity = (float)Element.Opacity;
		}

		/// <summary>
		/// Handles focus events.
		/// </summary>
		protected virtual void OnFocused(object sender, EventArgs e)
		{
			Element?.SetValue(VisualElement.IsFocusedPropertyKey, true);
		}

		/// <summary>
		/// Handles unfocus events.
		/// </summary>
		protected virtual void OnUnfocused(object sender, EventArgs e)
		{
			Element?.SetValue(VisualElement.IsFocusedPropertyKey, false);
		}

		/// <summary>
		/// Adds a new child if it's derived from the VisualElement class. Otherwise this method does nothing.
		/// </summary>
		/// <param name="child">Child to be added.</param>
		protected virtual void AddChild(Element child)
		{
			if (child is VisualElement ve)
			{
				var childRenderer = Platform.GetOrCreateRenderer(ve);
				// if the native view can have children, attach the new child
				if (NativeView is IContainable<NView> containerView)
				{
					containerView.Children.Add(childRenderer.NativeView);
				}
			}
		}


		protected virtual void RemoveChild(VisualElement view)
		{
			var renderer = Platform.GetRenderer(view);
			if (NativeView is IContainable<NView> containerObject)
			{
				containerObject.Children.Remove(renderer.NativeView);
			}
			renderer?.Dispose();
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
			FocusManager.Instance.SetCurrentFocusView(NativeView);
			e.Result = true;
		}

		/// <summary>
		/// On register the effect
		/// </summary>
		/// <param name="effect">The effect to register.</param>
		void OnRegisterEffect(PlatformEffect effect)
		{
			effect.Container = Element.Parent == null ? null : Platform.GetRenderer(Element.Parent)?.NativeView;
			effect.Control = NativeView;
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

			NativeView.SetEnable(Element.IsEnabled);
		}

		/// <summary>
		/// Updates the InputTransparent property.
		/// </summary>
		protected virtual void UpdateInputTransparent(bool initialize)
		{
			if (initialize && Element.InputTransparent == default(bool))
				return;

			NativeView.Sensitive = !Element.InputTransparent;
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

			if (isFocusAllowed != null)
			{
				NativeView.Focusable = (bool)isFocusAllowed;
			}
		}

		void UpdateFocusDirection(bool initialize)
		{
			// TODO
		}

		void UpdateToolTip(bool initialize)
		{
			// TODO
		}

		void SetNextFocusViewInternal(string direction)
		{
			// TODO
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

		void ApplyRotation(bool init)
		{
			var rotationX = Element.RotationX;
			var rotationY = Element.RotationY;
			var rotationZ = Element.Rotation;

			if (init && rotationX == 0 && rotationY == 0 && rotationZ == 0)
				return;

			var zRotation = new Rotation(new Radian(DegreeToRadian((float)rotationZ)), PositionAxis.Z);
			var xRotation = new Rotation(new Radian(DegreeToRadian((float)rotationX)), PositionAxis.X);
			var yRotation = new Rotation(new Radian(DegreeToRadian((float)rotationY)), PositionAxis.Y);
			var totalRotation = zRotation * xRotation * yRotation;
			NativeView.Orientation = totalRotation;

			float DegreeToRadian(float degree)
			{
				return (float)(degree * Math.PI / 180);
			}
		}

		void ApplyScale(bool init)
		{
			var scale = Element.Scale;
			var scaleX = Element.ScaleX * scale;
			var scaleY = Element.ScaleY * scale;
			if (init && scaleX == 1.0 && scaleY == 1.0)
				return;

			NativeView.ScaleX = (float)scaleX;
			NativeView.ScaleY = (float)scaleY;
		}
		protected void ApplyTranslation(bool init)
		{
			var shiftX = Forms.ConvertToScaledPixel(Element.TranslationX);
			var shiftY = Forms.ConvertToScaledPixel(Element.TranslationY);
			if (init && shiftX == 0 && shiftY == 0)
			{
				return;
			}

			var pos = Element.Bounds.ToPixel();
			NativeView.UpdatePosition(new TPoint(pos.X + shiftX, pos.Y + shiftY));
		}
		void ApplyAnchor(bool init)
		{
			if (init && Element.AnchorX == 0.5 && Element.AnchorY == 0.5)
				return;

			NativeView.PivotPoint = new Position((float)Element.AnchorX, (float)Element.AnchorY, 0);
		}

		protected virtual void ApplyTransformation(bool init = false)
		{
			if (null == NativeView)
			{
				Log.Error("Trying to apply transformation to the non-existent native control");
				return;
			}
			ApplyAnchor(init);
			ApplyScale(init);
			ApplyRotation(init);
			ApplyTranslation(init);
		}
	}
}
