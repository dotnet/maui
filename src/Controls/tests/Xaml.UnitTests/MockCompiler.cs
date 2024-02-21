using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.MSBuild.UnitTests;
using Mono.Cecil;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public static class MockCompiler
	{
		public static void Compile(Type type, string targetFramework = null)
		{
			Compile(type, out _, targetFramework);
		}

		public static void Compile(Type type, out MethodDefinition methodDefinition, string targetFramework = null)
		{
			methodDefinition = null;
			var assembly = type.Assembly.Location;
			var refs = from an in type.Assembly.GetReferencedAssemblies()
					   let a = System.Reflection.Assembly.Load(an)
					   select a.Location;

			var xamlc = new XamlCTask
			{
				Assembly = assembly,
				ReferencePath = refs.ToArray(),
				KeepXamlResources = true,
				OptimizeIL = true,
				DebugSymbols = false,
				ValidateOnly = true,
				Type = type.FullName,
				TargetFramework = targetFramework,
				BuildEngine = new MSBuild.UnitTests.DummyBuildEngine()
			};

			if (xamlc.Execute(out IList<Exception> exceptions) || exceptions == null || !exceptions.Any())
			{
				methodDefinition = xamlc.InitCompForType;
				return;
			}
			if (exceptions.Count > 1)
				throw new AggregateException(exceptions);
			throw exceptions[0];
		}
	}
}