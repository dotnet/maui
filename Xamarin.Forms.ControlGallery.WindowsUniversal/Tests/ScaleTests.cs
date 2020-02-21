using System;
using System.Collections;
using NUnit.Framework;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Xamarin.Forms.ControlGallery.WindowsUniversal.Tests
{
	[TestFixture]
	public class ScaleTests : PlatformTestFixture
	{
		static IEnumerable ScaleXCases
		{
			get
			{
				foreach (var element in BasicViews)
				{
					element.ScaleX = 0.45;
					yield return CreateTestCase(element);
				}
			}
		}

		static IEnumerable ScaleYCases
		{
			get
			{
				foreach (var element in BasicViews)
				{
					element.ScaleY = 1.23;
					yield return CreateTestCase(element);
				}
			}
		}

		static IEnumerable ScaleCases
		{
			get
			{
				foreach (var element in BasicViews)
				{
					element.Scale = 0.5;
					yield return CreateTestCase(element);
				}
			}
		}

		void AssertScaleConsistent(View view, Func<View, double> getScale,
			Func<FrameworkElement, double> getNativeScale)
		{
			var frameworkElement = GetRenderer(view).ContainerElement;

			var expected = getScale(view);
			var actual = getNativeScale(frameworkElement);

			Assert.That(actual, Is.EqualTo(expected).Within(0.001d));
		}

		[Test, Category("ScaleX"), TestCaseSource(nameof(ScaleXCases))]
		[Description("View X scale should match renderer X scale")]
		public void ScaleXConsistent(View view)
		{
			AssertScaleConsistent(view, e => e.ScaleX, GetScaleX);
		}

		[Test, Category("ScaleY"), TestCaseSource(nameof(ScaleYCases))]
		[Description("View Y scale should match renderer Y scale")]
		public void ScaleYConsistent(View view)
		{
			AssertScaleConsistent(view, e => e.ScaleY, GetScaleY);
		}

		[Test, Category("Scale"), TestCaseSource(nameof(ScaleCases))]
		[Description("View scale should match renderer scale")]
		public void ScaleConsistent(View view)
		{
			AssertScaleConsistent(view, e => e.Scale, GetScaleX);
			AssertScaleConsistent(view, e => e.Scale, GetScaleY);
		}

		double GetScaleX(FrameworkElement fe)
		{
			if (fe.RenderTransform is CompositeTransform compositeTransform)
			{
				return compositeTransform.ScaleX;
			}

			throw new Exception("Could not determine ScaleX");
		}

		double GetScaleY(FrameworkElement fe)
		{
			if (fe.RenderTransform is CompositeTransform compositeTransform)
			{
				return compositeTransform.ScaleY;
			}

			throw new Exception("Could not determine ScaleY");
		}
	}
}
