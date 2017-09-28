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
		static int typedBindingCount;

		static readonly IList<XmlName> skips = new List<XmlName>
		{
			XmlName.xKey,
			XmlName.xTypeArguments,
			XmlName.xArguments,
			XmlName.xFactoryMethod,
			XmlName.xName,
			XmlName.xDataType
		};

		public SetPropertiesVisitor(ILContext context, bool stopOnResourceDictionary = false)
		{
			Context = context;
			Module = context.Body.Method.Module;
			StopOnResourceDictionary = stopOnResourceDictionary;
		}

		public ILContext Context { get; }
		public bool StopOnResourceDictionary { get; }
		public TreeVisitingMode VisitingMode => TreeVisitingMode.BottomUp;
		public bool StopOnDataTemplate => true;
		public bool VisitNodeOnDataTemplate => true;

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
			if (propertyName.Equals(XamlParser.McUri, "Ignorable"))
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

			if (propertyName == XmlName._CreateContent) {
				SetDataTemplate((IElementNode)parentNode, node, Context, node);
				return;
			}

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

			if (propertyName != XmlName.Empty) {
				if (skips.Contains(propertyName))
					return;
				if (parentNode is IElementNode && ((IElementNode)parentNode).SkipProperties.Contains (propertyName))
					return;
				
				Context.IL.Append(SetPropertyValue(Context.Variables[(IElementNode)parentNode], propertyName, node, Context, node));
			}
			else if (IsCollectionItem(node, parentNode) && parentNode is IElementNode) {
				var parentVar = Context.Variables[(IElementNode)parentNode];
				string contentProperty;

				// Implicit Style Resource in a ResourceDictionary
				if (IsResourceDictionary((IElementNode)parentNode)
					&& node.XmlType.Name == "Style"
					&& !node.Properties.ContainsKey(XmlName.xKey)) {
					Context.IL.Emit(OpCodes.Ldloc, parentVar);
					Context.IL.Emit(OpCodes.Ldloc, Context.Variables[node]);
					Context.IL.Emit(OpCodes.Callvirt,
						Module.ImportReference(
							Module.ImportReference(typeof(ResourceDictionary))
								.Resolve()
								.Methods.Single(md => md.Name == "Add" && md.Parameters.Count == 1)));
				}
				// Resource without a x:Key in a ResourceDictionary
				else if (IsResourceDictionary((IElementNode)parentNode)
						 && !node.Properties.ContainsKey(XmlName.xKey)) {
					throw new XamlParseException("resources in ResourceDictionary require a x:Key attribute", node);
				}
				// Resource in a ResourceDictionary
				else if (IsResourceDictionary((IElementNode)parentNode)
						 && node.Properties.ContainsKey(XmlName.xKey)) {
//					IL_0013: ldloc.0
//					IL_0014:  ldstr "key"
//					IL_0019:  ldstr "foo"
//					IL_001e:  callvirt instance void class [Xamarin.Forms.Core]Xamarin.Forms.ResourceDictionary::Add(string, object)
					Context.IL.Emit(OpCodes.Ldloc, parentVar);
					Context.IL.Emit(OpCodes.Ldstr, (node.Properties[XmlName.xKey] as ValueNode).Value as string);
					var varDef = Context.Variables[node];
					Context.IL.Emit(OpCodes.Ldloc, varDef);
					if (varDef.VariableType.IsValueType)
						Context.IL.Emit(OpCodes.Box, Module.ImportReference(varDef.VariableType));
					Context.IL.Emit(OpCodes.Callvirt,
						Module.ImportReference(
							Module.ImportReference(typeof(ResourceDictionary))
								.Resolve()
								.Methods.Single(md => md.Name == "Add" && md.Parameters.Count == 2)));
				}
				// Collection element, implicit content, or implicit collection element.
				else if (parentVar.VariableType.ImplementsInterface(Module.ImportReference(typeof (IEnumerable))) && parentVar.VariableType.GetMethods(md => md.Name == "Add" && md.Parameters.Count == 1, Module).Any()) {
					var elementType = parentVar.VariableType;
					var adderTuple = elementType.GetMethods(md => md.Name == "Add" && md.Parameters.Count == 1, Module).First();
					var adderRef = Module.ImportReference(adderTuple.Item1);
					adderRef = Module.ImportReference(adderRef.ResolveGenericParameters(adderTuple.Item2, Module));

					Context.IL.Emit(OpCodes.Ldloc, parentVar);
					Context.IL.Emit(OpCodes.Ldloc, vardef);
					Context.IL.Emit(OpCodes.Callvirt, adderRef);
					if (adderRef.ReturnType.FullName != "System.Void")
						Context.IL.Emit(OpCodes.Pop);
				}
				else if ((contentProperty = GetContentProperty(parentVar.VariableType)) != null) {
					var name = new XmlName(node.NamespaceURI, contentProperty);
					if (skips.Contains(name))
						return;
					if (parentNode is IElementNode && ((IElementNode)parentNode).SkipProperties.Contains (propertyName))
						return;
					Context.IL.Append(SetPropertyValue(Context.Variables[(IElementNode)parentNode], name, node, Context, node));
				}
				else
					throw new XamlParseException($"Can not set the content of {((IElementNode)parentNode).XmlType.Name} as it doesn't have a ContentPropertyAttribute", node);
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
					var propertyGetterRef = Module.ImportReference(propertyGetter);
					propertyGetterRef = Module.ImportReference(propertyGetterRef.ResolveGenericParameters(propertyDeclaringType, Module));
					var propertyType = propertyGetterRef.ReturnType.ResolveGenericParameters(propertyDeclaringType);

					var adderTuple = propertyType.GetMethods(md => md.Name == "Add" && md.Parameters.Count == 1, Module).First();
					var adderRef = Module.ImportReference(adderTuple.Item1);
					adderRef = Module.ImportReference(adderRef.ResolveGenericParameters(adderTuple.Item2, Module));

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

		bool IsResourceDictionary(IElementNode node)
		{
			var parentVar = Context.Variables[(IElementNode)node];
			return parentVar.VariableType.FullName == "Xamarin.Forms.ResourceDictionary"
				|| parentVar.VariableType.Resolve().BaseType?.FullName == "Xamarin.Forms.ResourceDictionary";
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
		                                                    ModuleDefinition module, ElementNode node, FieldReference bpRef = null,
		                                                    PropertyReference propertyRef = null, TypeReference propertyDeclaringTypeRef = null)
		{
			GenericInstanceType markupExtension;
			IList<TypeReference> genericArguments;
			if (vardefref.VariableDefinition.VariableType.FullName == "Xamarin.Forms.Xaml.ArrayExtension" &&
			    vardefref.VariableDefinition.VariableType.ImplementsGenericInterface("Xamarin.Forms.Xaml.IMarkupExtension`1",
				    out markupExtension, out genericArguments))
			{
				var markExt = markupExtension.Resolve();
				var provideValueInfo = markExt.Methods.First(md => md.Name == "ProvideValue");
				var provideValue = module.ImportReference(provideValueInfo);
				provideValue =
					module.ImportReference(provideValue.ResolveGenericParameters(markupExtension, module));

				var typeNode = node.Properties[new XmlName("", "Type")];
				TypeReference arrayTypeRef;
				if (context.TypeExtensions.TryGetValue(typeNode, out arrayTypeRef))
					vardefref.VariableDefinition = new VariableDefinition(module.ImportReference(arrayTypeRef.MakeArrayType()));
				else
					vardefref.VariableDefinition = new VariableDefinition(module.ImportReference(genericArguments.First()));
				yield return Instruction.Create(OpCodes.Ldloc, context.Variables[node]);
				foreach (var instruction in node.PushServiceProvider(context, bpRef, propertyRef, propertyDeclaringTypeRef))
					yield return instruction;
				yield return Instruction.Create(OpCodes.Callvirt, provideValue);

				if (arrayTypeRef != null)
					yield return Instruction.Create(OpCodes.Castclass, module.ImportReference(arrayTypeRef.MakeArrayType()));
				yield return Instruction.Create(OpCodes.Stloc, vardefref.VariableDefinition);
			}
			else if (vardefref.VariableDefinition.VariableType.ImplementsGenericInterface("Xamarin.Forms.Xaml.IMarkupExtension`1",
				out markupExtension, out genericArguments))
			{
				var acceptEmptyServiceProvider = vardefref.VariableDefinition.VariableType.GetCustomAttribute(module.ImportReference(typeof(AcceptEmptyServiceProviderAttribute))) != null;
				if (vardefref.VariableDefinition.VariableType.FullName == "Xamarin.Forms.Xaml.BindingExtension")
					foreach (var instruction in CompileBindingPath(node, context, vardefref.VariableDefinition))
						yield return instruction;

				var markExt = markupExtension.Resolve();
				var provideValueInfo = markExt.Methods.First(md => md.Name == "ProvideValue");
				var provideValue = module.ImportReference(provideValueInfo);
				provideValue =
					module.ImportReference(provideValue.ResolveGenericParameters(markupExtension, module));

				vardefref.VariableDefinition = new VariableDefinition(module.ImportReference(genericArguments.First()));
				yield return Instruction.Create(OpCodes.Ldloc, context.Variables[node]);
				if (acceptEmptyServiceProvider)
					yield return Instruction.Create(OpCodes.Ldnull);
				else
					foreach (var instruction in node.PushServiceProvider(context, bpRef, propertyRef, propertyDeclaringTypeRef))
						yield return instruction;
				yield return Instruction.Create(OpCodes.Callvirt, provideValue);
				yield return Instruction.Create(OpCodes.Stloc, vardefref.VariableDefinition);
			}
			else if (context.Variables[node].VariableType.ImplementsInterface(module.ImportReference(typeof (IMarkupExtension))))
			{
				var acceptEmptyServiceProvider = context.Variables[node].VariableType.GetCustomAttribute(module.ImportReference(typeof(AcceptEmptyServiceProviderAttribute))) != null;
				var markExt = module.ImportReference(typeof (IMarkupExtension)).Resolve();
				var provideValueInfo = markExt.Methods.First(md => md.Name == "ProvideValue");
				var provideValue = module.ImportReference(provideValueInfo);

				vardefref.VariableDefinition = new VariableDefinition(module.TypeSystem.Object);
				yield return Instruction.Create(OpCodes.Ldloc, context.Variables[node]);
				if (acceptEmptyServiceProvider)
					yield return Instruction.Create(OpCodes.Ldnull);
				else
					foreach (var instruction in node.PushServiceProvider(context, bpRef, propertyRef, propertyDeclaringTypeRef))
						yield return instruction;
				yield return Instruction.Create(OpCodes.Callvirt, provideValue);
				yield return Instruction.Create(OpCodes.Stloc, vardefref.VariableDefinition);
			}
			else if (context.Variables[node].VariableType.ImplementsInterface(module.ImportReference(typeof (IValueProvider))))
			{
				var acceptEmptyServiceProvider = context.Variables[node].VariableType.GetCustomAttribute(module.ImportReference(typeof(AcceptEmptyServiceProviderAttribute))) != null;
				var valueProviderType = context.Variables[node].VariableType;
				//If the IValueProvider has a ProvideCompiledAttribute that can be resolved, shortcut this
				var compiledValueProviderName = valueProviderType?.GetCustomAttribute(module.ImportReference(typeof(ProvideCompiledAttribute)))?.ConstructorArguments?[0].Value as string;
				Type compiledValueProviderType;
				if (compiledValueProviderName != null && (compiledValueProviderType = Type.GetType(compiledValueProviderName)) != null) {
					var compiledValueProvider = Activator.CreateInstance(compiledValueProviderType);
					var cProvideValue = typeof(ICompiledValueProvider).GetMethods().FirstOrDefault(md => md.Name == "ProvideValue");
					var instructions = (IEnumerable<Instruction>)cProvideValue.Invoke(compiledValueProvider, new object[] {
						vardefref,
						context.Body.Method.Module,
						node as BaseNode,
						context});
					foreach (var i in instructions)
						yield return i;
					yield break;
				}

				var valueProviderDef = module.ImportReference(typeof (IValueProvider)).Resolve();
				var provideValueInfo = valueProviderDef.Methods.First(md => md.Name == "ProvideValue");
				var provideValue = module.ImportReference(provideValueInfo);

				vardefref.VariableDefinition = new VariableDefinition(module.TypeSystem.Object);
				yield return Instruction.Create(OpCodes.Ldloc, context.Variables[node]);
				if (acceptEmptyServiceProvider)
					yield return Instruction.Create(OpCodes.Ldnull);
				else
					foreach (var instruction in node.PushServiceProvider(context, bpRef, propertyRef, propertyDeclaringTypeRef))
						yield return instruction;
				yield return Instruction.Create(OpCodes.Callvirt, provideValue);
				yield return Instruction.Create(OpCodes.Stloc, vardefref.VariableDefinition);
			}
		}

		//Once we get compiled IValueProvider, this will move to the BindingExpression
		static IEnumerable<Instruction> CompileBindingPath(ElementNode node, ILContext context, VariableDefinition bindingExt)
		{
			//TODO implement handlers[]
			//TODO support casting operators

			INode pathNode;
			if (!node.Properties.TryGetValue(new XmlName("", "Path"), out pathNode) && node.CollectionItems.Any())
				pathNode = node.CollectionItems [0];
			var path = (pathNode as ValueNode)?.Value as string;

			INode dataTypeNode = null;
			IElementNode n = node;
			while (n != null) {
				if (n.Properties.TryGetValue(XmlName.xDataType, out dataTypeNode))
					break;
				n = n.Parent as IElementNode;
			}
			var dataType = (dataTypeNode as ValueNode)?.Value as string;
			if (dataType == null)
				yield break; //throw

			var namespaceuri = dataType.Contains(":") ? node.NamespaceResolver.LookupNamespace(dataType.Split(':') [0].Trim()) : "";
			var dtXType = new XmlType(namespaceuri, dataType, null);

			var tSourceRef = dtXType.GetTypeReference(context.Module, (IXmlLineInfo)node);
			if (tSourceRef == null)
				yield break; //throw

			var properties = ParsePath(path, tSourceRef, node as IXmlLineInfo, context.Module);
			var tPropertyRef = properties != null && properties.Any() ? properties.Last().Item1.PropertyType : tSourceRef;

			var funcRef = context.Module.ImportReference(context.Module.ImportReference(typeof(Func<,>)).MakeGenericInstanceType(new [] { tSourceRef, tPropertyRef }));
			var actionRef = context.Module.ImportReference(context.Module.ImportReference(typeof(Action<,>)).MakeGenericInstanceType(new [] { tSourceRef, tPropertyRef }));
			var funcObjRef = context.Module.ImportReference(context.Module.ImportReference(typeof(Func<,>)).MakeGenericInstanceType(new [] { tSourceRef, context.Module.TypeSystem.Object }));
			var tupleRef = context.Module.ImportReference(context.Module.ImportReference(typeof(Tuple<,>)).MakeGenericInstanceType(new [] { funcObjRef, context.Module.TypeSystem.String}));
			var typedBindingRef = context.Module.ImportReference(context.Module.ImportReference(typeof(TypedBinding<,>)).MakeGenericInstanceType(new [] { tSourceRef, tPropertyRef}));

			TypeReference _;
			var ctorInfo =  context.Module.ImportReference(typedBindingRef.Resolve().Methods.FirstOrDefault(md => md.IsConstructor && !md.IsStatic && md.Parameters.Count == 3 ));
			var ctorinforef = ctorInfo.MakeGeneric(typedBindingRef, funcRef, actionRef, tupleRef);
			var setTypedBinding = context.Module.ImportReference(typeof(BindingExtension)).GetProperty(pd => pd.Name == "TypedBinding", out _).SetMethod;

			yield return Instruction.Create(OpCodes.Ldloc, bindingExt);
			foreach (var instruction in CompiledBindingGetGetter(tSourceRef, tPropertyRef, properties, node, context))
				yield return instruction;
			foreach (var instruction in CompiledBindingGetSetter(tSourceRef, tPropertyRef, properties, node, context))
				yield return instruction;
			foreach (var instruction in CompiledBindingGetHandlers(tSourceRef, tPropertyRef, properties, node, context))
				yield return instruction;
			yield return Instruction.Create(OpCodes.Newobj, context.Module.ImportReference(ctorinforef));
			yield return Instruction.Create(OpCodes.Callvirt, context.Module.ImportReference(setTypedBinding));
		}

		static IList<Tuple<PropertyDefinition, string>> ParsePath(string path, TypeReference tSourceRef, IXmlLineInfo lineInfo, ModuleDefinition module)
		{
			if (string.IsNullOrWhiteSpace(path))
				return null;
			path = path.Trim(' ', '.'); //trim leading or trailing dots
			var parts = path.Split(new [] { '.' }, StringSplitOptions.RemoveEmptyEntries);
			var properties = new List<Tuple<PropertyDefinition, string>>();

			var previousPartTypeRef = tSourceRef;
			TypeReference _;
			foreach (var part in parts) {
				var p = part;
				string indexArg = null;
				var lbIndex = p.IndexOf('[');
				if (lbIndex != -1) {
					var rbIndex = p.LastIndexOf(']');
					if (rbIndex == -1)
						throw new XamlParseException("Binding: Indexer did not contain closing bracket", lineInfo);
					
					var argLength = rbIndex - lbIndex - 1;
					if (argLength == 0)
						throw new XamlParseException("Binding: Indexer did not contain arguments", lineInfo);

					indexArg = p.Substring(lbIndex + 1, argLength).Trim();
					if (indexArg.Length == 0)
						throw new XamlParseException("Binding: Indexer did not contain arguments", lineInfo);
					
					p = p.Substring(0, lbIndex);
					p = p.Trim();
				}

				if (p.Length > 0) {
					var property = previousPartTypeRef.GetProperty(pd => pd.Name == p && pd.GetMethod != null && pd.GetMethod.IsPublic, out _);
					properties.Add(new Tuple<PropertyDefinition, string>(property,null));
					previousPartTypeRef = property.PropertyType;
				}
				if (indexArg != null) {
					var defaultMemberAttribute = previousPartTypeRef.GetCustomAttribute(module.ImportReference(typeof(System.Reflection.DefaultMemberAttribute)));
					var indexerName = defaultMemberAttribute?.ConstructorArguments?.FirstOrDefault().Value as string ?? "Item";
					var indexer = previousPartTypeRef.GetProperty(pd => pd.Name == indexerName && pd.GetMethod != null && pd.GetMethod.IsPublic, out _);
					properties.Add(new Tuple<PropertyDefinition, string>(indexer, indexArg));
					if (indexer.PropertyType != module.TypeSystem.String && indexer.PropertyType != module.TypeSystem.Int32)
						throw new XamlParseException($"Binding: Unsupported indexer index type: {indexer.PropertyType.FullName}", lineInfo);
					previousPartTypeRef = indexer.PropertyType;
				}
			}
			return properties;
		}

		static IEnumerable<Instruction> CompiledBindingGetGetter(TypeReference tSourceRef, TypeReference tPropertyRef, IList<Tuple<PropertyDefinition, string>> properties, ElementNode node, ILContext context)
		{
//			.method private static hidebysig default string '<Main>m__0' (class ViewModel vm)  cil managed
//			{
//				.custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ...
//
//				IL_0000:  ldarg.0 
//				IL_0001:  callvirt instance class ViewModel class ViewModel::get_Model()
//				IL_0006:  callvirt instance string class ViewModel::get_Text()
//				IL_0006:  ret
//			}

			var module = context.Module;
			var compilerGeneratedCtor = module.ImportReference(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)).GetMethods(md => md.IsConstructor, module).First().Item1;
			var getter = new MethodDefinition($"<{context.Body.Method.Name}>typedBindingsM__{typedBindingCount++}",
											  MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static,
											  tPropertyRef) {
				Parameters = {
					new ParameterDefinition(tSourceRef)
				},
				CustomAttributes = {
					new CustomAttribute (context.Module.ImportReference(compilerGeneratedCtor))
				}
			};
			getter.Body.InitLocals = true;
			var il = getter.Body.GetILProcessor();

			il.Emit(OpCodes.Ldarg_0);
			if (properties != null && properties.Count != 0) {
				foreach (var propTuple in properties) {
					var property = propTuple.Item1;
					var indexerArg = propTuple.Item2;
					if (indexerArg != null) {
						if (property.GetMethod.Parameters [0].ParameterType == module.TypeSystem.String)
							il.Emit(OpCodes.Ldstr, indexerArg);
						else if (property.GetMethod.Parameters [0].ParameterType == module.TypeSystem.Int32) {
							int index;
							if (!int.TryParse(indexerArg, out index))
								throw new XamlParseException($"Binding: {indexerArg} could not be parsed as an index for a {property.Name}", node as IXmlLineInfo);
							il.Emit(OpCodes.Ldc_I4, index);
						}
					}
					il.Emit(OpCodes.Callvirt, module.ImportReference(property.GetMethod));
				}
			}

			il.Emit(OpCodes.Ret);

			context.Body.Method.DeclaringType.Methods.Add(getter);

			var funcRef = module.ImportReference(typeof(Func<,>));
			funcRef = module.ImportReference(funcRef.MakeGenericInstanceType(new [] { tSourceRef, tPropertyRef }));
			var funcCtor = module.ImportReference(funcRef.Resolve().GetConstructors().First());
			funcCtor = funcCtor.MakeGeneric(funcRef, new [] { tSourceRef, tPropertyRef });

//			IL_0007:  ldnull
//			IL_0008:  ldftn string class Test::'<Main>m__0'(class ViewModel)
//			IL_000e:  newobj instance void class [mscorlib]System.Func`2<class ViewModel, string>::'.ctor'(object, native int)

			yield return Instruction.Create(OpCodes.Ldnull);
			yield return Instruction.Create(OpCodes.Ldftn, getter);
			yield return Instruction.Create(OpCodes.Newobj, module.ImportReference(funcCtor));
		}

		static IEnumerable<Instruction> CompiledBindingGetSetter(TypeReference tSourceRef, TypeReference tPropertyRef, IList<Tuple<PropertyDefinition, string>> properties, ElementNode node, ILContext context)
		{
			if (properties == null || properties.Count == 0) {
				yield return Instruction.Create(OpCodes.Ldnull);
				yield break;
			}

			//			.method private static hidebysig default void '<Main>m__1' (class ViewModel vm, string s)  cil managed
			//			{
			//				.custom instance void class [mscorlib]System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....
			//
			//				IL_0000:  ldarg.0 
			//				IL_0001:  callvirt instance class ViewModel class ViewModel::get_Model()
			//				IL_0006:  ldarg.1 
			//				IL_0007:  callvirt instance void class ViewModel::set_Text(string)
			//				IL_000c:  ret
			//			}

			var module = context.Module;
			var compilerGeneratedCtor = module.ImportReference(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)).GetMethods(md => md.IsConstructor, module).First().Item1;
			var setter = new MethodDefinition($"<{context.Body.Method.Name}>typedBindingsM__{typedBindingCount++}",
											  MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static,
			                                  module.TypeSystem.Void) {
				Parameters = {
					new ParameterDefinition(tSourceRef),
					new ParameterDefinition(tPropertyRef)
				},
				CustomAttributes = {
					new CustomAttribute (module.ImportReference(compilerGeneratedCtor))
				}
			};
			setter.Body.InitLocals = true;

			var il = setter.Body.GetILProcessor();
			var lastProperty = properties.LastOrDefault();
			var setterRef = lastProperty?.Item1.SetMethod;
			if (setterRef == null) {
				yield return Instruction.Create(OpCodes.Ldnull); //throw or not ?
				yield break;
			}

			il.Emit(OpCodes.Ldarg_0);
			for (int i = 0; i < properties.Count - 1; i++) {
				var property = properties[i].Item1;
				var indexerArg = properties[i].Item2;
				if (indexerArg != null) {
					if (property.GetMethod.Parameters [0].ParameterType == module.TypeSystem.String)
						il.Emit(OpCodes.Ldstr, indexerArg);
					else if (property.GetMethod.Parameters [0].ParameterType == module.TypeSystem.Int32) {
						int index;
						if (!int.TryParse(indexerArg, out index))
							throw new XamlParseException($"Binding: {indexerArg} could not be parsed as an index for a {property.Name}", node as IXmlLineInfo);
						il.Emit(OpCodes.Ldc_I4, index);
					}
				}
				il.Emit(OpCodes.Callvirt, module.ImportReference(property.GetMethod));
			}

			var indexer = properties.Last().Item2;
			if (indexer != null) {
				if (lastProperty.Item1.GetMethod.Parameters [0].ParameterType == module.TypeSystem.String)
					il.Emit(OpCodes.Ldstr, indexer);
				else if (lastProperty.Item1.GetMethod.Parameters [0].ParameterType == module.TypeSystem.Int32) {
					int index;
					if (!int.TryParse(indexer, out index))
						throw new XamlParseException($"Binding: {indexer} could not be parsed as an index for a {lastProperty.Item1.Name}", node as IXmlLineInfo);
					il.Emit(OpCodes.Ldc_I4, index);
				}
			}
			il.Emit(OpCodes.Ldarg_1);
			il.Emit(OpCodes.Callvirt, module.ImportReference(setterRef));
			il.Emit(OpCodes.Ret);

			context.Body.Method.DeclaringType.Methods.Add(setter);

			var actionRef = module.ImportReference(typeof(Action<,>));
			actionRef = module.ImportReference(actionRef.MakeGenericInstanceType(new [] { tSourceRef, tPropertyRef }));
			var actionCtor = module.ImportReference(actionRef.Resolve().GetConstructors().First());
			actionCtor = actionCtor.MakeGeneric(actionRef, new [] { tSourceRef, tPropertyRef });

//			IL_0024: ldnull
//			IL_0025: ldftn void class Test::'<Main>m__1'(class ViewModel, string)
//			IL_002b: newobj instance void class [mscorlib]System.Action`2<class ViewModel, string>::'.ctor'(object, native int)
			yield return Instruction.Create(OpCodes.Ldnull);
			yield return Instruction.Create(OpCodes.Ldftn, setter);
			yield return Instruction.Create(OpCodes.Newobj, module.ImportReference(actionCtor));
		}

		static IEnumerable<Instruction> CompiledBindingGetHandlers(TypeReference tSourceRef, TypeReference tPropertyRef, IList<Tuple<PropertyDefinition, string>> properties, ElementNode node, ILContext context)
		{
//			.method private static hidebysig default object '<Main>m__2'(class ViewModel vm)  cil managed {
//				.custom instance void class [mscorlib] System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....
//				IL_0000:  ldarg.0 
//				IL_0001:  ret
//			} // end of method Test::<Main>m__2

//			.method private static hidebysig default object '<Main>m__3' (class ViewModel vm)  cil managed {
//				.custom instance void class [mscorlib] System.Runtime.CompilerServices.CompilerGeneratedAttribute::'.ctor'() =  (01 00 00 00 ) // ....
//				IL_0000:  ldarg.0 
//				IL_0001:  callvirt instance class ViewModel class ViewModel::get_Model()
//				IL_0006:  ret
//			}

			var module = context.Module;
			var compilerGeneratedCtor = module.ImportReference(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)).GetMethods(md => md.IsConstructor, module).First().Item1;

			var partGetters = new List<MethodDefinition>();
			if (properties == null || properties.Count == 0) {
				yield return Instruction.Create(OpCodes.Ldnull);
				yield break;
			}
				
			for (int i = 0; i < properties.Count; i++) {
				var tuple = properties [i];
				var partGetter = new MethodDefinition($"<{context.Body.Method.Name}>typedBindingsM__{typedBindingCount++}", MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.Static, tPropertyRef) {
					Parameters = {
						new ParameterDefinition(tSourceRef)
					},
					CustomAttributes = {
						new CustomAttribute (context.Module.ImportReference(compilerGeneratedCtor))
					}
				};
				partGetter.Body.InitLocals = true;
				var il = partGetter.Body.GetILProcessor();
				il.Emit(OpCodes.Ldarg_0);
				for (int j = 0; j < i; j++) {
					var propTuple = properties [j];
					var property = propTuple.Item1;
					var indexerArg = propTuple.Item2;
					if (indexerArg != null) {
						if (property.GetMethod.Parameters [0].ParameterType == module.TypeSystem.String)
							il.Emit(OpCodes.Ldstr, indexerArg);
						else if (property.GetMethod.Parameters [0].ParameterType == module.TypeSystem.Int32) {
							int index;
							if (!int.TryParse(indexerArg, out index))
								throw new XamlParseException($"Binding: {indexerArg} could not be parsed as an index for a {property.Name}", node as IXmlLineInfo);
							il.Emit(OpCodes.Ldc_I4, index);
						}
					}
					il.Emit(OpCodes.Callvirt, module.ImportReference(property.GetMethod));
				}
				il.Emit(OpCodes.Ret);
				context.Body.Method.DeclaringType.Methods.Add(partGetter);
				partGetters.Add(partGetter);
			}

			var funcObjRef = context.Module.ImportReference(module.ImportReference(typeof(Func<,>)).MakeGenericInstanceType(new [] { tSourceRef, module.TypeSystem.Object }));
			var tupleRef = context.Module.ImportReference(module.ImportReference(typeof(Tuple<,>)).MakeGenericInstanceType(new [] { funcObjRef, module.TypeSystem.String }));
			var funcCtor = module.ImportReference(funcObjRef.Resolve().GetConstructors().First());
			funcCtor = funcCtor.MakeGeneric(funcObjRef, new [] { tSourceRef, module.TypeSystem.Object });
			var tupleCtor = module.ImportReference(tupleRef.Resolve().GetConstructors().First());
			tupleCtor = tupleCtor.MakeGeneric(tupleRef, new [] { funcObjRef, module.TypeSystem.String});

//			IL_003a:  ldc.i4.2 
//			IL_003b:  newarr class [mscorlib] System.Tuple`2<class [mscorlib]System.Func`2<class ViewModel,object>,string>

//			IL_0040:  dup
//			IL_0041:  ldc.i4.0 
//			IL_0049:  ldnull
//			IL_004a:  ldftn object class Test::'<Main>m__2'(class ViewModel)
//			IL_0050:  newobj instance void class [mscorlib]System.Func`2<class ViewModel, object>::'.ctor'(object, native int)
//			IL_005f:  ldstr "Model"
//			IL_0064:  newobj instance void class [mscorlib]System.Tuple`2<class [mscorlib]System.Func`2<class ViewModel, object>, string>::'.ctor'(!0, !1)
//			IL_0069:  stelem.ref 

//			IL_006a:  dup
//			IL_006b:  ldc.i4.1 
//			IL_0073:  ldnull
//			IL_0074:  ldftn object class Test::'<Main>m__3'(class ViewModel)
//			IL_007a:  newobj instance void class [mscorlib]System.Func`2<class ViewModel, object>::'.ctor'(object, native int)
//			IL_0089:  ldstr "Text"
//			IL_008e:  newobj instance void class [mscorlib]System.Tuple`2<class [mscorlib]System.Func`2<class ViewModel, object>, string>::'.ctor'(!0, !1)
//			IL_0093:  stelem.ref 

			yield return Instruction.Create(OpCodes.Ldc_I4, properties.Count);
			yield return Instruction.Create(OpCodes.Newarr, tupleRef);

			for (var i = 0; i < properties.Count; i++) {
				yield return Instruction.Create(OpCodes.Dup);
				yield return Instruction.Create(OpCodes.Ldc_I4, i);
				yield return Instruction.Create(OpCodes.Ldnull);
				yield return Instruction.Create(OpCodes.Ldftn, partGetters [i]);
				yield return Instruction.Create(OpCodes.Newobj, module.ImportReference(funcCtor));
				yield return Instruction.Create(OpCodes.Ldstr, properties [i].Item1.Name);
				yield return Instruction.Create(OpCodes.Newobj, module.ImportReference(tupleCtor));
				yield return Instruction.Create(OpCodes.Stelem_Ref);
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

			throw new XamlParseException($"No property, bindable property, or event found for '{localName}', or mismatching type between value and property.", iXmlLineInfo);
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
				bpRef = module.ImportReference(bpRef.ResolveGenericParameters(declaringTypeReference));
				bpRef.FieldType = module.ImportReference(bpRef.FieldType);
			}
			return bpRef;
		}

		static bool CanConnectEvent(VariableDefinition parent, string localName)
		{
			TypeReference _;
			return parent.VariableType.GetEvent(ed => ed.Name == localName, out _) != null;
		}

		static IEnumerable<Instruction> ConnectEvent(VariableDefinition parent, string localName, INode valueNode, IXmlLineInfo iXmlLineInfo, ILContext context)
		{
			var elementType = parent.VariableType;
			var module = context.Body.Method.Module;
			TypeReference eventDeclaringTypeRef;
			var eventinfo = elementType.GetEvent(ed => ed.Name == localName, out eventDeclaringTypeRef);

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
			var ctor = module.ImportReference(eventinfo.EventType.Resolve().GetConstructors().First());
			ctor = ctor.ResolveGenericParameters(eventinfo.EventType, module);
			yield return Instruction.Create(OpCodes.Newobj, module.ImportReference(ctor));
			var adder = module.ImportReference(eventinfo.AddMethod);
			adder = adder.ResolveGenericParameters(eventDeclaringTypeRef, module);
			yield return Instruction.Create(OpCodes.Callvirt, module.ImportReference(adder));
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
			var setDynamicResource = module.ImportReference(typeof(IDynamicResourceHandler)).Resolve().Methods.First(m => m.Name == "SetDynamicResource");
			var getKey = typeof(DynamicResource).GetProperty("Key").GetMethod;

			yield return Instruction.Create(OpCodes.Ldloc, parent);
			yield return Instruction.Create(OpCodes.Ldsfld, bpRef);
			yield return Instruction.Create(OpCodes.Ldloc, varValue);
			yield return Instruction.Create(OpCodes.Callvirt, module.ImportReference(getKey));
			yield return Instruction.Create(OpCodes.Callvirt, module.ImportReference(setDynamicResource));
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
			var implicitOperator = varValue.VariableType.GetImplicitOperatorTo(module.ImportReference(typeof(BindingBase)), module);
			if (implicitOperator != null)
				return true;

			return varValue.VariableType.InheritsFromOrImplements(module.ImportReference(typeof(BindingBase)));
		}

		static IEnumerable<Instruction> SetBinding(VariableDefinition parent, FieldReference bpRef, IElementNode elementNode, IXmlLineInfo iXmlLineInfo, ILContext context)
		{
			var module = context.Body.Method.Module;
			var varValue = context.Variables [elementNode];
			var implicitOperator = varValue.VariableType.GetImplicitOperatorTo(module.ImportReference(typeof(BindingBase)), module);

			//TODO: check if parent is a BP
			var setBinding = typeof(BindableObject).GetMethod("SetBinding", new [] { typeof(BindableProperty), typeof(BindingBase) });

			yield return Instruction.Create(OpCodes.Ldloc, parent);
			yield return Instruction.Create(OpCodes.Ldsfld, bpRef);
			yield return Instruction.Create(OpCodes.Ldloc, varValue);
			if (implicitOperator != null) 
//				IL_000f:  call !0 class [Xamarin.Forms.Core]Xamarin.Forms.OnPlatform`1<BindingBase>::op_Implicit(class [Xamarin.Forms.Core]Xamarin.Forms.OnPlatform`1<!0>)
				yield return Instruction.Create(OpCodes.Call, module.ImportReference(implicitOperator));
			yield return Instruction.Create(OpCodes.Callvirt, module.ImportReference(setBinding));
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

			//as we're in the SetValue Scenario, we can accept value types, they'll be boxed
			if (varValue.VariableType.IsValueType && bpTypeRef.FullName == "System.Object")
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
					yield return Instruction.Create(OpCodes.Call, module.ImportReference(implicitOperator));
					varType = module.ImportReference(bpTypeRef);
				}
				if (varType.IsValueType)
					yield return Instruction.Create(OpCodes.Box, varType);
			}

			yield return Instruction.Create(OpCodes.Callvirt, module.ImportReference(setValue));
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
			var propertyType = property.ResolveGenericPropertyType(declaringTypeReference, module);
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

			module.ImportReference(parent.VariableType.Resolve());
			var propertySetterRef = module.ImportReference(module.ImportReference(propertySetter).ResolveGenericParameters(declaringTypeReference, module));
			propertySetterRef.ImportTypes(module);
			var propertyType = property.ResolveGenericPropertyType(declaringTypeReference, module);
			var valueNode = node as ValueNode;
			var elementNode = node as IElementNode;

			//if it's a value type, load the address so we can invoke methods on it
			if (parent.VariableType.IsValueType)
				yield return Instruction.Create(OpCodes.Ldloca, parent);
			else
				yield return Instruction.Create(OpCodes.Ldloc, parent);

			if (valueNode != null) {
				foreach (var instruction in valueNode.PushConvertedValue(context, propertyType, new ICustomAttributeProvider [] { property, propertyType.Resolve() }, valueNode.PushServiceProvider(context, propertyRef:property), false, true))
					yield return instruction;
				if (parent.VariableType.IsValueType)
					yield return Instruction.Create(OpCodes.Call, propertySetterRef);
				else
					yield return Instruction.Create(OpCodes.Callvirt, propertySetterRef);
			} else if (elementNode != null) {
				var vardef = context.Variables [elementNode];
				var implicitOperator = vardef.VariableType.GetImplicitOperatorTo(propertyType, module);
				yield return Instruction.Create(OpCodes.Ldloc, vardef);
				if (implicitOperator != null) {
//					IL_000f:  call !0 class [Xamarin.Forms.Core]Xamarin.Forms.OnPlatform`1<bool>::op_Implicit(class [Xamarin.Forms.Core]Xamarin.Forms.OnPlatform`1<!0>)
					yield return Instruction.Create(OpCodes.Call, module.ImportReference(implicitOperator));
				} else if (!vardef.VariableType.IsValueType && propertyType.IsValueType)
					yield return Instruction.Create(OpCodes.Unbox_Any, module.ImportReference(propertyType));
				else if (vardef.VariableType.IsValueType && propertyType.FullName == "System.Object")
					yield return Instruction.Create(OpCodes.Box, vardef.VariableType);
				if (parent.VariableType.IsValueType)
					yield return Instruction.Create(OpCodes.Call, propertySetterRef);
				else
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
			var propertyGetterRef = module.ImportReference(propertyGetter);
			propertyGetterRef = module.ImportReference(propertyGetterRef.ResolveGenericParameters(declaringTypeReference, module));
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
			var propertyGetterRef = module.ImportReference(propertyGetter);
			propertyGetterRef = module.ImportReference(propertyGetterRef.ResolveGenericParameters(declaringTypeReference, module));
			var propertyType = propertyGetterRef.ReturnType.ResolveGenericParameters(declaringTypeReference);
			//TODO check md.Parameters[0] type
			var adderTuple = propertyType.GetMethods(md => md.Name == "Add" && md.Parameters.Count == 1, module).FirstOrDefault();
			var adderRef = module.ImportReference(adderTuple.Item1);
			adderRef = module.ImportReference(adderRef.ResolveGenericParameters(adderTuple.Item2, module));
			var childType = GetParameterType(adderRef.Parameters [0]);
			var implicitOperator = vardef.VariableType.GetImplicitOperatorTo(childType, module);

			yield return Instruction.Create(OpCodes.Ldloc, parent);
			yield return Instruction.Create(OpCodes.Callvirt, propertyGetterRef);
			yield return Instruction.Create(OpCodes.Ldloc, vardef);
			if (implicitOperator != null)
				yield return Instruction.Create(OpCodes.Call, module.ImportReference(implicitOperator));
			if (implicitOperator == null && vardef.VariableType.IsValueType && !childType.IsValueType)
				yield return Instruction.Create(OpCodes.Box, vardef.VariableType);
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


			var module = parentContext.Module;
			var compilerGeneratedCtor = module.ImportReference(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute)).GetMethods(md => md.IsConstructor, module).First().Item1;
			var anonType = new TypeDefinition(
				null,
				"<" + parentContext.Body.Method.Name + ">_anonXamlCDataTemplate_" + dtcount++,
				TypeAttributes.BeforeFieldInit |
				TypeAttributes.Sealed |
				TypeAttributes.NestedPrivate)
			{
				BaseType = module.TypeSystem.Object,
				CustomAttributes = {
					new CustomAttribute (module.ImportReference(compilerGeneratedCtor))
				}
			};

			parentContext.Body.Method.DeclaringType.NestedTypes.Add(anonType);
			var ctor = anonType.AddDefaultConstructor();

			var loadTemplate = new MethodDefinition("LoadDataTemplate",
				MethodAttributes.Assembly | MethodAttributes.HideBySig,
				module.TypeSystem.Object);
			loadTemplate.Body.InitLocals = true;
			anonType.Methods.Add(loadTemplate);

			var parentValues = new FieldDefinition("parentValues", FieldAttributes.Assembly, module.ImportReference(typeof (object[])));
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
			var templateContext = new ILContext(templateIl, loadTemplate.Body, module, parentValues)
			{
				Root = root
			};
			node.Accept(new CreateObjectVisitor(templateContext), null);
			node.Accept(new SetNamescopesAndRegisterNamesVisitor(templateContext), null);
			node.Accept(new SetFieldVisitor(templateContext), null);
			node.Accept(new SetResourcesVisitor(templateContext), null);
			node.Accept(new SetPropertiesVisitor(templateContext, stopOnResourceDictionary: true), null);

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
			var funcObjRef = module.ImportReference(typeof(Func<object>));
			var funcCtor =
				funcObjRef
					.Resolve()
					.Methods.First(md => md.IsConstructor && md.Parameters.Count == 2)
					.ResolveGenericParameters(funcObjRef, module);
			parentIl.Emit(OpCodes.Newobj, module.ImportReference(funcCtor));

#pragma warning disable 0612
			var propertySetter =
				module.ImportReference(typeof (IDataTemplate)).Resolve().Properties.First(p => p.Name == "LoadTemplate").SetMethod;
#pragma warning restore 0612
			parentContext.IL.Emit(OpCodes.Callvirt, module.ImportReference(propertySetter));

			loadTemplate.Body.Optimize();
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
