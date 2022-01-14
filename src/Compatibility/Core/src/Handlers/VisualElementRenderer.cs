#nullable enable
#if WINDOWS || ANDROID

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
#if WINDOWS
using PlatformView = Microsoft.UI.Xaml.FrameworkElement;
#elif ANDROID
using PlatformView = Android.Views.View;
#endif

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
#if WINDOWS
	public abstract partial class VisualElementRenderer<TElement, TNativeElement> : INativeViewHandler
		where TElement : VisualElement
		where TNativeElement : UI.Xaml.FrameworkElement
#else
	public abstract partial class VisualElementRenderer<TElement> : INativeViewHandler
		where TElement : Element, IView
#endif
	{

		public static IPropertyMapper<TElement, INativeViewHandler> VisualElementRendererMapper = new PropertyMapper<TElement, INativeViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IView.AutomationId)] = MapAutomationId,
			[nameof(IView.Background)] = MapBackground,
			[nameof(VisualElement.BackgroundColor)] = MapBackgroundColor,
			[nameof(AutomationProperties.IsInAccessibleTreeProperty)] = MapIsInAccessibleTreeProperty,
		};

		public static CommandMapper<TElement, INativeViewHandler> VisualElementRendererCommandMapper = new CommandMapper<TElement, INativeViewHandler>(ViewHandler.ViewCommandMapper);

		TElement? _virtualView;
		IMauiContext _mauiContext;
		protected IPropertyMapper _mapper;
		protected CommandMapper? _commandMapper;
		protected readonly IPropertyMapper _defaultMapper;
		protected IMauiContext MauiContext => _mauiContext;
		public TElement? Element => _virtualView;

		public VisualElementRenderer(IMauiContext context) : this(context, VisualElementRendererMapper, VisualElementRendererCommandMapper)
		{
		}

		internal VisualElementRenderer(IMauiContext context, IPropertyMapper mapper, CommandMapper? commandMapper = null)
#if ANDROID
			: base(context.Context)
#else
			: base()
#endif
		{
			_ = mapper ?? throw new ArgumentNullException(nameof(mapper));
			_mauiContext = context;
			_defaultMapper = mapper;
			_mapper = _defaultMapper;
			_commandMapper = commandMapper;
		}

		public event EventHandler<ElementChangedEventArgs<TElement>>? ElementChanged;
		public event EventHandler<PropertyChangedEventArgs>? ElementPropertyChanged;

		public void SetElement(IView view)
		{
			((INativeViewHandler)this).SetVirtualView(view);
		}

		protected virtual void OnElementChanged(ElementChangedEventArgs<TElement> e)
		{
			ElementChanged?.Invoke(this, e);
		}

		protected virtual void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ElementPropertyChanged?.Invoke(sender, e);
		}

		public virtual Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var size = this.GetDesiredSizeFromHandler(widthConstraint, heightConstraint);
			var minSize = MinimumSize();

			if (size.Height < minSize.Height || size.Width < minSize.Width)
			{
				return new Size(
						size.Width < minSize.Width ? minSize.Width : size.Width,
						size.Height < minSize.Height ? minSize.Height : size.Height
					);
			}

			return size;
		}

		protected virtual Size MinimumSize()
		{
			return new Size();
		}


		protected virtual void UpdateBackgroundColor()
		{
			if (Element != null)
				ViewHandler.MapBackground(this, Element);
		}

		protected virtual void UpdateBackground()
		{
			if (Element != null)
				ViewHandler.MapBackground(this, Element);
		}


		protected virtual void SetAutomationId(string id)
		{
			if (Element != null)
				ViewHandler.MapAutomationId(this, Element);
		}

		protected virtual void SetImportantForAccessibility()
		{
			if (Element != null)
				VisualElement.MapIsInAccessibleTree(this, Element);
		}


		bool IViewHandler.HasContainer { get => true; set { } }

		object? IViewHandler.ContainerView => null;

		IView? IViewHandler.VirtualView => Element;

		object IElementHandler.NativeView => this;

		Maui.IElement? IElementHandler.VirtualView => Element;

		IMauiContext? IElementHandler.MauiContext => _mauiContext;

		PlatformView INativeViewHandler.NativeView => this;

		PlatformView? INativeViewHandler.ContainerView => this;

		void IViewHandler.NativeArrange(Rectangle rect) =>
			this.NativeArrangeHandler(rect);

		void IElementHandler.SetMauiContext(IMauiContext mauiContext)
		{
			_mauiContext = mauiContext;
		}

		void IElementHandler.SetVirtualView(Maui.IElement view)
		{
			var oldElement = _virtualView;
			_virtualView = view as TElement;
			OnElementChanged(new ElementChangedEventArgs<TElement>(oldElement, _virtualView));

			_ = view ?? throw new ArgumentNullException(nameof(view));

			if (Element == view)
				return;

			var oldVirtualView = Element;
			if (oldVirtualView?.Handler != null)
				oldVirtualView.Handler = null;

			_virtualView = (TElement)view;

			if (_virtualView.Handler != (INativeViewHandler)this)
				_virtualView.Handler = this;

			_mapper = _defaultMapper;

			if (Element is IPropertyMapperView imv)
			{
				var map = imv.GetPropertyMapperOverrides();
				if (map is not null)
				{
					map.Chained = new[] { _defaultMapper };
					_mapper = map;
				}
			}

			_mapper.UpdateProperties(this, _virtualView);
		}

		void IElementHandler.UpdateValue(string property)
		{
			if (Element != null)
			{
				_mapper.UpdateProperty(this, Element, property);
			}
		}

		void IElementHandler.Invoke(string command, object? args)
		{
			_commandMapper?.Invoke(this, Element, command, args);
		}

		void IElementHandler.DisconnectHandler()
		{
			DisconnectHandlerCore();
			if (Element != null && Element.Handler == (INativeViewHandler)this)
				Element.Handler = null;

			_virtualView = null;
		}

		private protected virtual void DisconnectHandlerCore()
		{

		}

		public static void MapIsInAccessibleTreeProperty(INativeViewHandler handler, TElement view)
		{
#if WINDOWS
			if (handler is VisualElementRenderer<TElement, TNativeElement> ver)
#else
			if (handler is VisualElementRenderer<TElement> ver)
#endif
				ver.SetImportantForAccessibility();
		}

		public static void MapAutomationId(INativeViewHandler handler, TElement view)
		{
#if WINDOWS
			if (handler is VisualElementRenderer<TElement, TNativeElement> ver)
#else
			if (handler is VisualElementRenderer<TElement> ver)
#endif
				ver.SetAutomationId(view.AutomationId);
		}

		public static void MapBackgroundColor(INativeViewHandler handler, TElement view)
		{
#if WINDOWS
			if (handler is VisualElementRenderer<TElement, TNativeElement> ver)
#else
			if (handler is VisualElementRenderer<TElement> ver)
#endif
				ver.UpdateBackgroundColor();
		}

		public static void MapBackground(INativeViewHandler handler, TElement view)
		{
#if WINDOWS
			if (handler is VisualElementRenderer<TElement, TNativeElement> ver)
#else
			if (handler is VisualElementRenderer<TElement> ver)
#endif
				ver.UpdateBackground();
		}

	}
}
#endif