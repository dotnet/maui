using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Build.Tasks;
using Mono.Cecil;
using NUnit.Framework;
using IOPath = System.IO.Path;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[TestFixture]
	public class CecilExtensionsTests : IAssemblyResolver
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

		[SetUp]
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

		[TearDown]
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
			// "IsCompiledDefault",
			"X2006Namespace",
			"X2009Primitives",
			"Validation.MissingXClass.rtxc",
		};

		[Test, TestCaseSource(nameof(IsXamlTrueSource))]
		public void IsXamlTrue(string name)
		{
			var resource = GetResource(name);
			Assert.IsTrue(resource.IsXaml(new XamlCache(), assembly.MainModule, out string className), $"IsXaml should return true for '{name}'.");
			if (!className.StartsWith("__XamlGeneratedCode__"))
				Assert.AreEqual(className, $"{testNamespace}.{name}"); // Test cases x:Class matches the file name
		}

		static string[] IsXamlFalseSource = new[]
		{
			"Validation.NotXaml",
		};

		[Test, TestCaseSource(nameof(IsXamlFalseSource))]
		public void IsXamlFalse(string name)
		{
			var resource = GetResource(name);
			Assert.IsFalse(resource.IsXaml(new XamlCache(), assembly.MainModule, out _), $"IsXaml should return false for '{name}'.");
		}
	}
}
