using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.RadioButton)]
	public partial class RadioButtonTests : ControlsHandlerTestBase
	{
#if WINDOWS
		[Theory(DisplayName = "IsChecked Initializes Correctly")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task IsCheckedInitializesCorrectly(bool isChecked)
		{
			bool xplatIsChecked = isChecked;
			var radioButton = new RadioButton() { IsChecked = xplatIsChecked };
			bool expectedValue = isChecked;
			var layoutFirst = new VerticalStackLayout();
			var rdFirst = new RadioButton { GroupName = "FirstGroup", IsChecked = xplatIsChecked };
			layoutFirst.Add(rdFirst);
			layoutFirst.Add(new RadioButton { GroupName = "FirstGroup" });
			layoutFirst.Add(new RadioButton { GroupName = "FirstGroup" });
			var layoutSecond = new VerticalStackLayout();
			layoutSecond.Add(new RadioButton { GroupName = "SecondGroup" });
			var rdSecond = new RadioButton { GroupName = "SecondGroup", IsChecked = xplatIsChecked };
			layoutSecond.Add(rdSecond);
			layoutSecond.Add(new RadioButton { GroupName = "SecondGroup" });
			var layout = new VerticalStackLayout
			{
				layoutFirst,
				layoutSecond
			};
			var valuesFirst = await GetValueAsync(rdFirst, (handler) => { return new { ViewValue = rdFirst.IsChecked, PlatformViewValue = GetNativeIsChecked(handler as RadioButtonHandler) }; });
			var valuesSecond = await GetValueAsync(rdSecond, (handler) => { return new { ViewValue = rdSecond.IsChecked, PlatformViewValue = GetNativeIsChecked(handler as RadioButtonHandler) }; });
			Assert.Equal(xplatIsChecked, valuesFirst.ViewValue);
			Assert.Equal(expectedValue, valuesFirst.PlatformViewValue);
			Assert.Equal(xplatIsChecked, valuesSecond.ViewValue);
			Assert.Equal(expectedValue, valuesSecond.PlatformViewValue);
		}
#endif

		[Fact("Parsed XAML can use mscorlib")]
		public void Namespace_mscorlib_Parsed()
		{
			var page = new ContentPage();
			page.LoadFromXaml(
"""
<?xml version="1.0" encoding="UTF-8"?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
	xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
	xmlns:sys="clr-namespace:System;assembly=mscorlib">
	<RadioButton>
		<RadioButton.Value>
			<sys:Int32>1</sys:Int32>
		</RadioButton.Value>
	</RadioButton>
</ContentPage>
""");
			Assert.IsType<RadioButton>(page.Content);
			Assert.Equal(1, ((RadioButton)page.Content).Value);
		}

		[Fact("Compiled XAML can use mscorlib")]
		public void Namespace_mscorlib_Compiled()
		{
			var page = new RadioButtonUsing();
			Assert.IsType<RadioButton>(page.Content);
			Assert.Equal(1, ((RadioButton)page.Content).Value);
		}
	}
}