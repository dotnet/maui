using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class DefaultCtorRouting2 : ContentPage
	{
		[TypeConverter(typeof(IsCompiledTypeConverter))]
		public bool IsCompiled { get; set; }

		public DefaultCtorRouting2()
		{
			InitializeComponent();
		}

		[TestFixture]
		class Tests
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
#pragma warning disable 0618
				Internals.XamlLoader.XamlFileProvider = null;
#pragma warning restore 0618

			}

			[Test]
			public void ShouldBeCompiled()
			{
				var p = new DefaultCtorRouting2();
				Assert.True(p.IsCompiled);
			}

			[Test]
			public void ShouldntBeCompiled()
			{
#pragma warning disable 0618
				Internals.XamlLoader.XamlFileProvider = (t) =>
				{
#pragma warning restore 0618
					if (t == typeof(DefaultCtorRouting2))
						return @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ContentPage xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	x:Class=""Microsoft.Maui.Controls.Xaml.UnitTests.DefaultCtorRouting2""
	IsCompiled=""IsCompiled?"">
</ContentPage>";
					return null;
				};
				var p = new DefaultCtorRouting2();
				Assert.False(p.IsCompiled);
			}
		}
	}
}