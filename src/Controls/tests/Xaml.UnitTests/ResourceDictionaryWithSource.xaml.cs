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
		public void CanLoadInflatedResources([Values] XamlInflator from, [Values] XamlInflator rd)
		{
			var layout = new ResourceDictionaryWithSource(from);
			var key = "Colors";
			switch (rd)
			{
				case XamlInflator.Runtime:
					key = "Colors.rt";
					break;
				case XamlInflator.SourceGen:
					key = "Colors.sgen";
					break;
				case XamlInflator.XamlC:
					key = "Colors.xc";
					break;
			}
			var rdLoaded = layout.Resources[key] as ResourceDictionary;
			Assert.That(rdLoaded, Is.Not.Null);
			Assert.That(rdLoaded["MediumGrayTextColor"], Is.TypeOf<Color>());
			Assert.That(rdLoaded["MediumGrayTextColor"] as Color, Is.EqualTo(Color.Parse("#ff4d4d4d")));
		}

		[Test]
		public void XRIDIsGeneratedForRDWithoutCodeBehind([Values] XamlInflator rd)
		{
			var path = "AppResources/Colors.xaml";
			switch (rd)
			{
				case XamlInflator.Runtime:
					path = "AppResources/Colors.rt.xaml";
					break;
				case XamlInflator.SourceGen:
					path = "AppResources/Colors.sgen.xaml";
					break;
				case XamlInflator.XamlC:
					path = "AppResources/Colors.xc.xaml";
					break;
			}

			var asm = typeof(ResourceDictionaryWithSource).Assembly;
			var resourceId = XamlResourceIdAttribute.GetResourceIdForPath(asm, path);
			Assert.That(resourceId, Is.Not.Null);
			var type = XamlResourceIdAttribute.GetTypeForResourceId(asm, resourceId);
			Assert.That(type?.Name, Does.StartWith("__Type"), "We add a type for all RD without Class, this should have a type associated with it");

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