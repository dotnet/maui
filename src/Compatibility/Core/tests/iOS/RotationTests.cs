using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using static Microsoft.Maui.Controls.Compatibility.UITests.NumericExtensions;
using static Microsoft.Maui.Controls.Compatibility.UITests.ParsingUtils;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS.UnitTests
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
					element.RotationY = 87.0;
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
		[Description("VisualElement X rotation should match renderer X rotation")]
		public async Task RotationXConsistent(View view)
		{
			var transform = await GetRendererProperty(view, r => r.NativeView.Layer.Transform, requiresLayout: true);
			var actual = ParseCATransform3D(transform.ToString());
			var expected = CalculateRotationMatrixForDegrees((float)view.RotationX, UITests.Axis.X);
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test, Category("RotationY"), TestCaseSource(nameof(RotationYCases))]
		[Description("VisualElement Y rotation should match renderer Y rotation")]
		public async Task RotationYConsistent(View view)
		{
			var transform = await GetRendererProperty(view, r => r.NativeView.Layer.Transform, requiresLayout: true);
			var actual = ParseCATransform3D(transform.ToString());
			var expected = CalculateRotationMatrixForDegrees((float)view.RotationY, UITests.Axis.Y);
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test, Category("Rotation"), TestCaseSource(nameof(RotationCases))]
		[Description("VisualElement rotation should match renderer rotation")]
		public async Task RotationConsistent(View view)
		{
			var transform = await GetRendererProperty(view, r => r.NativeView.Layer.Transform, requiresLayout: true);
			var actual = ParseCATransform3D(transform.ToString());
			var expected = CalculateRotationMatrixForDegrees((float)view.Rotation, UITests.Axis.Z);
			Assert.That(actual, Is.EqualTo(expected));
		}
	}
}