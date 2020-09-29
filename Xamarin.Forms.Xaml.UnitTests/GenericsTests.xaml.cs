using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
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
		}

		[TestFixture]
		public class Tests
		{
			[Test]
			public void NoGenericsOnXaml2006()
			{
				var xaml = @"
				<ContentPage 
				xmlns=""http://xamarin.com/schemas/2014/forms""
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

			[TestCase(false)]
			[TestCase(true)]
			public void GenericSupportOnXaml2009(bool useCompiledXaml)
			{
				var layout = new GenericsTests(useCompiledXaml);
				Assert.True(layout.Resources.ContainsKey("genericButtonList"));
				var list = layout.Resources["genericButtonList"];
				Assert.That(list, Is.TypeOf<List<Button>>());
				Assert.AreEqual(2, ((List<Button>)list).Count);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void FindGenericByName(bool useCompiledXaml)
			{
				var layout = new GenericsTests(useCompiledXaml);
				var list = layout.FindByName<List<Button>>("myList");
				Assert.That(list, Is.Not.Null);
				Assert.That(list, Is.TypeOf<List<Button>>());

				var nestedGenericList = layout.TestListMember;
				Assert.That(nestedGenericList, Is.Not.Null);
				Assert.That(nestedGenericList, Is.TypeOf<List<KeyValuePair<string, string>>>());

				Assert.That(nestedGenericList.Count, Is.EqualTo(1));
			}

			[TestCase(false)]
			[TestCase(true)]
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

			[TestCase(false)]
			[TestCase(true)]
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