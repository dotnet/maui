using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.Core.UnitTests
{
	public class FlexLayoutMarginTests : BaseTestFixture
	{
		[Test]
		public void TestMarginLeft()
		{
			var platform = new UnitPlatform();
			var view0 = new View { Platform = platform, IsPlatformEnabled = true, WidthRequest = 10, Margin = new Thickness(10, 0, 0, 0), };
			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				Children = {
					view0,
				},
				Direction = FlexDirection.Row,
			};

			layout.Layout(new Rectangle(0, 0, 100, 100));
			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 100, 100)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rectangle(10, 0, 10, 100)));
		}

		[Test]
		public void TestMarginTop()
		{
			var platform = new UnitPlatform();
			var view0 = new View { Platform = platform, IsPlatformEnabled = true, HeightRequest = 10, Margin = new Thickness(0, 10, 0, 0), };
			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				Children = {
					view0,
				},

				Direction = FlexDirection.Column,
			};

			layout.Layout(new Rectangle(0, 0, 100, 100));
			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 100, 100)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rectangle(0, 10, 100, 10)));
		}

		[Test]
		public void TestMarginRight()
		{
			var platform = new UnitPlatform();
			var view0 = new View { Platform = platform, IsPlatformEnabled = true, WidthRequest = 10, Margin = new Thickness(0, 0, 10, 0), };
			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				Children = {
					view0,
				},

				Direction = FlexDirection.Row,
				JustifyContent = FlexJustify.End,
			};

			layout.Layout(new Rectangle(0, 0, 100, 100));
			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 100, 100)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rectangle(80, 0, 10, 100)));
		}

		[Test]
		public void TestMarginBottom()
		{
			var platform = new UnitPlatform();
			var view0 = new View { Platform = platform, IsPlatformEnabled = true, HeightRequest = 10, Margin = new Thickness(0, 0, 0, 10), };
			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				Children = {
					view0,
				},

				Direction = FlexDirection.Column,
				JustifyContent = FlexJustify.End,
			};

			layout.Layout(new Rectangle(0, 0, 100, 100));
			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 100, 100)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rectangle(0, 80, 100, 10)));
		}

		[Test]
		public void TestMarginAndFlexRow()
		{
			var platform = new UnitPlatform();
			var view0 = new View { Platform = platform, IsPlatformEnabled = true, Margin = new Thickness(10, 0, 10, 0), };
			FlexLayout.SetGrow(view0, 1);
			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				Children = {
					view0,
				},

				Direction = FlexDirection.Row,
			};
			layout.Layout(new Rectangle(0, 0, 100, 100));
			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 100, 100)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rectangle(10, 0, 80, 100)));
		}

		[Test]
		public void TestMarginAndFlexColumn()
		{
			var platform = new UnitPlatform();
			var view0 = new View { Platform = platform, IsPlatformEnabled = true, Margin = new Thickness(0, 10, 0, 10), };
			FlexLayout.SetGrow(view0, 1);
			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				Children = {
					view0,
				},

				Direction = FlexDirection.Column,
			};
			layout.Layout(new Rectangle(0, 0, 100, 100));
			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 100, 100)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rectangle(0, 10, 100, 80)));
		}

		[Test]
		public void TestMarginAndStretchRow()
		{
			var platform = new UnitPlatform();
			var view0 = new View { Platform = platform, IsPlatformEnabled = true, Margin = new Thickness(0, 10, 0, 10), };
			FlexLayout.SetGrow(view0, 1);
			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				Children = {
					view0,
				},

				Direction = FlexDirection.Row,
			};

			layout.Layout(new Rectangle(0, 0, 100, 100));
			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 100, 100)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rectangle(0, 10, 100, 80)));
		}

		[Test]
		public void TestMarginAndStretchColumn()
		{

			var platform = new UnitPlatform();
			var view0 = new View { Platform = platform, IsPlatformEnabled = true,Margin = new Thickness(10, 0, 10, 0) };
			FlexLayout.SetGrow(view0, 1);
			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				Children = {
					view0,
				},
				Direction = FlexDirection.Column,
			};

			layout.Layout(new Rectangle(0, 0, 100, 100));
			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 100, 100)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rectangle(10, 0, 80, 100)));
		}

		[Test]
		public void TestMarginWithSiblingRow()
		{
			var platform = new UnitPlatform((visual, width, height) => new SizeRequest(new Size(0, 0)));
			var view0 = new View { Platform = platform, IsPlatformEnabled = true, Margin = new Thickness(0, 0, 10, 0) };
			FlexLayout.SetGrow(view0, 1);
			var view1 = new View { Platform = platform, IsPlatformEnabled = true };
			FlexLayout.SetGrow(view1, 1);
			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				Children = {
					view0,
					view1,
				},

				Direction = FlexDirection.Row,
			};

			layout.Layout(new Rectangle(0, 0, 100, 100));
			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 100, 100)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rectangle(0, 0, 45, 100)));
			Assert.That(view1.Bounds, Is.EqualTo(new Rectangle(55, 0, 45, 100)));
		}

		[Test]
		public void TestMarginWithSiblingColumn()
		{
			var platform = new UnitPlatform();
			var view0 = new View { Platform = platform, IsPlatformEnabled = true, Margin = new Thickness(0, 0, 0, 10) };
			FlexLayout.SetGrow(view0, 1);
			var view1 = new View { Platform = platform, IsPlatformEnabled = true };
			FlexLayout.SetGrow(view1, 1);

			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				Children = {
					view0,
					view1,
				},

				Direction = FlexDirection.Column,
			};

			layout.Layout(new Rectangle(0, 0, 100, 100));
			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 100, 100)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rectangle(0, 0, 100, 45)));
			Assert.That(view1.Bounds, Is.EqualTo(new Rectangle(0, 55, 100, 45)));
		}
	}
}