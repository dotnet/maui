using System;
using Microsoft.Maui.Controls.Build.Tasks;
using Mono.Cecil;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.XamlcUnitTests
{
	[TestFixture]
	public class ModuleDefinitionExtensionsTests
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

		[SetUp]
		public void SetUp()
		{
			resolver = new XamlCAssemblyResolver();
			resolver.AddAssembly(Uri.UnescapeDataString((new UriBuilder(typeof(ModuleDefinitionExtensionsTests).Assembly.CodeBase)).Path));
			resolver.AddAssembly(Uri.UnescapeDataString((new UriBuilder(typeof(byte).Assembly.CodeBase)).Path));

			module = ModuleDefinition.CreateModule("foo", new ModuleParameters
			{
				AssemblyResolver = resolver,
				Kind = ModuleKind.Dll
			});
		}

		[TearDown]
		public void TearDown()
		{
			resolver?.Dispose();
			module?.Dispose();
		}

		[Test]
		public void TestImportCtorReferenceWithGenericInstanceCtorParameter()
		{
			var type = module.ImportReference(typeof(WithGenericInstanceCtorParameter));
			var byteTuple = module.ImportReference(typeof(Tuple<byte>));
			var byteTupleCtor = module.ImportCtorReference(type, new[] { byteTuple });
			var int16Tuple = module.ImportReference(typeof(Tuple<short>));
			var int16TupleCtor = module.ImportCtorReference(type, new[] { int16Tuple });

			Assert.AreEqual("System.Tuple`1<System.Byte>", byteTupleCtor.Parameters[0].ParameterType.FullName);
			Assert.AreEqual("System.Tuple`1<System.Int16>", int16TupleCtor.Parameters[0].ParameterType.FullName);
		}

		[Test]
		public void TestImportCtorReferenceWithGenericInstanceTypeParameter()
		{
			var byteTuple = module.ImportReference(typeof(Tuple<byte>));
			var byteTupleCtor = module.ImportCtorReference(("mscorlib", "System", "Tuple`1"), 1, new[] { byteTuple });
			var in16Tuple = module.ImportReference(typeof(Tuple<short>));
			var int16TupleCtor = module.ImportCtorReference(("mscorlib", "System", "Tuple`1"), 1, new[] { in16Tuple });

			Assert.AreEqual("System.Tuple`1<System.Byte>", ((GenericInstanceType)byteTupleCtor.DeclaringType).GenericArguments[0].FullName);
			Assert.AreEqual("System.Tuple`1<System.Int16>", ((GenericInstanceType)int16TupleCtor.DeclaringType).GenericArguments[0].FullName);
		}
	}
}
