using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class FlexLayoutAlignItemsTests : BaseTestFixture
	{
		[Test]
		public void TestAlignItemsStretch()
		{
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,

				Direction = FlexDirection.Column,
			};
			var view0 = new View { IsPlatformEnabled = true, HeightRequest = 10, };
			layout.Children.Add(view0);

			layout.Layout(new Rectangle(0, 0, 100, 100));
			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 100, 100)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rectangle(0, 0, 100, 10)));
		}

		[Test]
		public void TestAlignItemsCenter()
		{
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,

				AlignItems = FlexAlignItems.Center,
				Direction = FlexDirection.Column,
			};
			var view0 = new View { IsPlatformEnabled = true, WidthRequest = 10, HeightRequest = 10 };
			layout.Children.Add(view0);

			layout.Layout(new Rectangle(0, 0, 100, 100));
			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 100, 100)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rectangle(45, 0, 10, 10)));
		}

		[Test]
		public void TestAlignItemsFlexStart()
		{
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,

				AlignItems = FlexAlignItems.Start,
				Direction = FlexDirection.Column,
			};
			var view0 = new View { IsPlatformEnabled = true, WidthRequest = 10, HeightRequest = 10 };
			layout.Children.Add(view0);

			layout.Layout(new Rectangle(0, 0, 100, 100));
			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 100, 100)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rectangle(0, 0, 10, 10)));
		}

		[Test]
		public void TestAlignItemsFlexEnd()
		{
			var layout = new FlexLayout
			{
				IsPlatformEnabled = true,

				AlignItems = FlexAlignItems.End,
				Direction = FlexDirection.Column,
			};
			var view0 = new View { IsPlatformEnabled = true, WidthRequest = 10, HeightRequest = 10 };
			layout.Children.Add(view0);

			layout.Layout(new Rectangle(0, 0, 100, 100));
			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 100, 100)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rectangle(90, 0, 10, 10)));
		}
	}
}