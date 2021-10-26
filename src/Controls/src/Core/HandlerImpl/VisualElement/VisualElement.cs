using Microsoft.Maui.Handlers;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	public partial class VisualElement
	{
		public static IPropertyMapper<IView, IViewHandler> ControlsVisualElementMapper = new PropertyMapper<VisualElement, ViewHandler>(Element.ControlsElementMapper)
		{
			[nameof(BackgroundColor)] = MapBackgroundColor,
		};

		public static void RemapForControls()
		{
			ViewHandler.ViewMapper = ControlsVisualElementMapper;
		}

		public static void MapBackgroundColor(ViewHandler handler, VisualElement view)
		{
			handler.UpdateValue(nameof(Background));
		}
	}
}
