using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit.Abstractions;

#nullable enable
namespace Microsoft.Maui.TestUtils.SourceGen
{
	public abstract class BaseSourceGeneratorTests<T> : IDisposable where T : class, ISourceGenerator, new()
	{
		protected ITestOutputHelper Output { get; }
		protected AssemblyGenerator Generator { get; }
		protected Compilation? Compilation { get; set; }


		protected BaseSourceGeneratorTests(ITestOutputHelper output)
		{
			Output = output;
			Generator = new AssemblyGenerator();
		}


		public void Dispose()
		{
			if (Compilation != null)
			{
				foreach (var syntaxTree in Compilation.SyntaxTrees)
				{
					Output.WriteLine(syntaxTree.FilePath);
					Output.WriteLine("===================START====================");
					Output.WriteLine(syntaxTree.ToString());
					Output.WriteLine("====================END=====================");
					Output.WriteLine("");
				}
			}
		}


		protected virtual void RunGenerator([CallerMemberName] string? compileAssemblyName = null)
		{
			var sourceGenerator = Create();
			Compilation = Generator.Generate(
				compileAssemblyName ?? "AssemblyToTest",
				sourceGenerator
			);
		}


		protected virtual T Create() => new T();
	}
}
