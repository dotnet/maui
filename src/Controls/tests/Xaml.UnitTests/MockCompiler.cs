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
		public static void Compile(
			Type type,
			out bool hasLoggedErrors,
			string targetFramework = null,
			bool treatWarningsAsErrors = false,
			bool compileBindingsWithSource = true)
			=> Compile(type, out _, out hasLoggedErrors, targetFramework, treatWarningsAsErrors, compileBindingsWithSource);

		public static void Compile(
			Type type,
			string targetFramework = null,
			bool treatWarningsAsErrors = false,
			bool compileBindingsWithSource = true)
		{
			Compile(type, out _, out var hasLoggedErrors, targetFramework, treatWarningsAsErrors, compileBindingsWithSource);
			if (hasLoggedErrors)
				throw new Exception("XamlC failed");
		}

		public static void Compile(
			Type type,
			out MethodDefinition methodDefinition,
			out bool hasLoggedErrors,
			string targetFramework = null,
			bool treatWarningsAsErrors = false,
			bool compileBindingsWithSource = true,
			bool generateFullIl = true)
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
				GenerateFullILInValidateOnlyMode = generateFullIl,
				Type = type.FullName,
				TargetFramework = targetFramework,
				TreatWarningsAsErrors = treatWarningsAsErrors,
				CompileBindingsWithSource = compileBindingsWithSource,
				BuildEngine = new MSBuild.UnitTests.DummyBuildEngine()
			};

			if (xamlc.Execute(out IList<Exception> exceptions) || exceptions == null || !exceptions.Any())
			{
				methodDefinition = xamlc.InitCompForType;
				hasLoggedErrors = xamlc.LoggingHelper.HasLoggedErrors;
				return;
			}
			if (exceptions.Count > 1)
				throw new AggregateException(exceptions);
			throw exceptions[0];
		}
	}
}