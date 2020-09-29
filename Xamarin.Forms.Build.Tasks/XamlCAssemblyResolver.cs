using System;
using Mono.Cecil;

namespace Xamarin.Forms.Build.Tasks
{
	class XamlCAssemblyResolver : DefaultAssemblyResolver
	{
		public void AddAssembly(string p)
		{
			RegisterAssembly(AssemblyDefinition.ReadAssembly(p, new ReaderParameters
			{
				AssemblyResolver = this
			}));
		}

		public override AssemblyDefinition Resolve(AssemblyNameReference name)
		{
			if (TryResolve(name, out AssemblyDefinition assembly))
				return assembly;
			if (IsMscorlib(name)
				&& (TryResolve(AssemblyNameReference.Parse("mscorlib"), out assembly)
				   || TryResolve(AssemblyNameReference.Parse("netstandard"), out assembly)
				   || TryResolve(AssemblyNameReference.Parse("System.Runtime"), out assembly)))
				return assembly;
			throw new AssemblyResolutionException(name);
		}

		bool TryResolve(AssemblyNameReference assemblyNameReference, out AssemblyDefinition assembly)
		{
			try
			{
				assembly = base.Resolve(assemblyNameReference);
				return true;
			}
			catch (AssemblyResolutionException)
			{
				assembly = null;
				return false;
			}
		}

		static bool IsMscorlib(AssemblyNameReference name)
		{
			return name.Name == "mscorlib"
				   || name.Name == "System.Runtime"
				   || name.Name == "netstandard";
		}
	}
}