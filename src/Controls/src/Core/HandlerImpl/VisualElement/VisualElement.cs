using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class VisualElement
	{
		public static IPropertyMapper<IView, ViewHandler> ControlsVisualElementMapper = new PropertyMapper<View, ViewHandler>(Element.ControlsElementMapper)
		{
			[nameof(BackgroundColor)] = MapBackgroundColor,
		};

		internal static void RemapForControls()
		{
			ViewHandler.ViewMapper = ControlsVisualElementMapper;
		}

		public static void MapBackgroundColor(ViewHandler handler, View view)
		{
			handler.UpdateValue(nameof(Background));
		}
	}
}
