using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Microsoft.Maui.Controls.Build.Tasks;

namespace Microsoft.Maui.Controls.Xaml
{
	interface ICompiledValueProvider
	{
		IEnumerable<Instruction> ProvideValue(VariableDefinitionReference vardefref, ModuleDefinition module, BaseNode node, ILContext context);
	}
}