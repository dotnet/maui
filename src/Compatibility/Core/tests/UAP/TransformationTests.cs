using System;
using System.Collections;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using NUnit.Framework;
using Windows.UI.Xaml;
using WCompositeTransform = Microsoft.UI.Xaml.Media.CompositeTransform;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UAP.UnitTests
{
	[TestFixture]
	public class TransformationTests : PlatformTestFixture
	{
		static IEnumerable TransformationCases
		{
			get
			{
				foreach (var element in BasicViews)
				{
					element.TranslationX = 10.0;
					element.TranslationY = 30.0;
					element.Rotation = 248.0;
					element.Scale = 2.0;
					element.ScaleX = 2.0;
					yield return CreateTestCase(element);
				}
			}
		}

		[Test, Category("Transformation"), TestCaseSource(nameof(TransformationCases))]
		[Description("View transformation should match renderer transformation")]
		public async Task TransformationConsistent(View view)
		{
			var expected = (10d, 30d, 248d, 4d, 2d);
			var actual = await GetRendererProperty(view, ver =>
			{
				var t = GetTransform(ver.ContainerElement);
				return (t.TranslateX, t.TranslateY, t.Rotation, t.ScaleX, t.ScaleY);
			});
			Assert.That(actual, Is.EqualTo(expected));
		}

		WCompositeTransform GetTransform(FrameworkElement fe)
		{
			if (fe.RenderTransform is WCompositeTransform transform)
				return transform;

			throw new Exception("No rotation found");
		}
	}
}
