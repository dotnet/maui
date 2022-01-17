#nullable enable
using System;
using Microsoft.Maui.Graphics;
using AViewGroup = Android.Views.ViewGroup;
using AView = Android.Views.View;
using Microsoft.Maui.Controls.Platform;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public abstract partial class VisualElementRenderer<TElement> : AViewGroup, INativeViewHandler
		where TElement : Element, IView
	{

		public static IPropertyMapper<TElement, INativeViewHandler> VisualElementRendererMapper = new PropertyMapper<TElement, INativeViewHandler>(ViewHandler.ViewMapper);
		public static CommandMapper<TElement, INativeViewHandler> VisualElementRendererCommandMapper = new CommandMapper<TElement, INativeViewHandler>(ViewHandler.ViewCommandMapper);

		TElement? _virtualView;
		IMauiContext _mauiContext;
		protected IPropertyMapper _mapper;
		protected CommandMapper? _commandMapper;
		protected readonly IPropertyMapper _defaultMapper;
		protected IMauiContext MauiContext => _mauiContext;
		public TElement? Element => _virtualView;
		public event EventHandler<ElementChangedEventArgs<TElement>>? ElementChanged;
		public event EventHandler<PropertyChangedEventArgs>? ElementPropertyChanged;

		public VisualElementRenderer(IMauiContext context) : this(context, VisualElementRendererMapper, VisualElementRendererCommandMapper)
		{
		}

		internal VisualElementRenderer(IMauiContext context, IPropertyMapper mapper, CommandMapper? commandMapper = null) : base(context.Context)
		{
			_ = mapper ?? throw new ArgumentNullException(nameof(mapper));
			_mauiContext = context;
			_defaultMapper = mapper;
			_mapper = _defaultMapper;
			_commandMapper = commandMapper;
		}

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

		protected virtual void UpdateBackgroundColor()
		{
			if (Element != null)
				_mapper.UpdateProperty(this, Element, VisualElement.BackgroundColorProperty.PropertyName);
		}

		protected virtual void UpdateBackground()
		{
			if (Element != null)
				_mapper.UpdateProperty(this, Element, VisualElement.BackgroundProperty.PropertyName);
		}


		protected virtual void SetAutomationId(string id)
		{
			if (Element != null)
				_mapper.UpdateProperty(this, Element, VisualElement.AutomationIdProperty.PropertyName);
		}

		protected virtual void SetImportantForAccessibility()
		{
			if (Element != null)
				_mapper.UpdateProperty(this, Element, AutomationProperties.IsInAccessibleTreeProperty.PropertyName);
		}

		public void UpdateLayout()
		{
			if (Element != null)
				this.InvalidateMeasure(Element);
		}

		protected virtual Size MinimumSize()
		{
			return new Size();
		}


		bool IViewHandler.HasContainer { get => true; set { } }

		object? IViewHandler.ContainerView => null;

		IView? IViewHandler.VirtualView => Element;

		object IElementHandler.NativeView => this;

		Maui.IElement? IElementHandler.VirtualView => Element;

		IMauiContext? IElementHandler.MauiContext => _mauiContext;

		AView INativeViewHandler.NativeView => this;

		AView? INativeViewHandler.ContainerView => this;

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

		void IViewHandler.NativeArrange(Rectangle rect) =>
			this.NativeArrangeHandler(rect);

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (ChildCount > 0)
			{
				var platformView = GetChildAt(0);
				if (platformView != null)
				{
					platformView.Layout(l, t, r, b);
				}
			}
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			if (ChildCount > 0)
			{
				var platformView = GetChildAt(0);
				if (platformView != null)
				{
					platformView.Measure(widthMeasureSpec, heightMeasureSpec);
					SetMeasuredDimension(platformView.MeasuredWidth, platformView.MeasuredHeight);
					return;
				}
			}

			SetMeasuredDimension(0, 0);
		}


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

			if (_virtualView.Handler != this)
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
				if (property == VisualElement.BackgroundProperty.PropertyName)
				{
					UpdateBackground();
				}
				else if (property == VisualElement.BackgroundColorProperty.PropertyName)
				{
					UpdateBackgroundColor();
				}
				else if (property == VisualElement.AutomationIdProperty.PropertyName)
				{
					SetAutomationId(Element.AutomationId);
				}
				else if (property == AutomationProperties.IsInAccessibleTreeProperty.PropertyName)
				{
					SetImportantForAccessibility();
				}
				else
				{
					_mapper.UpdateProperty(this, Element, property);
				}
			}
		}

		void IElementHandler.Invoke(string command, object? args)
		{
			_commandMapper?.Invoke(this, Element, command, args);
		}

		void IElementHandler.DisconnectHandler()
		{
			DisconnectHandlerCore();
			if (Element != null && Element.Handler == this)
				Element.Handler = null;

			_virtualView = null;
		}

		private protected virtual void DisconnectHandlerCore()
		{

		}
	}
}
