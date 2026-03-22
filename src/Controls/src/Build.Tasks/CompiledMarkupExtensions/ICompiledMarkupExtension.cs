using System.Collections.Generic;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Microsoft.Maui.Controls.Build.Tasks
{
	interface ICompiledMarkupExtension
	{
		IEnumerable<Instruction> ProvideValue(ElementNode node, ModuleDefinition module, ILContext context, out TypeReference typeRef);
	}
}