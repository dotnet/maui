using System.Threading.Tasks;
using NUnit.Framework;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS.UnitTests
{
	[TestFixture]
	public class FlowDirectionTests : PlatformTestFixture
	{
		[Test, Category("FlowDirection")]
		public void FlowDirectionConversion() 
		{
			var ltr = UIUserInterfaceLayoutDirection.LeftToRight.ToFlowDirection();
			Assert.That(ltr, Is.EqualTo(FlowDirection.LeftToRight));

			var rtl = UIUserInterfaceLayoutDirection.RightToLeft.ToFlowDirection();
			Assert.That(rtl, Is.EqualTo(FlowDirection.RightToLeft));
		}

		[TestCase(true, FlowDirection.LeftToRight, Category = "FlowDirection,Entry", ExpectedResult = UITextAlignment.Left)]
		[TestCase(true, FlowDirection.RightToLeft, Category = "FlowDirection,Entry", ExpectedResult = UITextAlignment.Right)]
		[TestCase(false, FlowDirection.LeftToRight, Category = "FlowDirection,Entry", ExpectedResult = UITextAlignment.Left)]
		[TestCase(false, FlowDirection.RightToLeft, Category = "FlowDirection,Entry", ExpectedResult = UITextAlignment.Right)]
		public async Task<UITextAlignment> EntryAlignmentMatchesFlowDirection(bool isExplicit, FlowDirection flowDirection) 
		{
			var entry = new Entry { Text = "Checking flow direction", HorizontalTextAlignment = TextAlignment.Start };
			var contentPage = new ContentPage { Title = "Flow Direction", Content = entry };

			if (isExplicit)
			{
				entry.FlowDirection = flowDirection;
			}
			else
			{
				contentPage.FlowDirection = flowDirection;
			}

			var nativeAlignment = await Device.InvokeOnMainThreadAsync(() => {
				if (!isExplicit)
				{
					GetRenderer(contentPage);
				}
				var textField = GetNativeControl(entry);
				return textField.TextAlignment;
			});

			return nativeAlignment;
		}

		[TestCase(true, FlowDirection.LeftToRight, Category = "FlowDirection,Entry", ExpectedResult = UITextAlignment.Left)]
		[TestCase(true, FlowDirection.RightToLeft, Category = "FlowDirection,Entry", ExpectedResult = UITextAlignment.Right)]
		[TestCase(false, FlowDirection.LeftToRight, Category = "FlowDirection,Entry", ExpectedResult = UITextAlignment.Left)]
		[TestCase(false, FlowDirection.RightToLeft, Category = "FlowDirection,Entry", ExpectedResult = UITextAlignment.Right)]
		public async Task<UITextAlignment> EditorAlignmentMatchesFlowDirection(bool isExplicit, FlowDirection flowDirection)
		{
			var editor = new Editor { Text = "Checking flow direction" };
			var contentPage = new ContentPage { Title = "Flow Direction", Content = editor };

			if (isExplicit)
			{
				editor.FlowDirection = flowDirection;
			}
			else
			{
				contentPage.FlowDirection = flowDirection;
			}

			var nativeAlignment = await Device.InvokeOnMainThreadAsync(() => {
				if (!isExplicit)
				{
					GetRenderer(contentPage);
				}
				var textField = GetNativeControl(editor);
				return textField.TextAlignment;
			});

			return nativeAlignment;
		}
	}
}