#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls.Shapes
{
	public partial class Shape
	{
		internal new static void RemapForControls()
		{
			ShapeViewHandler.Mapper.ReplaceMapping<IShapeView, IShapeViewHandler>(nameof(StrokeDashArray), MapStrokeDashArray);
		}
	}
}
