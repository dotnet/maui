using System.Collections.Generic;
using System.Linq;
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

		static bool IsOfAnyType(XmlType xmlType, params string[] types)
		{
			if (types == null || types.Length == 0)
				return false;
			if (xmlType == null)
				return false;
			if (xmlType.NamespaceUri != XamlParser.MauiUri && xmlType.NamespaceUri != XamlParser.MauiGlobalUri)
				return false;
			if (types.Contains(xmlType.Name))
				return true;
			return false;
		}

		public FieldReference GetBindablePropertyFieldReference(string value, ILContext context, ModuleDefinition module, BaseNode node)
		{
			FieldReference bpRef = null;
			XmlType typeName = null;
			string propertyName = null;

			var parts = value.Split('.');
			if (parts.Length == 1)
			{
				var parent = node.Parent?.Parent as IElementNode ?? (node.Parent?.Parent as IListNode)?.Parent as IElementNode;
				if (IsOfAnyType((node.Parent as ElementNode)?.XmlType, nameof(Setter), nameof(PropertyCondition)))
				{
					if (IsOfAnyType(parent.XmlType, nameof(Trigger), nameof(DataTrigger), nameof(MultiTrigger), nameof(Style)))
					{
						typeName = GetTargetTypeName(parent);
					}
					else if (IsOfAnyType(parent.XmlType, nameof(VisualState)))
					{
						typeName = FindTypeNameForVisualState(parent, node, context);
					}
				}
				else if (IsOfAnyType((node.Parent as ElementNode)?.XmlType, nameof(Trigger)))
				{
					typeName = GetTargetTypeName(node.Parent);
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

			static XmlType GetTargetTypeName(INode node)
			{
				var targetType = ((node as ElementNode).Properties[new XmlName("", "TargetType")] as ValueNode)?.Value as string;
				return TypeArgumentsParser.ParseSingle(targetType, node.NamespaceResolver, (IXmlLineInfo)node);
			}
		}

		static XmlType FindTypeNameForVisualState(IElementNode parent, IXmlLineInfo lineInfo, ILContext context)
		{
			//1. parent is VisualState, don't check that

			//2. check that the VS is in a VSG
			// if (!(parent.Parent is IElementNode target) || target.XmlType.NamespaceUri != XamlParser.MauiUri || target.XmlType.Name != nameof(VisualStateGroup))
			if (!(parent.Parent is IElementNode target) || !IsOfAnyType(target.XmlType, nameof(VisualStateGroup)))
				throw new XamlParseException($"Expected {nameof(VisualStateGroup)} but found {parent.Parent}", lineInfo);

			//3. if the VSG is in a VSGL, skip that as it could be implicit
			if (target.Parent is ListNode
				|| IsOfAnyType((target.Parent as IElementNode)?.XmlType, nameof(VisualStateGroupList)))
				target = target.Parent.Parent as IElementNode;
			else
				target = target.Parent as IElementNode;

			//4. target is now a Setter in a Style, or a VE
			if (IsOfAnyType(target.XmlType, nameof(Setter)))
			{
				var targetType = ((target?.Parent as IElementNode)?.Properties[new XmlName("", "TargetType")] as ValueNode)?.Value as string;
				return TypeArgumentsParser.ParseSingle(targetType, parent.NamespaceResolver, lineInfo);
			}
			else
				return target.XmlType;
		}

		public static FieldReference GetBindablePropertyFieldReference(XamlCache cache, TypeReference typeRef, string propertyName, ModuleDefinition module)
		{
			TypeReference declaringTypeReference;
			FieldReference bpRef = typeRef.GetField(cache, fd => fd.Name == $"{propertyName}Property" && fd.IsStatic && fd.IsPublic, out declaringTypeReference);
			if (bpRef != null)
			{
				bpRef = module.ImportReference(bpRef.ResolveGenericParameters(declaringTypeReference));
				bpRef.FieldType = module.ImportReference(bpRef.FieldType);
			}
			return bpRef;
		}
	}
}