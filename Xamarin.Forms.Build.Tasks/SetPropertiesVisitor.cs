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

		readonly IList<XmlName> skips = new List<XmlName>
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

		ModuleDefinition Module { get; }

		public bool VisitChildrenFirst
		{
			get { return true; }
		}

		public bool StopOnDataTemplate
		{
			get { return true; }
		}

		public bool StopOnResourceDictionary { get; }

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
			if (propertyName.NamespaceURI == "http://schemas.openxmlformats.org/markup-compatibility/2006" &&
			    propertyName.LocalName == "Ignorable")
				return;
			SetPropertyValue(Context.Variables[(IElementNode)parentNode], propertyName, node, Context, node);
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

			//if this node is an IMarkupExtension, invoke ProvideValue() and replace the variable
			var vardef = Context.Variables[node];
			var vardefref = new VariableDefinitionReference(vardef);
			Context.IL.Append(ProvideValue(vardefref, Context, Module, node));
			if (vardef != vardefref.VariableDefinition)
			{
				vardef = vardefref.VariableDefinition;
				Context.Body.Variables.Add(vardef);
				Context.Variables[node] = vardef;
			}

			if (propertyName != XmlName.Empty || TryGetPropertyName(node, parentNode, out propertyName))
			{
				if (skips.Contains(propertyName))
					return;

				if (propertyName == XmlName._CreateContent)
					SetDataTemplate((IElementNode)parentNode, node, Context, node);
				else
					SetPropertyValue(Context.Variables[(IElementNode)parentNode], propertyName, node, Context, node);
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
					SetPropertyValue(Context.Variables[(IElementNode)parentNode], name, node, Context, node);
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
				}
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
			ModuleDefinition module, ElementNode node)
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
				foreach (var instruction in node.PushServiceProvider(context))
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
				foreach (var instruction in node.PushServiceProvider(context))
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
				foreach (var instruction in node.PushServiceProvider(context))
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
				foreach (var instruction in node.PushServiceProvider(context))
					yield return instruction;
				yield return Instruction.Create(OpCodes.Callvirt, provideValue);
				yield return Instruction.Create(OpCodes.Stloc, vardefref.VariableDefinition);
			}
		}

		public static void SetPropertyValue(VariableDefinition parent, XmlName propertyName, INode valueNode,
			ILContext context, IXmlLineInfo iXmlLineInfo)
		{
			var elementType = parent.VariableType;
			var localName = propertyName.LocalName;
			var module = context.Body.Method.Module;
			var br = Instruction.Create(OpCodes.Nop);
			TypeReference declaringTypeReference;
			var handled = false;

			//If it's an attached BP, update elementType and propertyName
			GetNameAndTypeRef(ref elementType, propertyName.NamespaceURI, ref localName, context, iXmlLineInfo);

			//If the target is an event, connect
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

			var eventinfo = elementType.GetEvent(ed => ed.Name == localName);
			if (eventinfo != null)
			{
				var value = ((ValueNode)valueNode).Value;

				context.IL.Emit(OpCodes.Ldloc, parent);
				if (context.Root is VariableDefinition)
					context.IL.Emit(OpCodes.Ldloc, context.Root as VariableDefinition);
				else if (context.Root is FieldDefinition)
				{
					context.IL.Emit(OpCodes.Ldarg_0);
					context.IL.Emit(OpCodes.Ldfld, context.Root as FieldDefinition);
				}
				else
					throw new InvalidProgramException();
				var declaringType = context.Body.Method.DeclaringType;
				if (declaringType.IsNested)
					declaringType = declaringType.DeclaringType;
				var handler = declaringType.AllMethods().FirstOrDefault(md => md.Name == value as string);
				if (handler == null)
				{
					throw new XamlParseException(
						string.Format("EventHandler \"{0}\" not found in type \"{1}\"", value, context.Body.Method.DeclaringType.FullName),
						iXmlLineInfo);
				}
				if (handler.IsVirtual)
				{
					context.IL.Emit(OpCodes.Ldarg_0);
					context.IL.Emit(OpCodes.Ldvirtftn, handler);
				}
				else
					context.IL.Emit(OpCodes.Ldftn, handler);

				//FIXME: eventually get the right ctor instead fo the First() one, just in case another one could exists (not even sure it's possible).
				var ctor = module.Import(eventinfo.EventType.Resolve().GetConstructors().First());
				ctor = ctor.ResolveGenericParameters(eventinfo.EventType, module);
				context.IL.Emit(OpCodes.Newobj, module.Import(ctor));
				context.IL.Emit(OpCodes.Callvirt, module.Import(eventinfo.AddMethod));
				return;
			}

			FieldReference bpRef = elementType.GetField(fd => fd.Name == localName + "Property" && fd.IsStatic && fd.IsPublic,
				out declaringTypeReference);
			if (bpRef != null)
			{
				bpRef = module.Import(bpRef.ResolveGenericParameters(declaringTypeReference));
				bpRef.FieldType = module.Import(bpRef.FieldType);
			}

			//If Value is DynamicResource, SetDynamicResource
			VariableDefinition varValue;
			if (bpRef != null && valueNode is IElementNode &&
			    context.Variables.TryGetValue(valueNode as IElementNode, out varValue) &&
			    varValue.VariableType.FullName == typeof (DynamicResource).FullName)
			{
				var setDynamicResource =
					module.Import(typeof (IDynamicResourceHandler)).Resolve().Methods.First(m => m.Name == "SetDynamicResource");
				var getKey = typeof (DynamicResource).GetProperty("Key").GetMethod;

				context.IL.Emit(OpCodes.Ldloc, parent);
				context.IL.Emit(OpCodes.Ldsfld, bpRef);
				context.IL.Emit(OpCodes.Ldloc, varValue);
				context.IL.Emit(OpCodes.Callvirt, module.Import(getKey));
				context.IL.Emit(OpCodes.Callvirt, module.Import(setDynamicResource));
				return;
			}

			//If Value is a BindingBase and target is a BP, SetBinding
			if (bpRef != null && valueNode is IElementNode &&
			    context.Variables.TryGetValue(valueNode as IElementNode, out varValue) &&
			    varValue.VariableType.InheritsFromOrImplements(module.Import(typeof (BindingBase))))
			{
				//TODO: check if parent is a BP
				var setBinding = typeof (BindableObject).GetMethod("SetBinding",
					new[] { typeof (BindableProperty), typeof (BindingBase) });

				context.IL.Emit(OpCodes.Ldloc, parent);
				context.IL.Emit(OpCodes.Ldsfld, bpRef);
				context.IL.Emit(OpCodes.Ldloc, varValue);
				context.IL.Emit(OpCodes.Callvirt, module.Import(setBinding));
				return;
			}

			//If it's a BP, SetValue ()
//			IL_0007:  ldloc.0 
//			IL_0008:  ldsfld class [Xamarin.Forms.Core]Xamarin.Forms.BindableProperty [Xamarin.Forms.Core]Xamarin.Forms.Label::TextProperty
//			IL_000d:  ldstr "foo"
//			IL_0012:  callvirt instance void class [Xamarin.Forms.Core]Xamarin.Forms.BindableObject::SetValue(class [Xamarin.Forms.Core]Xamarin.Forms.BindableProperty, object)
			if (bpRef != null)
			{
				//TODO: check if parent is a BP
				var setValue = typeof (BindableObject).GetMethod("SetValue", new[] { typeof (BindableProperty), typeof (object) });

				if (valueNode is ValueNode)
				{
					context.IL.Emit(OpCodes.Ldloc, parent);
					context.IL.Emit(OpCodes.Ldsfld, bpRef);
					context.IL.Append(((ValueNode)valueNode).PushConvertedValue(context, bpRef, valueNode.PushServiceProvider(context),
						true, false));
					context.IL.Emit(OpCodes.Callvirt, module.Import(setValue));
					return;
				}
				if (valueNode is IElementNode)
				{
					var getPropertyReturnType = module.Import(typeof (BindableProperty).GetProperty("ReturnType").GetGetMethod());
					//FIXME: this should check for inheritance too
					var isInstanceOfType = module.Import(typeof (Type).GetMethod("IsInstanceOfType", new[] { typeof (object) }));
					var brfalse = Instruction.Create(OpCodes.Nop);

					context.IL.Emit(OpCodes.Ldsfld, bpRef);
					context.IL.Emit(OpCodes.Call, getPropertyReturnType);
					context.IL.Emit(OpCodes.Ldloc, context.Variables[valueNode as IElementNode]);
					if (context.Variables[valueNode as IElementNode].VariableType.IsValueType)
						context.IL.Emit(OpCodes.Box, context.Variables[valueNode as IElementNode].VariableType);
					context.IL.Emit(OpCodes.Callvirt, isInstanceOfType);
					context.IL.Emit(OpCodes.Brfalse, brfalse);
					context.IL.Emit(OpCodes.Ldloc, parent);
					context.IL.Emit(OpCodes.Ldsfld, bpRef);
					context.IL.Emit(OpCodes.Ldloc, context.Variables[(IElementNode)valueNode]);
					if (context.Variables[valueNode as IElementNode].VariableType.IsValueType)
						context.IL.Emit(OpCodes.Box, context.Variables[valueNode as IElementNode].VariableType);
					context.IL.Emit(OpCodes.Callvirt, module.Import(setValue));
					context.IL.Emit(OpCodes.Br, br);
					context.IL.Append(brfalse);
					handled = true;
				}
			}

			//If it's a property, set it
//			IL_0007:  ldloc.0 
//			IL_0008:  ldstr "foo"
//			IL_000d:  callvirt instance void class [Xamarin.Forms.Core]Xamarin.Forms.Label::set_Text(string)
			PropertyDefinition property = elementType.GetProperty(pd => pd.Name == localName, out declaringTypeReference);
			MethodDefinition propertySetter;
			if (property != null && (propertySetter = property.SetMethod) != null && propertySetter.IsPublic)
			{
				module.Import(elementType.Resolve());
				var propertySetterRef =
					module.Import(module.Import(propertySetter).ResolveGenericParameters(declaringTypeReference, module));
				propertySetterRef.ImportTypes(module);
				var propertyType = property.ResolveGenericPropertyType(declaringTypeReference);
				ValueNode vnode = null;

				if ((vnode = valueNode as ValueNode) != null)
				{
					context.IL.Emit(OpCodes.Ldloc, parent);
					context.IL.Append(vnode.PushConvertedValue(context,
						propertyType,
						new ICustomAttributeProvider[] { property, propertyType.Resolve() },
						valueNode.PushServiceProvider(context), false, true));
					context.IL.Emit(OpCodes.Callvirt, propertySetterRef);

					context.IL.Append(br);
					return;
				}
				if (valueNode is IElementNode)
				{
					var vardef = context.Variables[(ElementNode)valueNode];
					var implicitOperator = vardef.VariableType.GetImplicitOperatorTo(propertyType, module);

					//TODO replace latest check by a runtime type check
					if (implicitOperator != null || vardef.VariableType.InheritsFromOrImplements(propertyType) ||
					    propertyType.FullName == "System.Object" || vardef.VariableType.FullName == "System.Object")
					{
						context.IL.Emit(OpCodes.Ldloc, parent);
						context.IL.Emit(OpCodes.Ldloc, vardef);
						if (implicitOperator != null)
						{
//							IL_000f:  call !0 class [Xamarin.Forms.Core]Xamarin.Forms.OnPlatform`1<bool>::op_Implicit(class [Xamarin.Forms.Core]Xamarin.Forms.OnPlatform`1<!0>)
							context.IL.Emit(OpCodes.Call, module.Import(implicitOperator));
						}
						else if (!vardef.VariableType.IsValueType && propertyType.IsValueType)
							context.IL.Emit(OpCodes.Unbox_Any, module.Import(propertyType));
						else if (vardef.VariableType.IsValueType && propertyType.FullName == "System.Object")
							context.IL.Emit(OpCodes.Box, vardef.VariableType);
						context.IL.Emit(OpCodes.Callvirt, propertySetterRef);
						context.IL.Append(br);
						return;
					}
					handled = true;
				}
			}

			//If it's an already initialized property, add to it
			MethodDefinition propertyGetter;
			//TODO: check if property is assigned
			if (property != null && (propertyGetter = property.GetMethod) != null && propertyGetter.IsPublic)
			{
				var propertyGetterRef = module.Import(propertyGetter);
				propertyGetterRef = module.Import(propertyGetterRef.ResolveGenericParameters(declaringTypeReference, module));
				var propertyType = propertyGetterRef.ReturnType.ResolveGenericParameters(declaringTypeReference);
				var vardef = context.Variables [(ElementNode)valueNode];

				//TODO check md.Parameters[0] type
				var adderTuple =
					propertyType.GetMethods(md => md.Name == "Add" && md.Parameters.Count == 1, module).FirstOrDefault();
				if (adderTuple != null)
				{
					var adderRef = module.Import(adderTuple.Item1);
					adderRef = module.Import(adderRef.ResolveGenericParameters(adderTuple.Item2, module));
					var childType = GetParameterType(adderRef.Parameters [0]);
					var implicitOperator = vardef.VariableType.GetImplicitOperatorTo(childType, module);

					if (valueNode is IElementNode)
					{
						context.IL.Emit(OpCodes.Ldloc, parent);
						context.IL.Emit(OpCodes.Callvirt, propertyGetterRef);
						context.IL.Emit(OpCodes.Ldloc, vardef);
						if (implicitOperator != null)
							context.IL.Emit(OpCodes.Call, module.Import(implicitOperator));
						context.IL.Emit(OpCodes.Callvirt, adderRef);
						if (adderRef.ReturnType.FullName != "System.Void")
							context.IL.Emit(OpCodes.Pop);
						context.IL.Append(br);
						return;
					}
				}
			}
			if (!handled)
			{
				throw new XamlParseException(string.Format("No property, bindable property, or event found for '{0}'", localName),
					iXmlLineInfo);
			}
			context.IL.Append(br);
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
