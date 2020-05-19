using System.Collections.Generic;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Xamarin.Forms.Build.Tasks;

namespace Xamarin.Forms.Xaml
{
	interface ICompiledValueProvider
	{
		IEnumerable<Instruction> ProvideValue(VariableDefinitionReference vardefref, ModuleDefinition module, BaseNode node, ILContext context);
	}
}