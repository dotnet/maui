#nullable disable
using System;
using System.Threading;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Shapes
{
	public partial class Shape
	{
		static int s_remappedForControls;

		internal override void RemapForControls()
		{
			if (Interlocked.CompareExchange(ref s_remappedForControls, 1, 0) != 0)
				return;

			base.RemapForControls();

				ShapeViewHandler.Mapper.ReplaceMapping<IShapeView, IShapeViewHandler>(nameof(StrokeDashArray), MapStrokeDashArray);
		}
	}
}
