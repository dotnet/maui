using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Xamarin.Forms.Platform.Android.UnitTests
{
	[TestFixture]
	public class TransformationTests : PlatformTestFixture
	{
		static IEnumerable TransformationCases
		{
			get
			{
				foreach (var element in BasicElements)
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
		public async Task TransformationConsistent(View element)
		{
			var expected = (Context.ToPixels(10d), Context.ToPixels(30d), 248d, 4d, 2d);
			var actual = await GetRendererProperty(element, ver =>
			{
				var v = ver.View;
				return (v.TranslationX, v.TranslationY, v.Rotation, v.ScaleX, v.ScaleY);
			}, requiresParent: true);

			Assert.That(actual, Is.EqualTo(expected));
		}
	}
}