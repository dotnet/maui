using Microsoft.Maui.Handlers;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	public partial class VisualElement
	{
		public new static IPropertyMapper<IView, IViewHandler> Mapper = new ControlsRemapper<IView, ViewHandler>(Element.Mapper)
		{
			[nameof(BackgroundColor)] = MapBackgroundColor,
		};

		public new static void RemapForControls()
		{
			if (Mapper is ControlsRemapper<IView, ViewHandler> cr)
			{
				// Grab all keys that the user set themselves on here and propagate them to the ViewhandlerMapper
				foreach(var kvp in cr.Mapper)
				{
					ViewHandler.ViewMapper.Add(kvp.Key, cr.Mapper[kvp.Key]);
				}
			}

			Mapper = ViewHandler.ViewMapper;
		}

		public static void MapBackgroundColor(ViewHandler handler, IView view)
		{
			handler.UpdateValue(nameof(Background));
		}
	}
}
