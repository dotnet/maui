using System.Threading.Tasks;
using NUnit.Framework;
using WTextAlignment = Microsoft.UI.Xaml.TextAlignment;
using WFlowDirection = Microsoft.UI.Xaml.FlowDirection;
using System;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UAP.UnitTests
{
	public class FlowDirectionTests : PlatformTestFixture
	{
		[TestCase(true, FlowDirection.LeftToRight, Category = "FlowDirection,Entry", ExpectedResult = WTextAlignment.Left)]
		[TestCase(true, FlowDirection.RightToLeft, Category = "FlowDirection,Entry", ExpectedResult = WTextAlignment.Right)]
		[TestCase(false, FlowDirection.LeftToRight, Category = "FlowDirection,Entry", ExpectedResult = WTextAlignment.Left)]
		[TestCase(false, FlowDirection.RightToLeft, Category = "FlowDirection,Entry", ExpectedResult = WTextAlignment.Right)]
		public async Task<WTextAlignment> EntryAlignmentMatchesFlowDirection(bool isExplicit, FlowDirection flowDirection)
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

		async Task<Tuple<WTextAlignment, WFlowDirection>> GetEditorAlignmentAndFlowDirection(bool isExplicit, FlowDirection flowDirection) 
		{
			var editor = new Editor { Text = " تسجيل الدخول" };
			var contentPage = new ContentPage { Title = "Flow Direction", Content = editor };

			if (isExplicit)
			{
				editor.FlowDirection = flowDirection;
			}
			else
			{
				contentPage.FlowDirection = flowDirection;
			}

			var (nativeAlignment, nativeFlowDirection) = await Device.InvokeOnMainThreadAsync(() => {
				if (!isExplicit)
				{
					GetRenderer(contentPage);
				}
				var textField = GetNativeControl(editor);
				return (textField.TextAlignment, textField.FlowDirection);
			});

			return new Tuple<WTextAlignment, WFlowDirection>(nativeAlignment, nativeFlowDirection);
		}

		// The Left TextAlignment seems counterintuitive, but for the Editor the FlowDirection
		// is going to automatically handle the alignment anyway.
		// The important thing is that the TextAlignment is *not* set to DetectFromContent,
		// as that will override our FlowDirection settings if they don't agree

		[Test, Category("Editor"), Category("FlowDirection")]
		public async Task EditorAlignmentMatchesFlowDirectionRtlExplicit()
		{
			var results = await GetEditorAlignmentAndFlowDirection(true, FlowDirection.RightToLeft);
			Assert.That(results.Item1, Is.EqualTo(WTextAlignment.Left));
			Assert.That(results.Item2, Is.EqualTo(WFlowDirection.RightToLeft));
		}

		[Test, Category("Editor"), Category("FlowDirection")]
		public async Task EditorAlignmentMatchesFlowDirectionLtrExplicit()
		{
			var results = await GetEditorAlignmentAndFlowDirection(true, FlowDirection.LeftToRight);
			Assert.That(results.Item1, Is.EqualTo(WTextAlignment.Left));
			Assert.That(results.Item2, Is.EqualTo(WFlowDirection.LeftToRight));
		}

		[Test, Category("Editor"), Category("FlowDirection")]
		public async Task EditorAlignmentMatchesFlowDirectionRtlImplicit()
		{
			var results = await GetEditorAlignmentAndFlowDirection(false, FlowDirection.RightToLeft);
			Assert.That(results.Item1, Is.EqualTo(WTextAlignment.Left));
			Assert.That(results.Item2, Is.EqualTo(WFlowDirection.RightToLeft));
		}

		[Test, Category("Editor"), Category("FlowDirection")]
		public async Task EditorAlignmentMatchesFlowDirectionLtrImplicit()
		{
			var results = await GetEditorAlignmentAndFlowDirection(false, FlowDirection.LeftToRight);
			Assert.That(results.Item1, Is.EqualTo(WTextAlignment.Left));
			Assert.That(results.Item2, Is.EqualTo(WFlowDirection.LeftToRight));
		}
	}
}
