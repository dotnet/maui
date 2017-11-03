using System;
using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
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
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

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
				Assert.That(((ResourceDictionary)layout.Resources["relURI"]).Source, Is.EqualTo(new Uri("./SharedResourceDictionary.xaml", UriKind.Relative)));
				Assert.That(((ResourceDictionary)layout.Resources["relURI"])["sharedfoo"], Is.TypeOf<Style>());
				Assert.That(((ResourceDictionary)layout.Resources["absURI"]).Source, Is.EqualTo(new Uri("/SharedResourceDictionary.xaml", UriKind.Relative)));
				Assert.That(((ResourceDictionary)layout.Resources["absURI"])["sharedfoo"], Is.TypeOf<Style>());
				Assert.That(((ResourceDictionary)layout.Resources["shortURI"]).Source, Is.EqualTo(new Uri("SharedResourceDictionary.xaml", UriKind.Relative)));
				Assert.That(((ResourceDictionary)layout.Resources["shortURI"])["sharedfoo"], Is.TypeOf<Style>());
			}
		}
	}
}