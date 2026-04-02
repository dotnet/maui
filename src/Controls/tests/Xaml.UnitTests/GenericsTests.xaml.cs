using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class GenericsTests : ContentPage
{
	public List<string> P { get; set; }

	public GenericsTests() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Fact]
		public void NoGenericsOnXaml2006()
		{
			var xaml = @"
				<ContentPage 
				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
				xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
				xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib"">
					<ContentPage.Resources>
						<ResourceDictionary>
							<scg:List x:TypeArguments=""Button"" x:Key=""genericList""/>
						</ResourceDictionary>
					</ContentPage.Resources>
				</ContentPage>";
			var ex = Assert.Throws<XamlParseException>(() => new ContentPage().LoadFromXaml(xaml));
			Assert.Contains("8", ex.XmlInfo.LineNumber.ToString(), StringComparison.Ordinal);
		}

		[Theory]
		[XamlInflatorData]
		internal void GenericSupportOnXaml2009(XamlInflator inflator)
		{
			var layout = new GenericsTests(inflator);
			Assert.True(layout.Resources.ContainsKey("genericButtonList"));
			var list = layout.Resources["genericButtonList"];
			Assert.IsType<List<Button>>(list);
			Assert.Equal(2, ((List<Button>)list).Count);
		}

		[Theory]
		[XamlInflatorData]
		internal void FindGenericByName(XamlInflator inflator)
		{
			var layout = new GenericsTests(inflator);
			var list = layout.FindByName<List<Button>>("myList");
			Assert.NotNull(list);
			Assert.IsType<List<Button>>(list);

			var nestedGenericList = layout.TestListMember;
			Assert.NotNull(nestedGenericList);
			Assert.IsType<List<KeyValuePair<string, string>>>(nestedGenericList);

			Assert.Single(nestedGenericList);
		}

		[Theory]
		[XamlInflatorData]
		internal void TestGenericParsing(XamlInflator inflator)
		{
			var layout = new GenericsTests(inflator);

			Assert.NotNull(layout.P);

			var list = layout.Resources["list"];
			Assert.NotNull(list);
			Assert.IsType<List<String>>(list);

			var dict = layout.Resources["dict"];
			Assert.NotNull(dict);
			Assert.IsType<Dictionary<string, string>>(dict);

			var queue = layout.Resources["genericsquaredlist"];
			Assert.NotNull(dict);
			Assert.IsType<List<KeyValuePair<string, string>>>(queue);
		}

		[Theory]
		[XamlInflatorData]
		internal void TestXamlPrimitives(XamlInflator inflator)
		{
			var layout = new GenericsTests(inflator);
			var list = layout.Resources["stringList"];
			Assert.NotNull(list);
			Assert.IsType<List<String>>(list);
		}
	}
}