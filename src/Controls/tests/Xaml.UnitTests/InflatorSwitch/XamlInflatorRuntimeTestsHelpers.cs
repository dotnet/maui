using System;
using System.Reflection;
using NUnit.Framework;
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class XamlInflatorRuntimeTestsHelpers
{
	public static void TestInflator(Type type, XamlInflator inflator, bool generateinflatorswitch = false)
	{
		if (!generateinflatorswitch)
			Assert.IsNull(type.GetMethod("InitializeComponentSourceGen", BindingFlags.Instance | BindingFlags.NonPublic), $"{type.Name} should not have InitializeComponentSourceGen method");

		Assert.IsNull(type.GetMethod("__InitComponentRuntime", BindingFlags.Instance | BindingFlags.NonPublic), $"{type.Name} should not have __InitComponentRuntime method");

		//check that there is an InitializeComponent method that takes an argument of type XamlInflator
		if (generateinflatorswitch)
			Assert.IsNotNull(type.GetConstructor([typeof(XamlInflator)]), $"{type.Name} should have InitializeComponent method with XamlInflator argument");

		Assert.IsNotNull(type.GetMethod("InitializeComponent", BindingFlags.Instance | BindingFlags.NonPublic), $"{type.Name} should have InitializeComponent method");

		var body = type.GetMethod("InitializeComponent", BindingFlags.Instance | BindingFlags.NonPublic).GetMethodBody();

		//quite bad heuristics. this should use Cecil, and check for known calls, like LoadFromXaml, etc
		var instructions = body.GetILAsByteArray();
		if ((inflator & XamlInflator.Runtime) == XamlInflator.Runtime)
			Assert.AreEqual(36, instructions.Length, "Method body should be 36 bytes long");
		if ((inflator & XamlInflator.SourceGen) == XamlInflator.SourceGen)
			Assert.AreEqual(267, instructions.Length, "Method body should be 267 bytes long");
		if ((inflator & XamlInflator.XamlC) == XamlInflator.XamlC)
			Assert.AreEqual(190, instructions.Length, "Method body should be 190 bytes long");

		if (generateinflatorswitch)
		{
			var runtime = type.GetMethod("InitializeComponentRuntime", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.IsNotNull(runtime, $"{type.Name} should have InitializeComponentRuntime method");
			Assert.AreEqual(36, runtime.GetMethodBody().GetILAsByteArray().Length, "Method body should be 36 bytes long");

			var xamlc = type.GetMethod("InitializeComponentXamlC", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.IsNotNull(xamlc, $"{type.Name} should have InitializeComponentXamlC method");
			Assert.AreEqual(190, xamlc.GetMethodBody().GetILAsByteArray().Length, "Method body should be 190 bytes long");

			var sourcegen = type.GetMethod("InitializeComponentSourceGen", BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.IsNotNull(sourcegen, $"{type.Name} should have InitializeComponentSourceGen method");
			Assert.AreEqual(267, sourcegen.GetMethodBody().GetILAsByteArray().Length, "Method body should be 267 bytes long");	

			Assert.AreEqual(8, instructions.Length, "Method body should be 20 bytes long");
		}
	}
}