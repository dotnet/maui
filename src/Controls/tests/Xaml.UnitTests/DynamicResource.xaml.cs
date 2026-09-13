using System.Collections.Generic;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class DynamicResource : ContentPage
{
	public DynamicResource()
	{
		InitializeComponent();
	}

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void TestDynamicResources(XamlInflator inflator)
		{
			var layout = new DynamicResource(inflator);
			var label = layout.label0;

			Assert.Null(label.Text);

			layout.Resources = new ResourceDictionary {
				{"FooBar", "FOOBAR"},
			};
			Assert.Equal("FOOBAR", label.Text);
		}

		[Fact]
		public void RuntimeDynamicResourceReplacesExistingLocalValue()
		{
			const string xaml = """
				<Label xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
				       Text="{DynamicResource PageCaption}" />
				""";
			var label = new Label
			{
				Text = "Literal value",
				Resources = new ResourceDictionary
				{
					["PageCaption"] = "Resource value",
				},
			};
			var observedValues = new List<string>();
			label.PropertyChanged += (_, args) =>
			{
				if (args.PropertyName == Label.TextProperty.PropertyName)
					observedValues.Add(label.Text);
			};

			label.LoadFromXaml(xaml);
			Assert.Equal("Resource value", label.Text);
			Assert.Equal(["Resource value"], observedValues);

			label.Resources["PageCaption"] = "Updated resource value";
			Assert.Equal("Updated resource value", label.Text);
		}

		[Fact]
		public void RuntimeDynamicResourceReplacesExistingBinding()
		{
			const string xaml = """
				<Label xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
				       Text="{DynamicResource PageCaption}" />
				""";
			var label = new Label
			{
				BindingContext = new CaptionSource(),
				Resources = new ResourceDictionary
				{
					["PageCaption"] = "Resource value",
				},
			};
			label.SetBinding(Label.TextProperty, nameof(CaptionSource.Caption));
			Assert.Equal("Bound value", label.Text);
			Assert.True(label.GetIsBound(Label.TextProperty));

			label.LoadFromXaml(xaml);
			Assert.Equal("Resource value", label.Text);
			Assert.False(label.GetIsBound(Label.TextProperty));

			label.Resources["PageCaption"] = "Updated resource value";
			Assert.Equal("Updated resource value", label.Text);
		}

		sealed class CaptionSource
		{
			public string Caption { get; set; } = "Bound value";
		}
	}

}
