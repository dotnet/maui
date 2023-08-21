// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class EllipseStub : ShapeViewStub, IShapeView
	{
		public EllipseStub()
		{
			Shape = new EllipseShapeStub();
		}
	}

	public class EllipseShapeStub : StubBase, IShape
	{
		public PathF PathForBounds(Rect rect)
		{
			var path = new PathF();

			path.AppendEllipse(0f, 0f, (float)Width, (float)Height);

			return path.AsScaledPath((float)Width / (float)rect.Width);
		}
	}
}