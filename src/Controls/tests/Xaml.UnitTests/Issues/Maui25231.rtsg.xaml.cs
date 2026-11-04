using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Maui25231GenericBase<T> : ContentPage
{
}

public class Maui25231TypeHolder : BindableObject
{
	public static readonly BindableProperty BindableTypeProperty =
		BindableProperty.Create(nameof(BindableType), typeof(Type), typeof(Maui25231TypeHolder));
	public static readonly BindableProperty OverrideBindableTypeProperty =
		BindableProperty.Create(nameof(OverrideBindableType), typeof(Type), typeof(Maui25231TypeHolder));

	public Type TypeValue { get; set; }
	public Type GenericValue { get; set; }
	public Type BindableType
	{
		get => (Type)GetValue(BindableTypeProperty);
		set => SetValue(BindableTypeProperty, value);
	}

	[TypeConverter(typeof(TypeTypeConverter))]
	public Type ExplicitType { get; set; }

	[TypeConverter(typeof(Maui25231OverrideConverter))]
	public Type OverrideType { get; set; }

	[TypeConverter(typeof(Maui25231OverrideConverter))]
	public Type OverrideBindableType
	{
		get => (Type)GetValue(OverrideBindableTypeProperty);
		set => SetValue(OverrideBindableTypeProperty, value);
	}

	public Type MarkupType { get; set; }
}

public class Maui25231OverrideConverter : TypeConverter
{
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) => typeof(bool);
}

public class Maui25231ConstructorType
{
	public Maui25231ConstructorType([Parameter("Type")] Type type) => Type = type;
	public Type Type { get; }
}

public class Maui25231OverrideConstructorType
{
	public Maui25231OverrideConstructorType([Parameter("Type"), TypeConverter(typeof(Maui25231OverrideConverter))] Type type) => Type = type;
	public Type Type { get; }
}

[AcceptEmptyServiceProvider]
public class Maui25231GenericExtension<T> : IMarkupExtension<string>
{
	public string ProvideValue(IServiceProvider serviceProvider) => typeof(T).Name;
	object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
}

public partial class Maui25231 : Maui25231GenericBase<string>
{
	public Maui25231() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[InlineData(XamlInflator.Runtime)]
		[InlineData(XamlInflator.SourceGen)]
		internal void TypeNamesWorkInBothForms(XamlInflator inflator)
		{
			var page = new Maui25231(inflator);

			Assert.IsAssignableFrom<Maui25231GenericBase<string>>(page);
			Assert.Equal(new[] { 1.5, 2.5 }, Assert.IsType<double[]>(page.Resources["BareArray"]));
			var genericArray = Assert.IsType<List<string>[]>(page.Resources["GenericArray"]);
			Assert.Equal(new[] { "value" }, Assert.Single(genericArray));
			Assert.Equal(new[] { "value" }, Assert.IsType<string[]>(page.Resources["MarkupArray"]));
			Assert.IsType<Microsoft.Maui.Controls.OnPlatform<double>>(page.Resources["Platform"]);
			Assert.IsType<Dictionary<string, int>>(page.Resources["Dictionary"]);
			Assert.IsType<List<KeyValuePair<string, int>>>(page.Resources["GenericList"]);
			Assert.IsType<List<KeyValuePair<string, int>>>(page.NestedList);
			Assert.IsType<List<List<string>>>(page.ScopedList);
			Assert.IsType<List<List<string>>>(page.Resources["NestedNamedList"]);

			var holder = Assert.IsType<Maui25231TypeHolder>(page.Resources["Types"]);
			Assert.Equal(typeof(ContentPage), holder.TypeValue);
			Assert.Equal(typeof(List<string>), holder.GenericValue);
			Assert.Equal(typeof(double), holder.BindableType);
			Assert.Equal(typeof(List<string>), holder.ExplicitType);
			Assert.Equal(typeof(bool), holder.OverrideType);
			Assert.Equal(typeof(bool), holder.OverrideBindableType);
			Assert.Equal(typeof(string), holder.MarkupType);
			Assert.Equal(typeof(double), Assert.IsType<Maui25231ConstructorType>(page.Resources["Constructor"]).Type);
			Assert.Equal(typeof(bool), Assert.IsType<Maui25231OverrideConstructorType>(page.Resources["OverrideConstructor"]).Type);
			Assert.Equal(typeof(Button), Assert.IsType<VisualStateGroup>(page.Resources["VisualGroup"]).TargetType);
			Assert.Equal(typeof(Label), Assert.IsType<VisualState>(page.Resources["VisualState"]).TargetType);
			Assert.Equal("Ancestor", page.RelativeLabel.Text);
			Assert.Equal("String", page.GenericExtensionLabel.Text);
			Assert.Equal("String", page.NamedGenericExtensionLabel.Text);
		}

		[Theory]
		[InlineData("{x:Type}")]
		[InlineData("{x:Type x:String")]
		[InlineData("{x:Type x:String, x:Int32}")]
		[InlineData("{other:Type x:String}")]
		[InlineData("x:Missing(")]
		[InlineData("x:String, , x:Int32")]
		[InlineData("x:String,")]
		internal void InvalidTypeArgumentsFail(string expression)
		{
			var resolver = new XmlNamespaceManager(new NameTable());
			resolver.AddNamespace("", XamlParser.MauiUri);
			resolver.AddNamespace("x", XamlParser.X2009Uri);
			resolver.AddNamespace("scg", "clr-namespace:System.Collections.Generic;assembly=mscorlib");
			resolver.AddNamespace("other", XamlParser.MauiUri);

			Assert.Throws<XamlParseException>(() => TypeArgumentsParser.ParseExpression(expression, resolver, null));
		}

		[Fact]
		internal void NestedNamedTypeExtensionParsesAsGenericTypeArgument()
		{
			var resolver = new XmlNamespaceManager(new NameTable());
			resolver.AddNamespace("x", XamlParser.X2009Uri);
			resolver.AddNamespace("scg", "clr-namespace:System.Collections.Generic;assembly=mscorlib");

			var type = Assert.Single(TypeArgumentsParser.ParseExpression(
				"{x:Type scg:List({x:Type TypeName=x:String})}", resolver, null));

			Assert.Equal("List", type.Name);
			Assert.Single(type.TypeArguments);
			Assert.Equal(XamlParser.X2009Uri, type.TypeArguments[0].NamespaceUri);
			Assert.Equal("String", type.TypeArguments[0].Name);
		}

		[Theory]
		[InlineData("property", "x:Missing", "Missing")]
		[InlineData("property", "", "type expression")]
		[InlineData("property", "unknown:Missing", "unknown")]
		[InlineData("property", "scg:List(x:Missing)", "Missing")]
		[InlineData("array", "x:Missing", "Missing")]
		[InlineData("array", "", "type expression")]
		[InlineData("array", "scg:List(x:Missing)", "Missing")]
		[InlineData("ancestor", "x:Missing", "Missing")]
		[InlineData("ancestor", "", "Cannot resolve")]
		[InlineData("ancestor", "unknown:Missing", "unknown")]
		internal void InvalidTypeNamesAreReported(string location, string typeName, string expectedMessage)
		{
			var content = location switch
			{
				"property" => $"""<ContentPage.Resources><VisualState x:Key="Invalid" TargetType="{typeName}" /></ContentPage.Resources>""",
				"array" => $"""<ContentPage.Resources><x:Array x:Key="Invalid" Type="{typeName}" /></ContentPage.Resources>""",
				"ancestor" => $$"""<Label Text="{Binding Source={RelativeSource Mode=Self, AncestorType='{{typeName}}'}, Path=Text}" />""",
				_ => throw new ArgumentOutOfRangeException(nameof(location)),
			};

			string Xaml(string classAttribute) => $$"""
				<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
				             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
				             xmlns:scg="clr-namespace:System.Collections.Generic;assembly=mscorlib"
				             {{classAttribute}}>
				    {{content}}
				</ContentPage>
				""";

			Assert.Throws<XamlParseException>(() => new ContentPage().LoadFromXaml(Xaml("")));

			var result = CreateMauiCompilation()
				.WithAdditionalSource("""
					namespace Microsoft.Maui.Controls.Xaml.UnitTests;
					public partial class Maui25231InvalidTypePage : Microsoft.Maui.Controls.ContentPage { }
					""")
				.RunMauiSourceGenerator(new AdditionalXamlFile("Maui25231InvalidTypePage.xaml",
					Xaml("""x:Class="Microsoft.Maui.Controls.Xaml.UnitTests.Maui25231InvalidTypePage" """)));

			Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Id == "CS8785");
			Assert.True(result.Diagnostics.Any(diagnostic =>
				diagnostic.Severity == DiagnosticSeverity.Error &&
				diagnostic.GetMessage().Contains(expectedMessage, StringComparison.Ordinal)),
				string.Join(Environment.NewLine, result.Diagnostics));
		}

		[Fact]
		internal void NamedGenericTypeArgumentsUseTheLocalNamespaceScope()
		{
			const string xaml = """
				<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
				             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
				             xmlns:scg="clr-namespace:System.Collections.Generic;assembly=mscorlib"
				             xmlns:inner="clr-namespace:System;assembly=mscorlib"
				             x:Class="Microsoft.Maui.Controls.Xaml.UnitTests.Maui25231ScopedPage">
				    <ContentPage.Resources xmlns:inner="clr-namespace:System.Collections.Generic;assembly=mscorlib">
				        <scg:List x:Key="ScopedList" x:Name="ScopedList"
				                  x:TypeArguments="{x:Type inner:List(x:String)}" />
				    </ContentPage.Resources>
				</ContentPage>
				""";
			var result = CreateMauiCompilation()
				.WithAdditionalSource("""
					namespace Microsoft.Maui.Controls.Xaml.UnitTests;
					public partial class Maui25231ScopedPage : Microsoft.Maui.Controls.ContentPage { }
					""")
				.RunMauiSourceGenerator(new AdditionalXamlFile("Maui25231ScopedPage.xaml", xaml));

			Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
			Assert.Contains("global::System.Collections.Generic.List<global::System.Collections.Generic.List<string>> ScopedList", result.GeneratedCodeBehind(), StringComparison.Ordinal);
		}
	}
}
