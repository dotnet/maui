using System;
using System.ComponentModel;
using AppKit;
using Microsoft.Maui.Controls.Compatibility.PlatformConfiguration.macOSSpecific;

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
{
	public class FormsNSView : NSView
	{
		readonly IVisualElementRenderer _renderer;
		public FormsNSView(IVisualElementRenderer renderer)
		{
			_renderer = renderer;
		}

		public override void UpdateLayer()
		{
			base.UpdateLayer();

			UpdateBackground();
		}

		void UpdateBackground()
		{
			_renderer.ApplyNativeImageAsync(Page.BackgroundImageSourceProperty, bgImage =>
			{
				if (bgImage != null)
				{
					Layer.BackgroundColor = NSColor.FromPatternImage(bgImage).CGColor;
				}
				else
				{
					Brush background = _renderer.Element.Background;

					if (!Brush.IsNullOrEmpty(background))
						_renderer.NativeView.UpdateBackground(_renderer.Element.Background);
					else
					{
						Color bgColor = _renderer.Element.BackgroundColor;
						Layer.BackgroundColor = bgColor.IsDefault ? ColorExtensions.WindowBackgroundColor.CGColor : bgColor.ToCGColor();
					}
				}
			});
		}
	}
	public class PageRenderer : NSViewController, IVisualElementRenderer, IEffectControlProvider
	{
		bool _init;
		bool _appeared;
		bool _disposed;
		EventTracker _events;
		VisualElementPackager _packager;
		VisualElementTracker _tracker;

		Page Page => Element as Page;

		public PageRenderer()
		{
			View = new FormsNSView(this) { WantsLayer = true };
		}

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			var platformEffect = effect as PlatformEffect;
			if (platformEffect != null)
				platformEffect.SetContainer(View);
		}

		public VisualElement Element { get; private set; }

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return NativeView.GetSizeRequest(widthConstraint, heightConstraint);
		}

		public NSView NativeView => _disposed ? null : View;

		public void SetElement(VisualElement element)
		{
			VisualElement oldElement = Element;
			Element = element;
			UpdateTitle();

			RaiseElementChanged(new VisualElementChangedEventArgs(oldElement, element));

			if (Element != null && !string.IsNullOrEmpty(Element.AutomationId))
				SetAutomationId(Element.AutomationId);

			EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);
		}

		public void SetElementSize(Size size)
		{
			Element.Layout(new Rectangle(Element.X, Element.Y, size.Width, size.Height));
		}

		public NSViewController ViewController => _disposed ? null : this;

		public override void ViewDidAppear()
		{
			base.ViewDidAppear();

			if (_appeared || _disposed)
				return;

			_appeared = true;
			UpdateTabOrder();
			Page.SendAppearing();
		}

		public override void ViewDidDisappear()
		{
			base.ViewDidDisappear();

			if (!_appeared || _disposed)
				return;

			_appeared = false;
			Page.SendDisappearing();
		}

		public override void ViewWillAppear()
		{
			Init();
			base.ViewWillAppear();
		}

		public override void ViewDidLayout()
		{
			base.ViewDidLayout();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				Element.PropertyChanged -= OnElementPropertyChanged;
				Platform.SetRenderer(Element, null);
				if (_appeared)
					Page.SendDisappearing();

				_appeared = false;

				if (_events != null)
				{
					_events.Dispose();
					_events = null;
				}

				if (_packager != null)
				{
					_packager.Dispose();
					_packager = null;
				}

				if (_tracker != null)
				{
					_tracker.Dispose();
					_tracker = null;
				}

				Element = null;
				_disposed = true;
			}

			base.Dispose(disposing);
		}

		void RaiseElementChanged(VisualElementChangedEventArgs e)
		{
			OnElementChanged(e);
			ElementChanged?.Invoke(this, e);
		}

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e)
		{
		}

		void SetAutomationId(string id)
		{
			if (NativeView != null)
				NativeView.AccessibilityIdentifier = id;
		}

		void Init()
		{
			if (_init)
				return;
			UpdateBackground();

			_packager = new VisualElementPackager(this);
			_packager.Load();

			Element.PropertyChanged += OnElementPropertyChanged;
			_tracker = new VisualElementTracker(this);

			_events = new EventTracker(this);
			_events.LoadEvents(View);
			_init = true;
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName || e.PropertyName == VisualElement.BackgroundProperty.PropertyName)
				UpdateBackground();
			else if (e.PropertyName == Page.BackgroundImageSourceProperty.PropertyName)
				UpdateBackground();
			else if (e.PropertyName == Page.TitleProperty.PropertyName)
				UpdateTitle();
			else if (e.PropertyName == PlatformConfiguration.macOSSpecific.Page.TabOrderProperty.PropertyName)
				UpdateTabOrder();
		}

		void UpdateBackground()
		{
			// Moved bg logic to NSView inheritance to allow for dynamic updating through OS Theme
			View.UpdateLayer();
		}

		void UpdateTitle()
		{
			if (!string.IsNullOrWhiteSpace(((Page)Element).Title))
				Title = ((Page)Element).Title;
		}

		NSView GetNativeControl(VisualElement visualElement)
		{
			var nativeView = Platform.GetRenderer(visualElement)?.NativeView;
			var subViews = nativeView?.Subviews;
			if (subViews != null && subViews.Length > 0)
				return subViews[0];

			return nativeView;
		}

		void UpdateTabOrder()
		{
			var tabOrderElements = ((Page)Element).OnThisPlatform().GetTabOrder();
			if(tabOrderElements != null && tabOrderElements.Length > 0)
			{
				var count = tabOrderElements.Length;

				var first = GetNativeControl(tabOrderElements[0]);
				var last = GetNativeControl(tabOrderElements[count - 1]);

				if (first != null && last != null)
				{
					var previous = first;
					for (int i = 1; i < count; i++)
					{
						var control = GetNativeControl(tabOrderElements[i]);
						if (control != null)
						{
							previous.NextKeyView = control;
							previous = control;
						}
					}

					last.NextKeyView = first;
					first.Window?.MakeFirstResponder(first);
				}
			}
		}
	}
}