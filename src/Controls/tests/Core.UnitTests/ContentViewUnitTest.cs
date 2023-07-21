using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using StackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;


	public class ContentViewUnitTests : BaseTestFixture
	{
		[Fact]
		public void TestConstructor()
		{
			var contentView = new ContentView();

			Assert.Null(contentView.Content);
			Assert.Null(contentView.BackgroundColor);
			Assert.Equal(new Thickness(0), contentView.Padding);
		}

		[Fact]
		public void TestSetChild()
		{
			var contentView = new ContentView();

			var child1 = new Label();

			bool added = false;

			contentView.ChildAdded += (sender, e) => added = true;

			contentView.Content = child1;

			Assert.True(added);
			Assert.Equal(child1, contentView.Content);
			Assert.Equal(child1.Parent, contentView);

			added = false;
			contentView.Content = child1;

			Assert.False(added);
		}

		[Fact]
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

			Assert.Null(child1.Parent);

			Assert.True(removed);
			Assert.True(added);
			Assert.Equal(child2, contentView.Content);
		}

		[Fact]
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

			Assert.Equal(new Size(120, 220), contentView.Measure(double.PositiveInfinity, double.PositiveInfinity).Request);

			contentView.Layout(new Rect(0, 0, 300, 300));

			Assert.Equal(new Rect(10, 10, 280, 280), child.Bounds);
		}

		[Fact]
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

			Assert.Equal(new Size(40, 220), contentView.Measure(double.PositiveInfinity, double.PositiveInfinity).Request);
		}

		[Fact]
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

			Assert.Equal(new Size(120, 40), contentView.Measure(double.PositiveInfinity, double.PositiveInfinity).Request);
		}

		[Fact]
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

			contentView.Layout(new Rect(0, 0, 200, 200));

			Assert.Equal(new Rect(0, 50, 200, 100), child.Bounds);
		}

		[Fact]
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

			contentView.Layout(new Rect(0, 0, 200, 200));

			Assert.Equal(new Rect(0, 0, 200, 100), child.Bounds);
		}

		[Fact]
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

			contentView.Layout(new Rect(0, 0, 200, 200));

			Assert.Equal(new Rect(0, 100, 200, 100), child.Bounds);
		}

		[Fact]
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

			contentView.Layout(new Rect(0, 0, 200, 200));

			Assert.Equal(new Rect(50, 0, 100, 200), child.Bounds);
		}

		[Fact]
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

			contentView.Layout(new Rect(0, 0, 200, 200));

			Assert.Equal(new Rect(0, 0, 100, 200), child.Bounds);
		}

		[Fact]
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

			contentView.Layout(new Rect(0, 0, 200, 200));

			Assert.Equal(new Rect(100, 0, 100, 200), child.Bounds);
		}

		[Fact]
		public void NullTemplateDirectlyHosts()
		{
			// order of setting properties carefully picked to emulate running on real backend

			var contentView = new ContentView();
			var child = new View();

			contentView.Content = child;

			Assert.Equal(child, ((IElementController)contentView).LogicalChildren[0]);
		}

		class SimpleTemplate : StackLayout
		{
			public SimpleTemplate()
			{
				Children.Add(new Label());
				Children.Add(new ContentPresenter());
			}
		}


		[Fact]
		public void TemplateInflates()
		{
			var contentView = new ContentView();

			contentView.ControlTemplate = new ControlTemplate(typeof(SimpleTemplate));

			Assert.IsType<SimpleTemplate>(((IElementController)contentView).LogicalChildren[0]);
		}

		[Fact]
		public void PacksContent()
		{
			var contentView = new ContentView();
			var child = new View();

			contentView.ControlTemplate = new ControlTemplate(typeof(SimpleTemplate));
			contentView.Content = child;

			Assert.IsType<SimpleTemplate>(((IElementController)contentView).LogicalChildren[0]);

			Assert.Contains(child, contentView.Descendants());
		}

		[Fact]
		public void DoesNotInheritBindingContextToTemplate()
		{
			var contentView = new ContentView();
			var child = new View();

			contentView.ControlTemplate = new ControlTemplate(typeof(SimpleTemplate));
			contentView.Content = child;

			var bc = "Test";
			contentView.BindingContext = bc;

			Assert.NotEqual(bc, ((IElementController)contentView).LogicalChildren[0].BindingContext);
			Assert.Null(((IElementController)contentView).LogicalChildren[0].BindingContext);
		}

		[Fact]
		public void ContentDoesGetBindingContext()
		{
			var contentView = new ContentView();
			var child = new View();

			contentView.ControlTemplate = new ControlTemplate(typeof(SimpleTemplate));
			contentView.Content = child;

			var bc = "Test";
			contentView.BindingContext = bc;

			Assert.Equal(bc, child.BindingContext);
		}

		[Fact]
		public void ContentParentIsNotInsideTempalte()
		{
			var contentView = new ContentView();
			var child = new View();

			contentView.ControlTemplate = new ControlTemplate(typeof(SimpleTemplate));
			contentView.Content = child;

			Assert.Equal(contentView, child.Parent);
		}

		[Fact]
		public void NonTemplatedContentInheritsBindingContext()
		{
			var contentView = new ContentView();
			var child = new View();

			contentView.Content = child;
			contentView.BindingContext = "Foo";

			Assert.Equal("Foo", child.BindingContext);
		}

		[Fact]
		public void ContentView_should_have_the_InternalChildren_correctly_when_Content_changed()
		{
			var sut = new ContentView();
			IList<Element> internalChildren = ((IControlTemplated)sut).InternalChildren;
			internalChildren.Add(new VisualElement());
			internalChildren.Add(new VisualElement());
			internalChildren.Add(new VisualElement());

			var expected = new View();
			sut.Content = expected;

			Assert.Single(internalChildren);
			Assert.Same(expected, internalChildren[0]);
		}
	}
}
