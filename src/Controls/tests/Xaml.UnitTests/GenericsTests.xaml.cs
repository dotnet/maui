using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class GenericsTests : ContentPage
	{
		public List<string> P { get; set; }

		public GenericsTests()
		{
			InitializeComponent();
		}

		public GenericsTests(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class Tests
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
				Assert.Throws(new XamlParseExceptionConstraint(8, 9), () => new ContentPage().LoadFromXaml(xaml));
			}

			[Theory]
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void GenericSupportOnXaml2009(bool useCompiledXaml)
			{
				var layout = new GenericsTests(useCompiledXaml);
				Assert.True(layout.Resources.ContainsKey("genericButtonList"));
				var list = layout.Resources["genericButtonList"];
				Assert.That(list, Is.TypeOf<List<Button>>());
				Assert.Equal(2, ((List<Button>)list).Count);
			}

			[Theory]
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void FindGenericByName(bool useCompiledXaml)
			{
				var layout = new GenericsTests(useCompiledXaml);
				var list = layout.FindByName<List<Button>>("myList");
				Assert.NotNull(list);
				Assert.That(list, Is.TypeOf<List<Button>>());

				var nestedGenericList = layout.TestListMember;
				Assert.NotNull(nestedGenericList);
				Assert.That(nestedGenericList, Is.TypeOf<List<KeyValuePair<string, string>>>());

				Assert.Equal(1, nestedGenericList.Count);
			}

			[Theory]
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void TestGenericParsing(bool useCompiledXaml)
			{
				var layout = new GenericsTests(useCompiledXaml);

				Assert.NotNull(layout.P);

				var list = layout.Resources["list"];
				Assert.NotNull(list);
				Assert.That(list, Is.TypeOf<List<String>>());

				var dict = layout.Resources["dict"];
				Assert.NotNull(dict);
				Assert.That(dict, Is.TypeOf<Dictionary<string, string>>());

				var queue = layout.Resources["genericsquaredlist"];
				Assert.NotNull(dict);
				Assert.That(queue, Is.TypeOf<List<KeyValuePair<string, string>>>());
			}

			[Theory]
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void TestXamlPrimitives(bool useCompiledXaml)
			{
				var layout = new GenericsTests(useCompiledXaml);
				var list = layout.Resources["stringList"];
				Assert.NotNull(list);
				Assert.That(list, Is.TypeOf<List<String>>());
			}
		}
	}
}