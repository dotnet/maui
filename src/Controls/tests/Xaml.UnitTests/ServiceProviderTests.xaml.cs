using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class MarkupExtensionBase : IMarkupExtension
{
	public object ProvideValue(IServiceProvider serviceProvider)
	{
		if (serviceProvider == null)
			return null;
		var services = new List<string>();
		if (serviceProvider.GetService(typeof(IProvideValueTarget)) != null)
			services.Add("IProvideValueTarget");
		if (serviceProvider.GetService(typeof(IXamlTypeResolver)) != null)
			services.Add("IXamlTypeResolver");
		if (serviceProvider.GetService(typeof(IRootObjectProvider)) != null)
			services.Add($"IRootObjectProvider({((IRootObjectProvider)serviceProvider.GetService(typeof(IRootObjectProvider))).RootObject.GetType().Name})");
		if (serviceProvider.GetService(typeof(XamlLineInfo)) != null)
			services.Add("XamlLineInfo");
		if (serviceProvider.GetService(typeof(IValueConverterProvider)) != null)
			services.Add("IValueConverterProvider");
		if (serviceProvider.GetService(typeof(IProvideParentValues)) != null)
			services.Add("IProvideParentValues");
		if (serviceProvider.GetService(typeof(IReferenceProvider)) != null)
			services.Add("IReferenceProvider");

		return string.Join(", ", services);
	}
}

[AcceptEmptyServiceProvider]
public class SPMarkup0 : MarkupExtensionBase { }

[RequireService([typeof(IProvideValueTarget)])]
public class SPMarkup1 : MarkupExtensionBase { }

[RequireService([typeof(IProvideParentValues)])]
public class SPMarkup2 : MarkupExtensionBase { }

[RequireService([typeof(XamlLineInfo)])]
public class SPMarkup3 : IMarkupExtension
{
	public object ProvideValue(IServiceProvider serviceProvider)
	{
		var lineInfo = (XamlLineInfo)serviceProvider.GetService(typeof(XamlLineInfo));
		return $"XamlLineInfo({lineInfo.LineNumber},{lineInfo.LinePosition})";
	}
}

[RequireService([typeof(IRootObjectProvider)])]
public class SPMarkup4 : MarkupExtensionBase { }


public partial class ServiceProviderTests : ContentPage
{
	public ServiceProviderTests() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : IDisposable
	{
		public Tests() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Theory]
		[InlineData(XamlInflator.Runtime)]
		[InlineData(XamlInflator.XamlC)]
		[InlineData(XamlInflator.SourceGen)]
		internal void TestServiceProviders(XamlInflator inflator)
		{
			var page = new ServiceProviderTests(inflator);
			if (inflator == XamlInflator.XamlC)
			{
				MockCompiler.Compile(typeof(ServiceProviderTests), out var initializeComponent, out _);
				var lineInfoConstructor = initializeComponent.Body.Instructions
					.Where(instruction => instruction.OpCode == OpCodes.Newobj)
					.Select(instruction => instruction.Operand)
					.OfType<Mono.Cecil.MethodReference>()
					.Single(method => method.DeclaringType.FullName == "Microsoft.Maui.Controls.Xaml.XamlLineInfo");

				Assert.Collection(
					lineInfoConstructor.Parameters,
					parameter => Assert.Equal("System.Int32", parameter.ParameterType.FullName),
					parameter => Assert.Equal("System.Int32", parameter.ParameterType.FullName));
			}

			Assert.Null(page.label0.Text);
			Assert.Contains("IProvideValueTarget", page.label1.Text, StringComparison.Ordinal);
			Assert.Equal("XamlLineInfo(11,32)", page.label3.Text);
			Assert.Contains("IRootObjectProvider(ServiceProviderTests)", page.label4.Text, StringComparison.Ordinal); //https://github.com/dotnet/maui/issues/16881
		}
	}
}
