using System;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class ResourceLoader : ContentPage
	{
		public ResourceLoader()
		{
			InitializeComponent();
		}

		public ResourceLoader(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
				Xamarin.Forms.Internals.ResourceLoader.ResourceProvider = null;
			}

			[TestCase(false), TestCase(true)]
			public void XamlLoadingUsesResourceLoader(bool useCompiledXaml)
			{
				var layout = new ResourceLoader(useCompiledXaml);
				Assert.That(layout.label.TextColor, Is.EqualTo(Color.FromHex("#368F95")));

				Xamarin.Forms.Internals.ResourceLoader.ResourceProvider = (asmName, path) => {
					if (path == "ResourceLoader.xaml")
						return @"
<ContentPage 
	xmlns=""http://xamarin.com/schemas/2014/forms""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	x:Class=""Xamarin.Forms.Xaml.UnitTests.ResourceLoader"" >
  
	<Label x:Name = ""label"" TextColor = ""Pink"" />
</ContentPage >";
					return null;
				};
				layout = new ResourceLoader(useCompiledXaml);
				Assert.That(layout.label.TextColor, Is.EqualTo(Color.Pink));

			}

			[TestCase(false), TestCase(true)]
			public void RDLoadingUsesResourceLoader(bool useCompiledXaml)
			{
				var layout = new ResourceLoader(useCompiledXaml);
				Assert.That(layout.label.TextColor, Is.EqualTo(Color.FromHex("#368F95")));

				Xamarin.Forms.Internals.ResourceLoader.ResourceProvider = (asmName, path) => {
					if (path == "AppResources/Colors.xaml")
						return @"
<ResourceDictionary
	xmlns=""http://xamarin.com/schemas/2014/forms""
	xmlns:x = ""http://schemas.microsoft.com/winfx/2009/xaml"" >
	<Color x:Key = ""GreenColor"" >#36FF95</Color>
</ResourceDictionary >";
					return null;
				};
				layout = new ResourceLoader(useCompiledXaml);

				Assert.That(layout.label.TextColor, Is.EqualTo(Color.FromHex("#36FF95")));
			}
		}
	}
}
