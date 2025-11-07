using System.IO;
using Mono.Cecil;
using IOPath = System.IO.Path;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class MockAssemblyResolver : BaseAssemblyResolver
	{
		public override AssemblyDefinition Resolve(AssemblyNameReference name)
		{
			AssemblyDefinition assembly;
			var assemblyLocation = typeof(MockAssemblyResolver).Assembly.Location;
			var testDirectory = IOPath.GetDirectoryName(assemblyLocation);
			var localPath = IOPath.GetFullPath(IOPath.Combine(testDirectory, $"{name.Name}.dll"));
			if (File.Exists(localPath))
				assembly = AssemblyDefinition.ReadAssembly(localPath);
			else
				assembly = base.Resolve(name);
			return assembly;
		}
	}
}