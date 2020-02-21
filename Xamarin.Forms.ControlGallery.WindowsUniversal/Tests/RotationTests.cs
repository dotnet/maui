using System;
using System.Collections;
using NUnit.Framework;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Xamarin.Forms.ControlGallery.WindowsUniversal.Tests
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

		void AssertRotationConsistent(View view, Func<View, double> getRotation,
			Func<FrameworkElement, double> getNativeRotation)
		{
			var frameworkElement = GetRenderer(view).ContainerElement;
				
			var expected = getRotation(view);
			var actual = getNativeRotation(frameworkElement);

			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test, Category("RotationX"), TestCaseSource(nameof(RotationXCases))]
		[Description("View X rotation should match renderer X rotation")]
		public void RotationXConsistent(View view)
		{
			AssertRotationConsistent(view,
			e => e.RotationX, GetRotationX);
		}

		[Test, Category("RotationY"), TestCaseSource(nameof(RotationYCases))]
		[Description("View Y rotation should match renderer Y rotation")]
		public void RotationYConsistent(View view)
		{
			AssertRotationConsistent(view,
			e => e.RotationY, GetRotationY);
		}

		[Test, Category("Rotation"), TestCaseSource(nameof(RotationCases))]
		[Description("View rotation should match renderer rotation")]
		public void RotationConsistent(View view)
		{
			AssertRotationConsistent(view,
			e => e.Rotation, GetRotation);
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
