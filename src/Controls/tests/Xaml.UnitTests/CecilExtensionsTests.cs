using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Build.Tasks;
using Mono.Cecil;
using Xunit;
using IOPath = System.IO.Path;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{	public class CecilExtensionsTests : IAssemblyResolver
	{
		const string testNamespace = "Microsoft.Maui.Controls.Xaml.UnitTests";
		AssemblyDefinition assembly;
		readonly List<AssemblyDefinition> assemblies = new List<AssemblyDefinition>();
		readonly ReaderParameters readerParameters;

		public CecilExtensionsTests()
		{
			readerParameters = new ReaderParameters
			{
				AssemblyResolver = this,
			};
		}

		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
		public void SetUp()
		{
			assembly = AssemblyDefinition.ReadAssembly(GetType().Assembly.Location, readerParameters);
			assemblies.Add(assembly);
		}

		public AssemblyDefinition Resolve(AssemblyNameReference name)
		{
			var path = IOPath.Combine(IOPath.GetDirectoryName(GetType().Assembly.Location), name.Name + ".dll");
			var assembly = AssemblyDefinition.ReadAssembly(path, readerParameters);
			assemblies.Add(assembly);
			return assembly;
		}

		public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
		{
			var path = IOPath.Combine(IOPath.GetDirectoryName(GetType().Assembly.Location), name.Name + ".dll");
			var assembly = AssemblyDefinition.ReadAssembly(path, parameters);
			assemblies.Add(assembly);
			return assembly;
		}

		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown]
		public void Dispose()
		{
			foreach (var assembly in assemblies)
			{
				assembly.Dispose();
			}
			assemblies.Clear();
		}

		EmbeddedResource GetResource(string name)
		{
			var resourceName = $"{testNamespace}.{name}.xaml";
			foreach (EmbeddedResource res in assembly.MainModule.Resources)
			{
				if (res.Name == resourceName)
					return res;
			}
			throw new InvalidOperationException($"Resource '{resourceName}' not found in assembly '{assembly.Name.Name}'.");
		}

		static string[] IsXamlTrueSource = new[]
		{
			"IsCompiledDefault",
			"X2006Namespace",
			"X2009Primitives",
			"Validation.MissingXClass",
		};

		[Theory, MemberData(nameof(IsXamlTrueSource))]
		public void IsXamlTrue(string name)
		{
			var resource = GetResource(name);
			Assert.True(resource.IsXaml(new XamlCache(), assembly.MainModule, out string className), $"IsXaml should return true for '{name}'.");
			if (!className.StartsWith("__XamlGeneratedCode__"))
				Assert.Equal(className, $"{testNamespace}.{name}"); // Test cases x:Class matches the file name;
		}

		static string[] IsXamlFalseSource = new[]
		{
			"Validation.NotXaml",
		};

		[Theory, MemberData(nameof(IsXamlFalseSource))]
		public void IsXamlFalse(string name)
		{
			var resource = GetResource(name);
			Assert.False(resource.IsXaml(new XamlCache(), assembly.MainModule, out _), $"IsXaml should return false for '{name}'.");
		}
	}
}
