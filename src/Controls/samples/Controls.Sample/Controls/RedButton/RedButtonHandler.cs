using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Maui.Controls.Sample.Controls
{
	public partial class RedButtonHandler : ButtonHandler
	{
		public static PropertyMapper<RedButton, RedButtonHandler> RedButtonMapper = new PropertyMapper<RedButton, RedButtonHandler>(ButtonHandler.ButtonMapper)
		{
			[nameof(IButton.Background)] = MapBackground,
		};

		public RedButtonHandler()
			: base(RedButtonMapper)
		{
		}

		public RedButtonHandler(PropertyMapper mapper = null)
			: base(mapper ?? RedButtonMapper)
		{
		}

#if __ANDROID__
		public static void MapBackground(RedButtonHandler handler, RedButton redButton)
		{
			handler.NativeView.SetBackgroundColor(Colors.Red.ToNative());
		}
#elif __IOS__
		public static void MapBackground(RedButtonHandler handler, RedButton redButton)
		{
			handler.NativeView.BackgroundColor = Colors.Red.ToNative();
		}
#elif WINDOWS
		public static void MapBackground(RedButtonHandler handler, RedButton redButton)
		{
			handler.NativeView.Background = Colors.Red.ToNative();
		}
#endif
	}
}