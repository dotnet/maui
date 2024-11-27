using System.Collections.Generic;
using System.Xml;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Microsoft.Maui.Controls.XamlC
{
	class SetterValueProvider : ICompiledValueProvider
	{
		public IEnumerable<Instruction> ProvideValue(VariableDefinitionReference vardefref, ModuleDefinition module, BaseNode node, ILContext context)
		{
			INode valueNode = null;
			if (!((IElementNode)node).Properties.TryGetValue(new XmlName("", "Value"), out valueNode) &&
				!((IElementNode)node).Properties.TryGetValue(new XmlName(XamlParser.MauiUri, "Value"), out valueNode) &&
				((IElementNode)node).CollectionItems.Count == 1)
				valueNode = ((IElementNode)node).CollectionItems[0];

			var bpNode = ((ValueNode)((IElementNode)node).Properties[new XmlName("", "Property")]);
			var bpRef = (new BindablePropertyConverter()).GetBindablePropertyFieldReference((string)bpNode.Value, context, module, bpNode);

			if (SetterValueIsCollection(bpRef, module, node, context))
				yield break;

			// valueNode is null, for example, when OnPlatform doesn't have a match for the current platform, so the property should not be set
			if (valueNode == null)
				yield break;

			//if it's an elementNode, there's probably no need to convert it
			if (valueNode is IElementNode)
				yield break;

			var value = ((string)((ValueNode)valueNode).Value);
			var setterType = ("Microsoft.Maui.Controls", "Microsoft.Maui.Controls", "Setter");

			//push the setter
			foreach (var instruction in vardefref.VariableDefinition.LoadAs(context.Cache, module.GetTypeDefinition(context.Cache, setterType), module))
				yield return instruction;

			//push the value
			foreach (var instruction in ((ValueNode)valueNode).PushConvertedValue(context, bpRef, (requiredServices) => valueNode.PushServiceProvider(context, requiredServices, bpRef: bpRef), boxValueTypes: true, unboxValueTypes: false))
				yield return instruction;

			//set the value
			yield return Instruction.Create(OpCodes.Callvirt, module.ImportPropertySetterReference(context.Cache, setterType, propertyName: "Value"));
		}

		static bool SetterValueIsCollection(FieldReference bindablePropertyReference, ModuleDefinition module, BaseNode node, ILContext context)
		{
			var items = (node as IElementNode)?.CollectionItems;

			if (items == null || items.Count <= 0)
				return false;

			// Is this a generic type ?
			var generic = bindablePropertyReference.GetBindablePropertyType(context.Cache, node, module) as GenericInstanceType;

			// With a single generic argument?
			if (generic?.GenericArguments.Count != 1)
				return false;

			// Is the generic argument assignable from this value?
			var genericType = generic.GenericArguments[0];

			if (!(items[0] is IElementNode firstItem))
				return false;

			return context.Variables[firstItem].VariableType.InheritsFromOrImplements(context.Cache, genericType);
		}
	}
}