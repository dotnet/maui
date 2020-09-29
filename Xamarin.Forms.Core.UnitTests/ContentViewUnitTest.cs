using System;
using System.Collections.Generic;
using System.Linq;

using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ContentViewUnitTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
		}

		[Test]
		public void TestConstructor()
		{
			var contentView = new ContentView();

			Assert.Null(contentView.Content);
			Assert.AreEqual(Color.Default, contentView.BackgroundColor);
			Assert.AreEqual(new Thickness(0), contentView.Padding);
		}

		[Test]
		public void TestSetChild()
		{
			var contentView = new ContentView();

			var child1 = new Label();

			bool added = false;

			contentView.ChildAdded += (sender, e) => added = true;

			contentView.Content = child1;

			Assert.True(added);
			Assert.AreEqual(child1, contentView.Content);

			added = false;
			contentView.Content = child1;

			Assert.False(added);
		}

		[Test]
		public void TestReplaceChild()
		{
			var contentView = new ContentView();

			var child1 = new Label();
			var child2 = new Label();

			contentView.Content = child1;

			bool removed = false;
			bool added = false;

			contentView.ChildRemoved += (sender, e) => removed = true;
			contentView.ChildAdded += (sender, e) => added = true;

			contentView.Content = child2;

			Assert.True(removed);
			Assert.True(added);
			Assert.AreEqual(child2, contentView.Content);
		}

		[Test]
		public void TestFrameLayout()
		{
			View child;

			var contentView = new ContentView
			{
				Padding = new Thickness(10),
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 200,
					IsPlatformEnabled = true
				},
				IsPlatformEnabled = true,
			};

			Assert.AreEqual(new Size(120, 220), contentView.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity).Request);

			contentView.Layout(new Rectangle(0, 0, 300, 300));

			Assert.AreEqual(new Rectangle(10, 10, 280, 280), child.Bounds);
		}

		[Test]
		public void WidthRequest()
		{
			View child;

			var contentView = new ContentView
			{
				Padding = new Thickness(10),
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 200,
					IsPlatformEnabled = true
				},
				IsPlatformEnabled = true,
				WidthRequest = 20
			};

			Assert.AreEqual(new Size(40, 220), contentView.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity).Request);
		}

		[Test]
		public void HeightRequest()
		{
			View child;

			var contentView = new ContentView
			{
				Padding = new Thickness(10),
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 200,
					IsPlatformEnabled = true,
				},
				IsPlatformEnabled = true,
				HeightRequest = 20
			};

			Assert.AreEqual(new Size(120, 40), contentView.GetSizeRequest(double.PositiveInfinity, double.PositiveInfinity).Request);
		}

		[Test]
		public void LayoutVerticallyCenter()
		{
			View child;

			var contentView = new ContentView
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 100,
					IsPlatformEnabled = true,
					VerticalOptions = LayoutOptions.Center
				},
				IsPlatformEnabled = true,
			};

			contentView.Layout(new Rectangle(0, 0, 200, 200));

			Assert.AreEqual(new Rectangle(0, 50, 200, 100), child.Bounds);
		}

		[Test]
		public void LayoutVerticallyBegin()
		{
			View child;

			var contentView = new ContentView
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 100,
					IsPlatformEnabled = true,
					VerticalOptions = LayoutOptions.Start
				},
				IsPlatformEnabled = true,
			};

			contentView.Layout(new Rectangle(0, 0, 200, 200));

			Assert.AreEqual(new Rectangle(0, 0, 200, 100), child.Bounds);
		}

		[Test]
		public void LayoutVerticallyEnd()
		{
			View child;

			var contentView = new ContentView
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 100,
					IsPlatformEnabled = true,
					VerticalOptions = LayoutOptions.End
				},
				IsPlatformEnabled = true,
			};

			contentView.Layout(new Rectangle(0, 0, 200, 200));

			Assert.AreEqual(new Rectangle(0, 100, 200, 100), child.Bounds);
		}

		[Test]
		public void LayoutHorizontallyCenter()
		{
			View child;

			var contentView = new ContentView
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 100,
					IsPlatformEnabled = true,
					HorizontalOptions = LayoutOptions.Center
				},
				IsPlatformEnabled = true,
			};

			contentView.Layout(new Rectangle(0, 0, 200, 200));

			Assert.AreEqual(new Rectangle(50, 0, 100, 200), child.Bounds);
		}

		[Test]
		public void LayoutHorizontallyBegin()
		{
			View child;

			var contentView = new ContentView
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 100,
					IsPlatformEnabled = true,
					HorizontalOptions = LayoutOptions.Start
				},
				IsPlatformEnabled = true,
			};

			contentView.Layout(new Rectangle(0, 0, 200, 200));

			Assert.AreEqual(new Rectangle(0, 0, 100, 200), child.Bounds);
		}

		[Test]
		public void LayoutHorizontallyEnd()
		{
			View child;

			var contentView = new ContentView
			{
				Content = child = new View
				{
					WidthRequest = 100,
					HeightRequest = 100,
					IsPlatformEnabled = true,
					HorizontalOptions = LayoutOptions.End
				},
				IsPlatformEnabled = true,
			};

			contentView.Layout(new Rectangle(0, 0, 200, 200));

			Assert.AreEqual(new Rectangle(100, 0, 100, 200), child.Bounds);
		}

		[Test]
		public void NullTemplateDirectlyHosts()
		{
			// order of setting properties carefully picked to emulate running on real backend

			var contentView = new ContentView();
			var child = new View();

			contentView.Content = child;

			Assert.AreEqual(child, ((IElementController)contentView).LogicalChildren[0]);
		}

		class SimpleTemplate : StackLayout
		{
			public SimpleTemplate()
			{
				Children.Add(new Label());
				Children.Add(new ContentPresenter());
			}
		}


		[Test]
		public void TemplateInflates()
		{
			var contentView = new ContentView();

			contentView.ControlTemplate = new ControlTemplate(typeof(SimpleTemplate));

			Assert.That(((IElementController)contentView).LogicalChildren[0], Is.TypeOf<SimpleTemplate>());
		}

		[Test]
		public void PacksContent()
		{
			var contentView = new ContentView();
			var child = new View();

			contentView.ControlTemplate = new ControlTemplate(typeof(SimpleTemplate));
			contentView.Content = child;

			Assume.That(((IElementController)contentView).LogicalChildren[0], Is.TypeOf<SimpleTemplate>());
			Assert.That(contentView.Descendants(), Contains.Item(child));
		}

		[Test]
		public void DoesNotInheritBindingContextToTemplate()
		{
			var contentView = new ContentView();
			var child = new View();

			contentView.ControlTemplate = new ControlTemplate(typeof(SimpleTemplate));
			contentView.Content = child;

			var bc = "Test";
			contentView.BindingContext = bc;

			Assert.AreNotEqual(bc, ((IElementController)contentView).LogicalChildren[0].BindingContext);
			Assert.IsNull(((IElementController)contentView).LogicalChildren[0].BindingContext);
		}

		[Test]
		public void ContentDoesGetBindingContext()
		{
			var contentView = new ContentView();
			var child = new View();

			contentView.ControlTemplate = new ControlTemplate(typeof(SimpleTemplate));
			contentView.Content = child;

			var bc = "Test";
			contentView.BindingContext = bc;

			Assert.AreEqual(bc, child.BindingContext);
		}

		[Test]
		public void ContentParentIsNotInsideTempalte()
		{
			var contentView = new ContentView();
			var child = new View();

			contentView.ControlTemplate = new ControlTemplate(typeof(SimpleTemplate));
			contentView.Content = child;

			Assert.AreEqual(contentView, child.Parent);
		}

		[Test]
		public void NonTemplatedContentInheritsBindingContext()
		{
			var contentView = new ContentView();
			var child = new View();

			contentView.Content = child;
			contentView.BindingContext = "Foo";

			Assert.AreEqual("Foo", child.BindingContext);
		}

		[Test]
		public void ContentView_should_have_the_InternalChildren_correctly_when_Content_changed()
		{
			var sut = new ContentView();
			IList<Element> internalChildren = ((IControlTemplated)sut).InternalChildren;
			internalChildren.Add(new VisualElement());
			internalChildren.Add(new VisualElement());
			internalChildren.Add(new VisualElement());

			var expected = new View();
			sut.Content = expected;

			Assert.AreEqual(1, internalChildren.Count);
			Assert.AreSame(expected, internalChildren[0]);
		}
	}
}