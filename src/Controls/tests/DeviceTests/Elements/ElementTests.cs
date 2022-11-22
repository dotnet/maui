using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Element)]
	public partial class ElementTests : ControlsHandlerTestBase
	{
		[Theory("IsInAccessibleTree initializes correctly")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task IsInAccessibleTree(bool result)
		{
			var button = new Button();
			AutomationProperties.SetIsInAccessibleTree(button, result);
			var important = await GetValueAsync<bool, ButtonHandler>(button, handler => GetIsAccessibilityElement(handler));
			Assert.Equal(result, important);
		}

#if !WINDOWS
		[Theory("ExcludedWithChildren initializes correctly")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task ExcludedWithChildren(bool result)
		{
			var button = new Button();
			AutomationProperties.SetExcludedWithChildren(button, result);
			var excluded = await GetValueAsync<bool, ButtonHandler>(button, handler => GetExcludedWithChildren(handler));
			Assert.Equal(result, excluded);
		}
#endif

	}
}
