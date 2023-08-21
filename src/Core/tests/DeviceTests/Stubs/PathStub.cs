// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class GeometryStub
	{
		public void AppendToPath(PathF path)
		{

		}
	}

	public class PathStub : ShapeViewStub, IShapeView
	{
		public PathStub()
		{

		}

		public PathStub(GeometryStub data)
		{
			Shape = new PathShapeStub(data);
		}
	}

	public class PathShapeStub : StubBase, IShape
	{
		public PathShapeStub()
		{

		}

		public PathShapeStub(GeometryStub data)
		{
			Data = data;
		}

		public GeometryStub? Data { get; set; }

		public PathF PathForBounds(Rect rect)
		{
			var path = new PathF();

			Data?.AppendToPath(path);

			return path.AsScaledPath((float)Width / (float)rect.Width);
		}
	}
}