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
		TElement? _element;
		public TElement? Element => _element;
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
			if (_element == null)
				return;

			if (_element.Handler == _viewHandler)
				_element.Handler = null;

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