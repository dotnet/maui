using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class ResourceDictionaryWithSource : ContentPage
	{
		public ResourceDictionaryWithSource()
		{
			InitializeComponent();
		}

		public ResourceDictionaryWithSource(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[TestCase(false), TestCase(true)]
			public void RDWithSourceAreFound(bool useCompiledXaml)
			{
				var layout = new ResourceDictionaryWithSource(useCompiledXaml);
				Assert.That(layout.label.TextColor, Is.EqualTo(Color.Pink));
			}

			[TestCase(false), TestCase(true)]
			public void RelativeAndAbsoluteURI(bool useCompiledXaml)
			{
				var layout = new ResourceDictionaryWithSource(useCompiledXaml);
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
				var resourceId = XamlResourceIdAttribute.GetResourceIdForPath(asm, "AppResources/Colors.xaml");
				Assert.That(resourceId, Is.Not.Null);
				var type = XamlResourceIdAttribute.GetTypeForResourceId(asm, resourceId);
				Assert.That(type, Is.Null);
			}

			[Test]
			public void CodeBehindIsGeneratedForRDWithXamlComp()
			{
				var asm = typeof(ResourceDictionaryWithSource).Assembly;
				var resourceId = XamlResourceIdAttribute.GetResourceIdForPath(asm, "AppResources/CompiledColors.xaml");
				Assert.That(resourceId, Is.Not.Null);
				var type = XamlResourceIdAttribute.GetTypeForResourceId(asm, resourceId);
				Assert.That(type, Is.Not.Null);
				var rd = Activator.CreateInstance(type);
				Assert.That(rd as ResourceDictionary, Is.Not.Null);
			}

			[TestCase(false), TestCase(true)]
			public void LoadResourcesWithAssembly(bool useCompiledXaml)
			{
				var layout = new ResourceDictionaryWithSource(useCompiledXaml);
				Assert.That(((ResourceDictionary)layout.Resources["inCurrentAssembly"]).Source, Is.EqualTo(new Uri("/AppResources/Colors.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative)));
				Assert.That(((ResourceDictionary)layout.Resources["inCurrentAssembly"])["MediumGrayTextColor"], Is.TypeOf<Color>());
				Assert.That(((ResourceDictionary)layout.Resources["inOtherAssembly"]).Source, Is.EqualTo(new Uri("/AppResources.xaml;assembly=Microsoft.Maui.Controls.Compatibility.ControlGallery", UriKind.Relative)));
				Assert.That(((ResourceDictionary)layout.Resources["inOtherAssembly"])["notBlue"], Is.TypeOf<Color>());
			}

		}
	}
}