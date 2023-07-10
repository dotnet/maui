#nullable disable

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public abstract partial class Layout
	{
		internal static new void RemapForControls()
		{
			ViewHandler.ViewMapper = ControlsLayoutMapper;
		}

		public static IPropertyMapper<IView, IViewHandler> ControlsLayoutMapper = new PropertyMapper<IView, IViewHandler>(ControlsVisualElementMapper)
		{
		};
	}
}
