using System;
using System.Reflection;
using Xunit;
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class XamlInflatorTestsHelpers
{
	internal static void TestInflator(Type type, XamlInflator inflator, bool generateinflatorswitch)
	{
		if (!generateinflatorswitch)
			Assert.Null(type.GetMethod("InitializeComponentSourceGen", BindingFlags.Instance | BindingFlags.NonPublic));

		Assert.Null(type.GetMethod("__InitComponentRuntime", BindingFlags.Instance | BindingFlags.NonPublic));

		//check that there is an InitializeComponent method that takes an argument of type XamlInflator
		if (generateinflatorswitch)
			Assert.NotNull(type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, [typeof(XamlInflator)]));

		string initComp = "InitializeComponent";

		Assert.NotNull(type.GetMethod(initComp, BindingFlags.Instance | BindingFlags.NonPublic));

		if ((inflator & XamlInflator.Runtime) == XamlInflator.Runtime)
			initComp = "InitializeComponentRuntime";
		else if ((inflator & XamlInflator.XamlC) == XamlInflator.XamlC)
			initComp = "InitializeComponentXamlC";
		else if ((inflator & XamlInflator.SourceGen) == XamlInflator.SourceGen)
			initComp = "InitializeComponentSourceGen";

		var body = type.GetMethod(initComp, BindingFlags.Instance | BindingFlags.NonPublic).GetMethodBody();

		//quite bad heuristics. this should use Cecil, and check for known calls, like LoadFromXaml, etc
		var instructions = body.GetILAsByteArray();
		if ((inflator & XamlInflator.Runtime) == XamlInflator.Runtime)
#if DEBUG
			Assert.Equal(36, instructions.Length);
#else
			Assert.Equal(35, instructions.Length);
#endif
		if ((inflator & XamlInflator.SourceGen) == XamlInflator.SourceGen)
#if DEBUG
			Assert.Equal(394, instructions.Length);
#else
			Assert.Equal(196, instructions.Length);
#endif
		if ((inflator & XamlInflator.XamlC) == XamlInflator.XamlC)
			Assert.Equal(190, instructions.Length);

		if (generateinflatorswitch)
		{
			if ((inflator & XamlInflator.Runtime) == XamlInflator.Runtime)
			{
				var runtime = type.GetMethod("InitializeComponentRuntime", BindingFlags.Instance | BindingFlags.NonPublic);
				Assert.NotNull(runtime);
#if DEBUG
				Assert.Equal(36, runtime.GetMethodBody().GetILAsByteArray().Length);
#else
			Assert.Equal(35, runtime.GetMethodBody().GetILAsByteArray().Length);
#endif
			}

			if ((inflator & XamlInflator.XamlC) == XamlInflator.XamlC)
			{
				var xamlc = type.GetMethod("InitializeComponentXamlC", BindingFlags.Instance | BindingFlags.NonPublic);
				Assert.NotNull(xamlc);
				Assert.Equal(190, xamlc.GetMethodBody().GetILAsByteArray().Length);
			}

			if ((inflator & XamlInflator.SourceGen) == XamlInflator.SourceGen)
			{
				var sourcegen = type.GetMethod("InitializeComponentSourceGen", BindingFlags.Instance | BindingFlags.NonPublic);
				Assert.NotNull(sourcegen);
#if DEBUG
				Assert.Equal(394, sourcegen.GetMethodBody().GetILAsByteArray().Length);
#else
				Assert.Equal(196, sourcegen.GetMethodBody().GetILAsByteArray().Length);
#endif
			}

		}
	}
}