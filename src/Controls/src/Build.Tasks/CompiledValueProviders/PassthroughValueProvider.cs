using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.XamlC
{
	class PassthroughValueProvider : ICompiledValueProvider
	{
		public IEnumerable<Instruction> ProvideValue(VariableDefinitionReference vardefref, ModuleDefinition module, BaseNode node, ILContext context)
		{
			yield break;
		}
	}
}