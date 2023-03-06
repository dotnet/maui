using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler : ViewHandler<IButton, FrameworkElement>
	{
		ViewHandlerProxy? _proxyHandler;

		partial void InitHandler()
		{
			if (UseNet7)
			{
				_proxyHandler = new ButtonHandlerNET7();
			}
			else
			{
				_proxyHandler = new ButtonHandlerNet8();
			}
		}

		protected override FrameworkElement CreatePlatformView() => _proxyHandler!.CreatePlatformView();

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);
			_proxyHandler?.SetVirtualView(view);
		}

		protected override void ConnectHandler(FrameworkElement platformView)
		{
			base.ConnectHandler(platformView);
			_proxyHandler?.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(FrameworkElement platformView)
		{
			base.DisconnectHandler(platformView);
			_proxyHandler?.DisconnectHandler(platformView);
		}

		static IButtonHandler GetHandler(IButtonHandler handler)
		{
			if (handler is ButtonHandler buttonHandler &&
				buttonHandler?._proxyHandler is IButtonHandler returnMe)
			{
				return returnMe;
			}

			return handler;
		}

		// We could obsolete all of these
		// These are only here because they are currently public
		public static void MapBackground(IButtonHandler handler, IButton button)
		{
			Mapper.UpdateProperty(GetHandler(handler), button, nameof(IButton.Background));
		}

		public static void MapStrokeColor(IButtonHandler handler, IButtonStroke buttonStroke)
		{
			Mapper.UpdateProperty(GetHandler(handler), (IElement)buttonStroke, nameof(IButtonStroke.StrokeColor));
		}

		public static void MapStrokeThickness(IButtonHandler handler, IButtonStroke buttonStroke)
		{
			Mapper.UpdateProperty(GetHandler(handler), (IElement)buttonStroke, nameof(IButtonStroke.StrokeThickness));
		}

		public static void MapCornerRadius(IButtonHandler handler, IButtonStroke buttonStroke)
		{
			Mapper.UpdateProperty(GetHandler(handler), (IElement)buttonStroke, nameof(IButtonStroke.CornerRadius));
		}

		public static void MapText(IButtonHandler handler, IText button)
		{
			Mapper.UpdateProperty(GetHandler(handler), (IElement)button, nameof(IText.Text));
		}

		public static void MapTextColor(IButtonHandler handler, ITextStyle button)
		{
			Mapper.UpdateProperty(GetHandler(handler), (IElement)button, nameof(ITextStyle.TextColor));
		}

		public static void MapCharacterSpacing(IButtonHandler handler, ITextStyle button)
		{
			Mapper.UpdateProperty(GetHandler(handler), (IElement)button, nameof(ITextStyle.CharacterSpacing));
		}

		public static void MapFont(IButtonHandler handler, ITextStyle button)
		{
			Mapper.UpdateProperty(GetHandler(handler), (IElement)button, nameof(ITextStyle.Font));
		}

		public static void MapPadding(IButtonHandler handler, IButton button)
		{
			Mapper.UpdateProperty(GetHandler(handler), (IElement)button, nameof(IButton.Padding));
		}

		public static void MapImageSource(IButtonHandler handler, IImage image)
		{
			Mapper.UpdateProperty(GetHandler(handler), (IElement)image, nameof(IImage.Source));
		}


		void OnSetImageSource(ImageSource? obj)
		{
		}
	}
}