using System.Collections.Generic;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.UnitTests.Extensions
{
	[System.ComponentModel.Category(TestCategory.Extensions)]
	public class IPickerExtensionTests
	{
		[Theory]
		[InlineData("Macaque", "Capuchin", "Saki")]
		[InlineData("Baboon", null, "Tamarin")]
		[InlineData]
		public void GetItemsAsArrayReturnsAllItems(string[] items)
		{
			var picker = CreateMockPicker(items);
			var result = picker.GetItemsAsArray();
			Assert.Equal(items, result);
		}

		[Theory]
		[InlineData("Macaque", "Capuchin", "Saki")]
		[InlineData("Baboon", null, "Tamarin")]
		[InlineData]
		public void GetItemsAsListReturnsAllItems(string[] items)
		{
			var picker = CreateMockPicker(items);
			var result = picker.GetItemsAsList();
			Assert.Equal(new List<string>(items), result);
		}

		private static IPicker CreateMockPicker(string[] items)
		{
			var picker = Substitute.For<IPicker>();
			picker.GetCount().Returns(items.Length);
			picker.GetItem(Arg.Any<int>()).Returns(x => items[(int)x[0]]);
			return picker;
		}
	}
}
