using System;
using System.Reflection;
using System.Linq;

using NUnit.Framework;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[TestFixture]
	public class XFCorePostProcessorCodeInjected
	{
		[Test]
		public void InjectedCodeIsPresent()
		{
			var resLoader = typeof(Xamarin.Forms.Internals.ResourceLoader);
			Assert.True(resLoader.GetMethods().Any(mi => mi.Name == "get_ResourceProvider" && mi.ReturnType == typeof(Func<string, string>)));
			Assert.True(resLoader.GetMethods().Any(mi => mi.Name == "get_ResourceProvider" && mi.ReturnType == typeof(Func<AssemblyName, string, string>)));
			Assert.True(resLoader.GetProperty("ResourceProvider") != null);
			Assert.True(resLoader.GetProperty("ResourceProvider").GetMethod.ReturnType == typeof(Func<AssemblyName, string, string>));
		}
	}
}
