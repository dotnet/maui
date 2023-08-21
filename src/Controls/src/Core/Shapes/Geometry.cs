// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Geometry.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.Geometry']/Docs/*" />
	public abstract class Geometry : BindableObject, IGeometry
	{
		public abstract void AppendPath(PathF path);

		PathF IShape.PathForBounds(Graphics.Rect bounds)
		{
			var path = new PathF();

			AppendPath(path);

			return path;
		}
	}
}
