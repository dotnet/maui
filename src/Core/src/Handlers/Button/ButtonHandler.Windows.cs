using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : ViewHandler<IButton, Button>
	{
		PointerEventHandler? _pointerPressedHandler;
		PointerEventHandler? _pointerReleasedHandler;
		bool _isPressed;
		long _contentChangedToken;
		long _flowDirectionChangedToken;
		object? _defaultHorizontalContentAlignment;
		object? _defaultVerticalContentAlignment;
		object? _defaultPanelHorizontalAlignment;
		object? _defaultPanelVerticalAlignment;
		object? _defaultTextAlignment;
		DefaultMauiButtonContent? _alignmentContent;
		TextBlock? _alignmentTextBlock;
		bool _horizontalTextAlignmentSet;
		bool _verticalTextAlignmentSet;

		protected override Button CreatePlatformView() => new MauiButton();

		protected override void ConnectHandler(Button platformView)
		{
			_defaultHorizontalContentAlignment = CaptureAlignmentValue(platformView, Control.HorizontalContentAlignmentProperty);
			_defaultVerticalContentAlignment = CaptureAlignmentValue(platformView, Control.VerticalContentAlignmentProperty);
			CaptureAlignmentContent(platformView);
			_contentChangedToken = platformView.RegisterPropertyChangedCallback(ContentControl.ContentProperty, OnAlignmentContentChanged);
			_flowDirectionChangedToken = platformView.RegisterPropertyChangedCallback(FrameworkElement.FlowDirectionProperty, OnAlignmentFlowDirectionChanged);

			_pointerPressedHandler = new PointerEventHandler(OnPointerPressed);
			_pointerReleasedHandler = new PointerEventHandler(OnPointerReleased);

			platformView.Click += OnClick;
			platformView.Unloaded += OnUnloaded;
			platformView.AddHandler(UIElement.PointerPressedEvent, _pointerPressedHandler, true);
			platformView.AddHandler(UIElement.PointerReleasedEvent, _pointerReleasedHandler, true);

			base.ConnectHandler(platformView);
		}


		protected override void DisconnectHandler(Button platformView)
		{
			platformView.UnregisterPropertyChangedCallback(ContentControl.ContentProperty, _contentChangedToken);
			platformView.UnregisterPropertyChangedCallback(FrameworkElement.FlowDirectionProperty, _flowDirectionChangedToken);
			RestoreHorizontalTextAlignment(platformView);
			RestoreVerticalTextAlignment(platformView);
			_alignmentContent = null;
			_alignmentTextBlock = null;
			_defaultHorizontalContentAlignment = _defaultVerticalContentAlignment = null;
			_defaultPanelHorizontalAlignment = _defaultPanelVerticalAlignment = _defaultTextAlignment = null;

			platformView.Click -= OnClick;
			platformView.Unloaded -= OnUnloaded;
			platformView.RemoveHandler(UIElement.PointerPressedEvent, _pointerPressedHandler);
			platformView.RemoveHandler(UIElement.PointerReleasedEvent, _pointerReleasedHandler);

			_pointerPressedHandler = null;
			_pointerReleasedHandler = null;

			base.DisconnectHandler(platformView);
		}

		internal void UpdateTextAlignment(TextAlignment? horizontal, TextAlignment? vertical)
		{
			if (PlatformView is not { } platformView ||
				_alignmentContent is not { } content ||
				_alignmentTextBlock is not { } textBlock)
				return;

			if (horizontal is { } horizontalAlignment)
			{
				// WinUI mirrors the content panel in RTL; text itself requires physical line alignment.
				platformView.HorizontalContentAlignment = content.HorizontalAlignment = horizontalAlignment.ToPlatformHorizontalAlignment();
				textBlock.TextAlignment = horizontalAlignment.ToPlatform(platformView.FlowDirection == Microsoft.UI.Xaml.FlowDirection.LeftToRight);
				_horizontalTextAlignmentSet = true;
			}
			else
				RestoreHorizontalTextAlignment(platformView);

			if (vertical is { } verticalAlignment)
			{
				platformView.VerticalContentAlignment = content.VerticalAlignment = verticalAlignment.ToPlatformVerticalAlignment();
				_verticalTextAlignmentSet = true;
			}
			else
				RestoreVerticalTextAlignment(platformView);
		}

		void CaptureAlignmentContent(Button platformView)
		{
			_alignmentContent = platformView.Content as DefaultMauiButtonContent;
			_alignmentTextBlock = _alignmentContent is null ? null : platformView.GetContent<TextBlock>();
			_defaultPanelHorizontalAlignment = _defaultPanelVerticalAlignment = _defaultTextAlignment = null;
			if (_alignmentContent is { } content && _alignmentTextBlock is { } textBlock)
			{
				_defaultPanelHorizontalAlignment = CaptureAlignmentValue(content, FrameworkElement.HorizontalAlignmentProperty);
				_defaultPanelVerticalAlignment = CaptureAlignmentValue(content, FrameworkElement.VerticalAlignmentProperty);
				_defaultTextAlignment = CaptureAlignmentValue(textBlock, TextBlock.TextAlignmentProperty);
			}
		}

		void RestoreHorizontalTextAlignment(Button platformView)
		{
			if (!_horizontalTextAlignmentSet)
				return;

			RestoreAlignmentValue(platformView, Control.HorizontalContentAlignmentProperty, _defaultHorizontalContentAlignment);
			if (_alignmentContent is { } content)
				RestoreAlignmentValue(content, FrameworkElement.HorizontalAlignmentProperty, _defaultPanelHorizontalAlignment);
			if (_alignmentTextBlock is { } textBlock)
				RestoreAlignmentValue(textBlock, TextBlock.TextAlignmentProperty, _defaultTextAlignment);
			_horizontalTextAlignmentSet = false;
		}

		void RestoreVerticalTextAlignment(Button platformView)
		{
			if (!_verticalTextAlignmentSet)
				return;

			RestoreAlignmentValue(platformView, Control.VerticalContentAlignmentProperty, _defaultVerticalContentAlignment);
			if (_alignmentContent is { } content)
				RestoreAlignmentValue(content, FrameworkElement.VerticalAlignmentProperty, _defaultPanelVerticalAlignment);
			_verticalTextAlignmentSet = false;
		}

		static object CaptureAlignmentValue(FrameworkElement element, DependencyProperty property) =>
			element.GetBindingExpression(property) is { } expression ? expression.ParentBinding : element.ReadLocalValue(property);

		static void RestoreAlignmentValue(FrameworkElement element, DependencyProperty property, object? value)
		{
			if (value is Binding binding)
				element.SetBinding(property, binding);
			else if (value is null || value == DependencyProperty.UnsetValue)
				element.ClearValue(property);
			else
				element.SetValue(property, value);
		}

		void OnAlignmentContentChanged(DependencyObject sender, DependencyProperty property)
		{
			if (VirtualView is null || sender is not Button platformView)
				return;

			RestoreHorizontalTextAlignment(platformView);
			RestoreVerticalTextAlignment(platformView);
			CaptureAlignmentContent(platformView);
			UpdateValue(nameof(ITextAlignment.HorizontalTextAlignment));
		}

		void OnAlignmentFlowDirectionChanged(DependencyObject sender, DependencyProperty property)
		{
			if (VirtualView is not null)
				UpdateValue(nameof(ITextAlignment.HorizontalTextAlignment));
		}

		// This is a Windows-specific mapping
		public static void MapBackground(IButtonHandler handler, IButton button)
		{
			handler.PlatformView?.UpdateBackground(button);
		}

		public static void MapStrokeColor(IButtonHandler handler, IButtonStroke buttonStroke)
		{
			handler.PlatformView?.UpdateStrokeColor(buttonStroke);
		}

		public static void MapStrokeThickness(IButtonHandler handler, IButtonStroke buttonStroke)
		{
			handler.PlatformView?.UpdateStrokeThickness(buttonStroke);
		}

		public static void MapCornerRadius(IButtonHandler handler, IButtonStroke buttonStroke)
		{
			handler.PlatformView?.UpdateCornerRadius(buttonStroke);
			
			if (handler.VirtualView.Shadow is not null)
			{
				handler.UpdateValue(nameof(IButton.Shadow));
			}
		}

		public static void MapText(IButtonHandler handler, IText button)
		{
			handler.PlatformView?.UpdateText(button);
		}

		public static void MapTextColor(IButtonHandler handler, ITextStyle button)
		{
			handler.PlatformView?.UpdateTextColor(button);
		}

		public static void MapCharacterSpacing(IButtonHandler handler, ITextStyle button)
		{
			handler.PlatformView?.UpdateCharacterSpacing(button);
		}

		public static void MapFont(IButtonHandler handler, ITextStyle button)
		{
			var fontManager = handler.GetRequiredService<IFontManager>();

			handler.PlatformView?.UpdateFont(button, fontManager);
		}

		public static void MapPadding(IButtonHandler handler, IButton button)
		{
			handler.PlatformView?.UpdatePadding(button);
		}

		public static void MapImageSource(IButtonHandler handler, IImage image) =>
			handler
				.ImageSourceLoader
				.UpdateImageSourceAsync()
				.FireAndForget(handler);

		void OnClick(object sender, RoutedEventArgs e)
		{
			VirtualView?.Clicked();
		}

		void OnPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			_isPressed = true;
			VirtualView?.Pressed();
		}

		void OnPointerReleased(object sender, PointerRoutedEventArgs e)
		{
			_isPressed = false;
			VirtualView?.Released();
		}

		void OnUnloaded(object sender, RoutedEventArgs e)
		{
			// WinUI will not raise the PointerReleased event if the pointer is pressed and then unloaded
			if (_isPressed)
			{
				VirtualView?.Released();
			}
		}

		partial class ButtonImageSourcePartSetter
		{
			public override void SetImageSource(ImageSource? platformImage)
			{
				if (Handler?.PlatformView is not Button button)
					return;

				button.UpdateImageSource(platformImage);
				Handler.UpdateValue(nameof(ITextAlignment.HorizontalTextAlignment));
			}
		}
	}
}