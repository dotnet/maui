using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
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
#pragma warning disable CS0618 // Type or member is obsolete
				Microsoft.Maui.Controls.Internals.ResourceLoader.ResourceProvider = null;
#pragma warning restore CS0618 // Type or member is obsolete
			}

			[TestCase(false), TestCase(true)]
			public void XamlLoadingUsesResourceLoader(bool useCompiledXaml)
			{
				var layout = new ResourceLoader(useCompiledXaml);
				Assert.That(layout.label.TextColor, Is.EqualTo(Color.FromHex("#368F95")));

#pragma warning disable CS0618 // Type or member is obsolete
				Microsoft.Maui.Controls.Internals.ResourceLoader.ResourceProvider = (asmName, path) =>
				{
#pragma warning restore CS0618 // Type or member is obsolete
					if (path == "ResourceLoader.xaml")
						return @"
<ContentPage 
	xmlns=""http://xamarin.com/schemas/2014/forms""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	x:Class=""Microsoft.Maui.Controls.Xaml.UnitTests.ResourceLoader"" >
  
	<Label x:Name = ""label"" TextColor = ""Pink"" />
</ContentPage >";
					return null;
				};
				layout = new ResourceLoader(useCompiledXaml);
				Assert.That(layout.label.TextColor, Is.EqualTo(Color.Pink));
			}

			[Test]
			public void XamlLoadingUsesResourceProvider2([Values(false, true)] bool useCompiledXaml)
			{
				var layout = new ResourceLoader(useCompiledXaml);
				Assert.That(layout.label.TextColor, Is.EqualTo(Color.FromHex("#368F95")));
				object instance = null;
				Microsoft.Maui.Controls.Internals.ResourceLoader.ResourceProvider2 = (rlq) =>
				{
					if (rlq.ResourcePath == "ResourceLoader.xaml")
					{
						instance = rlq.Instance;
						return new Maui.Controls.Internals.ResourceLoader.ResourceLoadingResponse
						{
							ResourceContent = @"
<ContentPage 
	xmlns=""http://xamarin.com/schemas/2014/forms""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	x:Class=""Microsoft.Maui.Controls.Xaml.UnitTests.ResourceLoader""
	xmlns:d=""http://xamarin.com/schemas/2014/forms/design""
	xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006""
	mc:Ignorable=""d"" >
  
	<Label x:Name = ""label"" TextColor = ""Pink"" d:TextColor = ""HotPink"" />
</ContentPage >"
						};
					}
					return null;
				};


				layout = new ResourceLoader(useCompiledXaml);
				Assert.That(instance, Is.EqualTo(layout));
				Assert.That(layout.label.TextColor, Is.EqualTo(Color.Pink));
			}

			[Test]
			public void XamlLoadingUsesResourceProvider2WithDesignProperties([Values(false, true)] bool useCompiledXaml)
			{
				var layout = new ResourceLoader(useCompiledXaml);
				Assert.That(layout.label.TextColor, Is.EqualTo(Color.FromHex("#368F95")));

				Microsoft.Maui.Controls.Internals.ResourceLoader.ResourceProvider2 = (rlq) =>
				{
					if (rlq.ResourcePath == "ResourceLoader.xaml")
						return new Maui.Controls.Internals.ResourceLoader.ResourceLoadingResponse
						{
							UseDesignProperties = true,
							ResourceContent = @"
<ContentPage 
	xmlns=""http://xamarin.com/schemas/2014/forms""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	x:Class=""Microsoft.Maui.Controls.Xaml.UnitTests.ResourceLoader""
	xmlns:d=""http://xamarin.com/schemas/2014/forms/design""
	xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006""
	mc:Ignorable=""d"" >
  
	<Label x:Name = ""label"" TextColor = ""Pink"" d:TextColor = ""HotPink"" />
</ContentPage >"
						};
					return null;
				};


				layout = new ResourceLoader(useCompiledXaml);
				Assert.That(layout.label.TextColor, Is.EqualTo(Color.HotPink));
			}

			[TestCase(false), TestCase(true)]
			public void RDLoadingUsesResourceLoader(bool useCompiledXaml)
			{
				var layout = new ResourceLoader(useCompiledXaml);
				Assert.That(layout.label.TextColor, Is.EqualTo(Color.FromHex("#368F95")));

#pragma warning disable CS0618 // Type or member is obsolete
				Microsoft.Maui.Controls.Internals.ResourceLoader.ResourceProvider = (asmName, path) =>
				{
#pragma warning restore CS0618 // Type or member is obsolete
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
