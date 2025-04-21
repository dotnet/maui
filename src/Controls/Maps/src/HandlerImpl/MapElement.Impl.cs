﻿using System;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Maps;

namespace Microsoft.Maui.Controls.Maps
{
	public partial class MapElement : IMapElement
	{
		Paint? IStroke.Stroke => StrokeColor?.AsPaint();

		double IStroke.StrokeThickness => StrokeWidth;

		LineCap IStroke.StrokeLineCap => throw new NotImplementedException();

		LineJoin IStroke.StrokeLineJoin => throw new NotImplementedException();

		float[] IStroke.StrokeDashPattern => throw new NotImplementedException();

		float IStroke.StrokeDashOffset => throw new NotImplementedException();

		float IStroke.StrokeMiterLimit => throw new NotImplementedException();

#pragma warning disable RS0016 // Add public types and members to the declared API
		void IMapElement.Clicked() => throw new NotImplementedException();
	}
}
