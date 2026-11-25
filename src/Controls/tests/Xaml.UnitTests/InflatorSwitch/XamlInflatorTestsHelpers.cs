using System;
using System.Reflection;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class XamlInflatorTestsHelpers
{
	internal static void TestInflator(Type type, XamlInflator inflator, bool generateinflatorswitch)
	{
		const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
		Assert.NotNull(type.GetMethod("InitializeComponent", flags));
		Assert.Null(type.GetMethod("__InitComponentRuntime", flags));

		if (generateinflatorswitch)
			Assert.NotNull(type.GetConstructor(flags | BindingFlags.Public, [typeof(XamlInflator)]));
		else
			Assert.Null(type.GetMethod("InitializeComponentSourceGen", flags));

		if ((inflator & XamlInflator.Runtime) == XamlInflator.Runtime)
			Assert.NotNull(type.GetMethod("InitializeComponentRuntime", flags));
		if ((inflator & XamlInflator.SourceGen) == XamlInflator.SourceGen)
			Assert.NotNull(type.GetMethod("InitializeComponentSourceGen", flags));
	}
}
