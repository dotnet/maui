using NUnit.Framework;
using System;
using System.Globalization;
using System.Threading;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class FlexOrderTests : BaseTestFixture
	{
		[Test]
		public void TestOrderingElements()
		{
			var platform = new UnitPlatform();
			var label0 = new Label { Platform = platform, IsPlatformEnabled = true };
			var label1 = new Label { Platform = platform, IsPlatformEnabled = true };
			var label2 = new Label { Platform = platform, IsPlatformEnabled = true };
			var label3 = new Label { Platform = platform, IsPlatformEnabled = true };

			FlexLayout.SetOrder(label3, 0);
			FlexLayout.SetOrder(label2, 1);
			FlexLayout.SetOrder(label1, 2);
			FlexLayout.SetOrder(label0, 3);

			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				Direction = FlexDirection.Column,
				Children = {
					label0,
					label1,
					label2,
					label3
				}
			};

			layout.Layout(new Rectangle(0, 0, 912, 912));

			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 912, 912)));
			Assert.That(label3.Bounds, Is.EqualTo(new Rectangle(0, 0, 912, 20)));
			Assert.That(label2.Bounds, Is.EqualTo(new Rectangle(0, 20, 912, 20)));
			Assert.That(label1.Bounds, Is.EqualTo(new Rectangle(0, 40, 912, 20)));
			Assert.That(label0.Bounds, Is.EqualTo(new Rectangle(0, 60, 912, 20)));
		}
	}
}
