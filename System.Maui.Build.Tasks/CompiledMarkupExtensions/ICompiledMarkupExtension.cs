using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Maui.Xaml;

namespace System.Maui.Build.Tasks
{
	interface ICompiledMarkupExtension
	{
		IEnumerable<Instruction> ProvideValue(IElementNode node, ModuleDefinition module, ILContext context, out TypeReference typeRef);
	}
}