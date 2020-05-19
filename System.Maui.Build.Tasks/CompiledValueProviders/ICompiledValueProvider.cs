using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Maui.Build.Tasks;

namespace System.Maui.Xaml
{
	interface ICompiledValueProvider
	{
		IEnumerable<Instruction> ProvideValue(VariableDefinitionReference vardefref, ModuleDefinition module, BaseNode node, ILContext context);
	}
}