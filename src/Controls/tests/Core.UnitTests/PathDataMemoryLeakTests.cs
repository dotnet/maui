using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class PathDataMemoryLeakTests : BaseTestFixture
	{
		/// <summary>
		/// Verifies that assigning a shared/long-lived <see cref="Geometry"/> to a transient
		/// <see cref="Path"/>'s <see cref="Path.Data"/> does not root the Path via the
		/// Geometry.PropertyChanged subscription. Reproduces issue #36375.
		/// </summary>
		[Fact, Category(TestCategory.Memory)]
		public async Task PathDoesNotLeakWhenDataIsSharedGeometry()
		{
			// A long-lived geometry that outlives the Path (e.g. a shared vector-icon path).
			var sharedGeometry = new EllipseGeometry
			{
				Center = new Point(10, 10),
				RadiusX = 5,
				RadiusY = 5,
			};

			WeakReference CreatePathReference()
			{
				var path = new Path
				{
					Data = sharedGeometry,
				};

				return new WeakReference(path);
			}

			var reference = CreatePathReference();

			Assert.False(
				await reference.WaitForCollect(),
				"Path should be collected, but it was retained by the Geometry.PropertyChanged subscription on the shared Geometry.");

			GC.KeepAlive(sharedGeometry);
		}

		/// <summary>
		/// Ensures the weak subscription still delivers Geometry.PropertyChanged notifications so
		/// the Path invalidates when its Data geometry mutates (behavior preserved by the fix).
		/// </summary>
		[Fact]
		public async Task PathStillInvalidatesWhenDataGeometryChanges()
		{
			var geometry = new RectangleGeometry { Rect = new Rect(0, 0, 10, 10) };
			var path = new Path { Data = geometry };

			bool fired = false;
			path.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == nameof(Path.Data))
					fired = true;
			};

			await TestHelpers.Collect();
			GC.KeepAlive(path);

			geometry.Rect = new Rect(1, 2, 3, 4);

			Assert.True(fired, "Path did not invalidate when its Data geometry changed.");
		}
	}
}
