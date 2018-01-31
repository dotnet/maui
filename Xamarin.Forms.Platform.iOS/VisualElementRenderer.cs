using System;
using System.Collections.Generic;
using System.ComponentModel;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;
using Xamarin.Forms.Internals;

#if __MOBILE__
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using UIKit;
using NativeView = UIKit.UIView;
using NativeViewController = UIKit.UIViewController;
using NativeColor = UIKit.UIColor;

namespace Xamarin.Forms.Platform.iOS
#else
using AppKit;
using NativeView = AppKit.NSView;
using NativeViewController = AppKit.NSViewController;
using NativeColor = AppKit.NSColor;
using Xamarin.Forms.Platform.macOS.Extensions;

namespace Xamarin.Forms.Platform.MacOS
#endif
{
	[Flags]
	public enum VisualElementRendererFlags
	{
		Disposed = 1 << 0,
		AutoTrack = 1 << 1,
		AutoPackage = 1 << 2
	}

	public class VisualElementRenderer<TElement> : NativeView, IVisualElementRenderer, IEffectControlProvider where TElement : VisualElement
	{
		readonly NativeColor _defaultColor = NativeColor.Clear;

		readonly List<EventHandler<VisualElementChangedEventArgs>> _elementChangedHandlers = new List<EventHandler<VisualElementChangedEventArgs>>();

		readonly PropertyChangedEventHandler _propertyChangedHandler;
#if __MOBILE__
		string _defaultAccessibilityLabel;
		string _defaultAccessibilityHint;
		bool? _defaultIsAccessibilityElement;
#endif
		EventTracker _events;

		VisualElementRendererFlags _flags = VisualElementRendererFlags.AutoPackage | VisualElementRendererFlags.AutoTrack;

		VisualElementPackager _packager;
		VisualElementTracker _tracker;

#if __MOBILE__
		UIVisualEffectView _blur;
		BlurEffectStyle _previousBlur;
#endif

		protected VisualElementRenderer() : base(RectangleF.Empty)
		{
			_propertyChangedHandler = OnElementPropertyChanged;
#if __MOBILE__
			BackgroundColor = _defaultColor;
#else
			WantsLayer = true;
			Layer.BackgroundColor = _defaultColor.CGColor;
#endif
		}

#if __MOBILE__
		// prevent possible crashes in overrides
		public sealed override NativeColor BackgroundColor
		{
			get { return base.BackgroundColor; }
			set { base.BackgroundColor = value; }
		}
#endif

		public TElement Element { get; private set; }

		protected bool AutoPackage
		{
			get { return (_flags & VisualElementRendererFlags.AutoPackage) != 0; }
			set
			{
				if (value)
					_flags |= VisualElementRendererFlags.AutoPackage;
				else
					_flags &= ~VisualElementRendererFlags.AutoPackage;
			}
		}

		protected bool AutoTrack
		{
			get { return (_flags & VisualElementRendererFlags.AutoTrack) != 0; }
			set
			{
				if (value)
					_flags |= VisualElementRendererFlags.AutoTrack;
				else
					_flags &= ~VisualElementRendererFlags.AutoTrack;
			}
		}

		public static void RegisterEffect(Effect effect, NativeView container, NativeView control = null)
		{
			var platformEffect = effect as PlatformEffect;
			if (platformEffect == null)
				return;

			platformEffect.SetContainer(container);
			platformEffect.SetControl(control);
		}

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			var platformEffect = effect as PlatformEffect;
			if (platformEffect != null)
				OnRegisterEffect(platformEffect);
		}

		VisualElement IVisualElementRenderer.Element
		{
			get { return Element; }
		}

		event EventHandler<VisualElementChangedEventArgs> IVisualElementRenderer.ElementChanged
		{
			add { _elementChangedHandlers.Add(value); }
			remove { _elementChangedHandlers.Remove(value); }
		}

		public virtual SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return NativeView.GetSizeRequest(widthConstraint, heightConstraint);
		}

		public NativeView NativeView => this;

		void IVisualElementRenderer.SetElement(VisualElement element)
		{
			SetElement((TElement)element);
		}

		public void SetElementSize(Size size)
		{
			Xamarin.Forms.Layout.LayoutChildIntoBoundingRegion(Element, new Rectangle(Element.X, Element.Y, size.Width, size.Height));
		}

		public virtual NativeViewController ViewController => null;

		public event EventHandler<ElementChangedEventArgs<TElement>> ElementChanged;

		public void SetElement(TElement element)
		{
			var oldElement = Element;
			Element = element;

			var reference = Guid.NewGuid().ToString();
			Performance.Start(reference);

			if (oldElement != null)
				oldElement.PropertyChanged -= _propertyChangedHandler;

			if (element != null)
			{
				if (element.BackgroundColor != Color.Default || (oldElement != null && element.BackgroundColor != oldElement.BackgroundColor))
					SetBackgroundColor(element.BackgroundColor);

				UpdateClipToBounds();

				if (_tracker == null)
				{
					_tracker = new VisualElementTracker(this);
					_tracker.NativeControlUpdated += (sender, e) => UpdateNativeWidget();
				}

				if (AutoPackage && _packager == null)
				{
					_packager = new VisualElementPackager(this);
					_packager.Load();
				}

				if (AutoTrack && _events == null)
				{
					_events = new EventTracker(this);
					_events.LoadEvents(this);
				}

				element.PropertyChanged += _propertyChangedHandler;

			}

			OnElementChanged(new ElementChangedEventArgs<TElement>(oldElement, element));

			if (element != null)
				SendVisualElementInitialized(element, this);

			EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);

			if (Element != null && !string.IsNullOrEmpty(Element.AutomationId))
				SetAutomationId(Element.AutomationId);
#if __MOBILE__
			SetAccessibilityLabel();
			SetAccessibilityHint();
			SetIsAccessibilityElement();
#endif
			Performance.Stop(reference);
		}

#if __MOBILE__
		public override SizeF SizeThatFits(SizeF size)
		{
			return new SizeF(0, 0);
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			if (_blur != null && Superview != null)
			{
				_blur.Frame = Bounds;
				if (_blur.Superview == null)
					Superview.Add(_blur);
			}
		}
#else
		public override void MouseDown(NSEvent theEvent)
		{
			bool inViewCell = IsOnViewCell(Element);

			if (Element.InputTransparent || inViewCell)
				base.MouseDown(theEvent);
		}

		public override void RightMouseUp(NSEvent theEvent)
		{
			var menu = Xamarin.Forms.Element.GetMenu(Element);
			if (menu != null && NativeView != null)
				NSMenu.PopUpContextMenu(menu.ToNSMenu(), theEvent, NativeView);
		
			base.RightMouseUp(theEvent);
		}
#endif

		protected override void Dispose(bool disposing)
		{
			if ((_flags & VisualElementRendererFlags.Disposed) != 0)
				return;
			_flags |= VisualElementRendererFlags.Disposed;

			if (disposing)
			{
				if (_events != null)
				{
					_events.Dispose();
					_events = null;
				}
				if (_tracker != null)
				{
					_tracker.Dispose();
					_tracker = null;
				}
				if (_packager != null)
				{
					_packager.Dispose();
					_packager = null;
				}

				// The ListView can create renderers and unhook them from the Element before Dispose is called in CalculateHeightForCell.
				// Thus, it is possible that this work is already completed.
				if (Element != null)
				{
					Element.ClearValue(Platform.RendererProperty);
					SetElement(null);
				}
			}
			base.Dispose(disposing);
		}

		protected virtual void OnElementChanged(ElementChangedEventArgs<TElement> e)
		{
			var args = new VisualElementChangedEventArgs(e.OldElement, e.NewElement);
			for (var i = 0; i < _elementChangedHandlers.Count; i++)
				_elementChangedHandlers[i](this, args);

			ElementChanged?.Invoke(this, e);
#if __MOBILE__
			if (e.NewElement != null)
				SetBlur((BlurEffectStyle)e.NewElement.GetValue(PlatformConfiguration.iOSSpecific.VisualElement.BlurEffectProperty));
#endif
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				SetBackgroundColor(Element.BackgroundColor);
			else if (e.PropertyName == Xamarin.Forms.Layout.IsClippedToBoundsProperty.PropertyName)
				UpdateClipToBounds();
#if __MOBILE__
			else if (e.PropertyName == PlatformConfiguration.iOSSpecific.VisualElement.BlurEffectProperty.PropertyName)
				SetBlur((BlurEffectStyle)Element.GetValue(PlatformConfiguration.iOSSpecific.VisualElement.BlurEffectProperty));
			else if (e.PropertyName == AutomationProperties.HelpTextProperty.PropertyName)
				SetAccessibilityHint();
			else if (e.PropertyName == AutomationProperties.NameProperty.PropertyName)
				SetAccessibilityLabel();
			else if (e.PropertyName == AutomationProperties.IsInAccessibleTreeProperty.PropertyName)
				SetIsAccessibilityElement();
#endif
		}

		protected virtual void OnRegisterEffect(PlatformEffect effect)
		{
			effect.SetContainer(this);
		}

#if __MOBILE__
		protected virtual void SetAccessibilityHint()
		{
			if (Element == null)
				return;

			if (_defaultAccessibilityHint == null)
				_defaultAccessibilityHint = AccessibilityHint;

			AccessibilityHint = (string)Element.GetValue(AutomationProperties.HelpTextProperty) ?? _defaultAccessibilityHint;
		}

		protected virtual void SetAccessibilityLabel()
		{
			if (Element == null)
				return;

			if (_defaultAccessibilityLabel == null)
				_defaultAccessibilityLabel = AccessibilityLabel;

			AccessibilityLabel = (string)Element.GetValue(AutomationProperties.NameProperty) ?? _defaultAccessibilityLabel;
		}

		protected virtual void SetIsAccessibilityElement()
		{
			if (Element == null)
				return;

			if (!_defaultIsAccessibilityElement.HasValue)
				_defaultIsAccessibilityElement = IsAccessibilityElement;

			IsAccessibilityElement = (bool)((bool?)Element.GetValue(AutomationProperties.IsInAccessibleTreeProperty) ?? _defaultIsAccessibilityElement);
		}
#endif
		protected virtual void SetAutomationId(string id)
		{
			AccessibilityIdentifier = id;
		}

		protected virtual void SetBackgroundColor(Color color)
		{
			if (color == Color.Default)
#if __MOBILE__

				BackgroundColor = _defaultColor;
			else
				BackgroundColor = color.ToUIColor();

#else
				Layer.BackgroundColor = _defaultColor.CGColor;
			else
				Layer.BackgroundColor = color.ToCGColor();
#endif
		}

#if __MOBILE__
		protected virtual void SetBlur(BlurEffectStyle blur)
		{
			if (_previousBlur == blur)
				return;

			_previousBlur = blur;

			if (_blur != null)
			{
				_blur.RemoveFromSuperview();
				_blur = null;
			}

			if (blur == BlurEffectStyle.None)
			{
				SetNeedsDisplay();
				return;
			}

			UIBlurEffect blurEffect;
			switch (blur)
			{
				default:
				case BlurEffectStyle.ExtraLight:
					blurEffect = UIBlurEffect.FromStyle(UIBlurEffectStyle.ExtraLight);
					break;
				case BlurEffectStyle.Light:
					blurEffect = UIBlurEffect.FromStyle(UIBlurEffectStyle.Light);
					break;
				case BlurEffectStyle.Dark:
					blurEffect = UIBlurEffect.FromStyle(UIBlurEffectStyle.Dark);
					break;
			}

			_blur = new UIVisualEffectView(blurEffect);
			LayoutSubviews();
		}
#endif

		protected virtual void UpdateNativeWidget()
		{
		}

		internal virtual void SendVisualElementInitialized(VisualElement element, NativeView nativeView)
		{
			element.SendViewInitialized(nativeView);
		}

		void UpdateClipToBounds()
		{
#if __MOBILE__
			var clippableLayout = Element as Layout;
			if (clippableLayout != null)
				ClipsToBounds = clippableLayout.IsClippedToBounds;
#endif
		}

		static bool IsOnViewCell(Element element)
		{

			if (element.Parent == null)
				return false;
			else if (element.Parent is ViewCell)
				return true;
			else
				return IsOnViewCell(element.Parent);
		}
	}
}