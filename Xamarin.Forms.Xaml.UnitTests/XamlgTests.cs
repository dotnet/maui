using System;
using NUnit.Framework;
using System.IO;
using System.CodeDom;
using Xamarin.Forms.Build.Tasks;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[TestFixture]
	public class XamlgTests : BaseTestFixture
	{
		[Test]
		public void LoadXaml2006 ()
		{
			var xaml = @"<View
					xmlns=""http://xamarin.com/schemas/2014/forms""
					xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
					x:Class=""Xamarin.Forms.Xaml.UnitTests.CustomView"" >
						<Label x:Name=""label0""/>
					</View>";

			var reader = new StringReader (xaml);

			var generator = new XamlGenerator();
			generator.ParseXaml(reader);
			Assert.NotNull(generator.RootType);
			Assert.NotNull(generator.RootClrNamespace);
			Assert.NotNull (generator.BaseType);
			Assert.NotNull (generator.NamedFields);

			Assert.AreEqual ("CustomView", generator.RootType);
			Assert.AreEqual ("Xamarin.Forms.Xaml.UnitTests", generator.RootClrNamespace);
			Assert.AreEqual ("Xamarin.Forms.View", generator.BaseType.BaseType);
			Assert.AreEqual (1, generator.NamedFields.Count());
			Assert.AreEqual ("label0", generator.NamedFields.First().Name);
			Assert.AreEqual ("Xamarin.Forms.Label", generator.NamedFields.First().Type.BaseType);
		}

		[Test]
		public void LoadXaml2009 ()
		{
			var xaml = @"<View
					xmlns=""http://xamarin.com/schemas/2014/forms""
					xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
					x:Class=""Xamarin.Forms.Xaml.UnitTests.CustomView"" >
						<Label x:Name=""label0""/>
					</View>";

			var reader = new StringReader (xaml);

			var generator = new XamlGenerator();
			generator.ParseXaml(reader);
			Assert.NotNull(generator.RootType);
			Assert.NotNull(generator.RootClrNamespace);
			Assert.NotNull(generator.BaseType);
			Assert.NotNull(generator.NamedFields);

			Assert.AreEqual("CustomView", generator.RootType);
			Assert.AreEqual("Xamarin.Forms.Xaml.UnitTests", generator.RootClrNamespace);
			Assert.AreEqual("Xamarin.Forms.View", generator.BaseType.BaseType);
			Assert.AreEqual(1, generator.NamedFields.Count());
			Assert.AreEqual("label0", generator.NamedFields.First().Name);
			Assert.AreEqual("Xamarin.Forms.Label", generator.NamedFields.First().Type.BaseType);
		}

		[Test]
		//https://github.com/xamarin/Duplo/issues/1207#issuecomment-47159917
		public void xNameInCustomTypes ()
		{
			var xaml = @"<ContentPage
    xmlns=""http://xamarin.com/schemas/2014/forms""
    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
    xmlns:local=""clr-namespace:CustomListViewSample;assembly=CustomListViewSample""
    x:Class=""CustomListViewSample.TestPage"">
    <StackLayout 
        VerticalOptions=""CenterAndExpand""
        HorizontalOptions=""CenterAndExpand"">
        <Label Text=""Hello, Custom Renderer!"" />
        <local:CustomListView x:Name=""listView""
            WidthRequest=""960"" CornerRadius=""50"" OutlineColor=""Blue"" />
    </StackLayout>
</ContentPage>";

			var reader = new StringReader (xaml);

			var generator = new XamlGenerator();
			generator.ParseXaml(reader);

			Assert.AreEqual (1, generator.NamedFields.Count());
			Assert.AreEqual ("listView", generator.NamedFields.First ().Name);
			Assert.AreEqual ("CustomListViewSample.CustomListView", generator.NamedFields.First ().Type.BaseType);
		}

		[Test]
		public void xNameInDataTemplates ()
		{
			var xaml = @"<StackLayout 
						    xmlns=""http://xamarin.com/schemas/2014/forms""
						    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
							x:Class=""FooBar"" >
							<ListView>
								<ListView.ItemTemplate>
									<DataTemplate>
										<ViewCell>
											<Label x:Name=""notincluded""/>
										</ViewCell>
									</DataTemplate>
								</ListView.ItemTemplate>
							</ListView>
							<Label x:Name=""included""/>
						</StackLayout>";
			var reader = new StringReader (xaml);

			var generator = new XamlGenerator();
			generator.ParseXaml(reader);

			Assert.Contains ("included", generator.NamedFields.Select(cmf => cmf.Name).ToList());
			Assert.False (generator.NamedFields.Select(cmf => cmf.Name).Contains ("notincluded"));
			Assert.AreEqual (1, generator.NamedFields.Count());
		}

		[Test]
		public void xNameInStyles ()
		{
			var xaml = @"<StackLayout 
						    xmlns=""http://xamarin.com/schemas/2014/forms""
						    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
							x:Class=""FooBar"" >
							<StackLayout.Resources>
								<ResourceDictionary>
									<Style TargetType=""Label"" >
										<Setter Property=""Text"">
											<Setter.Value>
												<Label x:Name=""notincluded"" />
											</Setter.Value>
										</Setter>
									</Style>
								</ResourceDictionary>
							</StackLayout.Resources>
						</StackLayout>";
			var reader = new StringReader (xaml);

			var generator = new XamlGenerator();
			generator.ParseXaml(reader);

			Assert.False (generator.NamedFields.Select(cmf => cmf.Name).Contains ("notincluded"));
			Assert.AreEqual (0, generator.NamedFields.Count());
		}

		[Test]
		public void xTypeArgumentsOnRootElement ()
		{
			var xaml = @"<Foo 
						    xmlns=""http://xamarin.com/schemas/2014/forms""
						    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
							x:Class=""FooBar"" 
							x:TypeArguments=""x:String""
			/>";
			var reader = new StringReader (xaml);

			var generator = new XamlGenerator();
			generator.ParseXaml(reader);

			Assert.AreEqual("FooBar", generator.RootType);
			Assert.AreEqual ("Xamarin.Forms.Foo`1", generator.BaseType.BaseType);
			Assert.AreEqual (1, generator.BaseType.TypeArguments.Count);
			Assert.AreEqual ("System.String", generator.BaseType.TypeArguments [0].BaseType);
		}

		[Test]
		public void MulipleXTypeArgumentsOnRootElement ()
		{
			var xaml = @"<Foo 
						    xmlns=""http://xamarin.com/schemas/2014/forms""
						    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
							x:Class=""FooBar"" 
							x:TypeArguments=""x:String,x:Int32""
			/>";
			var reader = new StringReader (xaml);

			var generator = new XamlGenerator();
			generator.ParseXaml(reader);

			Assert.AreEqual ("FooBar", generator.RootType);
			Assert.AreEqual ("Xamarin.Forms.Foo`2", generator.BaseType.BaseType);
			Assert.AreEqual (2, generator.BaseType.TypeArguments.Count);
			Assert.AreEqual ("System.String", generator.BaseType.TypeArguments [0].BaseType);
			Assert.AreEqual ("System.Int32", generator.BaseType.TypeArguments [1].BaseType);
		}

		[Test]
		public void MulipleXTypeArgumentsOnRootElementWithWhitespace ()
		{
			var xaml = @"<Foo 
						    xmlns=""http://xamarin.com/schemas/2014/forms""
						    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
							x:Class=""FooBar"" 
							x:TypeArguments=""x:String, x:Int32""
			/>";
			var reader = new StringReader (xaml);

			var generator = new XamlGenerator();
			generator.ParseXaml(reader);

			Assert.AreEqual ("FooBar", generator.RootType);
			Assert.AreEqual ("Xamarin.Forms.Foo`2", generator.BaseType.BaseType);
			Assert.AreEqual (2, generator.BaseType.TypeArguments.Count);
			Assert.AreEqual ("System.String", generator.BaseType.TypeArguments [0].BaseType);
			Assert.AreEqual ("System.Int32", generator.BaseType.TypeArguments [1].BaseType);
		}

		[Test]
		public void MulipleXTypeArgumentsMulitpleNamespacesOnRootElement ()
		{
			var xaml = @"<Foo 
						    xmlns=""http://xamarin.com/schemas/2014/forms""
						    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
							x:Class=""FooBar"" 
							x:TypeArguments=""nsone:IDummyInterface,nstwo:IDummyInterfaceTwo""
							xmlns:nsone=""clr-namespace:Xamarin.Forms.Xaml.UnitTests.Bugzilla24258.Interfaces""
							xmlns:nstwo=""clr-namespace:Xamarin.Forms.Xaml.UnitTests.Bugzilla24258.InterfacesTwo""

			/>";
			var reader = new StringReader (xaml);

			var generator = new XamlGenerator();
			generator.ParseXaml(reader);

			Assert.AreEqual ("FooBar", generator.RootType);
			Assert.AreEqual ("Xamarin.Forms.Foo`2", generator.BaseType.BaseType);
			Assert.AreEqual (2, generator.BaseType.TypeArguments.Count);
			Assert.AreEqual ("Xamarin.Forms.Xaml.UnitTests.Bugzilla24258.Interfaces.IDummyInterface", generator.BaseType.TypeArguments [0].BaseType);
			Assert.AreEqual ("Xamarin.Forms.Xaml.UnitTests.Bugzilla24258.InterfacesTwo.IDummyInterfaceTwo", generator.BaseType.TypeArguments [1].BaseType);
		}

		[Test]
		public void MulipleXTypeArgumentsMulitpleNamespacesOnRootElementWithWhitespace ()
		{
			var xaml = @"<Foo 
						    xmlns=""http://xamarin.com/schemas/2014/forms""
						    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
							x:Class=""FooBar"" 
							x:TypeArguments=""nsone:IDummyInterface, nstwo:IDummyInterfaceTwo""
							xmlns:nsone=""clr-namespace:Xamarin.Forms.Xaml.UnitTests.Bugzilla24258.Interfaces""
							xmlns:nstwo=""clr-namespace:Xamarin.Forms.Xaml.UnitTests.Bugzilla24258.InterfacesTwo""

			/>";
			var reader = new StringReader (xaml);

			var generator = new XamlGenerator();
			generator.ParseXaml(reader);

			Assert.AreEqual ("FooBar", generator.RootType);
			Assert.AreEqual ("Xamarin.Forms.Foo`2", generator.BaseType.BaseType);
			Assert.AreEqual (2, generator.BaseType.TypeArguments.Count);
			Assert.AreEqual ("Xamarin.Forms.Xaml.UnitTests.Bugzilla24258.Interfaces.IDummyInterface", generator.BaseType.TypeArguments [0].BaseType);
			Assert.AreEqual ("Xamarin.Forms.Xaml.UnitTests.Bugzilla24258.InterfacesTwo.IDummyInterfaceTwo", generator.BaseType.TypeArguments [1].BaseType);
		}

		[Test]
		//https://bugzilla.xamarin.com/show_bug.cgi?id=33256
		public void AlwaysUseGlobalReference ()
		{
			var xaml = @"
			<ContentPage
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
				x:Class=""FooBar"" >
				<Label x:Name=""label0""/>
			</ContentPage>";
			using (var reader = new StringReader (xaml)) {

				var generator = new XamlGenerator();
				generator.ParseXaml(reader);

				Assert.IsTrue (generator.BaseType.Options.HasFlag (CodeTypeReferenceOptions.GlobalReference));
				Assert.IsTrue (generator.NamedFields.Select(cmf => cmf.Type).First ().Options.HasFlag (CodeTypeReferenceOptions.GlobalReference));
			}
		}

		[Test]
		public void FieldModifier()
		{
			var xaml = @"
			<ContentPage xmlns=""http://xamarin.com/schemas/2014/forms""
			             xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
			             xmlns:local=""clr-namespace:Xamarin.Forms.Xaml.UnitTests""
			             x:Class=""Xamarin.Forms.Xaml.UnitTests.FieldModifier"">
				<StackLayout>
			        <Label x:Name=""privateLabel"" />
			        <Label x:Name=""internalLabel"" x:FieldModifier=""NotPublic"" />
			        <Label x:Name=""publicLabel"" x:FieldModifier=""Public"" />
				</StackLayout>
			</ContentPage>";

			using (var reader = new StringReader(xaml))
			{

				var generator = new XamlGenerator();
				generator.ParseXaml(reader);

				Assert.That(generator.NamedFields.First(cmf => cmf.Name == "privateLabel").Attributes, Is.EqualTo(MemberAttributes.Private));
				Assert.That(generator.NamedFields.First(cmf => cmf.Name == "internalLabel").Attributes, Is.EqualTo(MemberAttributes.Assembly));
				Assert.That(generator.NamedFields.First(cmf => cmf.Name == "publicLabel").Attributes, Is.EqualTo(MemberAttributes.Public));
			}
		}

		[Test]
		//https://github.com/xamarin/Xamarin.Forms/issues/2574
		public void xNameOnRoot()
		{
			var xaml = @"<ContentPage
		xmlns=""http://xamarin.com/schemas/2014/forms""
		xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
		x:Class=""Foo""
		x:Name=""bar"">
	</ContentPage>";

			var generator = new XamlGenerator();
			generator.ParseXaml(new StringReader(xaml));

			Assert.AreEqual(1, generator.NamedFields.Count());
			Assert.AreEqual("bar", generator.NamedFields.First().Name);
			Assert.AreEqual("Xamarin.Forms.ContentPage", generator.NamedFields.First().Type.BaseType);
		}
	}
}