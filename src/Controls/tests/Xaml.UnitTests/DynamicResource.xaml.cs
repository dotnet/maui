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

			label.LoadFromXaml(xaml);
			Assert.Equal("Resource value", label.Text);

			label.Resources["PageCaption"] = "Updated resource value";
			Assert.Equal("Updated resource value", label.Text);
		}
	}

}