using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Xunit.Abstractions;

#nullable enable
namespace Microsoft.Maui.Controls.SourceGen.Tests
{
	public abstract class BaseSourceGeneratorTests<T> : IDisposable where T : class, ISourceGenerator, new()
	{
		protected ITestOutputHelper Output { get; }
		protected AssemblyGenerator Generator { get; }
		protected Compilation? Compilation { get; set; }


		protected BaseSourceGeneratorTests(ITestOutputHelper output, params string[] assemblies)
		{
			this.Output = output;
			this.Generator = new AssemblyGenerator();
			this.Generator.AddReferences(assemblies);
		}


		public void Dispose()
		{
			if (this.Compilation != null)
				foreach (var syntaxTree in this.Compilation.SyntaxTrees)
					this.Output.WriteLine(syntaxTree.ToString());
		}


		protected virtual void RunGenerator([CallerMemberName] string? compileAssemblyName = null)
		{
			var sourceGenerator = this.Create();
			this.Compilation = this.Generator.Generate(
				compileAssemblyName ?? "TestAssembly",
				sourceGenerator
			);
		}


		protected virtual T Create() => new T();
	}
}
