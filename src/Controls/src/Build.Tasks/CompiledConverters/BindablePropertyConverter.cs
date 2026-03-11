using System.Collections.Generic;
using System.Xml;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil;
using Mono.Cecil.Cil;
using static System.String;
using static Microsoft.Maui.Controls.Build.Tasks.BuildExceptionCode;

namespace Microsoft.Maui.Controls.XamlC
{
	class BindablePropertyConverter : ICompiledTypeConverter
	{
		public IEnumerable<Instruction> ConvertFromString(string value, ILContext context, BaseNode node)
		{
			var module = context.Body.Method.Module;

			if (IsNullOrEmpty(value))
			{
				yield return Instruction.Create(OpCodes.Ldnull);
				yield break;
			}
			var bpRef = GetBindablePropertyFieldReference(value, context, module, node);
			yield return Instruction.Create(OpCodes.Ldsfld, bpRef);
		}

		public FieldReference GetBindablePropertyFieldReference(string value, ILContext context, ModuleDefinition module, BaseNode node)
		{
			FieldReference bpRef = null;
			XmlType typeName = null;
			string propertyName = null;

			var parts = value.Split('.');
			if (parts.Length == 1)
			{
				var parent = node.Parent?.Parent as ElementNode ?? (node.Parent?.Parent as IListNode)?.Parent as ElementNode;
				if (node.Parent is ElementNode { XmlType: XmlType xt } && xt.IsOfAnyType(nameof(Setter), nameof(PropertyCondition)))				
				{
					if (parent.XmlType.IsOfAnyType(nameof(Trigger), nameof(DataTrigger), nameof(MultiTrigger), nameof(Style)))
					{
						typeName = GetTargetTypeFromElement(parent, node.NamespaceResolver, (IXmlLineInfo)node);
					}
					else if (parent.XmlType.IsOfAnyType(nameof(VisualState)))
					{
						typeName = FindTypeNameForVisualState(parent, node, context);
					}
				}
				else if (node.Parent is ElementNode { XmlType: XmlType xt1 } && xt1.IsOfAnyType(nameof(Trigger)))
				{
					typeName = GetTargetTypeFromElement(node.Parent as ElementNode, node.NamespaceResolver, (IXmlLineInfo)node);
				}
				propertyName = parts[0];
			}
			else if (parts.Length == 2)
			{
				var targetType = parts[0];
				typeName = TypeArgumentsParser.ParseSingle(targetType, node.NamespaceResolver, (IXmlLineInfo)node);
				propertyName = parts[1];
			}
			else
				throw new BuildException(Conversion, node, null, value, typeof(BindableProperty));

			if (typeName == null || propertyName == null)
				throw new BuildException(Conversion, node, null, value, typeof(BindableProperty));

			var typeRef = typeName.GetTypeReference(context.Cache, module, node);
			if (typeRef == null)
				throw new BuildException(TypeResolution, node, null, typeName);

			bpRef = GetBindablePropertyFieldReference(context.Cache, typeRef, propertyName, module);
			if (bpRef == null)
				throw new BuildException(PropertyResolution, node, null, propertyName, typeRef.Name);
			return bpRef;
		}

		static XmlType FindTypeNameForVisualState(ElementNode parent, IXmlLineInfo lineInfo, ILContext context)
		{
			// 1. parent is VisualState, don't check that

			// 2. check that the VS is in a VSG
			if (parent.Parent is not ElementNode target || !target.XmlType.IsOfAnyType(nameof(VisualStateGroup)))
				throw new XamlParseException($"Expected {nameof(VisualStateGroup)} but found {parent.Parent}", lineInfo);

			// 3. if the VSG is in a VSGL, skip that as it could be implicit
			if (target.Parent is ListNode
				|| target.Parent is ElementNode { XmlType: XmlType xt } && xt.IsOfAnyType(nameof(VisualStateGroupList)))
				target = target.Parent.Parent as ElementNode;
			else
				target = target.Parent as ElementNode;

			// 4. target is now a Setter in a Style, or a VE
			if (target.XmlType.IsOfAnyType(nameof(Setter)))
				return GetTargetTypeFromElement(target?.Parent as ElementNode, parent.NamespaceResolver, lineInfo);

			return target.XmlType;
		}

		/// <summary>
		/// Extracts the TargetType attribute from an element node and parses it as an XmlType.
		/// Returns null if the element is null, has no TargetType, or TargetType is empty.
		/// </summary>
		static XmlType GetTargetTypeFromElement(ElementNode element, IXmlNamespaceResolver namespaceResolver, IXmlLineInfo lineInfo)
		{
			if (element?.Properties.TryGetValue(new XmlName("", "TargetType"), out var targetTypeNode) != true)
				return null;

			var targetType = (targetTypeNode as ValueNode)?.Value as string;
			if (string.IsNullOrEmpty(targetType))
				return null;

			return TypeArgumentsParser.ParseSingle(targetType, namespaceResolver, lineInfo);
		}

		public static FieldReference GetBindablePropertyFieldReference(XamlCache cache, TypeReference typeRef, string propertyName, ModuleDefinition module)
		{
			FieldReference bpRef = typeRef.GetField(cache, fd => fd.Name == $"{propertyName}Property" && fd.IsStatic && fd.IsPublic, out TypeReference declaringTypeReference);
			if (bpRef != null)
			{
				bpRef = module.ImportReference(bpRef.ResolveGenericParameters(declaringTypeReference));
				bpRef.FieldType = module.ImportReference(bpRef.FieldType);
			}
			return bpRef;
		}
	}
}