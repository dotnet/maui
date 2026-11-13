global using Issue29708.Controls;
global using Microsoft.Maui.Controls.ControlGallery;

using System;
using System.Reflection;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Xunit;

namespace http29708.Controls
{
	internal class Maui29708HttpControl : Microsoft.Maui.Controls.Label
	{
	}
}

namespace Issue29708.Controls
{
	internal class Maui29708Control : Microsoft.Maui.Controls.Label
	{
	}

	public class Maui29708PublicControl : Microsoft.Maui.Controls.Label
	{
	}

	[AcceptEmptyServiceProvider]
	internal class Maui29708TextExtension : IMarkupExtension<string>
	{
		public string ProvideValue(IServiceProvider serviceProvider) => "Imported from a global using";
		object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider) => ProvideValue(serviceProvider);
	}
}

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Maui29708 : ContentPage
	{
		public Maui29708() => InitializeComponent();

		[Collection("Xaml Inflation")]
		public class Tests : BaseTestFixture
		{
			[Theory]
			[XamlInflatorData]
			internal void GlobalUsingImportsCurrentAssemblyTypes(XamlInflator inflator)
			{
				if (inflator == XamlInflator.XamlC)
					MockCompiler.Compile(typeof(Maui29708));

				var page = new Maui29708(inflator);
				Assert.IsType<Maui29708Control>(page.control);
				Assert.IsType<Maui29708HttpControl>(page.httpControl);
				Assert.Equal("Imported from a global using", page.control.Text);
			}

			[Theory]
			[InlineData(XamlInflator.Runtime)]
			[InlineData(XamlInflator.XamlC)]
			internal void GlobalUsingDoesNotImportReferencedAssemblyTypes(XamlInflator inflator)
			{
				var mapping = Assert.Single(typeof(Maui29708).Assembly.GetCustomAttributes<XmlnsDefinitionAttribute>(),
					attribute => attribute.XmlNamespace == "http://schemas.microsoft.com/dotnet/maui/global"
						&& attribute.Target == "Microsoft.Maui.Controls.ControlGallery");
				Assert.Null(mapping.AssemblyName);

				if (inflator == XamlInflator.XamlC)
					Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Maui29708External)));
				else
					Assert.Throws<XamlParseException>(() => new Maui29708External(inflator));
			}

			[Fact]
			public void RuntimeGlobalMappingsDoNotLeakToOtherAssemblies()
			{
				var xmlType = new XmlType(XamlParser.MauiGlobalUri, nameof(Maui29708PublicControl), null);
				var otherAssembly = typeof(Microsoft.Maui.Controls.ControlGallery.Issue3076Button).Assembly;

				Assert.Equal(typeof(Maui29708PublicControl),
					XamlParser.GetElementType(xmlType, null, typeof(Maui29708).Assembly, true, out var exception));
				Assert.Null(exception);

				Assert.Null(XamlParser.GetElementType(xmlType, null, otherAssembly, true, out _));
			}
		}
	}

	public partial class Maui29708External : ContentPage
	{
		public Maui29708External() => InitializeComponent();
	}
}
