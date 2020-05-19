using Mono.Cecil;
using NUnit.Framework;
using System.IO;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class MockAssemblyResolver : BaseAssemblyResolver
	{
		public override AssemblyDefinition Resolve(AssemblyNameReference name)
		{
			AssemblyDefinition assembly;
			var localPath = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.TestDirectory, $"{name.Name}.dll"));
			if (File.Exists(localPath))
				assembly = AssemblyDefinition.ReadAssembly(localPath);
			else
				assembly = base.Resolve(name);
			return assembly;
		}
	}
}