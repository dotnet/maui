#if WINDOWS || IOS || ANDROID || TIZEN
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
#if WINDOWS
	class ViewHandlerDelegator<TElement, TNativeElement>
		where TElement : VisualElement
		where TNativeElement : Microsoft.UI.Xaml.FrameworkElement
#else
	class ViewHandlerDelegator<TElement>
		where TElement : Element, IView
#endif
	{
		internal readonly IPropertyMapper _defaultMapper;
		internal IPropertyMapper _mapper;
		internal readonly CommandMapper _commandMapper;
		IPlatformViewHandler _viewHandler;
#if IOS || MACCATALYST
		TElement? _tempElement;
		WeakReference<TElement>? _element;
		public TElement? Element => _tempElement ?? _element?.GetTargetOrDefault();
#else
		TElement? _element;
		public TElement? Element => _element;
#endif
		bool _disposed;

		public ViewHandlerDelegator(
			IPropertyMapper mapper,
			CommandMapper commandMapper,
			IPlatformViewHandler viewHandler)
		{
			_defaultMapper = mapper;
			_mapper = _defaultMapper;
			_commandMapper = commandMapper;
			_viewHandler = viewHandler;
		}

		public void UpdateProperty(string property)
		{
			if (_viewHandler.VirtualView != null)
				_mapper.UpdateProperty(_viewHandler, _viewHandler.VirtualView, property);
		}

		public void Invoke(string command, object? args)
		{
			_commandMapper.Invoke(_viewHandler, _viewHandler.VirtualView, command, args);
		}

		public void DisconnectHandler()
		{
			if (Element is not TElement element)
				return;

			if (element.Handler == _viewHandler)
				element.Handler = null;

			_element = null;

			if (!_disposed && _viewHandler is IDisposable disposable)
			{
				disposable.Dispose();
				_disposed = true;
			}
		}

		public Size GetDesiredSize(double widthConstraint, double heightConstraint, Size? minimumSize = null) =>
#if WINDOWS
			VisualElementRenderer<TElement, TNativeElement>.GetDesiredSize(_viewHandler, widthConstraint, heightConstraint, minimumSize);
#else
			VisualElementRenderer<TElement>.GetDesiredSize(_viewHandler, widthConstraint, heightConstraint, minimumSize);
#endif

		public void PlatformArrange(Rect rect) =>
			_viewHandler.PlatformArrangeHandler(rect);

		public void SetVirtualView(
			Maui.IElement view,
			Action<ElementChangedEventArgs<TElement>> onElementChanged,
			bool autoPackage)
		{
#if WINDOWS
			VisualElementRenderer<TElement, TNativeElement>.SetVirtualView(view, _viewHandler, onElementChanged, ref _element, ref _mapper, _defaultMapper, autoPackage);
#elif IOS || MACCATALYST
			// _tempElement is used here, because the Element property is accessed before SetVirtualView() returns
			VisualElementRenderer<TElement>.SetVirtualView(view, _viewHandler, onElementChanged, ref _tempElement, ref _mapper, _defaultMapper, autoPackage);
			// We use _element as a WeakReference, and clear _tempElement
			_element = _tempElement is null ? null : new(_tempElement);
			_tempElement = null;
#else
			VisualElementRenderer<TElement>.SetVirtualView(view, _viewHandler, onElementChanged, ref _element, ref _mapper, _defaultMapper, autoPackage);
#endif
		}

		public void SetVirtualView(
			Maui.IElement view,
			Action<VisualElementChangedEventArgs> onElementChanged,
			bool autoPackage)
		{
			SetVirtualView(view, ElementChanged, false);

			void ElementChanged(ElementChangedEventArgs<TElement> e)
			{
				onElementChanged(new VisualElementChangedEventArgs(
					e.OldElement as VisualElement,
					e.NewElement as VisualElement));
			}
		}
	}
}
#endif