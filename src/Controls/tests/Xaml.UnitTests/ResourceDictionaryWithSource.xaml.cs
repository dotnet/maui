using System;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[Collection("Xaml Inflation feature")]
public partial class ResourceDictionaryWithSource : ContentPage
{
	public ResourceDictionaryWithSource() => InitializeComponent();

	public class Tests : BaseTestFixture
	{
		[Theory]
		[XamlInflatorData]
		internal void RDWithSourceAreFound(XamlInflator inflator)
		{
			var layout = new ResourceDictionaryWithSource(inflator);
			Assert.Equal(Colors.Pink, layout.label.TextColor);
		}

		[Theory]
		[XamlInflatorData]
		internal void RelativeAndAbsoluteURI(XamlInflator inflator)
		{
			var layout = new ResourceDictionaryWithSource(inflator);
			Assert.Equal(new Uri("./SharedResourceDictionary.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative), ((ResourceDictionary)layout.Resources["relURI"]).Source);
			Assert.IsType<Style>(((ResourceDictionary)layout.Resources["relURI"])["sharedfoo"]);
			Assert.Equal(new Uri("/SharedResourceDictionary.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative), ((ResourceDictionary)layout.Resources["absURI"]).Source);
			Assert.IsType<Style>(((ResourceDictionary)layout.Resources["absURI"])["sharedfoo"]);
			Assert.Equal(new Uri("SharedResourceDictionary.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative), ((ResourceDictionary)layout.Resources["shortURI"]).Source);
			Assert.IsType<Style>(((ResourceDictionary)layout.Resources["shortURI"])["sharedfoo"]);
			Assert.IsType<Color>(((ResourceDictionary)layout.Resources["Colors"])["MediumGrayTextColor"]);
			Assert.IsType<Color>(((ResourceDictionary)layout.Resources["CompiledColors"])["MediumGrayTextColor"]);
		}

		[Theory]
		[XamlInflatorPairData]
		internal void CanLoadInflatedResources(XamlInflator from, XamlInflator rd)
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
			Assert.NotNull(rdLoaded);
			Assert.IsType<Color>(rdLoaded["MediumGrayTextColor"]);
			Assert.Equal(Color.Parse("#ff4d4d4d"), rdLoaded["MediumGrayTextColor"] as Color);			
		}

		[Theory]
		[XamlInflatorData]
		internal void XRIDIsGeneratedForRDWithoutCodeBehind(XamlInflator rd)
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
			Assert.NotNull(resourceId);
			var type = XamlResourceIdAttribute.GetTypeForResourceId(asm, resourceId);
			Assert.StartsWith("__Type", type?.Name, StringComparison.Ordinal);

		}

		[Fact]
		public void CodeBehindIsGeneratedForRDWithXamlComp()
		{
			var asm = typeof(ResourceDictionaryWithSource).Assembly;
			var resourceId = XamlResourceIdAttribute.GetResourceIdForPath(asm, "AppResources/CompiledColors.rtxc.xaml");
			Assert.NotNull(resourceId);
			var type = XamlResourceIdAttribute.GetTypeForResourceId(asm, resourceId);
			Assert.NotNull(type);
			var rd = Activator.CreateInstance(type);
			Assert.NotNull(rd as ResourceDictionary);
		}

		[Theory]
		[XamlInflatorData]
		internal void LoadResourcesWithAssembly(XamlInflator inflator)
		{
			var layout = new ResourceDictionaryWithSource(inflator);
			Assert.Equal(new Uri("/AppResources/Colors.rtxc.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative), ((ResourceDictionary)layout.Resources["inCurrentAssembly"]).Source);
			Assert.IsType<Color>(((ResourceDictionary)layout.Resources["inCurrentAssembly"])["MediumGrayTextColor"]);
			Assert.Equal(new Uri("/AppResources.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests.ExternalAssembly", UriKind.Relative), ((ResourceDictionary)layout.Resources["inOtherAssembly"]).Source);
			Assert.IsType<Color>(((ResourceDictionary)layout.Resources["inOtherAssembly"])["notBlue"]);
		}

	}
}