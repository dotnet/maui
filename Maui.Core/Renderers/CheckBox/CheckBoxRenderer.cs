#if __IOS__
using NativeView = System.Maui.Platform.MauiCheckBox;
#elif __MACOS__
using NativeView = AppKit.NSButton;
#elif MONOANDROID
using NativeView = AndroidX.AppCompat.Widget.AppCompatCheckBox;
#elif NETCOREAPP
using NativeView = System.Windows.Controls.CheckBox;
#else
using NativeView = System.Object;
#endif

namespace System.Maui.Platform {
	public partial class CheckBoxRenderer : AbstractViewRenderer<ICheckBox, NativeView>
	{
		public static Color DefaultTextColor { get; private set; }

		public static PropertyMapper<ICheckBox> ButtonMapper = new PropertyMapper<ICheckBox>(ViewRenderer.ViewMapper)
		{
			[nameof(ICheckBox.IsChecked)] = MapPropertyIsChecked,
			[nameof(ICheckBox.Color)] = MapPropertyColor
		};

		public CheckBoxRenderer() : base (ButtonMapper)
		{

		}
		public CheckBoxRenderer(PropertyMapper mapper) : base (mapper ?? ButtonMapper)
		{

		}

		public static void MapPropertyIsChecked(IViewRenderer renderer, ICheckBox view)
		{
#if NETCOREAPP
			(renderer as CheckBoxRenderer)?.UpdateIsChecked();
#endif
		}

		public static void MapPropertyColor(IViewRenderer renderer, ICheckBox view)
		{
#if NETCOREAPP
			(renderer as CheckBoxRenderer)?.UpdateColor();
#endif
		}
	}
}
