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
		[Obsolete("Use ShapeViewHandler.Mapper instead.")]
		public static IPropertyMapper<IShapeView, IShapeViewHandler> ControlsShapeViewMapper = new PropertyMapper<IShapeView, IShapeViewHandler>(ShapeViewHandler.Mapper);

		internal new static void RemapForControls()
		{
			ShapeViewHandler.Mapper.ReplaceMapping<IShapeView, IShapeViewHandler>(nameof(StrokeDashArray), MapStrokeDashArray);
		}
	}
}
