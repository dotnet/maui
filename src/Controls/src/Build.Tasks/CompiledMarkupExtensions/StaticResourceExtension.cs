using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil;
using Mono.Cecil.Cil;
using static Mono.Cecil.Cil.Instruction;
using static Mono.Cecil.Cil.OpCodes;

namespace Microsoft.Maui.Controls.Build.Tasks
{
	//yes, this is a ICompiledMarkupExtension, but declared as ICompiledValueProvider so it's evaluated later (in SetPropertyValue, not CreateObject)
	class StaticResourceExtension : ICompiledValueProvider
	{
		public IEnumerable<Instruction> ProvideValue(VariableDefinitionReference vardefref, ModuleDefinition module, BaseNode node, ILContext context)
		{
			var name = new XmlName("", "Key");
			var eNode = node as ElementNode;

			if (!eNode.Properties.TryGetValue(name, out INode keyNode) && eNode.CollectionItems.Any())
				keyNode = eNode.CollectionItems[0];

			if (!(keyNode is ValueNode keyValueNode))
				throw new BuildException(BuildExceptionCode.StaticResourceSyntax, eNode as IXmlLineInfo, null, null);

			var n = eNode;
			while (n != null)
			{
				if (n.Properties.TryGetValue(new XmlName(XamlParser.MauiUri, "Resources"), out var resourcesNode))
				{
					//single resource in <Resources>
					if (resourcesNode is IElementNode irn
						&& irn.Properties.TryGetValue(XmlName.xKey, out INode xKeyNode)
						&& context.Variables.ContainsKey(irn)
						&& xKeyNode is ValueNode xKeyValueNode
						&& xKeyValueNode.Value as string == keyValueNode.Value as string)
					{
						if (context.Variables[resourcesNode as IElementNode].VariableType.FullName == "System.String")
						{
							foreach (var instruction in TryConvert(irn.CollectionItems[0] as ValueNode, eNode, vardefref, module, context))
								yield return instruction;
							yield break;
						}

						vardefref.VariableDefinition = context.Variables[irn];
						yield break;
					}
					//multiple resources in <Resources>
					else if (resourcesNode is ListNode lr)
					{
						foreach (var rn in lr.CollectionItems)
						{
							if (rn is IElementNode irn2
								&& irn2.Properties.TryGetValue(XmlName.xKey, out INode xKeyNode2)
								&& context.Variables.ContainsKey(irn2)
								&& xKeyNode2 is ValueNode xKeyValueNode2
								&& xKeyValueNode2.Value as string == keyValueNode.Value as string)
							{
								if (irn2.CollectionItems.Count == 1 && irn2.CollectionItems[0] is ValueNode vn2 && vn2.Value is string)
								{
									foreach (var instruction in TryConvert(vn2, eNode, vardefref, module, context))
										yield return instruction;
									yield break;
								}

								vardefref.VariableDefinition = context.Variables[irn2];
								yield break;
							}
						}
					}
					//explicit ResourceDictionary in Resources
					else if (resourcesNode is IElementNode resourceDictionary
							&& resourceDictionary.XmlType.Name == "ResourceDictionary")
					{
						foreach (var rn in resourceDictionary.CollectionItems)
						{
							if (rn is IElementNode irn3
								&& irn3.Properties.TryGetValue(XmlName.xKey, out INode xKeyNode3)
								&& irn3.XmlType.Name != "OnPlatform"
								&& context.Variables.ContainsKey(irn3)
								&& xKeyNode3 is ValueNode xKeyValueNode3
								&& xKeyValueNode3.Value as string == keyValueNode.Value as string)
							{
								if (irn3.CollectionItems.Count == 1 && irn3.CollectionItems[0] is ValueNode vn3 && vn3.Value is string)
								{
									foreach (var instruction in TryConvert(vn3, eNode, vardefref, module, context))
										yield return instruction;
									yield break;
								}

								vardefref.VariableDefinition = context.Variables[irn3];
								yield break;
							}
						}
					}
				}

				n = n.Parent as ElementNode;
			}


			//Fallback
			foreach (var instruction in FallBack(keyValueNode.Value as string, eNode, module, context).ToList())
				yield return instruction;

			var vardef = new VariableDefinition(module.TypeSystem.Object);
			yield return Create(Stloc, vardef);
			vardefref.VariableDefinition = vardef;
		}

		public static IEnumerable<Instruction> TryConvert(ValueNode stringResourceNode, IElementNode node, VariableDefinitionReference vardefref, ModuleDefinition module, ILContext context)
		{
			XmlName propertyName = XmlName.Empty;
			SetPropertiesVisitor.TryGetPropertyName(node, node.Parent, out propertyName);
			var localName = propertyName.LocalName;
			var parentType = module.ImportReference((node.Parent as IElementNode).XmlType.GetTypeReference(context.Cache, module, (IXmlLineInfo)node));

			var bpRef = SetPropertiesVisitor.GetBindablePropertyReference(parentType, propertyName.NamespaceURI, ref localName, out _, context, (IXmlLineInfo)node);
			//BindableProperty
			if (bpRef != null)
			{
				var targetTypeRef = module.ImportReference(bpRef.GetBindablePropertyType(context.Cache, node as IXmlLineInfo, module));
				foreach (var instruction in stringResourceNode.PushConvertedValue(context, bpRef, requiredServices => stringResourceNode.PushServiceProvider(context, requiredServices, bpRef: bpRef), true, false))
					yield return instruction;
				var vardef = new VariableDefinition(targetTypeRef);
				yield return Create(Stloc, vardef);
				vardefref.VariableDefinition = vardef;
				yield break;
			}

			var propertyRef = parentType.GetProperty(context.Cache, pd => pd.Name == localName, out var declaringTypeReference);
			if (propertyRef != null)
			{
				var propertyType = propertyRef.PropertyType.ResolveGenericParameters(declaringTypeReference);

				foreach (var instruction in stringResourceNode.PushConvertedValue(
						context,
						propertyType,
						[propertyRef, propertyType.ResolveCached(context.Cache)],
						requiredServices => stringResourceNode.PushServiceProvider(context, requiredServices, propertyRef: propertyRef),
						boxValueTypes: true,
						unboxValueTypes: false))
					yield return instruction;
				var vardef = new VariableDefinition(propertyType);
				yield return Create(Stloc, vardef);
				vardefref.VariableDefinition = vardef;
				yield break;
			}
		}

		public static IEnumerable<Instruction> FallBack(string key, IElementNode node, ModuleDefinition module, ILContext context)
		{
			var staticResourceExtensionType = module.ImportReference(context.Cache,
																	("Microsoft.Maui.Controls.Xaml", "Microsoft.Maui.Controls.Xaml", "StaticResourceExtension"));
			yield return Create(Newobj, module.ImportCtorReference(context.Cache,
																	staticResourceExtensionType,
																	paramCount: 0));

			SetPropertiesVisitor.TryGetPropertyName(node, node.Parent, out var propertyName);
			var localName = propertyName.LocalName;

			//Set the Key
			var keyProperty = staticResourceExtensionType.GetProperty(context.Cache, pd => pd.Name == "Key", out _);
			yield return Create(Dup);
			yield return Create(Ldstr, key);
			yield return Create(Callvirt, module.ImportReference(keyProperty.SetMethod));

			FieldReference bpRef = null;
			PropertyDefinition propertyRef = null;
			TypeReference declaringTypeReference = null;
			if (node.Parent is IElementNode parentNode && propertyName != XmlName.Empty)
			{
				var parentType = module.ImportReference(parentNode.XmlType.GetTypeReference(context.Cache, module, (IXmlLineInfo)node));
				bpRef = SetPropertiesVisitor.GetBindablePropertyReference(parentType,
																		propertyName.NamespaceURI,
																		ref localName,
																		out _,
																		context,
																		(IXmlLineInfo)node);
				propertyRef = parentType.GetProperty(context.Cache, pd => pd.Name == localName, out declaringTypeReference);

			}

			var requiredServices = staticResourceExtensionType.GetRequiredServices(context.Cache, module);
			foreach (var instruction in node.PushServiceProvider(context, requiredServices, bpRef, propertyRef, declaringTypeReference))
				yield return instruction;

			yield return Create(Callvirt, module.ImportMethodReference(context.Cache,
																	   ("Microsoft.Maui.Controls.Xaml", "Microsoft.Maui.Controls.Xaml", "StaticResourceExtension"),
																	   methodName: "ProvideValue",
																	   parameterTypes: new[] { ("System.ComponentModel", "System", "IServiceProvider") }));
		}
	}
}