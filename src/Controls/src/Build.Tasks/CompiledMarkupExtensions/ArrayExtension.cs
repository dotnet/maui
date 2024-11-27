using System.Collections.Generic;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace Microsoft.Maui.Controls.Build.Tasks
{
	class ArrayExtension : ICompiledMarkupExtension
	{
		public IEnumerable<Instruction> ProvideValue(IElementNode node, ModuleDefinition module, ILContext context, out TypeReference memberRef)
		{
			var typeNode = node.Properties[new XmlName("", "Type")] as IElementNode;
			var typeTypeRef = context.TypeExtensions[typeNode];

			memberRef = typeTypeRef.MakeArrayType();
			return ProvideValue(typeTypeRef, node.CollectionItems, module, context);
		}

		IEnumerable<Instruction> ProvideValue(TypeReference typeTypeRef, IReadOnlyList<INode> items, ModuleDefinition module, ILContext context)
		{
			var n = items.Count;

			yield return Instruction.Create(OpCodes.Ldc_I4, n);
			yield return Instruction.Create(OpCodes.Newarr, typeTypeRef);

			for (var i = 0; i < n; i++)
			{
				var node = items[i] as IElementNode;
				var vardef = context.Variables[node];
				var vardefref = new VariableDefinitionReference(vardef);
				context.IL.Append(SetPropertiesVisitor.ProvideValue(vardefref, context, module, node as ElementNode));
				if (vardef != vardefref.VariableDefinition)
				{
					vardef = vardefref.VariableDefinition;
					context.Body.Variables.Add(vardef);
					context.Variables[node] = vardef;
				}

				if (typeTypeRef.IsValueType)
				{
					yield return Instruction.Create(OpCodes.Dup);
					yield return Instruction.Create(OpCodes.Ldc_I4, i);
					yield return Instruction.Create(OpCodes.Ldelema, typeTypeRef);
					foreach (var instruction in vardef.LoadAs(context.Cache, typeTypeRef, module))
						yield return instruction;
					yield return Instruction.Create(OpCodes.Stobj, typeTypeRef);
				}
				else
				{
					yield return Instruction.Create(OpCodes.Dup);
					yield return Instruction.Create(OpCodes.Ldc_I4, i);
					foreach (var instruction in vardef.LoadAs(context.Cache, typeTypeRef, module))
						yield return instruction;
					yield return Instruction.Create(OpCodes.Stelem_Ref);
				}
			}
		}
	}
}