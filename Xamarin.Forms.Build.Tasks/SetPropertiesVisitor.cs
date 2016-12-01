using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Build.Tasks
{
	class SetPropertiesVisitor : IXamlNodeVisitor
	{
		static int dtcount;

		static readonly IList<XmlName> skips = new List<XmlName>
		{
			XmlName.xKey,
			XmlName.xTypeArguments,
			XmlName.xArguments,
			XmlName.xFactoryMethod,
			XmlName.xName
		};

		public SetPropertiesVisitor(ILContext context, bool stopOnResourceDictionary = false)
		{
			Context = context;
			Module = context.Body.Method.Module;
			StopOnResourceDictionary = stopOnResourceDictionary;
		}

		public ILContext Context { get; }
		public bool StopOnResourceDictionary { get; }
		public bool VisitChildrenFirst { get; } = true;
		public bool StopOnDataTemplate { get; } = true;

		ModuleDefinition Module { get; }

		public void Visit(ValueNode node, INode parentNode)
		{
			//TODO support Label text as element
			XmlName propertyName;
			if (!TryGetPropertyName(node, parentNode, out propertyName))
			{
				if (!IsCollectionItem(node, parentNode))
					return;
				string contentProperty;
				if (!Context.Variables.ContainsKey((IElementNode)parentNode))
					return;
				var parentVar = Context.Variables[(IElementNode)parentNode];
				if ((contentProperty = GetContentProperty(parentVar.VariableType)) != null)
					propertyName = new XmlName(((IElementNode)parentNode).NamespaceURI, contentProperty);
				else
					return;
			}

			if (skips.Contains(propertyName))
				return;
			if (parentNode is IElementNode && ((IElementNode)parentNode).SkipProperties.Contains (propertyName))
				return;
			if (propertyName.NamespaceURI == "http://schemas.openxmlformats.org/markup-compatibility/2006" &&
			    propertyName.LocalName == "Ignorable")
				return;
			Context.IL.Append(SetPropertyValue(Context.Variables [(IElementNode)parentNode], propertyName, node, Context, node));
		}

		public void Visit(MarkupNode node, INode parentNode)
		{
		}

		public void Visit(ElementNode node, INode parentNode)
		{
			XmlName propertyName = XmlName.Empty;

			//Simplify ListNodes with single elements
			var pList = parentNode as ListNode;
			if (pList != null && pList.CollectionItems.Count == 1) {
				propertyName = pList.XmlName;
				parentNode = parentNode.Parent;
			}

			if ((propertyName != XmlName.Empty || TryGetPropertyName(node, parentNode, out propertyName)) && skips.Contains(propertyName))
				return;

			//if this node is an IMarkupExtension, invoke ProvideValue() and replace the variable
			var vardef = Context.Variables[node];
			var vardefref = new VariableDefinitionReference(vardef);
			var localName = propertyName.LocalName;
			TypeReference declaringTypeReference = null;
			FieldReference bpRef = null;
			var _ = false;
			PropertyDefinition propertyRef = null;
			if (parentNode is IElementNode && propertyName != XmlName.Empty) {
				bpRef = GetBindablePropertyReference(Context.Variables [(IElementNode)parentNode], propertyName.NamespaceURI, ref localName, out _, Context, node);
				propertyRef = Context.Variables [(IElementNode)parentNode].VariableType.GetProperty(pd => pd.Name == localName, out declaringTypeReference);
			}
			Context.IL.Append(ProvideValue(vardefref, Context, Module, node, bpRef:bpRef, propertyRef:propertyRef, propertyDeclaringTypeRef: declaringTypeReference));
			if (vardef != vardefref.VariableDefinition)
			{
				vardef = vardefref.VariableDefinition;
				Context.Body.Variables.Add(vardef);
				Context.Variables[node] = vardef;
			}

			if (propertyName != XmlName.Empty)
			{
				if (skips.Contains(propertyName))
					return;
				if (parentNode is IElementNode && ((IElementNode)parentNode).SkipProperties.Contains (propertyName))
					return;

				if (propertyName == XmlName._CreateContent)
					SetDataTemplate((IElementNode)parentNode, node, Context, node);
				else
					Context.IL.Append(SetPropertyValue(Context.Variables[(IElementNode)parentNode], propertyName, node, Context, node));
			}
			else if (IsCollectionItem(node, parentNode) && parentNode is IElementNode)
			{
				// Collection element, implicit content, or implicit collection element.
				string contentProperty;
				var parentVar = Context.Variables[(IElementNode)parentNode];
				if (parentVar.VariableType.ImplementsInterface(Module.Import(typeof (IEnumerable))))
				{
					var elementType = parentVar.VariableType;
					if (elementType.FullName != "Xamarin.Forms.ResourceDictionary" && elementType.Resolve().BaseType.FullName != "Xamarin.Forms.ResourceDictionary")
					{
						var adderTuple = elementType.GetMethods(md => md.Name == "Add" && md.Parameters.Count == 1, Module).First();
						var adderRef = Module.Import(adderTuple.Item1);
						adderRef = Module.Import(adderRef.ResolveGenericParameters(adderTuple.Item2, Module));

						Context.IL.Emit(OpCodes.Ldloc, parentVar);
						Context.IL.Emit(OpCodes.Ldloc, vardef);
						Context.IL.Emit(OpCodes.Callvirt, adderRef);
						if (adderRef.ReturnType.FullName != "System.Void")
							Context.IL.Emit(OpCodes.Pop);
					}
				}
				else if ((contentProperty = GetContentProperty(parentVar.VariableType)) != null)
				{
					var name = new XmlName(node.NamespaceURI, contentProperty);
					if (skips.Contains(name))
						return;
					if (parentNode is IElementNode && ((IElementNode)parentNode).SkipProperties.Contains (propertyName))
						return;
					Context.IL.Append(SetPropertyValue(Context.Variables[(IElementNode)parentNode], name, node, Context, node));
				}
			}
			else if (IsCollectionItem(node, parentNode) && parentNode is ListNode)
			{
//				IL_000d:  ldloc.2 
//				IL_000e:  callvirt instance class [mscorlib]System.Collections.Generic.IList`1<!0> class [Xamarin.Forms.Core]Xamarin.Forms.Layout`1<class [Xamarin.Forms.Core]Xamarin.Forms.View>::get_Children()
//				IL_0013:  ldloc.0 
//				IL_0014:  callvirt instance void class [mscorlib]System.Collections.Generic.ICollection`1<class [Xamarin.Forms.Core]Xamarin.Forms.View>::Add(!0)

				var parentList = (ListNode)parentNode;
				var parent = Context.Variables[((IElementNode)parentNode.Parent)];

				if (skips.Contains(parentList.XmlName))
					return;
				if (parentNode is IElementNode && ((IElementNode)parentNode).SkipProperties.Contains (propertyName))
					return;
				var elementType = parent.VariableType;
				var localname = parentList.XmlName.LocalName;

				GetNameAndTypeRef(ref elementType, parentList.XmlName.NamespaceURI, ref localname, Context, node);

				TypeReference propertyDeclaringType;
				var property = elementType.GetProperty(pd => pd.Name == localname, out propertyDeclaringType);
				MethodDefinition propertyGetter;

				if (property != null && (propertyGetter = property.GetMethod) != null && propertyGetter.IsPublic)
				{
					var propertyGetterRef = Module.Import(propertyGetter);
					propertyGetterRef = Module.Import(propertyGetterRef.ResolveGenericParameters(propertyDeclaringType, Module));
					var propertyType = propertyGetterRef.ReturnType.ResolveGenericParameters(propertyDeclaringType);

					var adderTuple = propertyType.GetMethods(md => md.Name == "Add" && md.Parameters.Count == 1, Module).First();
					var adderRef = Module.Import(adderTuple.Item1);
					adderRef = Module.Import(adderRef.ResolveGenericParameters(adderTuple.Item2, Module));

					Context.IL.Emit(OpCodes.Ldloc, parent);
					Context.IL.Emit(OpCodes.Callvirt, propertyGetterRef);
					Context.IL.Emit(OpCodes.Ldloc, vardef);
					Context.IL.Emit(OpCodes.Callvirt, adderRef);
					if (adderRef.ReturnType.FullName != "System.Void")
						Context.IL.Emit(OpCodes.Pop);
				} else
					throw new XamlParseException(string.Format("Property {0} not found", localname), node);

			}
		}

		public void Visit(RootNode node, INode parentNode)
		{
		}

		public void Visit(ListNode node, INode parentNode)
		{
		}

		public static bool TryGetPropertyName(INode node, INode parentNode, out XmlName name)
		{
			name = default(XmlName);
			var parentElement = parentNode as IElementNode;
			if (parentElement == null)
				return false;
			foreach (var kvp in parentElement.Properties)
			{
				if (kvp.Value != node)
					continue;
				name = kvp.Key;
				return true;
			}
			return false;
		}

		static bool IsCollectionItem(INode node, INode parentNode)
		{
			var parentList = parentNode as IListNode;
			if (parentList == null)
				return false;
			return parentList.CollectionItems.Contains(node);
		}

		internal static string GetContentProperty(TypeReference typeRef)
		{
			var typeDef = typeRef.Resolve();
			var attributes = typeDef.CustomAttributes;
			var attr =
				attributes.FirstOrDefault(cad => ContentPropertyAttribute.ContentPropertyTypes.Contains(cad.AttributeType.FullName));
			if (attr != null)
				return attr.ConstructorArguments[0].Value as string;
			if (typeDef.BaseType == null)
				return null;
			return GetContentProperty(typeDef.BaseType);
		}

		public static IEnumerable<Instruction> ProvideValue(VariableDefinitionReference vardefref, ILContext context,
		                                                    ModuleDefinition module, ElementNode node, FieldReference bpRef = null, PropertyReference propertyRef = null, TypeReference propertyDeclaringTypeRef = null)
		{
			GenericInstanceType markupExtension;
			IList<TypeReference> genericArguments;
			if (vardefref.VariableDefinition.VariableType.FullName == "Xamarin.Forms.Xaml.ArrayExtension" &&
			    vardefref.VariableDefinition.VariableType.ImplementsGenericInterface("Xamarin.Forms.Xaml.IMarkupExtension`1",
				    out markupExtension, out genericArguments))
			{
				var markExt = markupExtension.Resolve();
				var provideValueInfo = markExt.Methods.First(md => md.Name == "ProvideValue");
				var provideValue = module.Import(provideValueInfo);
				provideValue =
					module.Import(provideValue.ResolveGenericParameters(markupExtension, module));

				var typeNode = node.Properties[new XmlName("", "Type")];
				TypeReference arrayTypeRef;
				if (context.TypeExtensions.TryGetValue(typeNode, out arrayTypeRef))
					vardefref.VariableDefinition = new VariableDefinition(module.Import(arrayTypeRef.MakeArrayType()));
				else
					vardefref.VariableDefinition = new VariableDefinition(module.Import(genericArguments.First()));
				yield return Instruction.Create(OpCodes.Ldloc, context.Variables[node]);
				foreach (var instruction in node.PushServiceProvider(context, bpRef, propertyRef, propertyDeclaringTypeRef))
					yield return instruction;
				yield return Instruction.Create(OpCodes.Callvirt, provideValue);

				if (arrayTypeRef != null)
					yield return Instruction.Create(OpCodes.Castclass, module.Import(arrayTypeRef.MakeArrayType()));
				yield return Instruction.Create(OpCodes.Stloc, vardefref.VariableDefinition);
			}
			else if (vardefref.VariableDefinition.VariableType.ImplementsGenericInterface("Xamarin.Forms.Xaml.IMarkupExtension`1",
				out markupExtension, out genericArguments))
			{
				var markExt = markupExtension.Resolve();
				var provideValueInfo = markExt.Methods.First(md => md.Name == "ProvideValue");
				var provideValue = module.Import(provideValueInfo);
				provideValue =
					module.Import(provideValue.ResolveGenericParameters(markupExtension, module));

				vardefref.VariableDefinition = new VariableDefinition(module.Import(genericArguments.First()));
				yield return Instruction.Create(OpCodes.Ldloc, context.Variables[node]);
				foreach (var instruction in node.PushServiceProvider(context, bpRef, propertyRef, propertyDeclaringTypeRef))
					yield return instruction;
				yield return Instruction.Create(OpCodes.Callvirt, provideValue);
				yield return Instruction.Create(OpCodes.Stloc, vardefref.VariableDefinition);
			}
			else if (context.Variables[node].VariableType.ImplementsInterface(module.Import(typeof (IMarkupExtension))))
			{
				var markExt = module.Import(typeof (IMarkupExtension)).Resolve();
				var provideValueInfo = markExt.Methods.First(md => md.Name == "ProvideValue");
				var provideValue = module.Import(provideValueInfo);

				vardefref.VariableDefinition = new VariableDefinition(module.TypeSystem.Object);
				yield return Instruction.Create(OpCodes.Ldloc, context.Variables[node]);
				foreach (var instruction in node.PushServiceProvider(context, bpRef, propertyRef, propertyDeclaringTypeRef))
					yield return instruction;
				yield return Instruction.Create(OpCodes.Callvirt, provideValue);
				yield return Instruction.Create(OpCodes.Stloc, vardefref.VariableDefinition);
			}
			else if (context.Variables[node].VariableType.ImplementsInterface(module.Import(typeof (IValueProvider))))
			{
				var markExt = module.Import(typeof (IValueProvider)).Resolve();
				var provideValueInfo = markExt.Methods.First(md => md.Name == "ProvideValue");
				var provideValue = module.Import(provideValueInfo);

				vardefref.VariableDefinition = new VariableDefinition(module.TypeSystem.Object);
				yield return Instruction.Create(OpCodes.Ldloc, context.Variables[node]);
				foreach (var instruction in node.PushServiceProvider(context, bpRef, propertyRef, propertyDeclaringTypeRef))
					yield return instruction;
				yield return Instruction.Create(OpCodes.Callvirt, provideValue);
				yield return Instruction.Create(OpCodes.Stloc, vardefref.VariableDefinition);
			}
		}

		public static IEnumerable<Instruction> SetPropertyValue(VariableDefinition parent, XmlName propertyName, INode valueNode, ILContext context, IXmlLineInfo iXmlLineInfo)
		{
			var module = context.Body.Method.Module;
			var localName = propertyName.LocalName;
			bool attached;
			var bpRef = GetBindablePropertyReference(parent, propertyName.NamespaceURI, ref localName, out attached, context, iXmlLineInfo);

			//If the target is an event, connect
			if (CanConnectEvent(parent, localName))
				return ConnectEvent(parent, localName, valueNode, iXmlLineInfo, context);

			//If Value is DynamicResource, SetDynamicResource
			if (CanSetDynamicResource(bpRef, valueNode, context))
				return SetDynamicResource(parent, bpRef, valueNode as IElementNode, iXmlLineInfo, context);

			//If Value is a BindingBase and target is a BP, SetBinding
			if (CanSetBinding(bpRef, valueNode, context))
				return SetBinding(parent, bpRef, valueNode as IElementNode, iXmlLineInfo, context);

			//If it's a BP, SetValue ()
			if (CanSetValue(bpRef, attached, valueNode, iXmlLineInfo, context))
				return SetValue(parent, bpRef, valueNode, iXmlLineInfo, context);

			//If it's a property, set it
			if (CanSet(parent, localName, valueNode, context))
				return Set(parent, localName, valueNode, iXmlLineInfo, context);

			//If it's an already initialized property, add to it
			if (CanAdd(parent, localName, valueNode, context))
				return Add(parent, localName, valueNode, iXmlLineInfo, context);

			throw new XamlParseException($"No property, bindable property, or event found for '{localName}'", iXmlLineInfo);
		}

		static FieldReference GetBindablePropertyReference(VariableDefinition parent, string namespaceURI, ref string localName, out bool attached, ILContext context, IXmlLineInfo iXmlLineInfo)
		{
			var module = context.Body.Method.Module;
			TypeReference declaringTypeReference;

			//If it's an attached BP, update elementType and propertyName
			var bpOwnerType = parent.VariableType;
			attached = GetNameAndTypeRef(ref bpOwnerType, namespaceURI, ref localName, context, iXmlLineInfo);
			var name = $"{localName}Property";
			FieldReference bpRef = bpOwnerType.GetField(fd => fd.Name == name &&
														fd.IsStatic &&
														fd.IsPublic, out declaringTypeReference);
			if (bpRef != null) {
				bpRef = module.Import(bpRef.ResolveGenericParameters(declaringTypeReference));
				bpRef.FieldType = module.Import(bpRef.FieldType);
			}
			return bpRef;
		}

		static bool CanConnectEvent(VariableDefinition parent, string localName)
		{
			return parent.VariableType.GetEvent(ed => ed.Name == localName) != null;
		}

		static IEnumerable<Instruction> ConnectEvent(VariableDefinition parent, string localName, INode valueNode, IXmlLineInfo iXmlLineInfo, ILContext context)
		{
			var elementType = parent.VariableType;
			var module = context.Body.Method.Module;
			var eventinfo = elementType.GetEvent(ed => ed.Name == localName);

//			IL_0007:  ldloc.0 
//			IL_0008:  ldarg.0 
//
//			IL_0009:  ldftn instance void class Xamarin.Forms.Xaml.XamlcTests.MyPage::OnButtonClicked(object, class [mscorlib]System.EventArgs)
//OR, if the handler is virtual
//			IL_000x:  ldarg.0 
//			IL_0009:  ldvirtftn instance void class Xamarin.Forms.Xaml.XamlcTests.MyPage::OnButtonClicked(object, class [mscorlib]System.EventArgs)
//
//			IL_000f:  newobj instance void class [mscorlib]System.EventHandler::'.ctor'(object, native int)
//			IL_0014:  callvirt instance void class [Xamarin.Forms.Core]Xamarin.Forms.Button::add_Clicked(class [mscorlib]System.EventHandler)

			var value = ((ValueNode)valueNode).Value;

			yield return Instruction.Create(OpCodes.Ldloc, parent);
			if (context.Root is VariableDefinition)
				yield return Instruction.Create(OpCodes.Ldloc, context.Root as VariableDefinition);
			else if (context.Root is FieldDefinition) {
				yield return Instruction.Create(OpCodes.Ldarg_0);
				yield return Instruction.Create(OpCodes.Ldfld, context.Root as FieldDefinition);
			} else 
				throw new InvalidProgramException();
			var declaringType = context.Body.Method.DeclaringType;
			while (declaringType.IsNested)
				declaringType = declaringType.DeclaringType;
			var handler = declaringType.AllMethods().FirstOrDefault(md => md.Name == value as string);
			if (handler == null) 
				throw new XamlParseException($"EventHandler \"{value}\" not found in type \"{context.Body.Method.DeclaringType.FullName}\"", iXmlLineInfo);
			if (handler.IsVirtual) {
				yield return Instruction.Create(OpCodes.Ldarg_0);
				yield return Instruction.Create(OpCodes.Ldvirtftn, handler);
			} else
				yield return Instruction.Create(OpCodes.Ldftn, handler);

			//FIXME: eventually get the right ctor instead fo the First() one, just in case another one could exists (not even sure it's possible).
			var ctor = module.Import(eventinfo.EventType.Resolve().GetConstructors().First());
			ctor = ctor.ResolveGenericParameters(eventinfo.EventType, module);
			yield return Instruction.Create(OpCodes.Newobj, module.Import(ctor));
			yield return Instruction.Create(OpCodes.Callvirt, module.Import(eventinfo.AddMethod));
		}

		static bool CanSetDynamicResource(FieldReference bpRef, INode valueNode, ILContext context)
		{
			if (bpRef == null)
				return false;
			var elementNode = valueNode as IElementNode;
			if (elementNode == null)
				return false;
			
			VariableDefinition varValue;
			if (!context.Variables.TryGetValue(valueNode as IElementNode, out varValue))
				return false;
			return varValue.VariableType.FullName == typeof(DynamicResource).FullName;
		}

		static IEnumerable<Instruction> SetDynamicResource(VariableDefinition parent, FieldReference bpRef, IElementNode elementNode, IXmlLineInfo iXmlLineInfo, ILContext context)
		{
			var module = context.Body.Method.Module;
			var varValue = context.Variables [elementNode];
			var setDynamicResource = module.Import(typeof(IDynamicResourceHandler)).Resolve().Methods.First(m => m.Name == "SetDynamicResource");
			var getKey = typeof(DynamicResource).GetProperty("Key").GetMethod;

			yield return Instruction.Create(OpCodes.Ldloc, parent);
			yield return Instruction.Create(OpCodes.Ldsfld, bpRef);
			yield return Instruction.Create(OpCodes.Ldloc, varValue);
			yield return Instruction.Create(OpCodes.Callvirt, module.Import(getKey));
			yield return Instruction.Create(OpCodes.Callvirt, module.Import(setDynamicResource));
		}

		static bool CanSetBinding(FieldReference bpRef, INode valueNode, ILContext context)
		{
			var module = context.Body.Method.Module;

			if (bpRef == null)
				return false;
			var elementNode = valueNode as IElementNode;
			if (elementNode == null)
				return false;

			VariableDefinition varValue;
			if (!context.Variables.TryGetValue(valueNode as IElementNode, out varValue))
				return false;
			return varValue.VariableType.InheritsFromOrImplements(module.Import(typeof(BindingBase)));
		}

		static IEnumerable<Instruction> SetBinding(VariableDefinition parent, FieldReference bpRef, IElementNode elementNode, IXmlLineInfo iXmlLineInfo, ILContext context)
		{
			var module = context.Body.Method.Module;
			var varValue = context.Variables [elementNode];

			//TODO: check if parent is a BP
			var setBinding = typeof(BindableObject).GetMethod("SetBinding", new [] { typeof(BindableProperty), typeof(BindingBase) });

			yield return Instruction.Create(OpCodes.Ldloc, parent);
			yield return Instruction.Create(OpCodes.Ldsfld, bpRef);
			yield return Instruction.Create(OpCodes.Ldloc, varValue);
			yield return Instruction.Create(OpCodes.Callvirt, module.Import(setBinding));
		}

		static bool CanSetValue(FieldReference bpRef, bool attached, INode node, IXmlLineInfo iXmlLineInfo, ILContext context)
		{
			var module = context.Body.Method.Module;

			if (bpRef == null)
				return false;

			if (node is ValueNode)
				return true;

			var elementNode = node as IElementNode;
			if (elementNode == null)
				return false;

			VariableDefinition varValue;
			if (!context.Variables.TryGetValue(elementNode, out varValue))
				return false;

			var bpTypeRef = bpRef.GetBindablePropertyType(iXmlLineInfo, module);
			// If it's an attached BP, there's no second chance to handle IMarkupExtensions, so we try here.
			// Worst case scenario ? InvalidCastException at runtime
			if (attached && varValue.VariableType.FullName == "System.Object") 
				return true;
			var implicitOperator = varValue.VariableType.GetImplicitOperatorTo(bpTypeRef, module);
			if (implicitOperator != null)
				return true;

			return varValue.VariableType.InheritsFromOrImplements(bpTypeRef);
		}

		static IEnumerable<Instruction> SetValue(VariableDefinition parent, FieldReference bpRef, INode node, IXmlLineInfo iXmlLineInfo, ILContext context)
		{
			var setValue = typeof(BindableObject).GetMethod("SetValue", new [] { typeof(BindableProperty), typeof(object) });
			var valueNode = node as ValueNode;
			var elementNode = node as IElementNode;
			var module = context.Body.Method.Module;

//			IL_0007:  ldloc.0 
//			IL_0008:  ldsfld class [Xamarin.Forms.Core]Xamarin.Forms.BindableProperty [Xamarin.Forms.Core]Xamarin.Forms.Label::TextProperty
//			IL_000d:  ldstr "foo"
//			IL_0012:  callvirt instance void class [Xamarin.Forms.Core]Xamarin.Forms.BindableObject::SetValue(class [Xamarin.Forms.Core]Xamarin.Forms.BindableProperty, object)

			yield return Instruction.Create(OpCodes.Ldloc, parent);
			yield return Instruction.Create(OpCodes.Ldsfld, bpRef);

			if (valueNode != null) {
				foreach (var instruction in valueNode.PushConvertedValue(context, bpRef, valueNode.PushServiceProvider(context, bpRef:bpRef), true, false))
					yield return instruction;
			} else if (elementNode != null) {
				var bpTypeRef = bpRef.GetBindablePropertyType(iXmlLineInfo, module);
				var varDef = context.Variables[elementNode];
				var varType = varDef.VariableType;
				var implicitOperator = varDef.VariableType.GetImplicitOperatorTo(bpTypeRef, module);
				yield return Instruction.Create(OpCodes.Ldloc, varDef);
				if (implicitOperator != null) {
					yield return Instruction.Create(OpCodes.Call, module.Import(implicitOperator));
					varType = module.Import(bpTypeRef);
				}
				if (varType.IsValueType)
					yield return Instruction.Create(OpCodes.Box, varType);
			}

			yield return Instruction.Create(OpCodes.Callvirt, module.Import(setValue));
		}

		static bool CanSet(VariableDefinition parent, string localName, INode node, ILContext context)
		{
			var module = context.Body.Method.Module;
			TypeReference declaringTypeReference;
			var property = parent.VariableType.GetProperty(pd => pd.Name == localName, out declaringTypeReference);
			if (property == null)
				return false;
			var propertySetter = property.SetMethod;
			if (propertySetter == null || !propertySetter.IsPublic || propertySetter.IsStatic)
				return false;

			if (node is ValueNode)
				return true;

			var elementNode = node as IElementNode;
			if (elementNode == null)
				return false;

			var vardef = context.Variables [elementNode];
			var propertyType = property.ResolveGenericPropertyType(declaringTypeReference);
			var implicitOperator = vardef.VariableType.GetImplicitOperatorTo(propertyType, module);

			if (implicitOperator != null)
				return true;
			if (vardef.VariableType.InheritsFromOrImplements(propertyType))
				return true;
			if (propertyType.FullName == "System.Object")
				return true;

			//I'd like to get rid of this condition. This comment used to be //TODO replace latest check by a runtime type check
			if (vardef.VariableType.FullName == "System.Object")
				return true;

			return false;
		}

		static IEnumerable<Instruction> Set(VariableDefinition parent, string localName, INode node, IXmlLineInfo iXmlLineInfo, ILContext context)
		{
			var module = context.Body.Method.Module;
			TypeReference declaringTypeReference;
			var property = parent.VariableType.GetProperty(pd => pd.Name == localName, out declaringTypeReference);
			var propertySetter = property.SetMethod;

//			IL_0007:  ldloc.0
//			IL_0008:  ldstr "foo"
//			IL_000d:  callvirt instance void class [Xamarin.Forms.Core]Xamarin.Forms.Label::set_Text(string)

			module.Import(parent.VariableType.Resolve());
			var propertySetterRef = module.Import(module.Import(propertySetter).ResolveGenericParameters(declaringTypeReference, module));
			propertySetterRef.ImportTypes(module);
			var propertyType = property.ResolveGenericPropertyType(declaringTypeReference);
			var valueNode = node as ValueNode;
			var elementNode = node as IElementNode;

			yield return Instruction.Create(OpCodes.Ldloc, parent);

			if (valueNode != null) {
				foreach (var instruction in valueNode.PushConvertedValue(context, propertyType, new ICustomAttributeProvider [] { property, propertyType.Resolve() }, valueNode.PushServiceProvider(context, propertyRef:property), false, true))
					yield return instruction;
				yield return Instruction.Create(OpCodes.Callvirt, propertySetterRef);
			} else if (elementNode != null) {
				var vardef = context.Variables [elementNode];
				var implicitOperator = vardef.VariableType.GetImplicitOperatorTo(propertyType, module);
				yield return Instruction.Create(OpCodes.Ldloc, vardef);
				if (implicitOperator != null) {
//					IL_000f:  call !0 class [Xamarin.Forms.Core]Xamarin.Forms.OnPlatform`1<bool>::op_Implicit(class [Xamarin.Forms.Core]Xamarin.Forms.OnPlatform`1<!0>)
					yield return Instruction.Create(OpCodes.Call, module.Import(implicitOperator));
				} else if (!vardef.VariableType.IsValueType && propertyType.IsValueType)
					yield return Instruction.Create(OpCodes.Unbox_Any, module.Import(propertyType));
				else if (vardef.VariableType.IsValueType && propertyType.FullName == "System.Object")
					yield return Instruction.Create(OpCodes.Box, vardef.VariableType);
				yield return Instruction.Create(OpCodes.Callvirt, propertySetterRef);
			}
		}

		static bool CanAdd(VariableDefinition parent, string localName, INode node, ILContext context)
		{
			var module = context.Body.Method.Module;
			TypeReference declaringTypeReference;
			var property = parent.VariableType.GetProperty(pd => pd.Name == localName, out declaringTypeReference);
			if (property == null)
				return false;
			var propertyGetter = property.GetMethod;
			if (propertyGetter == null || !propertyGetter.IsPublic || propertyGetter.IsStatic)
				return false;
			var elementNode = node as IElementNode;
			if (elementNode == null)
				return false;

			var vardef = context.Variables [elementNode];
			var propertyGetterRef = module.Import(propertyGetter);
			propertyGetterRef = module.Import(propertyGetterRef.ResolveGenericParameters(declaringTypeReference, module));
			var propertyType = propertyGetterRef.ReturnType.ResolveGenericParameters(declaringTypeReference);

			//TODO check md.Parameters[0] type
			var adderTuple = propertyType.GetMethods(md => md.Name == "Add" && md.Parameters.Count == 1, module).FirstOrDefault();
			if (adderTuple == null)
				return false;

			return true;
		}

		static IEnumerable<Instruction> Add(VariableDefinition parent, string localName, INode node, IXmlLineInfo iXmlLineInfo, ILContext context)
		{
			var module = context.Body.Method.Module;
			TypeReference declaringTypeReference;
			var property = parent.VariableType.GetProperty(pd => pd.Name == localName, out declaringTypeReference);
			var propertyGetter = property.GetMethod;
			var elementNode = node as IElementNode;
			var vardef = context.Variables [elementNode];
			var propertyGetterRef = module.Import(propertyGetter);
			propertyGetterRef = module.Import(propertyGetterRef.ResolveGenericParameters(declaringTypeReference, module));
			var propertyType = propertyGetterRef.ReturnType.ResolveGenericParameters(declaringTypeReference);
			//TODO check md.Parameters[0] type
			var adderTuple = propertyType.GetMethods(md => md.Name == "Add" && md.Parameters.Count == 1, module).FirstOrDefault();
			var adderRef = module.Import(adderTuple.Item1);
			adderRef = module.Import(adderRef.ResolveGenericParameters(adderTuple.Item2, module));
			var childType = GetParameterType(adderRef.Parameters [0]);
			var implicitOperator = vardef.VariableType.GetImplicitOperatorTo(childType, module);

			yield return Instruction.Create(OpCodes.Ldloc, parent);
			yield return Instruction.Create(OpCodes.Callvirt, propertyGetterRef);
			yield return Instruction.Create(OpCodes.Ldloc, vardef);
			if (implicitOperator != null)
				yield return Instruction.Create(OpCodes.Call, module.Import(implicitOperator));
			yield return Instruction.Create(OpCodes.Callvirt, adderRef);
			if (adderRef.ReturnType.FullName != "System.Void")
				yield return Instruction.Create(OpCodes.Pop);
		}

		public static TypeReference GetParameterType(ParameterDefinition param)
		{
			if (!param.ParameterType.IsGenericParameter)
				return param.ParameterType;
			var type = (param.Method as MethodReference).DeclaringType as GenericInstanceType;
			return type.GenericArguments [0];
		}

		static bool GetNameAndTypeRef(ref TypeReference elementType, string namespaceURI, ref string localname,
			ILContext context, IXmlLineInfo lineInfo)
		{
			var dotIdx = localname.IndexOf('.');
			if (dotIdx > 0)
			{
				var typename = localname.Substring(0, dotIdx);
				localname = localname.Substring(dotIdx + 1);
				elementType = new XmlType(namespaceURI, typename, null).GetTypeReference(context.Body.Method.Module, lineInfo);
				return true;
			}
			return false;
		}

		static void SetDataTemplate(IElementNode parentNode, ElementNode node, ILContext parentContext,
			IXmlLineInfo xmlLineInfo)
		{
			var parentVar = parentContext.Variables[parentNode];
			//Push the DataTemplate to the stack, for setting the template
			parentContext.IL.Emit(OpCodes.Ldloc, parentVar);

			//Create nested class
			//			.class nested private auto ansi sealed beforefieldinit '<Main>c__AnonStorey0'
			//			extends [mscorlib]System.Object

			var module = parentContext.Body.Method.Module;
			var anonType = new TypeDefinition(
				null,
				"<" + parentContext.Body.Method.Name + ">_anonXamlCDataTemplate_" + dtcount++,
				TypeAttributes.BeforeFieldInit |
				TypeAttributes.Sealed |
				TypeAttributes.NestedPrivate)
			{
				BaseType = module.TypeSystem.Object
			};

			parentContext.Body.Method.DeclaringType.NestedTypes.Add(anonType);
			var ctor = anonType.AddDefaultConstructor();

			var loadTemplate = new MethodDefinition("LoadDataTemplate",
				MethodAttributes.Assembly | MethodAttributes.HideBySig,
				module.TypeSystem.Object);
			anonType.Methods.Add(loadTemplate);

			var parentValues = new FieldDefinition("parentValues", FieldAttributes.Assembly, module.Import(typeof (object[])));
			anonType.Fields.Add(parentValues);

			TypeReference rootType = null;
			var vdefRoot = parentContext.Root as VariableDefinition;
			if (vdefRoot != null)
				rootType = vdefRoot.VariableType;
			var fdefRoot = parentContext.Root as FieldDefinition;
			if (fdefRoot != null)
				rootType = fdefRoot.FieldType;

			var root = new FieldDefinition("root", FieldAttributes.Assembly, rootType);
			anonType.Fields.Add(root);

			//Fill the loadTemplate Body
			var templateIl = loadTemplate.Body.GetILProcessor();
			templateIl.Emit(OpCodes.Nop);
			var templateContext = new ILContext(templateIl, loadTemplate.Body, parentValues)
			{
				Root = root
			};
			node.Accept(new CreateObjectVisitor(templateContext), null);
			node.Accept(new SetNamescopesAndRegisterNamesVisitor(templateContext), null);
			node.Accept(new SetFieldVisitor(templateContext), null);
			node.Accept(new SetResourcesVisitor(templateContext), null);
			node.Accept(new SetPropertiesVisitor(templateContext), null);
			templateIl.Emit(OpCodes.Ldloc, templateContext.Variables[node]);
			templateIl.Emit(OpCodes.Ret);

			//Instanciate nested class
			var parentIl = parentContext.IL;
			parentIl.Emit(OpCodes.Newobj, ctor);

			//Copy required local vars
			parentIl.Emit(OpCodes.Dup); //Duplicate the nestedclass instance
			parentIl.Append(node.PushParentObjectsArray(parentContext));
			parentIl.Emit(OpCodes.Stfld, parentValues);
			parentIl.Emit(OpCodes.Dup); //Duplicate the nestedclass instance
			if (parentContext.Root is VariableDefinition)
				parentIl.Emit(OpCodes.Ldloc, parentContext.Root as VariableDefinition);
			else if (parentContext.Root is FieldDefinition)
			{
				parentIl.Emit(OpCodes.Ldarg_0);
				parentIl.Emit(OpCodes.Ldfld, parentContext.Root as FieldDefinition);
			}
			else
				throw new InvalidProgramException();
			parentIl.Emit(OpCodes.Stfld, root);

			//SetDataTemplate
			parentIl.Emit(OpCodes.Ldftn, loadTemplate);
			var funcObjRef = module.Import(typeof(Func<object>));
			var funcCtor =
				funcObjRef
					.Resolve()
					.Methods.First(md => md.IsConstructor && md.Parameters.Count == 2)
					.ResolveGenericParameters(funcObjRef, module);
			parentIl.Emit(OpCodes.Newobj, module.Import(funcCtor));

#pragma warning disable 0612
			var propertySetter =
				module.Import(typeof (IDataTemplate)).Resolve().Properties.First(p => p.Name == "LoadTemplate").SetMethod;
#pragma warning restore 0612
			parentContext.IL.Emit(OpCodes.Callvirt, module.Import(propertySetter));
		}
	}

	class VariableDefinitionReference
	{
		public VariableDefinitionReference(VariableDefinition vardef)
		{
			VariableDefinition = vardef;
		}

		public VariableDefinition VariableDefinition { get; set; }

		public static implicit operator VariableDefinition(VariableDefinitionReference vardefref)
		{
			return vardefref.VariableDefinition;
		}
	}
}
