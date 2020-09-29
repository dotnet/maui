using System;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public partial class TypeConverterTests : ContentPage
	{
		public TypeConverterTests()
		{
			InitializeComponent();
		}

		public TypeConverterTests(bool useCompiledXaml)
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

			[TestCase(false)]
			[TestCase(true)]
			public void UriAreConverted(bool useCompiledXaml)
			{
				var layout = new TypeConverterTests(useCompiledXaml);
				Assert.That(layout.imageSource.Uri, Is.TypeOf<Uri>());
				Assert.AreEqual("https://xamarin.com/content/images/pages/branding/assets/xamagon.png", layout.imageSource.Uri.ToString());
			}
		}
	}
}