using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[Category(TestCategory.Memory)]
	public class PathMemoryTests
	{
		// A shared / long-lived Geometry assigned to Path.Data must not keep the Path alive.
		// Path subscribes to Geometry.PropertyChanged (and PathGeometry.InvalidatePathGeometryRequested)
		// and previously did so with a plain (strong) delegate, rooting every Path for the
		// lifetime of the shared geometry.
		[Theory]
		[InlineData(typeof(EllipseGeometry))]
		[InlineData(typeof(RectangleGeometry))]
		[InlineData(typeof(PathGeometry))]
		public async Task PathDataDoesNotLeakWhenGeometryOutlivesIt(Type geometryType)
		{
			var sharedGeometry = (Geometry)Activator.CreateInstance(geometryType);

			static WeakReference CreatePath(Geometry geometry)
			{
				var path = new Path { Data = geometry };
				return new WeakReference(path);
			}

			var reference = CreatePath(sharedGeometry);

			await TestHelpers.Collect();

			Assert.False(await reference.WaitForCollect(), "Path should not be alive!");

			// Keep the shared geometry rooted until after measuring so the leak is observable.
			GC.KeepAlive(sharedGeometry);
		}

		// A shared / long-lived Transform assigned to Path.RenderTransform must not keep the Path alive.
		[Fact]
		public async Task PathRenderTransformDoesNotLeakWhenTransformOutlivesIt()
		{
			var sharedTransform = new ScaleTransform { ScaleX = 2, ScaleY = 2 };

			static WeakReference CreatePath(Transform transform)
			{
				var path = new Path { RenderTransform = transform };
				return new WeakReference(path);
			}

			var reference = CreatePath(sharedTransform);

			await TestHelpers.Collect();

			Assert.False(await reference.WaitForCollect(), "Path should not be alive!");

			GC.KeepAlive(sharedTransform);
		}

		[Fact]
		public async Task SharedResourcesDoNotRetainTransientPaths()
		{
			const int pathCount = 32;
			var sharedGeometry = new PathGeometry();
			var sharedTransform = new ScaleTransform { ScaleX = 2, ScaleY = 2 };

			static WeakReference[] CreatePaths(Geometry geometry, Transform transform, int count)
			{
				var references = new WeakReference[count];

				for (var i = 0; i < count; i++)
				{
					var path = new Path
					{
						Data = geometry,
						RenderTransform = transform
					};
					references[i] = new WeakReference(path);
				}

				return references;
			}

			var references = CreatePaths(sharedGeometry, sharedTransform, pathCount);

			await TestHelpers.Collect();

			foreach (var reference in references)
				Assert.False(await reference.WaitForCollect(), "Path should not be alive!");

			GC.KeepAlive(sharedGeometry);
			GC.KeepAlive(sharedTransform);
		}

		[Fact]
		public async Task PathDataChangesStillNotifyAfterGc()
		{
			var geometry = new PathGeometry();
			var path = new Path { Data = geometry };
			bool changed = false;
			path.PropertyChanged += (_, e) => changed |= e.PropertyName == nameof(Path.Data);

			await TestHelpers.Collect();

			geometry.Figures.Add(new PathFigure());

			Assert.True(changed);
			GC.KeepAlive(path);
		}

		[Fact]
		public async Task DirectGeometryPropertyChangesStillNotifyAfterGc()
		{
			var geometry = new EllipseGeometry();
			var path = new Path { Data = geometry };
			bool changed = false;
			path.PropertyChanged += (_, e) => changed |= e.PropertyName == nameof(Path.Data);

			await TestHelpers.Collect();

			geometry.RadiusX = 10;

			Assert.True(changed);
			GC.KeepAlive(path);
		}

		[Fact]
		public async Task PathRenderTransformChangesStillNotifyAfterGc()
		{
			var transform = new RotateTransform();
			var path = new Path { RenderTransform = transform };
			bool changed = false;
			path.PropertyChanged += (_, e) => changed |= e.PropertyName == nameof(Path.RenderTransform);

			await TestHelpers.Collect();

			transform.Angle = 45;

			Assert.True(changed);
			GC.KeepAlive(path);
		}

		[Fact]
		public void PathDataReassignmentMovesChangeSubscription()
		{
			var oldGeometry = new PathGeometry();
			var newGeometry = new PathGeometry();
			var path = new Path { Data = oldGeometry };
			path.Data = newGeometry;

			bool changed = false;
			path.PropertyChanged += (_, e) => changed |= e.PropertyName == nameof(Path.Data);

			oldGeometry.Figures.Add(new PathFigure());
			Assert.False(changed);

			newGeometry.Figures.Add(new PathFigure());
			Assert.True(changed);
			GC.KeepAlive(path);
		}

		[Fact]
		public void PathDataNullAssignmentUnsubscribesOldSource()
		{
			var oldGeometry = new PathGeometry();
			var path = new Path { Data = oldGeometry };
			path.Data = null;

			bool changed = false;
			path.PropertyChanged += (_, e) => changed |= e.PropertyName == nameof(Path.Data);

			oldGeometry.Figures.Add(new PathFigure());

			Assert.False(changed);
			GC.KeepAlive(path);
		}

		[Fact]
		public void PathRenderTransformReassignmentMovesChangeSubscription()
		{
			var oldTransform = new RotateTransform();
			var newTransform = new RotateTransform();
			var path = new Path { RenderTransform = oldTransform };
			path.RenderTransform = newTransform;

			bool changed = false;
			path.PropertyChanged += (_, e) => changed |= e.PropertyName == nameof(Path.RenderTransform);

			oldTransform.Angle = 45;
			Assert.False(changed);

			newTransform.Angle = 45;
			Assert.True(changed);
			GC.KeepAlive(path);
		}

		[Fact]
		public void PathRenderTransformNullAssignmentUnsubscribesOldSource()
		{
			var oldTransform = new RotateTransform();
			var path = new Path { RenderTransform = oldTransform };
			path.RenderTransform = null;

			bool changed = false;
			path.PropertyChanged += (_, e) => changed |= e.PropertyName == nameof(Path.RenderTransform);

			oldTransform.Angle = 45;

			Assert.False(changed);
			GC.KeepAlive(path);
		}
	}
}
