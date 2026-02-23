using System;
using System.Linq;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[Collection("Xaml Inflation feature")]
public partial class Required : ContentPage
{
	public RequiredRandomSelector Selector { get; set; }
	public RequiredPerson Person { get; set; }
	public Required() => InitializeComponent();

	public class Tests : BaseTestFixture
	{
		[Theory]
		[XamlInflatorData]
		internal void RequiredFieldsAndPropertiesAreSet(XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
	"""
using System;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class Required : ContentPage
{
	public RequiredRandomSelector Selector { get; set; }
	public RequiredPerson Person { get; set; }
	public Required() => InitializeComponent();
}

public class RequiredRandomSelector : DataTemplateSelector
{
	public /*required*/ Controls.DataTemplate Template1 { get; set; }
	public required Controls.DataTemplate Template2 { get; set; }

	protected override Controls.DataTemplate OnSelectTemplate(object item, BindableObject container) => new Random().Next(2) == 0 ? Template1 : Template2;
}

public class RequiredPerson
{
	public required string Name { get; set; }

	public override string ToString()
		=> Name;
}
""")
					.RunMauiSourceGenerator(typeof(Required));

				Assert.True(result.Diagnostics.Length == 1, "warning expected");
			}

			var layout = new Required(inflator);
			Assert.NotNull(layout.Selector);
			Assert.NotNull(layout.Selector.Template1);
			Assert.NotNull(layout.Selector.Template2);
			Assert.NotNull(layout.Person);
			Assert.NotNull(layout.Person.Name);
		}
	}
}

public class RequiredRandomSelector : DataTemplateSelector
{
	public /*required*/ Controls.DataTemplate Template1 { get; set; }
	public required Controls.DataTemplate Template2 { get; set; }

	protected override Controls.DataTemplate OnSelectTemplate(object item, BindableObject container) => new Random().Next(2) == 0 ? Template1 : Template2;
}

public class RequiredPerson
{
	public required string Name { get; set; }
	public override string ToString()
		=> Name;
}
