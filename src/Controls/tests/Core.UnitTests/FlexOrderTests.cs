using System;
using System.Globalization;
using System.Threading;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using FlexLayout = Microsoft.Maui.Controls.Compatibility.FlexLayout;


	public class FlexOrderTests : BaseTestFixture
	{
		[Fact]
		public void TestOrderingElements()
		{
			var label0 = new Label { IsPlatformEnabled = true };
			var label1 = new Label { IsPlatformEnabled = true };
			var label2 = new Label { IsPlatformEnabled = true };
			var label3 = new Label { IsPlatformEnabled = true };

			FlexLayout.SetOrder(label3, 0);
			FlexLayout.SetOrder(label2, 1);
			FlexLayout.SetOrder(label1, 2);
			FlexLayout.SetOrder(label0, 3);

			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,
				Direction = FlexDirection.Column,
				Children = {
					label0,
					label1,
					label2,
					label3
				}
			};

			layout.Layout(new Rect(0, 0, 912, 912));

			Assert.Equal(layout.Bounds, new Rect(0, 0, 912, 912));
			Assert.Equal(label3.Bounds, new Rect(0, 0, 912, 20));
			Assert.Equal(label2.Bounds, new Rect(0, 20, 912, 20));
			Assert.Equal(label1.Bounds, new Rect(0, 40, 912, 20));
			Assert.Equal(label0.Bounds, new Rect(0, 60, 912, 20));
		}
	}
}
