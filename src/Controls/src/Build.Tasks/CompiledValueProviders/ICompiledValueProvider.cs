using System.Collections.Generic;
using Microsoft.Maui.Controls.Build.Tasks;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Microsoft.Maui.Controls.Xaml
{
	interface ICompiledValueProvider
	{
		IEnumerable<Instruction> ProvideValue(VariableDefinitionReference vardefref, ModuleDefinition module, BaseNode node, ILContext context);
	}
}