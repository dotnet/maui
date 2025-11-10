using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Required : ContentPage
{
	public RequiredRandomSelector Selector { get; set; }
	public RequiredPerson Person { get; set; }
	public Required() => InitializeComponent();

	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}

		[Theory]
		[Values]
		public void RequiredFieldsAndPropertiesAreSet(XamlInflator inflator)
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

				Assert.Empty(result.Diagnostics);
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
