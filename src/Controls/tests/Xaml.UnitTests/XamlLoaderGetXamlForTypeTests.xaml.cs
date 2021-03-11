using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class XamlLoaderGetXamlForTypeTests : ContentPage
	{
		public XamlLoaderGetXamlForTypeTests()
		{
			InitializeComponent();
		}

		public XamlLoaderGetXamlForTypeTests(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void SetUp()
			{
#pragma warning disable 0618
				Microsoft.Maui.Controls.Xaml.Internals.XamlLoader.XamlFileProvider = null;
#pragma warning restore 0618
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
				XamlLoader.FallbackTypeResolver = null;
				XamlLoader.ValueCreatedCallback = null;
				XamlLoader.InstantiationFailedCallback = null;
#pragma warning disable 0618
				Microsoft.Maui.Controls.Internals.ResourceLoader.ExceptionHandler = null;
				Microsoft.Maui.Controls.Xaml.Internals.XamlLoader.DoNotThrowOnExceptions = false;
#pragma warning restore 0618

			}

			[TestCase(false)]
			[TestCase(true)]
			public void XamlContentIsReplaced(bool useCompiledXaml)
			{
				var layout = new XamlLoaderGetXamlForTypeTests(useCompiledXaml);
				Assert.That(layout.Content, Is.TypeOf<Button>());

#pragma warning disable 0618
				Microsoft.Maui.Controls.Xaml.Internals.XamlLoader.XamlFileProvider = (t) =>
				{
#pragma warning restore 0618
					if (t == typeof(XamlLoaderGetXamlForTypeTests))
						return @"
	<ContentPage xmlns=""http://xamarin.com/schemas/2014/forms""
		xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
		x:Class=""Microsoft.Maui.Controls.Xaml.UnitTests.XamlLoaderGetXamlForTypeTests"">
		<Label x:Name=""Label""/>
	</ContentPage>";
					return null;
				};

				layout = new XamlLoaderGetXamlForTypeTests(useCompiledXaml);
				Assert.That(layout.Content, Is.TypeOf<Label>());
			}
		}
	}
}

