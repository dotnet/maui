using System;
using System.Collections.Generic;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ItemsViewTests : BaseTestFixture
	{
		public ItemsViewTests()
		{

			DeviceDisplay.SetCurrent(new MockDeviceDisplay());
		}

		[Fact]
		public void VerticalListMeasurement()
		{
			var itemsView = new StructuredItemsView();

			var sizeRequest = itemsView.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);

			var scaled = DeviceDisplay.MainDisplayInfo.GetScaledScreenSize();
			Assert.Equal(sizeRequest.Request.Height, scaled.Height);
			Assert.Equal(sizeRequest.Request.Width, scaled.Width);
		}

		[Fact]
		public void HorizontalListMeasurement()
		{
			var itemsView = new StructuredItemsView();

			itemsView.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);

			var sizeRequest = itemsView.Measure(double.PositiveInfinity, double.PositiveInfinity, MeasureFlags.None);

			var scaled = DeviceDisplay.MainDisplayInfo.GetScaledScreenSize();
			Assert.Equal(sizeRequest.Request.Height, scaled.Height);
			Assert.Equal(sizeRequest.Request.Width, scaled.Width);
		}

		[Fact]
		public void BindingContextPropagatesLayouts()
		{
			var bindingContext = new object();
			var itemsView = new StructuredItemsView();
			itemsView.BindingContext = bindingContext;
			var linearItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);
			itemsView.ItemsLayout = linearItemsLayout;

			// BindingContext is set when ItemsLayout is set
			Assert.Equal(itemsView.BindingContext, linearItemsLayout.BindingContext);

			// BindingContext is updated when BindingContext on ItemsView is changed
			bindingContext = new object();
			itemsView.BindingContext = bindingContext;
			Assert.Equal(itemsView.BindingContext, linearItemsLayout.BindingContext);
		}
	}
}