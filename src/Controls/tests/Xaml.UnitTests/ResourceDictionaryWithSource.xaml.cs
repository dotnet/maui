using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class ResourceDictionaryWithSource : ContentPage
{
	public ResourceDictionaryWithSource() => InitializeComponent();

	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}
		[Theory]
		[Values]
		public void RDWithSourceAreFound(XamlInflator inflator)
		{
			var layout = new ResourceDictionaryWithSource(inflator);
			Assert.Equal(Colors.Pink, layout.label.TextColor);
		}

		[Theory]
		[Values]
		public void RelativeAndAbsoluteURI(XamlInflator inflator)
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
		[Values]
		public void CanLoadInflatedResources(XamlInflator rd)
		{
			var layout = new ResourceDictionaryWithSource(rd);
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
		[Values]
		public void XRIDIsGeneratedForRDWithoutCodeBehind(XamlInflator rd)
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
			Assert.StartsWith("__Type", type?.Name);

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
		[Values]
		public void LoadResourcesWithAssembly(XamlInflator inflator)
		{
			var layout = new ResourceDictionaryWithSource(inflator);
			Assert.Equal(new Uri("/AppResources/Colors.rtxc.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", UriKind.Relative), ((ResourceDictionary)layout.Resources["inCurrentAssembly"]).Source);
			Assert.IsType<Color>(((ResourceDictionary)layout.Resources["inCurrentAssembly"])["MediumGrayTextColor"]);
			Assert.Equal(new Uri("/AppResources.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests.ExternalAssembly", UriKind.Relative), ((ResourceDictionary)layout.Resources["inOtherAssembly"]).Source);
			Assert.IsType<Color>(((ResourceDictionary)layout.Resources["inOtherAssembly"])["notBlue"]);
		}

	}
}