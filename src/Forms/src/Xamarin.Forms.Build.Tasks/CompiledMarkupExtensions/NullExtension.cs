using System;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Build.Tasks
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