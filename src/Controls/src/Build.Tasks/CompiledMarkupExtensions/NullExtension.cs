using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Microsoft.Maui.Controls.Build.Tasks
{
	class NullExtension : ICompiledMarkupExtension
	{

		public IEnumerable<Instruction> ProvideValue(IElementNode node, ModuleDefinition module, ILContext context, out TypeReference typeRef)
		{
			typeRef = module.TypeSystem.Object;
			return new[] { Instruction.Create(OpCodes.Ldnull) };
		}
	}
}