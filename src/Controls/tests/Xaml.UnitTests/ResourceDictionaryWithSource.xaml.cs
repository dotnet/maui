using System;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class ResourceDictionaryWithSource : ContentPage
{
	public ResourceDictionaryWithSource() => InitializeComponent();

	class Tests
	{
		[Test]
		public void RDWithSourceAreFound([Values] XamlInflator inflator)
		{
			var layout = new ResourceDictionaryWithSource(inflator);
			Assert.That(layout.label.TextColor, Is.EqualTo(Colors.Pink));
		}

		[Test]
		public void RelativeAndAbsoluteURI([Values] XamlInflator inflator)
		{
			var layout = new ResourceDictionaryWithSource(inflator);
			Assert.That(((ResourceDictionary)layout.Resources["relURI"]).Source, Is.EqualTo(new Uri("./SharedResourceDictionary.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative)));
			Assert.That(((ResourceDictionary)layout.Resources["relURI"])["sharedfoo"], Is.TypeOf<Style>());
			Assert.That(((ResourceDictionary)layout.Resources["absURI"]).Source, Is.EqualTo(new Uri("/SharedResourceDictionary.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative)));
			Assert.That(((ResourceDictionary)layout.Resources["absURI"])["sharedfoo"], Is.TypeOf<Style>());
			Assert.That(((ResourceDictionary)layout.Resources["shortURI"]).Source, Is.EqualTo(new Uri("SharedResourceDictionary.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative)));
			Assert.That(((ResourceDictionary)layout.Resources["shortURI"])["sharedfoo"], Is.TypeOf<Style>());
			Assert.That(((ResourceDictionary)layout.Resources["Colors"])["MediumGrayTextColor"], Is.TypeOf<Color>());
			Assert.That(((ResourceDictionary)layout.Resources["CompiledColors"])["MediumGrayTextColor"], Is.TypeOf<Color>());
		}

		[Test]
		public void XRIDIsGeneratedForRDWithoutCodeBehind()
		{
			var asm = typeof(ResourceDictionaryWithSource).Assembly;
			var resourceId = XamlResourceIdAttribute.GetResourceIdForPath(asm, "AppResources/Colors.rtxc.xaml");
			Assert.That(resourceId, Is.Not.Null);
			var type = XamlResourceIdAttribute.GetTypeForResourceId(asm, resourceId);
			Assert.That(type?.Name, Does.StartWith("__Type"), "xaml-comp default to true, this should have a type associated with it");

#if !FIXME_BEFORE_PUBLIC_RELEASE
			Assert.Fail(); //make sure to have the same default for sourcegen 
#endif
		}

		[Test]
		public void CodeBehindIsGeneratedForRDWithXamlComp()
		{
			var asm = typeof(ResourceDictionaryWithSource).Assembly;
			var resourceId = XamlResourceIdAttribute.GetResourceIdForPath(asm, "AppResources/CompiledColors.rtxc.xaml");
			Assert.That(resourceId, Is.Not.Null);
			var type = XamlResourceIdAttribute.GetTypeForResourceId(asm, resourceId);
			Assert.That(type, Is.Not.Null);
			var rd = Activator.CreateInstance(type);
			Assert.That(rd as ResourceDictionary, Is.Not.Null);
		}

		[Test]
		public void LoadResourcesWithAssembly([Values] XamlInflator inflator)
		{
			var layout = new ResourceDictionaryWithSource(inflator);
			Assert.That(((ResourceDictionary)layout.Resources["inCurrentAssembly"]).Source, Is.EqualTo(new Uri("/AppResources/Colors.rtxc.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative)));
			Assert.That(((ResourceDictionary)layout.Resources["inCurrentAssembly"])["MediumGrayTextColor"], Is.TypeOf<Color>());
			Assert.That(((ResourceDictionary)layout.Resources["inOtherAssembly"]).Source, Is.EqualTo(new Uri("/AppResources.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests.ExternalAssembly", UriKind.Relative)));
			Assert.That(((ResourceDictionary)layout.Resources["inOtherAssembly"])["notBlue"], Is.TypeOf<Color>());
		}

	}
}