using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms.Build.Tasks;
using Mono.Cecil;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public static class MockCompiler
	{
		public static void Compile(Type type)
		{
			MethodDefinition _;
			Compile(type, out _);
		}

		public static void Compile(Type type, out MethodDefinition methdoDefinition)
		{
			methdoDefinition = null;
			var assembly = type.Assembly.Location;
			var refs = from an in type.Assembly.GetReferencedAssemblies()
					   let a = System.Reflection.Assembly.Load(an)
					   select a.Location;

			var xamlc = new XamlCTask {
				Assembly = assembly,
				ReferencePath = string.Join(";", refs),
				KeepXamlResources = true,
				OptimizeIL = true,
				DebugSymbols = false,
				ReadOnly = true,
				Type = type.FullName,
				BuildEngine = new DummyBuildEngine()
			};

			IList<Exception> exceptions;
			if (xamlc.Execute(out exceptions) || exceptions == null || !exceptions.Any()) {
				methdoDefinition = xamlc.InitCompForType;
				return;
			}
			if (exceptions.Count > 1)
				throw new AggregateException(exceptions);
			throw exceptions[0];
		}
	}
}