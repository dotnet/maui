using System;
using System.Collections.Generic;
using Microsoft.Maui.Devices;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[TestFixture]
	public class ItemsViewTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			DeviceDisplay.SetCurrent(new MockDeviceDisplay());
		}

		[Test]
		public void VerticalListMeasurement()
		{
			var itemsView = new StructuredItemsView();

			var sizeRequest = itemsView.Measure(double.PositiveInfinity, double.PositiveInfinity);

			var scaled = DeviceDisplay.MainDisplayInfo.GetScaledScreenSize();
			Assert.That(sizeRequest.Request.Height, Is.EqualTo(scaled.Height));
			Assert.That(sizeRequest.Request.Width, Is.EqualTo(scaled.Width));
		}

		[Test]
		public void HorizontalListMeasurement()
		{
			var itemsView = new StructuredItemsView();

			itemsView.ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);

			var sizeRequest = itemsView.Measure(double.PositiveInfinity, double.PositiveInfinity);

			var scaled = DeviceDisplay.MainDisplayInfo.GetScaledScreenSize();
			Assert.That(sizeRequest.Request.Height, Is.EqualTo(scaled.Height));
			Assert.That(sizeRequest.Request.Width, Is.EqualTo(scaled.Width));
		}

		[Test]
		public void BindingContextPropagatesLayouts()
		{
			var bindingContext = new object();
			var itemsView = new StructuredItemsView();
			itemsView.BindingContext = bindingContext;
			var linearItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal);
			itemsView.ItemsLayout = linearItemsLayout;

			// BindingContext is set when ItemsLayout is set
			Assert.AreEqual(itemsView.BindingContext, linearItemsLayout.BindingContext);

			// BindingContext is updated when BindingContext on ItemsView is changed
			bindingContext = new object();
			itemsView.BindingContext = bindingContext;
			Assert.AreEqual(itemsView.BindingContext, linearItemsLayout.BindingContext);
		}
	}
}