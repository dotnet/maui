#nullable disable
using System;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Shapes
{
	public partial class Shape
	{
		static readonly OneTimeInitializationAction s_remappedForControls = new(RemapForControlsOnce);
		internal override void RemapForControls()
		{
			base.RemapForControls();
			s_remappedForControls.InvokeOnce();
		}

		static void RemapForControlsOnce()
		{
			ShapeViewHandler.Mapper.ReplaceMappingForControls<IShapeView, IShapeViewHandler>(nameof(StrokeDashArray), MapStrokeDashArray);
		}
	}
}
