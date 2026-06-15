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

		internal new static void RemapForControls()
		{
			if (Interlocked.CompareExchange(ref s_remappedForControls, 1, 0) != 0)
				return;

			VisualElement.RemapForControls();

			ShapeViewHandler.Mapper.ReplaceMapping<IShapeView, IShapeViewHandler>(nameof(StrokeDashArray), MapStrokeDashArray);
		}
	}
}
