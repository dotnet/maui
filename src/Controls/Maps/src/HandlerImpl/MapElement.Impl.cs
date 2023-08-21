// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
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
	}
}
