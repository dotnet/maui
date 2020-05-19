using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;

using System.Maui.Xaml;
using System.Maui.Build.Tasks;

namespace System.Maui.Core.XamlC
{
	class PassthroughValueProvider : ICompiledValueProvider
	{
		public IEnumerable<Instruction> ProvideValue(VariableDefinitionReference vardefref, ModuleDefinition module, BaseNode node, ILContext context)
		{
			yield break;
		}
	}
}