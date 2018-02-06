using NUnit.Framework;
using System;
using System.Globalization;
using System.Threading;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class FlexLayoutTests : BaseTestFixture
	{
		[Test]
		public void TestBasicLayout()
		{
			var platform = new UnitPlatform();
			var label1 = new Label { Platform = platform, IsPlatformEnabled = true };
			var label2 = new Label { Platform = platform, IsPlatformEnabled = true };

			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				Children = {
					label1,
					label2,
				}
			};

			layout.Layout(new Rectangle(0, 0, 912, 912));

			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 912, 912)));
			Assert.That(label1.Bounds, Is.EqualTo(new Rectangle(0, 0, 100, 912)));
			Assert.That(label2.Bounds, Is.EqualTo(new Rectangle(100, 0, 100, 912)));
		}

		[Test]
		public void TestBasicLayoutWithElementsWidth()
		{
			var platform = new UnitPlatform();
			var label1 = new Label { Platform = platform, IsPlatformEnabled = true, WidthRequest = 120 };
			var label2 = new Label { Platform = platform, IsPlatformEnabled = true, WidthRequest = 120 };

			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				Children = {
					label1,
					label2,
				}
			};

			layout.Layout(new Rectangle(0, 0, 912, 912));

			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 912, 912)));
			Assert.That(label1.Bounds, Is.EqualTo(new Rectangle(0, 0, 120, 912)));
			Assert.That(label2.Bounds, Is.EqualTo(new Rectangle(120, 0, 120, 912)));

		}

		[Test]
		public void TestBasicLayoutWithElementsWidthAndMargin()
		{
			var platform = new UnitPlatform();
			var label1 = new Label { Platform = platform, IsPlatformEnabled = true, WidthRequest = 100, Margin = new Thickness(5, 0, 0, 0) };
			var label2 = new Label { Platform = platform, IsPlatformEnabled = true, WidthRequest = 100, Margin = new Thickness(5, 0, 0, 0) };

			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				Children = {
					label1,
					label2,
				}
			};

			layout.Layout(new Rectangle(0, 0, 912, 912));

			Assert.AreEqual(912, layout.Width);
			Assert.AreEqual(912, layout.Height);

			Assert.AreEqual(new Rectangle(5, 0, 100, 912), label1.Bounds);
			Assert.AreEqual(new Rectangle(110, 0, 100, 912), label2.Bounds);
		}

		[Test]
		public void TestSetBounds()
		{
			var layoutSize = new Size(320, 50);
			var platform = new UnitPlatform();

			var layout = new FlexLayout {
				Direction = FlexDirection.Row,
				AlignItems = FlexAlignItems.Start,
				Platform = platform,
				IsPlatformEnabled = true
			};

			var label1 = new Label { Platform = platform, IsPlatformEnabled = true };
			FlexLayout.SetGrow(label1, 1);
			layout.Children.Add(label1);

			var label2 = new Label { Platform = platform, IsPlatformEnabled = true };
			FlexLayout.SetGrow(label2, 1);
			layout.Children.Add(label2);

			var label3 = new Label { Platform = platform, IsPlatformEnabled = true };
			FlexLayout.SetGrow(label3, 1);
			layout.Children.Add(label3);

			layout.Layout(new Rectangle(0, 0, layoutSize.Width, layoutSize.Height));

			Assert.AreEqual(label2.Bounds.Left, Math.Max(label1.Bounds.Left, label1.Bounds.Right), 1);
			Assert.AreEqual(label3.Bounds.Left, Math.Max(label2.Bounds.Left, label2.Bounds.Right), 1);

			double totalWidth = 0;
			foreach (var view in layout.Children)
			{
				totalWidth += view.Bounds.Width;
			}

			Assert.AreEqual(layoutSize.Width, totalWidth, 2);
		}

		[Test]
		public void TestRelayoutOnChildrenRemoved()
		{
			var layoutSize = new Size(300, 50);

			var platform = new UnitPlatform();

			var layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				Direction = FlexDirection.Row
			};

			var label1 = new Label { Platform = platform, IsPlatformEnabled = true };
			FlexLayout.SetGrow(label1, 1);
			layout.Children.Add(label1);

			var label2 = new Label { Platform = platform, IsPlatformEnabled = true };
			FlexLayout.SetGrow(label2, 1);
			layout.Children.Add(label2);

			var label3 = new Label { Platform = platform, IsPlatformEnabled = true };
			FlexLayout.SetGrow(label3, 1);
			layout.Children.Add(label3);

			layout.Layout(new Rectangle(0, 0, layoutSize.Width, layoutSize.Height));

			foreach (var view in layout.Children)
				Assert.That(view.Bounds.Width, Is.EqualTo(100));

			layout.Children.Remove(label3);

			Assert.That(label1.Bounds.Width, Is.EqualTo(150));
			Assert.That(label2.Bounds.Width, Is.EqualTo(150));
			Assert.That(label3.Bounds.Width, Is.EqualTo(100));
		}

		[Test]
		public void TestFlexLayoutIsIncludeChangeWorksOnSecondPass()
		{
			var layoutSize = new Size(300, 50);
			var platform = new UnitPlatform();
			var layout = new FlexLayout {
				Direction = FlexDirection.Row,
				Platform = platform,
				IsPlatformEnabled = true,
			};

			var label1 = new Label { Platform = platform, IsPlatformEnabled = true };
			FlexLayout.SetGrow(label1, 1);
			layout.Children.Add(label1);

			var label2 = new Label { Platform = platform, IsPlatformEnabled = true };
			FlexLayout.SetGrow(label2, 1);
			layout.Children.Add(label2);

			var label3 = new Label { Platform = platform, IsPlatformEnabled = true };
			FlexLayout.SetGrow(label3, 1);

			layout.Layout(new Rectangle(0, 0, layoutSize.Width, layoutSize.Height));

			Assert.AreEqual(150, label1.Bounds.Width);
			Assert.AreEqual(150, label2.Bounds.Width);
			Assert.AreEqual(-1, label3.Bounds.Width);

			layout.Children.Add(label3);

			layout.Layout(new Rectangle(0, 0, layoutSize.Width, layoutSize.Height));
			Assert.AreEqual(100, label1.Bounds.Width);
			Assert.AreEqual(100, label2.Bounds.Width);
			Assert.AreEqual(100, label3.Bounds.Width);
		}

		[Test]
		// fixed at https://github.com/xamarin/flex/commit/0ccb9f1625abdc5400def29651373937bf6610cd
		public void TestSwapChildrenOrder()
		{
			var layoutSize = new Size(300, 50);

			var platform = new UnitPlatform();
			var layout = new FlexLayout { 
				Direction = FlexDirection.Row,
				Platform = platform,
				IsPlatformEnabled = true,
			};

			var label0 = new Label { Platform = platform, IsPlatformEnabled = true, Text="Label0" };
			FlexLayout.SetGrow(label0, 1);
			layout.Children.Add(label0);

			var label1 = new Label { Platform = platform, IsPlatformEnabled = true, Text = "Label1" };
			FlexLayout.SetGrow(label1, 1);
			layout.Children.Add(label1);

			var label2 = new Label { Platform = platform, IsPlatformEnabled = true, Text = "Label2" };
			FlexLayout.SetGrow(label2, 1);
			layout.Children.Add(label2);

			layout.Layout(new Rectangle(0, 0, layoutSize.Width, layoutSize.Height));

			Assert.AreEqual(new Rectangle(0, 0, 100, 50), label0.Bounds);
			Assert.AreEqual(new Rectangle(100, 0, 100, 50), label1.Bounds);
			Assert.AreEqual(new Rectangle(200, 0, 100, 50), label2.Bounds);

			var lastItem = layout.Children[2];
			Assert.That(lastItem, Is.SameAs(label2));

			layout.Children.Remove(lastItem);
			Assert.AreEqual(new Rectangle(0, 0, 150, 50), label0.Bounds);
			Assert.AreEqual(new Rectangle(150, 0, 150, 50), label1.Bounds);

			layout.Children.Insert(0, lastItem);

			Assert.AreEqual(new Rectangle(0, 0, 100, 50), label2.Bounds);
			Assert.AreEqual(new Rectangle(100, 0, 100, 50), label0.Bounds);
			Assert.AreEqual(new Rectangle(200, 0, 100, 50), label1.Bounds);
		}

		[Test]
		public void TestSizeThatFits()
		{
			var platform = new UnitPlatform(useRealisticLabelMeasure: true);

			var layout = new FlexLayout {
				Direction = FlexDirection.Row,
				AlignItems = FlexAlignItems.Start,
				Platform = platform,
				IsPlatformEnabled = true
			};

			var label1 = new Label {
				Platform = platform,
				IsPlatformEnabled = true,
				LineBreakMode = LineBreakMode.TailTruncation,
				Text = @"This is a very very very very very very very very long piece of text."
			};
			FlexLayout.SetShrink(label1, 1);
			layout.Children.Add(label1);

			var label2 = new Label {
				Text = "",
				Platform = platform,
				IsPlatformEnabled = true,
				WidthRequest = 10,
				HeightRequest = 10
			};
			layout.Children.Add(label2);
			layout.Layout(new Rectangle(0, 0, 320, 50));

			var label2Size = label2.Measure(double.PositiveInfinity, double.PositiveInfinity);
			Assert.AreEqual(10, label2Size.Request.Height);
			Assert.AreEqual(10, label2Size.Request.Width);

			var label1Size = label1.Measure(double.PositiveInfinity, double.PositiveInfinity);
			//	var layoutSize = layout.Measure(-1, -1);
		}

		[Test]
		public void TestNesting()
		{
			var platform = new UnitPlatform();

			var header = new View { HeightRequest = 50, IsPlatformEnabled = true, };
			var footer = new View { HeightRequest = 50, IsPlatformEnabled = true, };
			Func<View> createItem = () => {
				var v = new View { WidthRequest = 50, Margin = 5, IsPlatformEnabled = true, };
				FlexLayout.SetGrow(v, 1);
				return v;
			};

			var layout = new FlexLayout
			{
				Platform = platform,
				IsPlatformEnabled = true,
				Children = {
					header,
					new FlexLayout{
						Direction = FlexDirection.Row,
						IsPlatformEnabled = true,
						Children = {
							createItem(),
							createItem(),
							createItem(),
							createItem(),
							createItem(),
						}
					},
					footer,
				},
				Direction = FlexDirection.Column,
			};

			var inner = layout.Children[1] as FlexLayout;
			FlexLayout.SetGrow(inner, 1);

			layout.Layout(new Rectangle(0, 0, 300, 600));
			Assert.That(layout.Bounds, Is.EqualTo(new Rectangle(0, 0, 300, 600)));
			Assert.That(header.Bounds, Is.EqualTo(new Rectangle(0, 0, 300, 50)));
			Assert.That(inner.Bounds, Is.EqualTo(new Rectangle(0, 50, 300, 500)));
			Assert.That(inner.Children[0].Bounds, Is.EqualTo(new Rectangle(5, 5, 50, 490)));
			Assert.That(inner.Children[1].Bounds, Is.EqualTo(new Rectangle(65, 5, 50, 490)));
			Assert.That(inner.Children[2].Bounds, Is.EqualTo(new Rectangle(125, 5, 50, 490)));
			Assert.That(inner.Children[3].Bounds, Is.EqualTo(new Rectangle(185, 5, 50, 490)));
			Assert.That(inner.Children[4].Bounds, Is.EqualTo(new Rectangle(245, 5, 50, 490)));

			Assert.That(footer.Bounds, Is.EqualTo(new Rectangle(0, 550, 300, 50)));
		}

		[Test]
		public void TestMeasuring()
		{
			var platform = new UnitPlatform();
			var label = new Label {
				Platform = platform,
				IsPlatformEnabled = true,
			};
			var Layout = new FlexLayout {
				Platform = platform,
				IsPlatformEnabled = true,
				Direction = FlexDirection.Row,
				Wrap = FlexWrap.Wrap,
				Children = { 
					label,
				}
			};

			//measure sith +inf as main-axis
			var measure = Layout.Measure(double.PositiveInfinity, 40);
			Assert.That(measure.Request, Is.EqualTo(new Size(100, 40)));

			//measure sith +inf as cross-axis
			measure = Layout.Measure(200, double.PositiveInfinity);
			Assert.That(measure.Request, Is.EqualTo(new Size(200, 20)));

			//measure with +inf as both axis
			measure = Layout.Measure(double.PositiveInfinity, double.PositiveInfinity);
			Assert.That(measure.Request, Is.EqualTo(new Size(100, 20)));

		}
	}
}