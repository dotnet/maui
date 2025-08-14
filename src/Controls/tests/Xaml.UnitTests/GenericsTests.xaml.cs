using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class GenericsTests : ContentPage
{
	public List<string> P { get; set; }

	public GenericsTests() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
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

		[Test]
		public void GenericSupportOnXaml2009([Values] XamlInflator inflator)
		{
			var layout = new GenericsTests(inflator);
			Assert.True(layout.Resources.ContainsKey("genericButtonList"));
			var list = layout.Resources["genericButtonList"];
			Assert.That(list, Is.TypeOf<List<Button>>());
			Assert.AreEqual(2, ((List<Button>)list).Count);
		}

		[Test]
		public void FindGenericByName([Values] XamlInflator inflator)
		{
			var layout = new GenericsTests(inflator);
			var list = layout.FindByName<List<Button>>("myList");
			Assert.That(list, Is.Not.Null);
			Assert.That(list, Is.TypeOf<List<Button>>());

			var nestedGenericList = layout.TestListMember;
			Assert.That(nestedGenericList, Is.Not.Null);
			Assert.That(nestedGenericList, Is.TypeOf<List<KeyValuePair<string, string>>>());

			Assert.That(nestedGenericList.Count, Is.EqualTo(1));
		}

		[Test]
		public void TestGenericParsing([Values] XamlInflator inflator)
		{
			var layout = new GenericsTests(inflator);

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

		[Test]
		public void TestXamlPrimitives([Values] XamlInflator inflator)
		{
			var layout = new GenericsTests(inflator);
			var list = layout.Resources["stringList"];
			Assert.NotNull(list);
			Assert.That(list, Is.TypeOf<List<String>>());
		}
	}
}