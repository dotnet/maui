using System;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UAP.UnitTests
{
	[TestFixture]
	public class RotationTests : PlatformTestFixture
	{
		static IEnumerable RotationXCases
		{
			get
			{
				foreach (var element in BasicViews)
				{
					element.RotationX = 33.0;
					yield return CreateTestCase(element);
				}
			}
		}

		static IEnumerable RotationYCases
		{
			get
			{
				foreach (var element in BasicViews)
				{
					element.RotationY = 86.0;
					yield return CreateTestCase(element);
				}
			}
		}

		static IEnumerable RotationCases
		{
			get
			{
				foreach (var element in BasicViews)
				{
					element.Rotation = 23.0;
					yield return CreateTestCase(element);
				}
			}
		}

		[Test, Category("RotationX"), TestCaseSource(nameof(RotationXCases))]
		[Description("View X rotation should match renderer X rotation")]
		public async Task RotationXConsistent(View view)
		{
			var expected = view.RotationX;
			var actual = await GetRendererProperty(view, ver => GetRotationX(ver.ContainerElement));
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test, Category("RotationY"), TestCaseSource(nameof(RotationYCases))]
		[Description("View Y rotation should match renderer Y rotation")]
		public async Task RotationYConsistent(View view)
		{
			var expected = view.RotationY;
			var actual = await GetRendererProperty(view, ver => GetRotationY(ver.ContainerElement));
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test, Category("Rotation"), TestCaseSource(nameof(RotationCases))]
		[Description("View rotation should match renderer rotation")]
		public async Task RotationConsistent(View view)
		{
			var expected = view.Rotation;
			var actual = await GetRendererProperty(view, ver => GetRotation(ver.ContainerElement));
			Assert.That(actual, Is.EqualTo(expected));
		}

		double GetRotationX(FrameworkElement fe)
		{
			if (fe.Projection is PlaneProjection planeProjection)
			{
				return -planeProjection.RotationX;
			}

			throw new Exception("No rotation found");
		}

		double GetRotationY(FrameworkElement fe)
		{
			if (fe.Projection is PlaneProjection planeProjection)
			{
				return -planeProjection.RotationY;
			}

			throw new Exception("No rotation found");
		}

		double GetRotation(FrameworkElement fe)
		{
			if (fe.RenderTransform is CompositeTransform compositeTransform)
			{
				return compositeTransform.Rotation;
			}

			throw new Exception("No rotation found");
		}
	}
}
