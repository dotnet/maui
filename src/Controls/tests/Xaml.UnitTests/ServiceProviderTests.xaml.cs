using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;
using AbsoluteLayoutFlags = Microsoft.Maui.Layouts.AbsoluteLayoutFlags;

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
		if (serviceProvider.GetService(typeof(IXmlLineInfoProvider)) != null)
			services.Add("IXmlLineInfoProvider");
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

[RequireService([typeof(IXmlLineInfoProvider)])]
public class SPMarkup3 : MarkupExtensionBase { }

[RequireService([typeof(IRootObjectProvider)])]
public class SPMarkup4 : MarkupExtensionBase { }


public partial class ServiceProviderTests : ContentPage
{
	public ServiceProviderTests() => InitializeComponent();

	public ServiceProviderTests(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}	public class Tests
	{
		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

		[Fact]
		public void TestServiceProviders([Values] bool useCompiledXaml)
		{
			var page = new ServiceProviderTests(useCompiledXaml);

			Assert.Equal(null, page.label0.Text);
			Assert.Contains("IProvideValueTarget", page.label1.Text);
			Assert.Contains("IXmlLineInfoProvider", page.label3.Text);
			Assert.Contains("IRootObjectProvider(ServiceProviderTests)", page.label4.Text); //https://github.com/dotnet/maui/issues/16881
		}
	}
}
