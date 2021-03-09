using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.Build.Tasks
{
	interface ICompiledMarkupExtension
	{
		IEnumerable<Instruction> ProvideValue(IElementNode node, ModuleDefinition module, ILContext context, out TypeReference typeRef);
	}
}