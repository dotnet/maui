using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Build.Tasks
{
	class ArrayExtension : ICompiledMarkupExtension
	{
		public IEnumerable<Instruction> ProvideValue(IElementNode node, ModuleDefinition module, ILContext context, out TypeReference memberRef)
		{
			var typeNode = node.Properties[new XmlName("", "Type")] as IElementNode;
			var typeTypeRef = context.TypeExtensions[typeNode];
			var n = node.CollectionItems.Count;

			var instructions = new List<Instruction>();
			instructions.Add(Instruction.Create(OpCodes.Ldc_I4, n));
			instructions.Add(Instruction.Create(OpCodes.Newarr, typeTypeRef));

			memberRef = typeTypeRef.MakeArrayType();
			for (var i = 0; i < n; i++) {
				instructions.Add(Instruction.Create(OpCodes.Dup));
				instructions.Add(Instruction.Create(OpCodes.Ldc_I4, i));
				instructions.Add(Instruction.Create(OpCodes.Ldloc, context.Variables[node.CollectionItems[i] as IElementNode]));
				instructions.Add(Instruction.Create(OpCodes.Stelem_Ref));
			}
			return instructions;
		}
	}
}