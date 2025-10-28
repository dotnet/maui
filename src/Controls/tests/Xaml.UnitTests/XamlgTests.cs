//using System.CodeDom;
//using System.IO;
//using System.Linq;
//using Microsoft.Build.Framework;
//using Microsoft.Maui.Controls.Build.Tasks;
//using Microsoft.Maui.Controls.Core.UnitTests;
//using Xunit;
//using IOPath = System.IO.Path;
//
//namespace Microsoft.Maui.Controls.MSBuild.UnitTests
//{
////	public class XamlgTests : BaseTestFixture
//	{
//		[Fact]
//		public void LoadXaml2006()
//		{
//			var xaml = @"<View
//					xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
//					xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
//					x:Class=""Microsoft.Maui.Controls.Xaml.UnitTests.CustomView"" >
//						<Label x:Name=""label0""/>
//					</View>";
//
//			var reader = new StringReader(xaml);
//
//			var references = string.Join(";",
//					IOPath.GetFullPath(
//						IOPath.Combine(
//							AppContext.BaseDirectory, "Microsoft.Maui.dll")),
//					IOPath.GetFullPath(
//						IOPath.Combine(
//							AppContext.BaseDirectory, "Microsoft.Maui.Controls.dll"))
//					);
//
//			var generator = new XamlGenerator(null, null, null, null, null, null, references);
//			generator.ParseXaml(reader);
//			Assert.NotNull(generator.RootType);
//			Assert.NotNull(generator.RootClrNamespace);
//			Assert.NotNull(generator.BaseType);
//			Assert.NotNull(generator.NamedFields);
//
//			Assert.Equal("CustomView", generator.RootType);
//			Assert.Equal("Microsoft.Maui.Controls.Xaml.UnitTests", generator.RootClrNamespace);
//			Assert.Equal("Microsoft.Maui.Controls.View", generator.BaseType.BaseType);
//			Assert.Equal(1, generator.NamedFields.Count());
//			Assert.Equal("label0", generator.NamedFields.First().Name);
//			Assert.Equal("Microsoft.Maui.Controls.Label", generator.NamedFields.First().Type.BaseType);
//		}
//
//		[Fact]
//		public void LoadXaml2009()
//		{
//			var xaml = @"<View
//					xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
//					xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
//					x:Class=""Microsoft.Maui.Controls.Xaml.UnitTests.CustomView"" >
//						<Label x:Name=""label0""/>
//					</View>";
//
//			var reader = new StringReader(xaml);
//
//			var references = string.Join(";",
//								IOPath.GetFullPath(
//									IOPath.Combine(
//										AppContext.BaseDirectory, "Microsoft.Maui.dll")),
//								IOPath.GetFullPath(
//									IOPath.Combine(
//										AppContext.BaseDirectory, "Microsoft.Maui.Controls.dll"))
//								);
//
//			var generator = new XamlGenerator(null, null, null, null, null, null, references);
//			generator.ParseXaml(reader);
//			Assert.NotNull(generator.RootType);
//			Assert.NotNull(generator.RootClrNamespace);
//			Assert.NotNull(generator.BaseType);
//			Assert.NotNull(generator.NamedFields);
//
//			Assert.Equal("CustomView", generator.RootType);
//			Assert.Equal("Microsoft.Maui.Controls.Xaml.UnitTests", generator.RootClrNamespace);
//			Assert.Equal("Microsoft.Maui.Controls.View", generator.BaseType.BaseType);
//			Assert.Equal(1, generator.NamedFields.Count());
//			Assert.Equal("label0", generator.NamedFields.First().Name);
//			Assert.Equal("Microsoft.Maui.Controls.Label", generator.NamedFields.First().Type.BaseType);
//		}
//
//		[Fact]
//		//https://github.com/xamarin/Duplo/issues/1207#issuecomment-47159917
//		public void xNameInCustomTypes()
//		{
//			var xaml = @"<ContentPage
//    xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
//    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
//    xmlns:local=""clr-namespace:CustomListViewSample;assembly=CustomListViewSample""
//    xmlns:localusing=""using:CustomListViewSample""
//    x:Class=""CustomListViewSample.TestPage"">
//    <StackLayout 
//        VerticalOptions=""CenterAndExpand""
//        HorizontalOptions=""CenterAndExpand"">
//        <Label Text=""Hello, Custom Renderer!"" />
//        <local:CustomListView x:Name=""listView""
//            WidthRequest=""960"" CornerRadius=""50"" OutlineColor=""Blue"" />
//		<localusing:CustomListView x:Name=""listViewusing"" />
//    </StackLayout>
//</ContentPage>";
//
//			var reader = new StringReader(xaml);
//
//			var references = string.Join(";",
//								IOPath.GetFullPath(
//									IOPath.Combine(
//										AppContext.BaseDirectory, "Microsoft.Maui.dll")),
//								IOPath.GetFullPath(
//									IOPath.Combine(
//										AppContext.BaseDirectory, "Microsoft.Maui.Controls.dll"))
//								);
//
//			var generator = new XamlGenerator(null, null, null, null, null, null, references);
//			generator.ParseXaml(reader);
//
//			Assert.Equal(2, generator.NamedFields.Count());
//			Assert.Equal("listView", generator.NamedFields.ToArray()[0].Name);
//			Assert.Equal("CustomListViewSample.CustomListView", generator.NamedFields.ToArray()[0].Type.BaseType);
//			Assert.Equal("listViewusing", generator.NamedFields.ToArray()[1].Name);
//			Assert.Equal("CustomListViewSample.CustomListView", generator.NamedFields.ToArray()[1].Type.BaseType);
//		}
//
//		[Fact]
//		public void xNameInDataTemplates()
//		{
//			var xaml = @"<StackLayout 
//						    xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
//						    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
//							x:Class=""FooBar"" >
//							<ListView>
//								<ListView.ItemTemplate>
//									<DataTemplate>
//										<ViewCell>
//											<Label x:Name=""notincluded""/>
//										</ViewCell>
//									</DataTemplate>
//								</ListView.ItemTemplate>
//							</ListView>
//							<Label x:Name=""included""/>
//						</StackLayout>";
//			var reader = new StringReader(xaml);
//
//			var references = string.Join(";",
//								IOPath.GetFullPath(
//									IOPath.Combine(
//										AppContext.BaseDirectory, "Microsoft.Maui.dll")),
//								IOPath.GetFullPath(
//									IOPath.Combine(
//										AppContext.BaseDirectory, "Microsoft.Maui.Controls.dll"))
//								);
//
//			var generator = new XamlGenerator(null, null, null, null, null, null, references);
//			generator.ParseXaml(reader);
//
//			Assert.Contains("included", generator.NamedFields.Select(cmf => cmf.Name).ToList());
//			Assert.False(generator.NamedFields.Select(cmf => cmf.Name).Contains("notincluded"));
//			Assert.Equal(1, generator.NamedFields.Count());
//		}
//
//		[Fact]
//		public void xNameInStyles()
//		{
//			var xaml = @"<StackLayout 
//						    xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
//						    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
//							x:Class=""FooBar"" >
//							<StackLayout.Resources>
//								<ResourceDictionary>
//									<Style TargetType=""Label"" >
//										<Setter Property=""Text"">
//											<Setter.Value>
//												<Label x:Name=""notincluded"" />
//											</Setter.Value>
//										</Setter>
//									</Style>
//								</ResourceDictionary>
//							</StackLayout.Resources>
//						</StackLayout>";
//			var reader = new StringReader(xaml);
//
//			var references = string.Join(";",
//								IOPath.GetFullPath(
//									IOPath.Combine(
//										AppContext.BaseDirectory, "Microsoft.Maui.dll")),
//								IOPath.GetFullPath(
//									IOPath.Combine(
//										AppContext.BaseDirectory, "Microsoft.Maui.Controls.dll"))
//								);
//
//			var generator = new XamlGenerator(null, null, null, null, null, null, references);
//			generator.ParseXaml(reader);
//
//			Assert.False(generator.NamedFields.Select(cmf => cmf.Name).Contains("notincluded"));
//			Assert.Equal(0, generator.NamedFields.Count());
//		}
//
//		[Fact]
//		public void xTypeArgumentsOnRootElement()
//		{
//			var xaml = @"<Layout 
//						    xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
//						    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
//							x:Class=""FooBar"" 
//							x:TypeArguments=""x:String""
//			/>";
//			var reader = new StringReader(xaml);
//
//			var references = string.Join(";",
//								IOPath.GetFullPath(
//									IOPath.Combine(
//										AppContext.BaseDirectory, "Microsoft.Maui.dll")),
//								IOPath.GetFullPath(
//									IOPath.Combine(
//										AppContext.BaseDirectory, "Microsoft.Maui.Controls.dll"))
//								);
//
//			var generator = new XamlGenerator(null, null, null, null, null, null, references);
//			generator.ParseXaml(reader);
//
//			Assert.Equal("FooBar", generator.RootType);
//			Assert.Equal("Microsoft.Maui.Controls.Layout`1", generator.BaseType.BaseType);
//			Assert.Equal(1, generator.BaseType.TypeArguments.Count);
//			Assert.Equal("System.String", generator.BaseType.TypeArguments[0].BaseType);
//		}
//
//		[Fact]
//		public void MulipleXTypeArgumentsOnRootElement()
//		{
//			var xaml = @"<ObservableWrapper 
//						    xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
//						    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
//							x:Class=""FooBar"" 
//							x:TypeArguments=""x:String,x:Int32""
//			/>";
//			var reader = new StringReader(xaml);
//
//			var references = string.Join(";",
//								IOPath.GetFullPath(
//									IOPath.Combine(
//										AppContext.BaseDirectory, "Microsoft.Maui.dll")),
//								IOPath.GetFullPath(
//									IOPath.Combine(
//										AppContext.BaseDirectory, "Microsoft.Maui.Controls.dll"))
//								);
//
//			var generator = new XamlGenerator(null, null, null, null, null, null, references);
//			generator.ParseXaml(reader);
//
//			Assert.Equal("FooBar", generator.RootType);
//			Assert.Equal("Microsoft.Maui.Controls.ObservableWrapper`2", generator.BaseType.BaseType);
//			Assert.Equal(2, generator.BaseType.TypeArguments.Count);
//			Assert.Equal("System.String", generator.BaseType.TypeArguments[0].BaseType);
//			Assert.Equal("System.Int32", generator.BaseType.TypeArguments[1].BaseType);
//		}
//
//		[Fact]
//		public void MulipleXTypeArgumentsOnRootElementWithWhitespace()
//		{
//			var xaml = @"<ObservableWrapper 
//						    xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
//						    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
//							x:Class=""FooBar"" 
//							x:TypeArguments=""x:String, x:Int32""
//			/>";
//			var reader = new StringReader(xaml);
//
//			var references = string.Join(";",
//								IOPath.GetFullPath(
//									IOPath.Combine(
//										AppContext.BaseDirectory, "Microsoft.Maui.dll")),
//								IOPath.GetFullPath(
//									IOPath.Combine(
//										AppContext.BaseDirectory, "Microsoft.Maui.Controls.dll"))
//								);
//
//			var generator = new XamlGenerator(null, null, null, null, null, null, references);
//			generator.ParseXaml(reader);
//
//			Assert.Equal("FooBar", generator.RootType);
//			Assert.Equal("Microsoft.Maui.Controls.ObservableWrapper`2", generator.BaseType.BaseType);
//			Assert.Equal(2, generator.BaseType.TypeArguments.Count);
//			Assert.Equal("System.String", generator.BaseType.TypeArguments[0].BaseType);
//			Assert.Equal("System.Int32", generator.BaseType.TypeArguments[1].BaseType);
//		}
//
//		[Fact]
//		public void MulipleXTypeArgumentsMulitpleNamespacesOnRootElement()
//		{
//			var xaml = @"<ObservableWrapper 
//						    xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
//						    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
//							x:Class=""FooBar"" 
//							x:TypeArguments=""nsone:IDummyInterface,nstwo:IDummyInterfaceTwo""
//							xmlns:nsone=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests.Bugzilla24258.Interfaces""
//							xmlns:nstwo=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests.Bugzilla24258.InterfacesTwo""
//
//			/>";
//			var reader = new StringReader(xaml);
//
//			var references = string.Join(";",
//								IOPath.GetFullPath(
//									IOPath.Combine(
//										AppContext.BaseDirectory, "Microsoft.Maui.dll")),
//								IOPath.GetFullPath(
//									IOPath.Combine(
//										AppContext.BaseDirectory, "Microsoft.Maui.Controls.dll"))
//								);
//
//			var generator = new XamlGenerator(null, null, null, null, null, null, references);
//			generator.ParseXaml(reader);
//
//			Assert.Equal("FooBar", generator.RootType);
//			Assert.Equal("Microsoft.Maui.Controls.ObservableWrapper`2", generator.BaseType.BaseType);
//			Assert.Equal(2, generator.BaseType.TypeArguments.Count);
//			Assert.Equal("Microsoft.Maui.Controls.Xaml.UnitTests.Bugzilla24258.Interfaces.IDummyInterface", generator.BaseType.TypeArguments[0].BaseType);
//			Assert.Equal("Microsoft.Maui.Controls.Xaml.UnitTests.Bugzilla24258.InterfacesTwo.IDummyInterfaceTwo", generator.BaseType.TypeArguments[1].BaseType);
//		}
//
//		[Fact]
//		public void MulipleXTypeArgumentsMulitpleNamespacesOnRootElementWithWhitespace()
//		{
//			var xaml = @"<ObservableWrapper 
//						    xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
//						    xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
//							x:Class=""FooBar"" 
//							x:TypeArguments=""nsone:IDummyInterface, nstwo:IDummyInterfaceTwo""
//							xmlns:nsone=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests.Bugzilla24258.Interfaces""
//							xmlns:nstwo=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests.Bugzilla24258.InterfacesTwo""
//
//			/>";
//			var reader = new StringReader(xaml);
//
//			var references = string.Join(";",
//								IOPath.GetFullPath(
//									IOPath.Combine(
//										AppContext.BaseDirectory, "Microsoft.Maui.dll")),
//								IOPath.GetFullPath(
//									IOPath.Combine(
//										AppContext.BaseDirectory, "Microsoft.Maui.Controls.dll"))
//								);
//
//			var generator = new XamlGenerator(null, null, null, null, null, null, references);
//			generator.ParseXaml(reader);
//
//			Assert.Equal("FooBar", generator.RootType);
//			Assert.Equal("Microsoft.Maui.Controls.ObservableWrapper`2", generator.BaseType.BaseType);
//			Assert.Equal(2, generator.BaseType.TypeArguments.Count);
//			Assert.Equal("Microsoft.Maui.Controls.Xaml.UnitTests.Bugzilla24258.Interfaces.IDummyInterface", generator.BaseType.TypeArguments[0].BaseType);
//			Assert.Equal("Microsoft.Maui.Controls.Xaml.UnitTests.Bugzilla24258.InterfacesTwo.IDummyInterfaceTwo", generator.BaseType.TypeArguments[1].BaseType);
//		}
//
//		[Fact]
//		//https://bugzilla.xamarin.com/show_bug.cgi?id=33256
//		public void AlwaysUseGlobalReference()
//		{
//			var xaml = @"
//			<ContentPage
//				xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
//				xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
//				x:Class=""FooBar"" >
//				<Label x:Name=""label0""/>
//			</ContentPage>";
//			using (var reader = new StringReader(xaml))
//			{
//				var references = string.Join(";",
//					IOPath.GetFullPath(
//						IOPath.Combine(
//							AppContext.BaseDirectory, "Microsoft.Maui.dll")),
//					IOPath.GetFullPath(
//						IOPath.Combine(
//							AppContext.BaseDirectory, "Microsoft.Maui.Controls.dll"))
//					);
//
//				var generator = new XamlGenerator(null, null, null, null, null, null, references);
//				generator.ParseXaml(reader);
//
//				Assert.True(generator.BaseType.Options.HasFlag(CodeTypeReferenceOptions.GlobalReference));
//				Assert.True(generator.NamedFields.Select(cmf => cmf.Type).First().Options.HasFlag(CodeTypeReferenceOptions.GlobalReference));
//			}
//		}
//
//		[Fact]
//		public void FieldModifier()
//		{
//			var xaml = @"
//			<ContentPage xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
//			             xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
//			             xmlns:local=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests""
//			             x:Class=""Microsoft.Maui.Controls.Xaml.UnitTests.FieldModifier"">
//				<StackLayout>
//			        <Label x:Name=""privateLabel"" />
//			        <Label x:Name=""internalLabel"" x:FieldModifier=""NotPublic"" />
//			        <Label x:Name=""publicLabel"" x:FieldModifier=""Public"" />
//				</StackLayout>
//			</ContentPage>";
//
//			using (var reader = new StringReader(xaml))
//			{
//				var references = string.Join(";",
//					IOPath.GetFullPath(
//						IOPath.Combine(
//							AppContext.BaseDirectory, "Microsoft.Maui.dll")),
//					IOPath.GetFullPath(
//						IOPath.Combine(
//							AppContext.BaseDirectory, "Microsoft.Maui.Controls.dll"))
//					);
//
//				var generator = new XamlGenerator(null, null, null, null, null, null, references);
//				generator.ParseXaml(reader);
//
//				Assert.Equal(MemberAttributes.Private, generator.NamedFields.First(cmf => cmf.Name == "privateLabel").Attributes);
//				Assert.Equal(MemberAttributes.Assembly, generator.NamedFields.First(cmf => cmf.Name == "internalLabel").Attributes);
//				Assert.Equal(MemberAttributes.Public, generator.NamedFields.First(cmf => cmf.Name == "publicLabel").Attributes);
//			}
//		}
//
//		[Fact]
//		//https://github.com/xamarin/Microsoft.Maui.Controls/issues/2574
//		public void xNameOnRoot()
//		{
//			var xaml = @"<ContentPage
//		xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
//		xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
//		x:Class=""Foo""
//		x:Name=""bar"">
//	</ContentPage>";
//
//			var references = string.Join(";",
//								IOPath.GetFullPath(
//									IOPath.Combine(
//										AppContext.BaseDirectory, "Microsoft.Maui.dll")),
//								IOPath.GetFullPath(
//									IOPath.Combine(
//										AppContext.BaseDirectory, "Microsoft.Maui.Controls.dll"))
//								);
//
//			var generator = new XamlGenerator(null, null, null, null, null, null, references);
//			generator.ParseXaml(new StringReader(xaml));
//
//			Assert.Equal(1, generator.NamedFields.Count());
//			Assert.Equal("bar", generator.NamedFields.First().Name);
//			Assert.Equal("Microsoft.Maui.Controls.ContentPage", generator.NamedFields.First().Type.BaseType);
//		}
//
//		[Fact]
//		public void XamlGDifferentInputOutputLengths()
//		{
//			var engine = new MSBuild.UnitTests.DummyBuildEngine();
//			var generator = new XamlGTask()
//			{
//				BuildEngine = engine,
//				AssemblyName = "test",
//				Language = "C#",
//				XamlFiles = new ITaskItem[1],
//				OutputFiles = new ITaskItem[2],
//			};
//
//			Assert.False(generator.Execute(), "XamlGTask.Execute() should fail.");
//			Assert.Equal(1, engine.Errors.Count, "XamlGTask should have 1 error.");
//			var error = engine.Errors.First();
//			Assert.Equal("\"XamlFiles\" refers to 1 item(s), and \"OutputFiles\" refers to 2 item(s). They must have the same number of items.", error.Message);
//		}
//	}
//}
