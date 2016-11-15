using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms.Build.Tasks;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public static class MockCompiler
	{
		public static void Compile(Type type)
		{
			var assembly = type.Assembly.Location;
			var refs = from an in type.Assembly.GetReferencedAssemblies()
					   let a = System.Reflection.Assembly.Load(an)
					   select a.Location;

			var xamlc = new XamlCTask {
				Assembly = assembly,
				ReferencePath = string.Join(";", refs),
				KeepXamlResources = true,
				Type = type.FullName
			};

			var exceptions = new List<Exception>();
			if (!xamlc.Execute(exceptions) && exceptions.Any())
				throw exceptions [0];
		}
	}
}