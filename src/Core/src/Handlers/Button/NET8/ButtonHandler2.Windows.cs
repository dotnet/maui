using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Handlers
{
	public partial class ButtonHandler2 : ViewHandler<IButton, FrameworkElement>
	{
		protected override FrameworkElement CreatePlatformView() => new StackPanel()
		{
			Children =
			{
				new Microsoft.UI.Xaml.Controls.TextBlock()
			}
		};

		protected override void ConnectHandler(FrameworkElement platformView)
		{
			base.ConnectHandler(platformView);
		}

		protected override void DisconnectHandler(FrameworkElement platformView)
		{
			base.DisconnectHandler(platformView);
		}

		// This is a Windows-specific mapping
		public static void MapBackground(IButtonHandler2 handler, IButton button)
		{
		}

		public static void MapStrokeColor(IButtonHandler2 handler, IButtonStroke buttonStroke)
		{
		}

		public static void MapStrokeThickness(IButtonHandler2 handler, IButtonStroke buttonStroke)
		{
		}

		public static void MapCornerRadius(IButtonHandler2 handler, IButtonStroke buttonStroke)
		{
		}

		public static void MapText(IButtonHandler2 handler, IText button)
		{
			((handler.PlatformView as StackPanel)!.Children[0] as TextBlock)!.Text = button.Text;
		}

		public static void MapTextColor(IButtonHandler2 handler, ITextStyle button)
		{
		}

		public static void MapCharacterSpacing(IButtonHandler2 handler, ITextStyle button)
		{
		}

		public static void MapFont(IButtonHandler2 handler, ITextStyle button)
		{
		}

		public static void MapPadding(IButtonHandler2 handler, IButton button)
		{
		}

		public static void MapImageSource(IButtonHandler2 handler, IImage image)
		{

		}

		void OnSetImageSource(ImageSource? platformImageSource)
		{
		}

		void OnClick(object sender, RoutedEventArgs e)
		{
			VirtualView?.Clicked();
		}

		void OnPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			VirtualView?.Pressed();
		}

		void OnPointerReleased(object sender, PointerRoutedEventArgs e)
		{
			VirtualView?.Released();
		}
	}
}