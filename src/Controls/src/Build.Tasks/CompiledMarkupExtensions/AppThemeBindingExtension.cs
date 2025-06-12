#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Xml;
using Microsoft.Maui.Controls.Xaml;
using Mono.Cecil;
using Mono.Cecil.Cil;
using static Mono.Cecil.Cil.Instruction;
using static Mono.Cecil.Cil.OpCodes;

namespace Microsoft.Maui.Controls.Build.Tasks
{
	//yes, this is a ICompiledMarkupExtension, but declared as ICompiledValueProvider so it's evaluated later (in SetPropertyValue, not CreateObject)
	class AppThemeBindingExtension : ICompiledValueProvider
	{
		public IEnumerable<Instruction> ProvideValue(VariableDefinitionReference vardefref, ModuleDefinition module, BaseNode node, ILContext context)
		{
			ElementNode markupExtensionElementNode = node as ElementNode ?? throw new NotSupportedException();
			IElementNode? parentElement = node.Parent as IElementNode;

			TypeReference appThemeBindingTypeRef = module.ImportReference(typeof(AppThemeBinding));
			MethodReference ctor = GetConstructor(appThemeBindingTypeRef);

			List<Instruction> instructions = [Create(Newobj, ctor)];

			SetPropertyValueIfNeeded("Default");
			SetPropertyValueIfNeeded("Light");
			SetPropertyValueIfNeeded("Dark");

			var vardef = new VariableDefinition(appThemeBindingTypeRef);
			instructions.Add(Create(Stloc, vardef));
			vardefref.VariableDefinition = vardef;

			return instructions;

			void SetPropertyValueIfNeeded(string propertyName)
			{
				// Load the property value on the stack
				if (markupExtensionElementNode.Properties.TryGetValue(new XmlName("", propertyName), out INode? propertyNode))
				{
					// Duplicate the AppThemeBinding instance on the stack
					instructions.Add(Create(Dup));

					if (propertyNode is IElementNode propertyElementNode)
					{
						if (!context.Variables.TryGetValue(propertyElementNode, out VariableDefinition? variableDefinition))
						{
							throw new InvalidOperationException($"Variable definition for element node '{propertyElementNode}' not found in context.");
						}

						// Load the variable onto the stack, no need to convert it
						instructions.Add(Create(Ldloc, variableDefinition));
					}
					else if (propertyNode is ValueNode valueNode
						&& parentElement is not null
						&& markupExtensionElementNode.TryGetPropertyName(parentElement, out XmlName parentPropertyName))
					{
						// Push the value onto the stack
						instructions.AddRange(
							TryConvert(valueNode, valueNode, parentElement, parentPropertyName, module, context));
					}
					else
					{
						throw new InvalidOperationException($"Unsupported property node type: {propertyNode.GetType().Name}");
					}

					// Store the value to the AppThemeBinding instance property
					var setMethod = module.ImportReference(typeof(AppThemeBinding).GetProperty(propertyName)?.SetMethod)
						?? throw new InvalidOperationException($"Property '{propertyName}' not found in 'AppThemeBinding'.");
					instructions.Add(Create(Call, setMethod));
				}
				else
				{
					// nothing to do, this property is not used
				}
			}

			MethodReference GetConstructor(TypeReference typeRef)
			{
				var (ctor, _) = typeRef.GetMethods(context.Cache, m => m.IsConstructor && !m.HasParameters, module).FirstOrDefault()
					?? throw new InvalidOperationException("AppThemeBinding does not have a parameterless constructor.");
				return module.ImportReference(ctor);
			}
		}

		private static IEnumerable<Instruction> TryConvert(
			ValueNode valueNode,
			IXmlLineInfo xmlLine,
			IElementNode parentElementNode,
			XmlName parentPropertyName,
			ModuleDefinition module,
			ILContext context)
		{
			var localName = parentPropertyName.LocalName;
			var parentType = module.ImportReference(parentElementNode.XmlType.GetTypeReference(context.Cache, module, xmlLine));

			var bpRef = SetPropertiesVisitor.GetBindablePropertyReference(parentType, parentPropertyName.NamespaceURI, ref localName, out _, context, xmlLine);
			if (bpRef != null)
			{
				foreach (var instruction in valueNode.PushConvertedValue(context, bpRef, requiredServices => valueNode.PushServiceProvider(context, requiredServices, bpRef: bpRef), true, false))
					yield return instruction;
				yield break;
			}

			var propertyRef = parentType.GetProperty(context.Cache, pd => pd.Name == localName, out var declaringTypeReference);
			if (propertyRef != null)
			{
				var propertyType = module.ImportReference(propertyRef.PropertyType.ResolveGenericParameters(declaringTypeReference));

				foreach (var instruction in valueNode.PushConvertedValue(
						context,
						propertyType,
						[propertyRef, propertyType.ResolveCached(context.Cache)],
						requiredServices => valueNode.PushServiceProvider(context, requiredServices, propertyRef: propertyRef),
						boxValueTypes: true,
						unboxValueTypes: false))
					yield return instruction;
				yield break;
			}

			throw new InvalidOperationException($"Property '{parentPropertyName}' not found in '{parentType.FullName}'.");
		}
	}
}