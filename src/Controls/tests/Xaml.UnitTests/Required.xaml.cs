using System;
using System.Linq;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Required : ContentPage
{
	public RequiredRandomSelector Selector { get; set; }
	public RequiredPerson Person { get; set; }
	public Required() => InitializeComponent();

	class Tests
	{
		[Test]
		public void RequiredFieldsAndPropertiesAreSet([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
	"""
using System;
using NUnit.Framework;

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

				Assert.That(result.Diagnostics.Length, Is.EqualTo(1), "warning expected");
			}

			var layout = new Required(inflator);
			Assert.IsNotNull(layout.Selector);
			Assert.IsNotNull(layout.Selector.Template1);
			Assert.IsNotNull(layout.Selector.Template2);
			Assert.IsNotNull(layout.Person);
			Assert.IsNotNull(layout.Person.Name);
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
