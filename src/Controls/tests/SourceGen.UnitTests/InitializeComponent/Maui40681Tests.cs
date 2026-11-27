using System;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class Maui40681Tests : SourceGenXamlInitializeComponentTestBase
{
	[Fact]
	public void ExplicitCollectionCastOnWholePropertyIsPreserved()
	{
		var xaml = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:Test"
			             x:Class="Test.TestPage">
				<local:ItemsHost Items="{x:Static local:Values.Items}" />
			</ContentPage>
			""";

		var code = """
			using System.Collections.Generic;
			using Microsoft.Maui.Controls;
			using Microsoft.Maui.Controls.Xaml;

			namespace Test;

			public interface IItems : IList<string> { }

			public class CustomItems : List<string>, IItems { }

			public static class Values
			{
				public static List<string> Items => new CustomItems();
			}

			public class ItemsHost : ContentView
			{
				public IItems Items { get; set; } = new CustomItems();
			}

			[XamlProcessing(XamlInflator.SourceGen)]
			public partial class TestPage : ContentPage
			{
				public TestPage() => InitializeComponent();
			}
			""";

		var (result, generated) = RunGenerator(xaml, code);

		Assert.Empty(result.Diagnostics);
		Assert.NotNull(generated);
		Assert.Contains(".Items = (global::Test.IItems)", generated, StringComparison.Ordinal);
		Assert.DoesNotContain(".Items.Add(", generated, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("VerticalStackLayout", true)]
	[InlineData("Grid", true)]
	[InlineData("Grid", false)]
	public void SingleLayoutChildOfIListViewIsAdded(string childType, bool attachedProperty)
	{
		var childElement = $"<{childType} />";
		var propertyContent = attachedProperty
			? $$"""
				<VerticalStackLayout>
					<local:Holder.Items>
						{{childElement}}
					</local:Holder.Items>
				</VerticalStackLayout>
				"""
			: $$"""
				<local:ViewsHost>
					<local:ViewsHost.Views>
						{{childElement}}
					</local:ViewsHost.Views>
				</local:ViewsHost>
				""";

		var xaml = $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             xmlns:local="clr-namespace:Test"
			             x:Class="Test.TestPage">
				{{propertyContent}}
			</ContentPage>
			""";

		var code = """
			using System.Collections.Generic;
			using Microsoft.Maui.Controls;
			using Microsoft.Maui.Controls.Xaml;

			namespace Test;

			public static class Holder
			{
				public static readonly BindableProperty ItemsProperty = BindableProperty.CreateAttached(
					"Items", typeof(IList<View>), typeof(Holder), null,
					defaultValueCreator: _ => new List<View>());

				public static IList<View> GetItems(BindableObject bindableObject) =>
					(IList<View>)bindableObject.GetValue(ItemsProperty);

				public static void SetItems(BindableObject bindableObject, IList<View> value) =>
					bindableObject.SetValue(ItemsProperty, value);
			}

			public class ViewsHost : ContentView
			{
				public IList<View> Views { get; set; } = new List<View>();
			}

			[XamlProcessing(XamlInflator.SourceGen)]
			public partial class TestPage : ContentPage
			{
				public TestPage() => InitializeComponent();
			}
			""";

		var (result, generated) = RunGenerator(xaml, code);

		Assert.Empty(result.Diagnostics);
		Assert.NotNull(generated);
		Assert.Contains(".Add(", generated, StringComparison.Ordinal);
	}
}
