using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Layouts
{
	[Category(TestCategory.Memory)]
	public class LayoutMemoryTests
	{
		[Theory]
		[InlineData(typeof(StackLayout))]
		[InlineData(typeof(VerticalStackLayout))]
		[InlineData(typeof(HorizontalStackLayout))]
		[InlineData(typeof(AbsoluteLayout))]
		public async Task LayoutDoesNotLeakAfterChildRemoved(
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type layoutType)
		{
			// A long-lived child (for example a view cached by a view model) must not keep a
			// transient layout alive once it has been removed from that layout, as happens when a
			// page is navigated away from and its layout tree is torn down.
			var sharedChild = new Label();

			WeakReference reference;
			{
				var layout = (Layout)Activator.CreateInstance(layoutType);
				layout.Add(sharedChild);
				layout.Remove(sharedChild);
				reference = new(layout);
			}

			await TestHelpers.Collect();

			Assert.False(await reference.WaitForCollect(), $"{layoutType.Name} should not be alive!");

			// Ensure the shared child isn't collected during the test.
			GC.KeepAlive(sharedChild);
		}
	}
}
