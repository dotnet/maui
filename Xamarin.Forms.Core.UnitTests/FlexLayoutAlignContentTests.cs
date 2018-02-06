using System;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class FlexLayoutAlignContentTests : BaseTestFixture
	{
		[Test]
		public void TestAlignContentFlexStart()
		{
			var platform = new UnitPlatform((visual, width, height) => new SizeRequest(new Size(50, 10)));
			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				WidthRequest = 130,
				HeightRequest = 100,

				AlignContent = FlexAlignContent.Start,
				AlignItems = FlexAlignItems.Start,
				Direction = FlexDirection.Row,
				Wrap = FlexWrap.Wrap,
			};

			var view0 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view4);

			layout.Layout(new Rectangle(0, 0, 130, 100));

			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 130, 100)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rectangle(0, 0, 50, 10)));
			Assert.That(view1.Bounds, Is.EqualTo(new Rectangle(50, 0, 50, 10)));
			Assert.That(view2.Bounds, Is.EqualTo(new Rectangle(0, 10, 50, 10)));
			Assert.That(view3.Bounds, Is.EqualTo(new Rectangle(50, 10, 50, 10)));
			Assert.That(view4.Bounds, Is.EqualTo(new Rectangle(0, 20, 50, 10)));
		}

		[Test]
		public void TestAlignContentFlexStartWithoutHeightOnChildren()
		{
			var platform = new UnitPlatform((visual, width, height) => new SizeRequest(new Size(50, 10)));
			var layout = new FlexLayout {
				WidthRequest = 100,
				HeightRequest = 100,
				Platform = platform,
				IsPlatformEnabled = true,

				AlignItems = FlexAlignItems.Start,
				Direction = FlexDirection.Column,
				Wrap = FlexWrap.Wrap,
			};
			var view0 = new View { IsPlatformEnabled = true, WidthRequest = 50, };
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true, WidthRequest = 50, };
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view4);

			layout.Layout(new Rectangle(0, 0, 100, 100));
			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 100, 100)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rectangle(0, 0, 50, 10)));
			Assert.That(view1.Bounds, Is.EqualTo(new Rectangle(0, 10, 50, 10)));
			Assert.That(view2.Bounds, Is.EqualTo(new Rectangle(0, 20, 50, 10)));
			Assert.That(view3.Bounds, Is.EqualTo(new Rectangle(0, 30, 50, 10)));
			Assert.That(view4.Bounds, Is.EqualTo(new Rectangle(0, 40, 50, 10)));
		}

		[Test]
		public void TestAlignContentFlexStartWithFlex()
		{
			var platform = new UnitPlatform((visual, width, height) => new SizeRequest(new Size(0, 0)));

			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				WidthRequest = 100,
				HeightRequest = 120,

				Direction = FlexDirection.Column,
				Wrap = FlexWrap.Wrap,
				AlignItems = FlexAlignItems.Start,
			};

			var view0 = new View { IsPlatformEnabled = true };
			FlexLayout.SetGrow(view0, 1);
			FlexLayout.SetBasis(view0, 0);
			view0.WidthRequest = 50;
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true };
			FlexLayout.SetGrow(view1, 1);
			FlexLayout.SetBasis(view1, 0);
			view1.WidthRequest = 50;
			view1.HeightRequest = 10;
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true };
			view2.WidthRequest = 50;
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true };
			FlexLayout.SetGrow(view3, 1);
			FlexLayout.SetShrink(view3, 1);
			FlexLayout.SetBasis(view3, 0);
			view3.WidthRequest = 50;
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true };
			view4.WidthRequest = 50;
			layout.Children.Add(view4);

			layout.Layout(new Rectangle(0, 0, 100, 120));

			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 100, 120)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rectangle(0, 0, 50, 40)));
			Assert.That(view1.Bounds, Is.EqualTo(new Rectangle(0, 40, 50, 40)));
			Assert.That(view2.Bounds, Is.EqualTo(new Rectangle(0, 80, 50, 0)));
			Assert.That(view3.Bounds, Is.EqualTo(new Rectangle(0, 80, 50, 40)));
			Assert.That(view4.Bounds, Is.EqualTo(new Rectangle(0, 120, 50, 0)));
		}

		[Test]
		public void TestAlignContentFlexEnd()
		{
			var platform = new UnitPlatform((visual, width, height) => new SizeRequest(new Size(50, 10)));
			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				WidthRequest = 100,
				HeightRequest = 100,

				Direction = FlexDirection.Column,
				AlignContent = FlexAlignContent.End,
				AlignItems = FlexAlignItems.Start,
				Wrap = FlexWrap.Wrap,
			};

			Func<View> createView = () => new View {
				IsPlatformEnabled = true,
				Platform = platform,
				WidthRequest = 50,
				HeightRequest = 10,
			};
			var view0 = createView();
			layout.Children.Add(view0);

			var view1 = createView();
			layout.Children.Add(view1);

			var view2 = createView();
			layout.Children.Add(view2);

			var view3 = createView();
			layout.Children.Add(view3);

			var view4 = createView();
			layout.Children.Add(view4);

			var measure = layout.Measure(100, 100);
			layout.Layout(new Rectangle(0, 0, 100, 100));

			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0,0,100,100)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rectangle(50, 0, 50, 10)));
			Assert.That(view1.Bounds, Is.EqualTo(new Rectangle(50, 10, 50, 10)));
			Assert.That(view2.Bounds, Is.EqualTo(new Rectangle(50, 20, 50, 10)));
			Assert.That(view3.Bounds, Is.EqualTo(new Rectangle(50, 30, 50, 10)));
			Assert.That(view4.Bounds, Is.EqualTo(new Rectangle(50, 40, 50, 10)));

		}

		[Test]
		public void TestAlignContentStretch()
		{
			var platform = new UnitPlatform((visual, width, height) => new SizeRequest(new Size(0, 0)));
			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				Direction = FlexDirection.Column,
				AlignContent = FlexAlignContent.Stretch,
				AlignItems = FlexAlignItems.Start,
				Wrap = FlexWrap.Wrap,
				WidthRequest = 150,
				HeightRequest = 100
			};
			var view0 = new View { IsPlatformEnabled = true };
			view0.WidthRequest = 50;
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true };
			view1.WidthRequest = 50;
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true };
			view2.WidthRequest = 50;
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true };
			view3.WidthRequest = 50;
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true };
			view4.WidthRequest = 50;
			layout.Children.Add(view4);

			layout.Layout(new Rectangle(0, 0, 150, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(150f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(0f, view0.X);
			Assert.AreEqual(0f, view0.Y);
			Assert.AreEqual(50f, view0.Width);
			Assert.AreEqual(0f, view0.Height);

			Assert.AreEqual(0f, view1.X);
			Assert.AreEqual(0f, view1.Y);
			Assert.AreEqual(50f, view1.Width);
			Assert.AreEqual(0f, view1.Height);

			Assert.AreEqual(0f, view2.X);
			Assert.AreEqual(0f, view2.Y);
			Assert.AreEqual(50f, view2.Width);
			Assert.AreEqual(0f, view2.Height);

			Assert.AreEqual(0f, view3.X);
			Assert.AreEqual(0f, view3.Y);
			Assert.AreEqual(50f, view3.Width);
			Assert.AreEqual(0f, view3.Height);

			Assert.AreEqual(0f, view4.X);
			Assert.AreEqual(0f, view4.Y);
			Assert.AreEqual(50f, view4.Width);
			Assert.AreEqual(0f, view4.Height);
		}

		[Test]
		public void TestAlignContentSpaceBetween()
		{
			var platform = new UnitPlatform((visual, width, height) => new SizeRequest(new Size(50, 10)));
			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				WidthRequest = 130,
				HeightRequest = 100,

				Direction = FlexDirection.Row,
				AlignContent = FlexAlignContent.SpaceBetween,
				Wrap = FlexWrap.Wrap,
			};

			var view0 = new View { IsPlatformEnabled = true, Platform = platform, WidthRequest=50, HeightRequest=10  };
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true, Platform = platform, WidthRequest = 50, HeightRequest = 10 };
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true, Platform = platform, WidthRequest = 50, HeightRequest = 10 };
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true, Platform = platform, WidthRequest = 50, HeightRequest = 10 };
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true, Platform = platform, WidthRequest = 50, HeightRequest = 10 };
			layout.Children.Add(view4);

			layout.Layout(new Rectangle(0, 0, 130, 100));

			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 130, 100)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rectangle(0, 0, 50, 10)));
			Assert.That(view1.Bounds, Is.EqualTo(new Rectangle(50, 0, 50, 10)));
			Assert.That(view2.Bounds, Is.EqualTo(new Rectangle(0, 45, 50, 10)));
			Assert.That(view3.Bounds, Is.EqualTo(new Rectangle(50, 45, 50, 10)));
			Assert.That(view4.Bounds, Is.EqualTo(new Rectangle(0, 90, 50, 10)));
		}

		[Test]
		public void TestAlignContentSpaceAround()
		{
			var platform = new UnitPlatform((visual, width, height) => new SizeRequest(new Size(50, 10)));
			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				WidthRequest = 140,
				HeightRequest = 120,

				Direction = FlexDirection.Row,
				AlignContent = FlexAlignContent.SpaceAround,
				Wrap = FlexWrap.Wrap,
			};
			var view0 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true, WidthRequest = 50, HeightRequest = 10, };
			layout.Children.Add(view4);

			layout.Layout(new Rectangle(0, 0, 140, 120));

			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 140, 120)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rectangle(0, 15, 50, 10)));
			Assert.That(view1.Bounds, Is.EqualTo(new Rectangle(50, 15, 50, 10)));
			Assert.That(view2.Bounds, Is.EqualTo(new Rectangle(0, 55, 50, 10)));
			Assert.That(view3.Bounds, Is.EqualTo(new Rectangle(50, 55, 50, 10)));
			Assert.That(view4.Bounds, Is.EqualTo(new Rectangle(0, 95, 50, 10)));
		}

		[Test]
		public void TestAlignContentStretchRow()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				Direction = FlexDirection.Row,
				AlignContent = FlexAlignContent.Stretch,
				AlignItems = FlexAlignItems.Start,
				Wrap = FlexWrap.Wrap,
				WidthRequest = 150,
				HeightRequest = 100
			};
			var view0 = new View { IsPlatformEnabled = true };
			view0.WidthRequest = 50;
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true };
			view1.WidthRequest = 50;
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true };
			view2.WidthRequest = 50;
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true };
			view3.WidthRequest = 50;
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true };
			view4.WidthRequest = 50;
			layout.Children.Add(view4);

			layout.Layout(new Rectangle(0, 0, 150, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(150f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(0f, view0.X);
			Assert.AreEqual(0f, view0.Y);
			Assert.AreEqual(50f, view0.Width);
			Assert.AreEqual(20f, view0.Height);

			Assert.AreEqual(50f, view1.X);
			Assert.AreEqual(0f, view1.Y);
			Assert.AreEqual(50f, view1.Width);
			Assert.AreEqual(20f, view1.Height);

			Assert.AreEqual(100f, view2.X);
			Assert.AreEqual(0f, view2.Y);
			Assert.AreEqual(50f, view2.Width);
			Assert.AreEqual(20f, view2.Height);

			Assert.AreEqual(0f, view3.X);
			Assert.AreEqual(50f, view3.Y);
			Assert.AreEqual(50f, view3.Width);
			Assert.AreEqual(20f, view3.Height);

			Assert.AreEqual(50f, view4.X);
			Assert.AreEqual(50f, view4.Y);
			Assert.AreEqual(50f, view4.Width);
			Assert.AreEqual(20f, view4.Height);
		}

		[Test]
		public void TestAlignContentStretchRowWithChildren()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				WidthRequest = 150,
				HeightRequest = 100,

				Direction = FlexDirection.Row,
				AlignContent = FlexAlignContent.Stretch,
				Wrap = FlexWrap.Wrap,
			};
			var view0 = new FlexLayout { IsPlatformEnabled = true, WidthRequest = 50, };
			layout.Children.Add(view0);

			//var view0_child0 = new View { IsPlatformEnabled = true };
			//FlexLayout.SetGrow(view0_child0, 1);
			//FlexLayout.SetShrink(view0_child0, 1);
			//FlexLayout.SetBasis(view0_child0, 0);
			//view0.Children.Add(view0_child0);

			var view1 = new View { IsPlatformEnabled = true, WidthRequest = 50, };
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true, WidthRequest = 50, };
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true, WidthRequest = 50, };
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true, WidthRequest = 50, };
			layout.Children.Add(view4);

			layout.Layout(new Rectangle(0, 0, 150, 100));

			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 150, 100)));

			Assert.AreEqual(0f, view0.X);
			Assert.AreEqual(0f, view0.Y);
			Assert.AreEqual(50f, view0.Width);
			Assert.AreEqual(50f, view0.Height);

			//Assert.AreEqual(0f, view0_child0.X);
			//Assert.AreEqual(0f, view0_child0.Y);
			//Assert.AreEqual(50f, view0_child0.Width);
			//Assert.AreEqual(50f, view0_child0.Height);

			Assert.AreEqual(50f, view1.X);
			Assert.AreEqual(0f, view1.Y);
			Assert.AreEqual(50f, view1.Width);
			Assert.AreEqual(50f, view1.Height);

			Assert.AreEqual(100f, view2.X);
			Assert.AreEqual(0f, view2.Y);
			Assert.AreEqual(50f, view2.Width);
			Assert.AreEqual(50f, view2.Height);

			Assert.AreEqual(0f, view3.X);
			Assert.AreEqual(50f, view3.Y);
			Assert.AreEqual(50f, view3.Width);
			Assert.AreEqual(50f, view3.Height);

			Assert.AreEqual(50f, view4.X);
			Assert.AreEqual(50f, view4.Y);
			Assert.AreEqual(50f, view4.Width);
			Assert.AreEqual(50f, view4.Height);
		}

		[Test]
		public void TestAlignContentStretchRowWithFlex()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,

				AlignContent = FlexAlignContent.Stretch,
				Direction = FlexDirection.Row,
				Wrap = FlexWrap.Wrap,
			};
			var view0 = new View { IsPlatformEnabled = true, WidthRequest = 50 };
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true, WidthRequest = 50 };
			FlexLayout.SetGrow(view1, 1);
			FlexLayout.SetShrink(view1, 1);
			FlexLayout.SetBasis(view1, 0);
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true, WidthRequest = 50 };
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true, WidthRequest = 50 };
			FlexLayout.SetGrow(view3, 1);
			FlexLayout.SetShrink(view3, 1);
			FlexLayout.SetBasis(view3, 0);
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true, WidthRequest = 50 };
			layout.Children.Add(view4);

			layout.Layout(new Rectangle(0, 0, 150, 100));

			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 150, 100)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rectangle(0, 0, 50, 100)));
			Assert.That(view1.Bounds, Is.EqualTo(new Rectangle(50, 0, 0, 100)));
			Assert.That(view2.Bounds, Is.EqualTo(new Rectangle(50, 0, 50, 100)));
			Assert.That(view3.Bounds, Is.EqualTo(new Rectangle(100, 0, 0, 100)));
			Assert.That(view4.Bounds, Is.EqualTo(new Rectangle(100, 0, 50, 100)));
		}

		[Test]
		public void TestAlignContentStretchRowWithFlexNoShrink()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				Direction = FlexDirection.Row,
				AlignContent = FlexAlignContent.Stretch,
				Wrap = FlexWrap.Wrap,
				WidthRequest = 150,
				HeightRequest = 100
			};
			var view0 = new View { IsPlatformEnabled = true, WidthRequest = 50 };
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true, WidthRequest = 50 };
			FlexLayout.SetGrow(view1, 1);
			FlexLayout.SetShrink(view1, 1);
			FlexLayout.SetBasis(view1, 0);
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true, WidthRequest = 50 };
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true, WidthRequest = 50 };
			FlexLayout.SetGrow(view3, 1);
			FlexLayout.SetBasis(view3, 0);
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true, WidthRequest = 50 };
			layout.Children.Add(view4);

			layout.Layout(new Rectangle(0, 0, 150, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(150f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(0f, view0.X);
			Assert.AreEqual(0f, view0.Y);
			Assert.AreEqual(50f, view0.Width);
			Assert.AreEqual(100f, view0.Height);

			Assert.AreEqual(50f, view1.X);
			Assert.AreEqual(0f, view1.Y);
			Assert.AreEqual(0f, view1.Width);
			Assert.AreEqual(100f, view1.Height);

			Assert.AreEqual(50f, view2.X);
			Assert.AreEqual(0f, view2.Y);
			Assert.AreEqual(50f, view2.Width);
			Assert.AreEqual(100f, view2.Height);

			Assert.AreEqual(100f, view3.X);
			Assert.AreEqual(0f, view3.Y);
			Assert.AreEqual(0f, view3.Width);
			Assert.AreEqual(100f, view3.Height);

			Assert.AreEqual(100f, view4.X);
			Assert.AreEqual(0f, view4.Y);
			Assert.AreEqual(50f, view4.Width);
			Assert.AreEqual(100f, view4.Height);
		}

		[Test]
		[Ignore("")]
		public void TestAlignContentStretchRowWithMargin()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				WidthRequest = 150,
				HeightRequest = 100,

				Direction = FlexDirection.Row,
				AlignContent = FlexAlignContent.Stretch,
				Wrap = FlexWrap.Wrap,
			};
			var view0 = new View {
				IsPlatformEnabled = true,
				WidthRequest = 50,
				HeightRequest = 20,
			};

			layout.Children.Add(view0);

			var view1 = new View {
				IsPlatformEnabled = true,
				Margin = 10,
				WidthRequest = 50,
				HeightRequest = 20,
			};
			layout.Children.Add(view1);

			var view2 = new View {
				IsPlatformEnabled = true,
				WidthRequest = 50,
				HeightRequest = 20,
			};
			layout.Children.Add(view2);

			var view3 = new View {
				IsPlatformEnabled = true,
				Margin = new Thickness(10),
				WidthRequest = 50,
				HeightRequest = 20,
			};
			layout.Children.Add(view3);

			var view4 = new View {
				IsPlatformEnabled = true,
				WidthRequest = 50,
				HeightRequest = 20,
			};
			layout.Children.Add(view4);

			layout.Layout(new Rectangle(0, 0, 150, 100));

			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 150, 100)));
			Assert.That(view0.Bounds, Is.EqualTo(new Rectangle(0, 0, 50, 20)));
			Assert.That(view1.Bounds, Is.EqualTo(new Rectangle(60, 10, 50, 20)));
			Assert.That(view2.Bounds, Is.EqualTo(new Rectangle(0, 40, 50, 20)));
			Assert.That(view3.Bounds, Is.EqualTo(new Rectangle(60, 50, 50, 20)));
			Assert.That(view4.Bounds, Is.EqualTo(new Rectangle(0, 80, 50, 20)));
		}

		[Test]
		public void TestAlignContentStretchRowWithSingleRow()
		{
			var platform = new UnitPlatform();
			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				Direction = FlexDirection.Row,
				AlignContent = FlexAlignContent.Stretch,
				Wrap = FlexWrap.Wrap,
				WidthRequest = 150,
				HeightRequest = 100
			};
			var view0 = new View { IsPlatformEnabled = true };
			view0.WidthRequest = 50;
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true };
			view1.WidthRequest = 50;
			layout.Children.Add(view1);

			layout.Layout(new Rectangle(0, 0, 150, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(150f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(0f, view0.X);
			Assert.AreEqual(0f, view0.Y);
			Assert.AreEqual(50f, view0.Width);
			Assert.AreEqual(100f, view0.Height);

			Assert.AreEqual(50f, view1.X);
			Assert.AreEqual(0f, view1.Y);
			Assert.AreEqual(50f, view1.Width);
			Assert.AreEqual(100f, view1.Height);
		}

		[Test]
		public void TestAlignContentStretchRowWithFixedHeight()
		{
			var platform = new UnitPlatform((visual, width, height) => new SizeRequest(new Size(0, 0)));
			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				Direction = FlexDirection.Row,
				AlignContent = FlexAlignContent.Stretch,
				AlignItems = FlexAlignItems.Start,
				Wrap = FlexWrap.Wrap,
				WidthRequest = 150,
				HeightRequest = 100
			};
			var view0 = new View { IsPlatformEnabled = true };
			view0.WidthRequest = 50;
			layout.Children.Add(view0);

			var view1 = new View { IsPlatformEnabled = true };
			view1.WidthRequest = 50;
			view1.HeightRequest = 60;
			layout.Children.Add(view1);

			var view2 = new View { IsPlatformEnabled = true };
			view2.WidthRequest = 50;
			layout.Children.Add(view2);

			var view3 = new View { IsPlatformEnabled = true };
			view3.WidthRequest = 50;
			layout.Children.Add(view3);

			var view4 = new View { IsPlatformEnabled = true };
			view4.WidthRequest = 50;
			layout.Children.Add(view4);

			layout.Layout(new Rectangle(0, 0, 150, 100));

			Assert.AreEqual(0f, layout.X);
			Assert.AreEqual(0f, layout.Y);
			Assert.AreEqual(150f, layout.Width);
			Assert.AreEqual(100f, layout.Height);

			Assert.AreEqual(0f, view0.X);
			Assert.AreEqual(0f, view0.Y);
			Assert.AreEqual(50f, view0.Width);
			Assert.AreEqual(0f, view0.Height);

			Assert.AreEqual(50f, view1.X);
			Assert.AreEqual(0f, view1.Y);
			Assert.AreEqual(50f, view1.Width);
			Assert.AreEqual(60f, view1.Height);

			Assert.AreEqual(100f, view2.X);
			Assert.AreEqual(0f, view2.Y);
			Assert.AreEqual(50f, view2.Width);
			Assert.AreEqual(0f, view2.Height);

			Assert.AreEqual(0f, view3.X);
			Assert.AreEqual(80f, view3.Y);
			Assert.AreEqual(50f, view3.Width);
			Assert.AreEqual(0f, view3.Height);

			Assert.AreEqual(50f, view4.X);
			Assert.AreEqual(80f, view4.Y);
			Assert.AreEqual(50f, view4.Width);
			Assert.AreEqual(0f, view4.Height);
		}
	}
}
