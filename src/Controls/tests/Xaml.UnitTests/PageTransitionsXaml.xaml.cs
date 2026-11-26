using System;
using Microsoft.Maui.Controls.Animations;
using Microsoft.Maui.Graphics;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class PageTransitionsXaml : ContentPage
{
	public PageTransitionsXaml() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void ConverterStringsSetAttachedProperties(XamlInflator inflator)
		{
			var page = new PageTransitionsXaml(inflator);

			var slide = Assert.IsType<SlideTransition>(PageTransitions.GetTransition(page));
			Assert.Equal(SlideEdge.Bottom, slide.From);
			Assert.Equal(TimeSpan.FromMilliseconds(300), slide.Duration);
			Assert.IsType<FadeTransition>(PageTransitions.GetModalTransition(page));
			Assert.Same(PageTransition.None, PageTransitions.GetTransition(page.none));
			Assert.Same(PageTransition.Platform, PageTransitions.GetTransition(page.platform));
		}

		[Theory]
		[XamlInflatorData]
		internal void TransitionElementsSetTheirProperties(XamlInflator inflator)
		{
			var page = new PageTransitionsXaml(inflator);

			var scale = Assert.IsType<ScaleTransition>(PageTransitions.GetTabTransition(page));
			Assert.Equal(0.5, scale.FromScale);
			Assert.Equal(TimeSpan.FromMilliseconds(250), scale.Duration);
			Assert.Same(Easing.CubicOut, scale.Easing);

			var slide = Assert.IsType<SlideTransition>(PageTransitions.GetTransition(page.explicitSlide));
			Assert.Equal(SlideEdge.Start, slide.From);
			Assert.Equal(TimeSpan.FromMilliseconds(450), slide.Duration);
			Assert.Same(Easing.SinInOut, slide.Easing);
			Assert.Equal(Colors.Red, Assert.IsAssignableFrom<SolidColorBrush>(slide.Background).Color);

			var fade = Assert.IsType<FadeTransition>(PageTransitions.GetTransition(page.fade));
			Assert.Equal(TimeSpan.FromSeconds(1), fade.Duration);
		}

		[Theory]
		[XamlInflatorData]
		internal void SharedTransitionAttachedPropertiesAreSet(XamlInflator inflator)
		{
			var page = new PageTransitionsXaml(inflator);

			Assert.Equal("photo", SharedTransition.GetName(page.photo));
			Assert.Equal("42", SharedTransition.GetGroup(page.photo));
			Assert.Equal("42", SharedTransition.GetGroup(page));
		}

		[Fact]
		internal void SourceGeneratorReportsNoDiagnostics()
		{
			var result = CreateMauiCompilation()
				.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class PageTransitionsXaml : ContentPage
{
	public PageTransitionsXaml() => InitializeComponent();
}
""")
				.RunMauiSourceGenerator(typeof(PageTransitionsXaml));

			Assert.Empty(result.Diagnostics);
		}
	}
}
