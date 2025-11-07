using System;
using Microsoft.Maui.Controls.Build.Tasks;
using Mono.Cecil;
using Xunit;

namespace Microsoft.Maui.Controls.XamlcUnitTests
{

	public class ModuleDefinitionExtensionsTests : IDisposable
	{
		class WithGenericInstanceCtorParameter
		{
			public WithGenericInstanceCtorParameter(Tuple<byte> argument)
			{
			}

			public WithGenericInstanceCtorParameter(Tuple<short> argument)
			{
			}
		}

		ModuleDefinition module;
		XamlCAssemblyResolver resolver;

		public ModuleDefinitionExtensionsTests()
		{
			resolver = new XamlCAssemblyResolver();
			resolver.AddAssembly(typeof(ModuleDefinitionExtensionsTests).Assembly.Location);
			resolver.AddAssembly(typeof(byte).Assembly.Location);

			module = ModuleDefinition.CreateModule("foo", new ModuleParameters
			{
				AssemblyResolver = resolver,
				Kind = ModuleKind.Dll
			});
		}

		public void Dispose()
		{
			resolver?.Dispose();
			module?.Dispose();
		}

		[Fact]
		public void TestImportCtorReferenceWithGenericInstanceCtorParameter()
		{
			var cache = new XamlCache();
			var type = module.ImportReference(typeof(WithGenericInstanceCtorParameter));
			var byteTuple = module.ImportReference(typeof(Tuple<byte>));
			var byteTupleCtor = module.ImportCtorReference(cache, type, new[] { byteTuple });
			var int16Tuple = module.ImportReference(typeof(Tuple<short>));
			var int16TupleCtor = module.ImportCtorReference(cache, type, new[] { int16Tuple });

			Assert.Equal("System.Tuple`1<System.Byte>", byteTupleCtor.Parameters[0].ParameterType.FullName);
			Assert.Equal("System.Tuple`1<System.Int16>", int16TupleCtor.Parameters[0].ParameterType.FullName);
		}

		[Fact]
		public void TestImportCtorReferenceWithGenericInstanceTypeParameter()
		{
			var cache = new XamlCache();
			var byteTuple = module.ImportReference(typeof(Tuple<byte>));
			var byteTupleCtor = module.ImportCtorReference(cache, ("mscorlib", "System", "Tuple`1"), 1, new[] { byteTuple });
			var in16Tuple = module.ImportReference(typeof(Tuple<short>));
			var int16TupleCtor = module.ImportCtorReference(cache, ("mscorlib", "System", "Tuple`1"), 1, new[] { in16Tuple });

			Assert.Equal("System.Tuple`1<System.Byte>", ((GenericInstanceType)byteTupleCtor.DeclaringType).GenericArguments[0].FullName);
			Assert.Equal("System.Tuple`1<System.Int16>", ((GenericInstanceType)int16TupleCtor.DeclaringType).GenericArguments[0].FullName);
		}
	}
}
