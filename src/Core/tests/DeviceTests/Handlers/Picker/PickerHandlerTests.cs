using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Picker)]
	public partial class PickerHandlerTests : CoreHandlerTestBase<PickerHandler, PickerStub>
	{
		[Theory(DisplayName = "Updating Font Does Not Affect HorizontalTextAlignment")]
		[InlineData(10, 20)]
		[InlineData(20, 10)]
		public async Task FontDoesNotAffectHorizontalTextAlignment(double initialSize, double newSize)
		{
			var picker = new PickerStub
			{
				Title = "This is TEXT!",
				HorizontalTextAlignment = TextAlignment.Center,
				Font = Font.SystemFontOfSize(initialSize),
			};

			await ValidateUnrelatedPropertyUnaffected(
				picker,
				GetNativeHorizontalTextAlignment,
				nameof(IPicker.Font),
				() => picker.Font = Font.SystemFontOfSize(newSize));
		}

		[Theory(DisplayName = "Updating Text Does Not Affect HorizontalTextAlignment")]
		[InlineData("Short", "Long text here")]
		[InlineData("Long text here", "Short")]
		public async Task TextDoesNotAffectHorizontalTextAlignment(string initialText, string newText)
		{
			var picker = new PickerStub
			{
				Title = initialText,
				HorizontalTextAlignment = TextAlignment.Center,
			};

			await ValidateUnrelatedPropertyUnaffected(
				picker,
				GetNativeHorizontalTextAlignment,
				nameof(IPicker.Title),
				() => picker.Title = newText);
		}

		[Theory(DisplayName = "Updating Font Does Not Affect VerticalTextAlignment")]
		[InlineData(10, 20)]
		[InlineData(20, 10)]
		public async Task FontDoesNotAffectVerticalTextAlignment(double initialSize, double newSize)
		{
			var picker = new PickerStub
			{
				Title = "This is TEXT!",
				VerticalTextAlignment = TextAlignment.Center,
				Font = Font.SystemFontOfSize(initialSize),
			};

			await ValidateUnrelatedPropertyUnaffected(
				picker,
				GetNativeVerticalTextAlignment,
				nameof(IPicker.Font),
				() => picker.Font = Font.SystemFontOfSize(newSize));
		}

		[Theory(DisplayName = "Updating Text Does Not Affect VerticalTextAlignment")]
		[InlineData("Short", "Long text here")]
		[InlineData("Long text here", "Short")]
		public async Task TextDoesNotAffectVerticalAlignment(string initialText, string newText)
		{
			var picker = new PickerStub
			{
				Title = initialText,
				VerticalTextAlignment = TextAlignment.Center,
			};

			await ValidateUnrelatedPropertyUnaffected(
				picker,
				GetNativeVerticalTextAlignment,
				nameof(IPicker.Title),
				() => picker.Title = newText);
		}

		[Category(TestCategory.Picker)]
		public class PickerTextStyleTests : TextStyleHandlerTests<PickerHandler, PickerStub>
		{
			protected override void SetText(PickerStub stub)
			{
				if (stub.Items.Count > 0)
				{
					stub.SelectedItem = stub.Items[0];
				}
				else
				{
					stub.Items = new List<string> { "test" };
					stub.SelectedItem = "test";
				}
			}
		}
	}
}