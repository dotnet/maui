using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class VisualElement
	{
		public static IPropertyMapper<IView, ViewHandler> ControlsViewMapper = new PropertyMapper<IView, ViewHandler>(ViewHandler.ViewMapper)
		{
			[nameof(BackgroundColor)] = MapBackgroundColor,
		};

		public static void RemapForControls()
		{
			ViewHandler.ViewMapper = ControlsViewMapper;
		}

		public static void MapBackgroundColor(ViewHandler handler, IView view)
		{
			handler.UpdateValue(nameof(Background));
		}
	}
}
