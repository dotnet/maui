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
	}
}