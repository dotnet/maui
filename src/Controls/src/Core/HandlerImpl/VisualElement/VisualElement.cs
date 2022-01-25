using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class VisualElement
	{
		public static IPropertyMapper<IView, IViewHandler> ControlsVisualElementMapper = new PropertyMapper<View, IViewHandler>(Element.ControlsElementMapper)
		{
			[nameof(BackgroundColor)] = MapBackgroundColor,
		};

		internal static void RemapForControls()
		{
			ViewHandler.ViewMapper = ControlsVisualElementMapper;
		}

		public static void MapBackgroundColor(IViewHandler handler, View view)
		{
			handler.UpdateValue(nameof(Background));
		}
	}
}
