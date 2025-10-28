using System;
using System.Reflection;
using NUnit.Framework;
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class XamlInflatorTestsHelpers
{
	internal static void TestInflator(Type type, XamlInflator inflator, bool generateinflatorswitch)
	{
		if (!generateinflatorswitch)
			Assert.IsNull(type.GetMethod("InitializeComponentSourceGen", BindingFlags.Instance | BindingFlags.NonPublic), $"{type.Name} should not have InitializeComponentSourceGen method");

		Assert.IsNull(type.GetMethod("__InitComponentRuntime", BindingFlags.Instance | BindingFlags.NonPublic), $"{type.Name} should not have __InitComponentRuntime method");

		//check that there is an InitializeComponent method that takes an argument of type XamlInflator
		if (generateinflatorswitch)
			Assert.IsNotNull(type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, [typeof(XamlInflator)]), $"{type.Name} should have InitializeComponent method with XamlInflator argument");

		string initComp = "InitializeComponent";

		Assert.IsNotNull(type.GetMethod(initComp, BindingFlags.Instance | BindingFlags.NonPublic), $"{type.Name} should have InitializeComponent method");

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
			Assert.AreEqual(36, instructions.Length, "Method body should be 36 bytes long");
#else
			Assert.AreEqual(35, instructions.Length, "Method body should be 35 bytes long");
#endif
		if ((inflator & XamlInflator.SourceGen) == XamlInflator.SourceGen)
#if DEBUG
			Assert.AreEqual(393, instructions.Length, "Method body should be 393 bytes long");
#else
			Assert.AreEqual(196, instructions.Length, "Method body should be 196 bytes long");
#endif
		if ((inflator & XamlInflator.XamlC) == XamlInflator.XamlC)
			Assert.AreEqual(190, instructions.Length, "Method body should be 190 bytes long");

		if (generateinflatorswitch)
		{
			if ((inflator & XamlInflator.Runtime) == XamlInflator.Runtime)
			{
				var runtime = type.GetMethod("InitializeComponentRuntime", BindingFlags.Instance | BindingFlags.NonPublic);
				Assert.IsNotNull(runtime, $"{type.Name} should have InitializeComponentRuntime method");
#if DEBUG
				Assert.AreEqual(36, runtime.GetMethodBody().GetILAsByteArray().Length, "Method body should be 36 bytes long");
#else
			Assert.AreEqual(35, runtime.GetMethodBody().GetILAsByteArray().Length, "Method body should be 35 bytes long");
#endif
			}

			if ((inflator & XamlInflator.XamlC) == XamlInflator.XamlC)
			{
				var xamlc = type.GetMethod("InitializeComponentXamlC", BindingFlags.Instance | BindingFlags.NonPublic);
				Assert.IsNotNull(xamlc, $"{type.Name} should have InitializeComponentXamlC method");
				Assert.AreEqual(190, xamlc.GetMethodBody().GetILAsByteArray().Length, "Method body should be 190 bytes long");
			}

			if ((inflator & XamlInflator.SourceGen) == XamlInflator.SourceGen)
			{
				var sourcegen = type.GetMethod("InitializeComponentSourceGen", BindingFlags.Instance | BindingFlags.NonPublic);
				Assert.IsNotNull(sourcegen, $"{type.Name} should have InitializeComponentSourceGen method");
#if DEBUG
				Assert.AreEqual(393, sourcegen.GetMethodBody().GetILAsByteArray().Length, "Method body should be 393 bytes long");
#else
				Assert.AreEqual(196, sourcegen.GetMethodBody().GetILAsByteArray().Length, "Method body should be 196 bytes long");
#endif
			}

		}
	}
}