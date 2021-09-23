using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Handlers
{
	public partial class ImageButtonHandler : ViewHandler<IImageButton, MauiButton>
	{

		PointerEventHandler? _pointerPressedHandler;
		Image? _image;
		protected override MauiButton CreateNativeView()
		{
			_image = new Image()
			{
				VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
				HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
				Stretch = Stretch.Uniform,
			};

			var mauiButton = new MauiButton()
			{
				Content = _image
			};

			mauiButton.Padding = WinUIHelpers.CreateThickness(0);
			mauiButton.BorderThickness = WinUIHelpers.CreateThickness(0);
			mauiButton.Background = null;

			return mauiButton;
		}

		protected override void ConnectHandler(MauiButton nativeView)
		{
			_pointerPressedHandler = new PointerEventHandler(OnPointerPressed);

			nativeView.Click += OnClick;
			nativeView.AddHandler(UI.Xaml.UIElement.PointerPressedEvent, _pointerPressedHandler, true);

			if (_image != null)
			{
				_image.ImageFailed += OnImageFailed;
				_image.ImageOpened += OnImageOpened;
			}

			base.ConnectHandler(nativeView);

			nativeView.Loaded += OnNativeViewLoaded;
			nativeView.Unloaded += OnNativeViewUnloaded;
		}

		protected override void DisconnectHandler(MauiButton nativeView)
		{
			nativeView.Click -= OnClick;
			nativeView.RemoveHandler(UI.Xaml.UIElement.PointerPressedEvent, _pointerPressedHandler);

			_pointerPressedHandler = null;

			base.DisconnectHandler(nativeView);

			if (nativeView.XamlRoot != null)
				nativeView.XamlRoot.Changed -= OnXamlRootChanged;

			nativeView.Loaded -= OnNativeViewLoaded;
			nativeView.Unloaded -= OnNativeViewUnloaded;

			SourceLoader.Reset();
		}

		void OnImageOpened(object sender, RoutedEventArgs e)
		{

		}

		void OnImageFailed(object sender, ExceptionRoutedEventArgs e)
		{
		}

		void OnSetImageSource(ImageSource? obj)
		{
			if (NativeView.Content is Image i)
				i.Source = obj;
		}

		public override Graphics.Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return base.GetDesiredSize(widthConstraint, heightConstraint);
		}

		public override void NativeArrange(Graphics.Rectangle rect)
		{
			base.NativeArrange(rect);
		}

		void OnNativeViewLoaded(object sender = null!, RoutedEventArgs e = null!)
		{
			if (NativeView?.XamlRoot != null)
			{
				NativeView.XamlRoot.Changed += OnXamlRootChanged;
			}
		}

		void OnNativeViewUnloaded(object sender = null!, RoutedEventArgs e = null!)
		{
			if (NativeView?.XamlRoot != null)
				NativeView.XamlRoot.Changed -= OnXamlRootChanged;
		}

		void OnXamlRootChanged(XamlRoot sender, XamlRootChangedEventArgs args)
		{
			UpdateValue(nameof(IImage.Source));
		}

		void OnClick(object sender, UI.Xaml.RoutedEventArgs e)
		{
			VirtualView?.Clicked();
			VirtualView?.Released();
		}

		void OnPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			VirtualView?.Pressed();
		}
	}
}
