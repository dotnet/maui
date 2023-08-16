using System.Collections.Generic;
using Microsoft.Maui.Controls.Xaml;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class TemplatedViewUnitTests : BaseTestFixture
	{
		[Fact]
		public void TemplatedView_should_have_the_InternalChildren_correctly_when_ControlTemplate_changed()
		{
			var sut = new TemplatedView();
			IList<Element> internalChildren = ((IControlTemplated)sut).InternalChildren;
			internalChildren.Add(new VisualElement());
			internalChildren.Add(new VisualElement());
			internalChildren.Add(new VisualElement());

			sut.ControlTemplate = new ControlTemplate(typeof(ExpectedView));

			Assert.Single(internalChildren);
			Assert.IsType<ExpectedView>(internalChildren[0]);
		}

		[Fact]
		public void ShouldHaveTemplatedRootSet()
		{
			var tv = new TemplatedView();
			var ct = (IControlTemplated)tv;
			Assert.Null(ct.TemplateRoot);

			tv.ControlTemplate = new ControlTemplate(typeof(ExpectedView));

			IList<Element> internalChildren = ct.InternalChildren;
			Assert.Equal(ct.TemplateRoot, internalChildren[0]);
		}

		[Fact]
		public void GetContentViewTemplateChildShouldWork()
		{
			var xaml = @"<ContentView
					xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
					xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
					x:Class=""Microsoft.Maui.Controls.Core.UnitTests.MyTestContentView"">
                       <ContentView.ControlTemplate>
                         <ControlTemplate>
                           <Label x:Name=""label0""/>
                         </ControlTemplate>
						</ContentView.ControlTemplate>
					</ContentView>";

			var contentView = new MyTestContentView();
			contentView.LoadFromXaml(xaml);

			IList<Element> internalChildren = contentView.InternalChildren;
			Assert.Equal(internalChildren[0], contentView.TemplateChildObtained);
		}

		[Fact]
		public void GetTemplatedViewTemplateChildShouldWork()
		{
			var xaml =
				@"<ContentView
					xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
					xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
					x:Class=""Microsoft.Maui.Controls.Core.UnitTests.MyTestTemplatedView"">
					<TemplatedView.ControlTemplate>
						<ControlTemplate>
							<Label x:Name=""label0""/>
						</ControlTemplate>
					</TemplatedView.ControlTemplate>
				</ContentView>";

			var contentView = new MyTestTemplatedView();
			contentView.LoadFromXaml(xaml);

			IList<Element> internalChildren = contentView.InternalChildren;
			Assert.Equal(internalChildren[0], contentView.TemplateChildObtained);
		}

		[Fact]
		public void GetContentPageTemplateChildShouldWork()
		{
			var xaml = @"<ContentPage
					xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
					xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
					x:Class=""Microsoft.Maui.Controls.Core.UnitTests.MyTestContentPage"">
                       <ContentPage.ControlTemplate>
                         <ControlTemplate>
                           <Label x:Name=""label0""/>
                         </ControlTemplate>
						</ContentPage.ControlTemplate>
					</ContentPage>";

			var contentPage = new MyTestContentPage();
			contentPage.LoadFromXaml(xaml);

			IList<Element> internalChildren = contentPage.InternalChildren;
			Assert.Equal(internalChildren[0], contentPage.TemplateChildObtained);
		}

		[Fact]
		public void OnContentViewApplyTemplateShouldBeCalled()
		{
			var xaml = @"<ContentView
					xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
					xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
					x:Class=""Microsoft.Maui.Controls.Core.UnitTests.MyTestContentView"">
                       <ContentView.ControlTemplate>
                         <ControlTemplate>
                           <Label x:Name=""label0""/>
                         </ControlTemplate>
						</ContentView.ControlTemplate>
					</ContentView>";

			var contentView = new MyTestContentView();
			contentView.LoadFromXaml(xaml);
			Assert.True(contentView.WasOnApplyTemplateCalled);
		}

		[Fact]
		public void OnTemplatedViewApplyTemplateShouldBeCalled()
		{
			var xaml =
				@"<ContentView
					xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
					xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
					x:Class=""Microsoft.Maui.Controls.Core.UnitTests.MyTestTemplatedView"">
					<ContentView.ControlTemplate>
						<ControlTemplate>
							<Label x:Name=""label0""/>
						</ControlTemplate>
					</ContentView.ControlTemplate>
				</ContentView>";

			var contentView = new MyTestTemplatedView();
			contentView.LoadFromXaml(xaml);

			Assert.True(contentView.WasOnApplyTemplateCalled);
		}

		[Fact]
		public void OnContentPageApplyTemplateShouldBeCalled()
		{
			var xaml = @"<ContentPage
					xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
					xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
					x:Class=""Microsoft.Maui.Controls.Core.UnitTests.MyTestContentPage"">
                       <ContentPage.ControlTemplate>
                         <ControlTemplate>
                           <Label x:Name=""label0""/>
                         </ControlTemplate>
						</ContentPage.ControlTemplate>
					</ContentPage>";

			var contentPage = new MyTestContentPage();
			contentPage.LoadFromXaml(xaml);
			Assert.True(contentPage.WasOnApplyTemplateCalled);
		}

		private class ExpectedView : View
		{
			public ExpectedView()
			{
			}
		}

		public class MyTemplate : StackLayout
		{
			public MyTemplate()
			{
				Children.Add(new ContentPresenter());
			}
		}

		[Fact]
		public void BindingsShouldBeAppliedOnTemplateChange()
		{
			var template0 = new ControlTemplate(typeof(MyTemplate));
			var template1 = new ControlTemplate(typeof(MyTemplate));
			var label = new Label();
			label.SetBinding(Label.TextProperty, ".");
			var cv = new ContentView
			{
				ControlTemplate = template0,
				Content = label
			};
			cv.BindingContext = "Foo";

			Assert.Equal("Foo", label.Text);
			cv.ControlTemplate = template1;
			Assert.Equal("Foo", label.Text);
		}
	}

	class MyTestTemplatedView : TemplatedView
	{
		public bool WasOnApplyTemplateCalled { get; private set; }

		public Element TemplateChildObtained { get; private set; }

		protected override void OnApplyTemplate()
		{
			WasOnApplyTemplateCalled = true;
			TemplateChildObtained = (Element)GetTemplateChild("label0");
		}
	}

	class MyTestContentView : ContentView
	{
		public bool WasOnApplyTemplateCalled { get; private set; }

		public Element TemplateChildObtained { get; private set; }

		protected override void OnApplyTemplate()
		{
			WasOnApplyTemplateCalled = true;
			TemplateChildObtained = (Element)GetTemplateChild("label0");
		}
	}

	class MyTestContentPage : ContentPage
	{
		public bool WasOnApplyTemplateCalled { get; private set; }

		public Element TemplateChildObtained { get; private set; }

		protected override void OnApplyTemplate()
		{
			WasOnApplyTemplateCalled = true;
			TemplateChildObtained = (Element)GetTemplateChild("label0");
		}
	}
}