using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;
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
			services.Add("IRootObjectProvider");
		if (serviceProvider.GetService(typeof(IXmlLineInfoProvider)) != null)
			services.Add("IXmlLineInfoProvider");
		if (serviceProvider.GetService(typeof(IValueConverterProvider)) != null)
			services.Add("IValueConverterProvider");
		if (serviceProvider.GetService(typeof(IProvideParentValues)) != null)
			services.Add("IProvideParentValues");
		if (serviceProvider.GetService(typeof(IReferenceProvider)) != null)
			services.Add("IReferenceProvider");

		return string.Join(",", services);
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



public partial class ServiceProviderTests : ContentPage
{
	public ServiceProviderTests() => InitializeComponent();

	public ServiceProviderTests(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	public class Tests
	{
		[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

		[TestCase(true)]
		public void TestServiceProviders(bool useCompiledXaml)
		{
			var page = new ServiceProviderTests(useCompiledXaml);
			MockCompiler.Compile(typeof(ServiceProviderTests));

			//IValueConverterProvider is builtin for free
			Assert.AreEqual(null, page.label0.Text);
			Assert.AreEqual("IProvideValueTarget,IValueConverterProvider", page.label1.Text);
			Assert.AreEqual("IProvideValueTarget,IValueConverterProvider,IReferenceProvider", page.label2.Text);
			Assert.AreEqual("IXmlLineInfoProvider,IValueConverterProvider", page.label3.Text);
		}
	}
}
