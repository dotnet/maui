using System;
using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UAP.UnitTests
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

		[Test, Category("ScaleX"), TestCaseSource(nameof(ScaleXCases))]
		[Description("View X scale should match renderer X scale")]
		public async Task ScaleXConsistent(View view)
		{
			var expected = view.ScaleX;
			var actual = await GetRendererProperty(view, ver => GetScaleX(ver.ContainerElement));
			Assert.That(actual, Is.EqualTo(expected).Within(0.001d));
		}

		[Test, Category("ScaleY"), TestCaseSource(nameof(ScaleYCases))]
		[Description("View Y scale should match renderer Y scale")]
		public async Task ScaleYConsistent(View view)
		{
			var expected = view.ScaleY;
			var actual = await GetRendererProperty(view, ver => GetScaleY(ver.ContainerElement));
			Assert.That(actual, Is.EqualTo(expected).Within(0.001d));
		}

		[Test, Category("Scale"), TestCaseSource(nameof(ScaleCases))]
		[Description("View scale should match renderer scale")]
		public async Task ScaleConsistent(View view)
		{
			var expected = view.Scale;
			var actualX = await GetRendererProperty(view, ver => GetScaleY(ver.ContainerElement));
			var actualY = await GetRendererProperty(view, ver => GetScaleY(ver.ContainerElement));
			Assert.That(actualX, Is.EqualTo(expected).Within(0.001d));
			Assert.That(actualY, Is.EqualTo(expected).Within(0.001d));
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
