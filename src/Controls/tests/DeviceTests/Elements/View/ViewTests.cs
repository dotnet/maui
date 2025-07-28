using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.View)]
	public partial class ViewTests : ControlsHandlerTestBase
	{
		[Theory]
		[ClassData(typeof(TestCases.ControlsViewTypesTestCases))]
		public async Task ViewWithMarginSetsFrameAndDesiredSizeCorrectly([DynamicallyAccessedMembers(System.Diagnostics.CodeAnalysis.DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type controlType)
		{
			EnsureHandlerCreated(TestCases.ControlsViewTypesTestCases.Setup);

			var control = Activator.CreateInstance(controlType) as View;

#if MACCATALYST || IOS
			if (control is SearchBar sb)
				return;
#endif
			control.Margin = new Thickness(5, 5, 5, 5);
			control.HeightRequest = 50;
			control.WidthRequest = 50;
			control.MinimumHeightRequest = 0;
			control.MinimumWidthRequest = 0;
			control.HorizontalOptions = LayoutOptions.Start;
			control.VerticalOptions = LayoutOptions.Start;

			IView layout = new VerticalStackLayout()
			{
				HeightRequest = 100,
				WidthRequest = 100,
				Children =
				{
					control
				}
			};

			await AttachAndRun<LayoutHandler>(layout, async (_) =>
			{
				await Task.Yield();

				var frame = control.Frame;
				var desiredSize = control.DesiredSize;

				Assert.Equal(5, frame.X, 0.5d);
				Assert.Equal(5, frame.Y, 0.5d);
				Assert.Equal(50, frame.Width, 0.5d);
				Assert.Equal(50, frame.Height, 0.5d);
				Assert.Equal(55, frame.Right, 0.5d);
				Assert.Equal(55, frame.Bottom, 0.5d);
				Assert.Equal(60, desiredSize.Width, 0.5d);
				Assert.Equal(60, desiredSize.Height, 0.5d);
			});
		}
	}
}
