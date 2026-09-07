using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Microsoft.Maui.Controls.Xaml.Internals;

using static System.String;

namespace Microsoft.Maui.Controls.Xaml
{
	[RequiresUnreferencedCode(TrimmerConstants.XamlRuntimeParsingNotSupportedWarning)]
#if !NETSTANDARD
	[RequiresDynamicCode(TrimmerConstants.XamlRuntimeParsingNotSupportedWarning)]
#endif
	class ApplyPropertiesVisitor : IXamlNodeVisitor
	{
		public static readonly IList<XmlName> Skips = [
			XmlName.xArguments,
			XmlName.xClass,
			XmlName.xClassModifier,
			XmlName.xDataType,
			XmlName.xFactoryMethod,
			XmlName.xFieldModifier,
			XmlName.xKey,
			XmlName.xName,
			XmlName.xTypeArguments,
		];

		public ApplyPropertiesVisitor(HydrationContext context, bool stopOnResourceDictionary = false)
		{
			Context = context;
			StopOnResourceDictionary = stopOnResourceDictionary;
		}

		Dictionary<INode, object> Values => Context.Values;
		HydrationContext Context { get; }

		public TreeVisitingMode VisitingMode => TreeVisitingMode.BottomUp;
		public bool StopOnDataTemplate => true;
		public bool StopOnResourceDictionary { get; }
		public bool VisitNodeOnDataTemplate => true;
		public bool SkipChildren(INode node, INode parentNode) => false;
		public bool IsResourceDictionary(ElementNode node) => Context.Types.TryGetValue(node, out var type) && typeof(ResourceDictionary).IsAssignableFrom(type);

		public void Visit(ValueNode node, INode parentNode)
		{
			var parentElement = parentNode as ElementNode;
			var value = Values[node];
			if (!Values.TryGetValue(parentNode, out var source) && Context.ExceptionHandler != null)
				return;


			if (node.TryGetPropertyName(parentNode, out XmlName propertyName))
			{
				if (TrySetRuntimeName(propertyName, source, value, node))
					return;
				if (propertyName == XmlName.xShared)
					throw new XamlParseException("x:Shared is only supported with SourceGen inflator. Remove the attribute or switch to SourceGen.", node);
				if (Skips.Contains(propertyName))
					return;
				if (parentElement.SkipProperties.Contains(propertyName))
					return;
				if (propertyName.Equals(XmlName.mcIgnorable))
					return;
				SetPropertyValue(source, propertyName, value, Context.RootElement, node, Context, node);
			}
			else if (IsCollectionItem(node, parentNode) && parentNode is ElementNode)
			{
				// Collection element, implicit content, or implicit collection element.
				var contentProperty = GetContentPropertyName(Context.Types[parentElement]);
				if (contentProperty != null)
				{
					var name = new XmlName(((ElementNode)parentNode).NamespaceURI, contentProperty);
					if (Skips.Contains(name))
						return;
					if (parentElement.SkipProperties.Contains(propertyName))
						return;
					SetPropertyValue(source, name, value, Context.RootElement, node, Context, node);
				}
			}
		}

		public void Visit(MarkupNode node, INode parentNode)
		{
		}

		public void Visit(ElementNode node, INode parentNode)
		{
			if (node.TryGetPropertyName(parentNode, out XmlName propertyName) && propertyName == XmlName._CreateContent)
			{
				var s0 = Values[parentNode];
				if (s0 is ElementTemplate)
				{
					SetTemplate(s0 as ElementTemplate, node);
					return;
				}
			}

			var parentElement = parentNode as ElementNode;
			propertyName = XmlName.Empty;

			//Simplify ListNodes with single elements
			if (parentNode is ListNode pList && pList.CollectionItems.Count == 1)
			{
				propertyName = pList.XmlName;
				parentNode = parentNode.Parent;
				parentElement = parentNode as ElementNode;
			}

			if (!Values.TryGetValue(node, out var value) && Context.ExceptionHandler != null)
				return;

			if (propertyName != XmlName.Empty || node.TryGetPropertyName(parentNode, out propertyName))
			{
				if (Skips.Contains(propertyName))
					return;
				if (parentElement.SkipProperties.Contains(propertyName))
					return;

				if (!Values.TryGetValue(parentNode, out var source) && Context.ExceptionHandler != null)
					return;
				ProvideValue(ref value, node, source, propertyName);
				SetPropertyValue(source, propertyName, value, Context.RootElement, node, Context, node);
			}
			else if (IsCollectionItem(node, parentNode) && parentNode is ElementNode)
			{
				if (!Values.TryGetValue(parentNode, out var source) && Context.ExceptionHandler != null)
					return;

				ProvideValue(ref value, node, source, XmlName.Empty);
				string contentProperty;
				Exception xpe = null;

				string xKey = null;
				if (xpe == null
					&& node.Properties.ContainsKey(XmlName.xKey))
				{
					if ((node.Properties[XmlName.xKey] is ValueNode valueNode))
						xKey = valueNode.Value as string;
					if (xKey == null)
						xpe = new XamlParseException("x:Key expects a string literal.", node as IXmlLineInfo);
				}

				//ResourceDictionary
				if (xpe == null
					&& TryAddToResourceDictionary(source as ResourceDictionary, value, xKey, node, out xpe))
					return;

				//ContentProperty
				if (xpe == null
					&& (contentProperty = GetContentPropertyName(Context.Types[parentElement])) != null)
				{
					var name = new XmlName(node.NamespaceURI, contentProperty);
					if (Skips.Contains(name))
						return;
					if (parentElement.SkipProperties.Contains(propertyName))
						return;

					SetPropertyValue(source, name, value, Context.RootElement, node, Context, node);
					return;
				}

				// Collection element, implicit content, or implicit collection element.
				if (xpe == null
					&& typeof(IEnumerable).IsAssignableFrom(Context.Types[parentElement])
					&& Context.Types[parentElement].GetRuntimeMethods().Any(mi => mi.Name == "Add" && mi.GetParameters().Length == 1))
				{
					var addMethod =
						Context.Types[parentElement].GetRuntimeMethods().First(mi => mi.Name == "Add" && mi.GetParameters().Length == 1);
					try
					{
						addMethod.Invoke(source, new[] { value.ConvertTo(addMethod.GetParameters()[0].ParameterType, (Func<TypeConverter>)null, new XamlServiceProvider(node, Context), out xpe) });
					}
					catch (Exception e)
					{
						xpe ??= e;
					}

					return;
				}

				xpe = xpe ?? new XamlParseException($"Cannot set the content of {((ElementNode)parentNode).XmlType.Name} as it doesn't have a ContentPropertyAttribute", node);
				if (Context.ExceptionHandler != null)
					Context.ExceptionHandler(xpe);
				else
					throw xpe;
			}
			else if (IsCollectionItem(node, parentNode) && parentNode is ListNode)
			{
				if (!Values.TryGetValue(parentNode.Parent, out var source) && Context.ExceptionHandler != null)
					return;
				ProvideValue(ref value, node, source, XmlName.Empty);
				var parentList = (ListNode)parentNode;
				if (Skips.Contains(parentList.XmlName))
					return;
				Exception xpe = null;
				string xKey = null;
				if (xpe == null && node.Properties.ContainsKey(XmlName.xKey))
				{
					if ((node.Properties[XmlName.xKey] is ValueNode valueNode))
						xKey = valueNode.Value as string;
					if (xKey == null)
						xpe = new XamlParseException("x:Key expects a string literal.", node as IXmlLineInfo);
				}

				var collection = GetPropertyValue(source, parentList.XmlName, Context.RootElement, parentList, out _, out _) as IEnumerable;
				if (xpe == null && collection == null)
					xpe = new XamlParseException($"Property {parentList.XmlName.LocalName} is null or is not IEnumerable", node);

				if (xpe == null && TryAddToResourceDictionary(collection as ResourceDictionary, value, xKey, node, out xpe))
					return;

				MethodInfo addMethod;
				if (xpe == null && (addMethod = collection.GetType().GetRuntimeMethods().First(mi => mi.Name == "Add" && mi.GetParameters().Length == 1)) != null)
				{
					addMethod.Invoke(collection, new[] { value });
					return;
				}
				xpe = xpe ?? new XamlParseException($"Value of {parentList.XmlName.LocalName} does not have a Add() method", node);
				if (Context.ExceptionHandler != null)
					Context.ExceptionHandler(xpe);
				else
					throw xpe;
			}
		}

		public void Visit(RootNode node, INode parentNode)
		{
		}

		public void Visit(ListNode node, INode parentNode)
		{
		}

		internal static bool IsCollectionItem(INode node, INode parentNode)
		{
			var parentList = parentNode as IListNode;
			if (parentList == null)
				return false;
			return parentList.CollectionItems.Contains(node);
		}

		internal static string GetContentPropertyName(Type type)
		{
			while (type != null)
			{
				var propName = GetContentPropertyName(type.CustomAttributes);
				if (propName != null)
					return propName;
				type = type.BaseType;
			}
			return null;
		}

		void ProvideValue(ref object value, ElementNode node, object source, XmlName propertyName)
		{
			var markupExtension = value as IMarkupExtension;
			var valueProvider = value as IValueProvider;

			if (markupExtension == null && valueProvider == null)
				return;

			XamlServiceProvider serviceProvider = null;
			if (value.GetType().GetCustomAttribute<AcceptEmptyServiceProviderAttribute>() == null)
				serviceProvider = new XamlServiceProvider(node, Context);

			if (serviceProvider != null && propertyName != XmlName.Empty)
				((XamlValueTargetProvider)serviceProvider.IProvideValueTarget).TargetProperty = GetTargetProperty(source, propertyName, Context.RootElement, node);

			try
			{
				if (markupExtension != null)
					value = markupExtension.ProvideValue(serviceProvider);
				else if (valueProvider != null)
					value = valueProvider.ProvideValue(serviceProvider);
			}
			catch (Exception e)
			{
				if (markupExtension is StaticResourceExtension)
				{
					if (Context.ExceptionHandler != null)
					{
						// During Hot Reload (handler present), report the exception and skip the
						// property rather than re-throwing.
						// Re-throwing here propagates through iOS UIKit lifecycle callbacks during
						// Shell item setup, corrupting Shell state and crashing the app (#35018).
						Context.ExceptionHandler(e);
						value = null;
						return;
					}
					throw;
				}

				if (Context.ExceptionHandler != null)
					Context.ExceptionHandler(e);
				else
					throw e;
			}
		}

		static string GetContentPropertyName(IEnumerable<CustomAttributeData> attributes)
		{
			var contentAttribute =
				attributes.FirstOrDefault(cad => ContentPropertyAttribute.ContentPropertyTypes.Contains(cad.AttributeType.FullName));
			if (contentAttribute == null || contentAttribute.ConstructorArguments.Count != 1)
				return null;
			if (contentAttribute.ConstructorArguments[0].ArgumentType == typeof(string))
				return (string)contentAttribute.ConstructorArguments[0].Value;
			return null;
		}

		static bool GetRealNameAndType(ref Type elementType, string namespaceURI, ref string localname,
			object rootElement, IXmlLineInfo lineInfo)
		{
			var dotIdx = localname.IndexOf(".", StringComparison.Ordinal);
			if (dotIdx > 0)
			{
				var typename = localname.Substring(0, dotIdx);
				localname = localname.Substring(dotIdx + 1);
				XamlParseException xpe;
				elementType = XamlParser.GetElementType(new XmlType(namespaceURI, typename, null), lineInfo,
					rootElement.GetType().Assembly, true, out xpe);

				if (xpe != null)
					throw xpe;
				return true;
			}
			return false;
		}

		static BindableProperty GetBindableProperty(Type elementType, string localName, IXmlLineInfo lineInfo)
		{
			// F# does not support public fields, so allow internal (Assembly) as well as public
			const BindingFlags supportedFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
			var bindableFieldInfo = elementType.GetField(localName + "Property", supportedFlags);
			if (bindableFieldInfo is not null && (bindableFieldInfo.IsAssembly || bindableFieldInfo.IsPublic))
			{
				return bindableFieldInfo.GetValue(null) as BindableProperty;
			}
			return null;
		}

		static object GetTargetProperty(object xamlelement, XmlName propertyName, object rootElement, IXmlLineInfo lineInfo)
		{
			var localName = propertyName.LocalName;
			//If it's an attached BP, update elementType and propertyName
			var bpOwnerType = xamlelement.GetType();
			GetRealNameAndType(ref bpOwnerType, propertyName.NamespaceURI, ref localName, rootElement, lineInfo);
			var property = GetBindableProperty(bpOwnerType, localName, lineInfo);

			if (property != null)
				return property;

			var elementType = xamlelement.GetType();
			var propertyInfo = elementType.GetRuntimeProperties().FirstOrDefault(p => p.Name == localName);
			return propertyInfo;
		}

		public static void SetPropertyValue(object xamlelement, XmlName propertyName, object value, object rootElement, INode node, HydrationContext context, IXmlLineInfo lineInfo)
		{
			var serviceProvider = new XamlServiceProvider(node, context);
			var xKey = node is ElementNode eNode && eNode.Properties.ContainsKey(XmlName.xKey) ? ((ValueNode)eNode.Properties[XmlName.xKey]).Value as string : null;

			// Special handling for ResourceDictionary.Source
			if (xamlelement is ResourceDictionary rd && propertyName.LocalName == "Source" && propertyName.NamespaceURI == "")
			{
				ResourceDictionaryHelpers.LoadFromSource(rd, (string)value, rootElement.GetType(), lineInfo);
				return;
			}

			if (TrySetPropertyValue(xamlelement, propertyName, xKey, value, rootElement, lineInfo, serviceProvider, out var xpe))
				return;

			if (context.ExceptionHandler != null)
				context.ExceptionHandler(xpe);
			else
				throw xpe;
		}

		//Used by HotReload, do not change signature
		public static bool TrySetPropertyValue(object element, XmlName propertyName, string xKey, object value, object rootElement, IXmlLineInfo lineInfo, IServiceProvider serviceProvider, out Exception xpe)
		{
			var localName = propertyName.LocalName;
			xpe = null;

			void registerSourceInfo(object target, string path)
			{
				if (VisualDiagnostics.GetSourceInfo(target) != null)
					return;
				var assemblyName = rootElement.GetType().Assembly?.GetName().Name;
				if (lineInfo != null)
					VisualDiagnostics.RegisterSourceInfo(target, new Uri($"{path};assembly={assemblyName}", UriKind.Relative), lineInfo.LineNumber, lineInfo.LinePosition);
			}

			//If it's an attached BP, update elementType and propertyName
			var bpOwnerType = element.GetType();
			var attached = GetRealNameAndType(ref bpOwnerType, propertyName.NamespaceURI, ref localName, rootElement, lineInfo);
			var property = GetBindableProperty(bpOwnerType, localName, lineInfo);

			//If the target is an event, connect
			if (xpe == null && TryConnectEvent(element, localName, attached, value, rootElement, lineInfo, out xpe))
				return true;

			//If Value is DynamicResource and it's a BP, SetDynamicResource
			if (xpe == null && TrySetDynamicResource(element, property, value, lineInfo, out xpe))
				return true;

			//If value is BindingBase, SetBinding
			if (xpe == null && TrySetBinding(element, property, localName, value, lineInfo, out var binding, out xpe))
			{
				if (binding != null && XamlFilePathAttribute.GetFilePathForObject(rootElement) is string path)
					registerSourceInfo(binding, path);
				return true;
			}

			//If it's a BindableProberty, SetValue
			if (xpe == null && TrySetValue(element, property, attached, value, lineInfo, serviceProvider, out xpe))
			{
				if (value != null && !value.GetType().IsValueType && XamlFilePathAttribute.GetFilePathForObject(rootElement) is string path)
					registerSourceInfo(value, path);
				return true;
			}

			//If the member is qualified with an extension container type, set the extension property
			if (xpe == null && attached && TrySetExtensionProperty(element, bpOwnerType, localName, value, lineInfo, serviceProvider, rootElement, out xpe))
			{
				if (value != null && !value.GetType().IsValueType && XamlFilePathAttribute.GetFilePathForObject(rootElement) is string path)
					registerSourceInfo(value, path);
				return true;
			}

			//If we can assign that value to a normal property, let's do it
			if (xpe == null && TrySetProperty(element, localName, value, lineInfo, serviceProvider, rootElement, out xpe))
			{
				if (value != null && !value.GetType().IsValueType && XamlFilePathAttribute.GetFilePathForObject(rootElement) is string path)
					registerSourceInfo(value, path);
				return true;
			}

			//If it's an already initialized property, add to it
			if (xpe == null && TryAddToProperty(element, propertyName, value, xKey, lineInfo, serviceProvider, rootElement, out xpe))
			{
				if (value != null && !value.GetType().IsValueType && XamlFilePathAttribute.GetFilePathForObject(rootElement) is string path)
					registerSourceInfo(value, path);
				return true;
			}

			xpe = xpe ?? new XamlParseException($"Cannot assign property \"{localName}\": Property does not exist, or is not assignable, or mismatching type between value and property", lineInfo);
			return false;
		}

		public static object GetPropertyValue(object xamlElement, XmlName propertyName, object rootElement, IXmlLineInfo lineInfo, out Exception xpe, out object targetProperty)
		{
			var localName = propertyName.LocalName;
			xpe = null;
			targetProperty = null;

			//If it's an attached BP, update elementType and propertyName
			var bpOwnerType = xamlElement.GetType();
			var attached = GetRealNameAndType(ref bpOwnerType, propertyName.NamespaceURI, ref localName, rootElement, lineInfo);
			var property = GetBindableProperty(bpOwnerType, localName, lineInfo);

			//If it's a BindableProberty, GetValue
			if (xpe == null && TryGetValue(xamlElement, property, attached, out var value, lineInfo, out xpe, out targetProperty))
				return value;

			//If it's a normal property, get it
			if (xpe == null && TryGetProperty(xamlElement, localName, out value, lineInfo, rootElement, out xpe, out targetProperty))
				return value;

			xpe = xpe ?? new XamlParseException($"Property {localName} is not found or does not have an accessible getter", lineInfo);

			return null;
		}

		static bool TryConnectEvent(object element, string localName, bool attached, object value, object rootElement, IXmlLineInfo lineInfo, out Exception exception)
		{
			exception = null;

			if (attached)
				return false;

			var elementType = element.GetType();
			var eventInfo = elementType.GetRuntimeEvent(localName) ?? elementType.GetRuntimeEvents().FirstOrDefault(ei => ei.Name == localName && !(ei.AddMethod.IsPrivate));
			var stringValue = value as string;

			if (eventInfo == null || IsNullOrEmpty(stringValue))
				return false;

			var addMethod = eventInfo.GetAddMethod(nonPublic: true);
			if (addMethod == null)
				return false;

			var rootElementType = rootElement.GetType();
			do
			{
				MethodInfo mi = null;
				try
				{
					mi = rootElementType.GetMethod((string)value, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				}
				catch (AmbiguousMatchException)
				{
					var n_params = eventInfo.EventHandlerType.GetMethod("Invoke").GetParameters().Length;
					var methodinfos = rootElementType.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
						.Where(mi => mi.Name == (string)value && mi.GetParameters().Length == n_params)
						.ToArray();

					//try to find an event handler with a signature matching the button delegate signature
					foreach (var methodinfo in methodinfos)
					{
						var parameters = methodinfo.GetParameters();
						for (var i = 0; i < n_params; i++)
						{
							if (!parameters[i].ParameterType.IsAssignableFrom(eventInfo.EventHandlerType.GetMethod("Invoke").GetParameters()[i].ParameterType))
								break;
						}
						mi = methodinfo;
						break;
					}
				}

				if (mi != null)
				{
					try
					{
						addMethod.Invoke(element, new[] { mi.CreateDelegate(eventInfo.EventHandlerType, mi.IsStatic ? null : rootElement) });
						return true;
					}
					catch (ArgumentException)
					{
						// incorrect method signature
					}
				}
				rootElementType = rootElementType.BaseType;
			} while (rootElementType is not null);

			exception = new XamlParseException($"No method {value} with correct signature found on type {rootElement.GetType()}", lineInfo);
			return false;
		}

		static bool TrySetDynamicResource(object element, BindableProperty property, object value, IXmlLineInfo lineInfo, out Exception exception)
		{
			exception = null;

			var elementType = element.GetType();
			var dynamicResource = value as DynamicResource;
			var bindable = element as BindableObject;

			if (dynamicResource == null || property == null)
				return false;

			if (bindable == null)
			{
				exception = new XamlParseException($"{elementType.Name} is not a BindableObject", lineInfo);
				return false;
			}

			bindable.RemoveBinding(property);
			bindable.SetDynamicResource(property, dynamicResource.Key);
			bindable.ClearValue(property);
			return true;
		}

		static bool TrySetBinding(object element, BindableProperty property, string localName, object value, IXmlLineInfo lineInfo, out BindingBase binding, out Exception exception)
		{
			exception = null;

			var elementType = element.GetType();
			binding = value.ConvertTo(typeof(BindingBase), pinfoRetriever: null, serviceProvider: null, exception: out exception) as BindingBase;
			if (exception != null)
				return false;

#pragma warning disable CS0618 // Type or member is obsolete
			var nativeBindingService = DependencyService.Get<INativeBindingService>();
#pragma warning restore CS0618 // Type or member is obsolete

			if (binding == null)
				return false;

			if (element is BindableObject bindable && property != null)
			{
				bindable.SetBinding(property, binding);
				return true;
			}

			if (nativeBindingService != null && property != null && nativeBindingService.TrySetBinding(element, property, binding))
				return true;

			if (nativeBindingService != null && nativeBindingService.TrySetBinding(element, localName, binding))
				return true;

			if (property != null)
				exception = new XamlParseException($"{elementType.Name} is not a BindableObject or does not support native bindings", lineInfo);

			return false;
		}

		static bool TrySetValue(object element, BindableProperty property, bool attached, object value, IXmlLineInfo lineInfo, IServiceProvider serviceProvider, out Exception exception)
		{
			exception = null;

			var elementType = element.GetType();
#pragma warning disable CS0618 // Type or member is obsolete
			var nativeBindingService = DependencyService.Get<INativeBindingService>();
#pragma warning restore CS0618 // Type or member is obsolete

			if (property == null)
				return false;

			if (serviceProvider?.GetService<IProvideValueTarget>() is XamlValueTargetProvider valueTargetProvider)
				valueTargetProvider.TargetProperty = property;

			Func<MemberInfo> minforetriever;
			if (attached)
				minforetriever = () =>
				{
					try
					{
						return property.DeclaringType.GetRuntimeMethod("Get" + property.PropertyName, new[] { typeof(BindableObject) });
					}
					catch (AmbiguousMatchException e)
					{
						throw new XamlParseException($"Multiple methods with name '{property.DeclaringType}.Get{property.PropertyName}' found.", lineInfo, innerException: e);
					}
				};
			else
				minforetriever = () => property.DeclaringType.GetRuntimeProperties().FirstOrDefault(pi => pi.Name == property.PropertyName);
			var convertedValue = value.ConvertTo(property.ReturnType, minforetriever, serviceProvider, out exception);
			if (exception != null)
				return false;

			if (element is BindableObject bindable)
			{
				//SetValue doesn't throw on mismatching type, so check before to get a chance to try the property setting or the collection adding
				var nullable = property.ReturnType.IsGenericType &&
							   property.ReturnType.GetGenericTypeDefinition() == typeof(Nullable<>);
				if ((convertedValue == null && (!property.ReturnType.IsValueType || nullable)) ||
					(property.ReturnType.IsInstanceOfType(convertedValue)))
				{
					try
					{
						bindable.SetValue(property, convertedValue);
						return true;
					}
					catch (Exception e)
					{
						exception = e;
						return false;
					}
				}

				// This might be a collection; see if we can add to it
				return TryAddValue(bindable, property, value, serviceProvider, out exception);
			}

			if (nativeBindingService != null && nativeBindingService.TrySetValue(element, property, convertedValue))
				return true;

			exception = new XamlParseException($"{elementType.Name} is not a BindableObject or does not support setting native BindableProperties", lineInfo);
			return false;
		}

		static bool TryGetValue(object element, BindableProperty property, bool attached, out object value, IXmlLineInfo lineInfo, out Exception exception, out object targetProperty)
		{
			exception = null;
			value = null;
			targetProperty = property;
			var elementType = element.GetType();
			var bindable = element as BindableObject;

			if (property == null)
				return false;

			if (bindable == null)
				return false;

			value = bindable.GetValue(property);
			return true;
		}

		static bool TrySetProperty(object element, string localName, object value, IXmlLineInfo lineInfo, IServiceProvider serviceProvider, object rootElement, out Exception exception)
		{
			exception = null;

			var elementType = element.GetType();
			var propertyInfo = elementType.GetRuntimeProperties().FirstOrDefault(p => p.Name == localName);
			MethodInfo setter;
			if (propertyInfo == null || !propertyInfo.CanWrite || (setter = propertyInfo.SetMethod) == null)
				return false;

			if (!IsVisibleFrom(setter, rootElement))
				return false;

			if (serviceProvider?.GetService<IProvideValueTarget>() is XamlValueTargetProvider valueTargetProvider)
				valueTargetProvider.TargetProperty = propertyInfo;

			object convertedValue = value.ConvertTo(propertyInfo.PropertyType, () => propertyInfo, serviceProvider, out exception);
			if (exception != null || (convertedValue != null && !propertyInfo.PropertyType.IsInstanceOfType(convertedValue)))
				return false;

			try
			{
				setter.Invoke(element, new object[] { convertedValue });
				return true;
			}
			catch (Exception e)
			{
				exception = e;
				return false;
			}
		}

		static bool TryGetProperty(object element, string localName, out object value, IXmlLineInfo lineInfo, object rootElement, out Exception exception, out object targetProperty)
		{
			exception = null;
			value = null;
			var elementType = element.GetType();
			PropertyInfo propertyInfo = null;

			while (elementType != null && propertyInfo == null)
			{
				try
				{
					propertyInfo = elementType.GetProperty(localName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
				}
				catch (AmbiguousMatchException e)
				{
					throw new XamlParseException($"Multiple properties with name '{elementType}.{localName}' found.", lineInfo, innerException: e);
				}
				elementType = elementType.BaseType;
			}

			MethodInfo getter;
			targetProperty = propertyInfo;
			if (propertyInfo == null || !propertyInfo.CanRead || (getter = propertyInfo.GetMethod) == null)
				return false;

			if (!IsVisibleFrom(getter, rootElement))
				return false;

			value = getter.Invoke(element, Array.Empty<object>());
			return true;
		}

		static bool IsVisibleFrom(MethodInfo method, object rootElement)
		{
			if (method.IsPublic)
				return true;
			if (method.IsPrivate && method.DeclaringType == rootElement.GetType())
				return true;
			if ((method.IsAssembly || method.IsFamilyOrAssembly) && method.DeclaringType.AssemblyQualifiedName == rootElement.GetType().AssemblyQualifiedName)
				return true;
			if (method.IsFamily && method.DeclaringType.IsAssignableFrom(rootElement.GetType()))
				return true;
			return false;
		}

		static bool IsVisibleFrom(Type type, object rootElement)
		{
			if (type.IsPublic || type.IsNestedPublic)
				return true;

			var consumer = rootElement.GetType().Assembly;
			if (type.Assembly == consumer)
				return true;

			var consumerName = consumer.GetName().Name;
			foreach (var ivt in type.Assembly.GetCustomAttributes<InternalsVisibleToAttribute>())
			{
				//InternalsVisibleTo values may carry a public key: "Friend, PublicKey=00..."
				var friend = ivt.AssemblyName;
				var comma = friend.IndexOf(",", StringComparison.Ordinal);
				if (comma >= 0)
					friend = friend.Substring(0, comma);
				if (string.Equals(friend.Trim(), consumerName, StringComparison.Ordinal))
					return true;
			}

			return false;
		}

		sealed class ExtensionPropertyAccessors
		{
			public static readonly ExtensionPropertyAccessors None = new ExtensionPropertyAccessors();

			public MethodInfo Getter { get; init; }
			public MethodInfo Setter { get; init; }
			public Type PropertyType { get; init; }
			public bool Ambiguous { get; init; }
			public bool Unsupported { get; init; }
			public bool Found => Getter != null || Setter != null;
		}

		static readonly ConcurrentDictionary<(Type Container, Type Target, string Name), ExtensionPropertyAccessors> s_extensionProperties = new();

		/// <summary>
		/// Resolves the <c>get_Name</c>/<c>set_Name</c> implementation methods of a C# extension property declared
		/// by <paramref name="containerType"/> and applicable to <paramref name="targetType"/>.
		/// See <see cref="ExtensionPropertyConventions"/> for the rules shared with XamlC and SourceGen.
		/// </summary>
		static ExtensionPropertyAccessors ResolveExtensionProperty(Type containerType, Type targetType, string propertyName)
		{
			if (containerType == null || targetType == null)
				return ExtensionPropertyAccessors.None;

			//static classes are abstract and sealed. Generic containers can't be named in XAML
			if (!containerType.IsAbstract || !containerType.IsSealed || containerType.IsGenericType || containerType.IsGenericTypeDefinition)
				return ExtensionPropertyAccessors.None;

			return s_extensionProperties.GetOrAdd((containerType, targetType, propertyName), static key => ResolveExtensionPropertyCore(key.Container, key.Target, key.Name));
		}

		static ExtensionPropertyAccessors ResolveExtensionPropertyCore(Type containerType, Type targetType, string propertyName)
		{
			var getterName = ExtensionPropertyConventions.GetterName(propertyName);
			var setterName = ExtensionPropertyConventions.SetterName(propertyName);

			List<MethodInfo> getters = null;
			List<MethodInfo> setters = null;
			var unsupported = false;

			foreach (var method in containerType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
			{
				var isGetter = method.Name == getterName;
				if (!isGetter && method.Name != setterName)
					continue;

				//generic extension blocks (extension<T>(ICollection<T>)) and static extension properties aren't supported
				if (method.IsGenericMethodDefinition)
				{
					unsupported = true;
					continue;
				}

				var parameters = method.GetParameters();
				if (parameters.Length != (isGetter ? 1 : 2))
				{
					unsupported = true;
					continue;
				}

				var receiverType = parameters[0].ParameterType;
				if (receiverType.IsByRef || !receiverType.IsAssignableFrom(targetType))
					continue;

				if (isGetter)
				{
					if (method.ReturnType == typeof(void))
						continue;
					(getters ??= new List<MethodInfo>()).Add(method);
				}
				else
				{
					if (method.ReturnType != typeof(void))
						continue;
					(setters ??= new List<MethodInfo>()).Add(method);
				}
			}

			if (getters == null && setters == null)
				return unsupported ? new ExtensionPropertyAccessors { Unsupported = true } : ExtensionPropertyAccessors.None;

			var getter = MostSpecific(getters, out var ambiguousGetter);
			var setter = MostSpecific(setters, out var ambiguousSetter);
			if (ambiguousGetter || ambiguousSetter)
				return new ExtensionPropertyAccessors { Ambiguous = true };

			var propertyType = getter?.ReturnType ?? setter.GetParameters()[1].ParameterType;
			if (getter != null && setter != null && setter.GetParameters()[1].ParameterType != propertyType)
				return new ExtensionPropertyAccessors { Ambiguous = true };

			return new ExtensionPropertyAccessors { Getter = getter, Setter = setter, PropertyType = propertyType };

			//the accessor whose receiver type is more derived than every other applicable one wins, as it would in C#
			static MethodInfo MostSpecific(List<MethodInfo> candidates, out bool ambiguous)
			{
				ambiguous = false;
				if (candidates == null)
					return null;
				if (candidates.Count == 1)
					return candidates[0];

				MethodInfo best = null;
				foreach (var candidate in candidates)
				{
					var candidateType = candidate.GetParameters()[0].ParameterType;
					var isBest = true;
					foreach (var other in candidates)
					{
						if (ReferenceEquals(other, candidate))
							continue;
						if (!other.GetParameters()[0].ParameterType.IsAssignableFrom(candidateType))
						{
							isBest = false;
							break;
						}
					}
					if (!isBest)
						continue;
					if (best != null)
					{
						ambiguous = true;
						return null;
					}
					best = candidate;
				}

				ambiguous = best == null;
				return best;
			}
		}

		static bool TryResolveExtensionProperty(Type containerType, Type targetType, string propertyName, object rootElement, IXmlLineInfo lineInfo, out ExtensionPropertyAccessors accessors, out Exception exception)
		{
			exception = null;
			accessors = ResolveExtensionProperty(containerType, targetType, propertyName);

			if (accessors.Ambiguous || accessors.Unsupported)
			{
				exception = new XamlParseException($"Cannot resolve the extension property \"{propertyName}\" on \"{containerType.FullName}\". The extension property must be a non-generic instance extension property, declared in a non-generic static extension container, with a receiver type matching \"{targetType.FullName}\", and it must be unambiguous.", lineInfo);
				return false;
			}

			if (!accessors.Found)
				return false;

			if (!IsVisibleFrom(containerType, rootElement))
			{
				exception = new XamlParseException($"The extension container \"{containerType.FullName}\" is not accessible from \"{rootElement.GetType().Assembly.GetName().Name}\".", lineInfo);
				return false;
			}

			return true;
		}

		static bool TrySetExtensionProperty(object element, Type containerType, string propertyName, object value, IXmlLineInfo lineInfo, IServiceProvider serviceProvider, object rootElement, out Exception exception)
		{
			if (!TryResolveExtensionProperty(containerType, element.GetType(), propertyName, rootElement, lineInfo, out var accessors, out exception))
				return false;

			if (accessors.Setter == null)
			{
				exception = new XamlParseException($"The extension property \"{containerType.FullName}.{propertyName}\" has no accessible setter.", lineInfo);
				return false;
			}

			if (serviceProvider?.GetService<IProvideValueTarget>() is XamlValueTargetProvider valueTargetProvider)
				valueTargetProvider.TargetProperty = accessors.Setter;

			var convertedValue = value.ConvertTo(accessors.PropertyType, (Func<MemberInfo>)(() => accessors.Setter), serviceProvider, out exception);
			if (exception != null)
				return false;
			if (convertedValue == null ? accessors.PropertyType.IsValueType && Nullable.GetUnderlyingType(accessors.PropertyType) == null : !accessors.PropertyType.IsInstanceOfType(convertedValue))
			{
				exception = new XamlParseException($"Cannot assign property \"{propertyName}\": mismatching type between value and property", lineInfo);
				return false;
			}

			try
			{
				accessors.Setter.Invoke(null, new[] { element, convertedValue });
				return true;
			}
			catch (TargetInvocationException e)
			{
				exception = e.InnerException ?? e;
				return false;
			}
		}

		static bool TryAddToProperty(object element, XmlName propertyName, object value, string xKey, IXmlLineInfo lineInfo, IServiceProvider serviceProvider, object rootElement, out Exception exception)
		{
			exception = null;
			if (!(GetPropertyValue(element, propertyName, rootElement, lineInfo, out _, out var targetProperty) is IEnumerable collection))
				return false;

			if (exception == null && TryAddToResourceDictionary(collection as ResourceDictionary, value, xKey, lineInfo, out exception))
				return true;

			if (exception != null)
				return false;

			var addMethod = collection.GetType().GetRuntimeMethods().First(mi => mi.Name == "Add" && mi.GetParameters().Length == 1);
			if (addMethod == null)
				return false;

			if (serviceProvider?.GetService<IProvideValueTarget>() is XamlValueTargetProvider valueTargetProvider)
				valueTargetProvider.TargetProperty = targetProperty;

			try
			{
				addMethod.Invoke(collection, new[] { value.ConvertTo(addMethod.GetParameters()[0].ParameterType, (Func<TypeConverter>)null, serviceProvider, out exception) });
			}
			catch (Exception e)
			{
				exception = e;
			}
			return exception == null;
		}

		static bool TryAddToResourceDictionary(ResourceDictionary resourceDictionary, object value, string xKey, IXmlLineInfo lineInfo, out Exception exception)
		{
			exception = null;

			if (resourceDictionary == null)
				return false;

			if (xKey != null)
				resourceDictionary.Add(xKey, value);
			else if (value is Style)
				resourceDictionary.Add((Style)value);
			else if (value is ResourceDictionary)
				resourceDictionary.Add((ResourceDictionary)value);
			else if (value is StyleSheets.StyleSheet)
				resourceDictionary.Add((StyleSheets.StyleSheet)value);
			else
			{
				exception = new XamlParseException("resources in ResourceDictionary require a x:Key attribute", lineInfo);
				return false;
			}
			return true;
		}

		void SetTemplate(ElementTemplate dt, INode node)
		{
			dt.LoadTemplate = () =>
			{
				var cnode = node.Clone();
				var context = new HydrationContext { ParentContext = Context, RootAssembly = Context.RootAssembly, RootElement = Context.RootElement, ExceptionHandler = Context.ExceptionHandler };
				cnode.Accept(new XamlNodeVisitor((n, parent) => n.Parent = parent), node.Parent); //set parents for {StaticResource}
				cnode.Accept(new ExpandMarkupsVisitor(context), null);
				cnode.Accept(new NamescopingVisitor(context), null);
				cnode.Accept(new CreateValuesVisitor(context), null);
				cnode.Accept(new RegisterXNamesVisitor(context), null);
				cnode.Accept(new FillResourceDictionariesVisitor(context), null);
				cnode.Accept(new ApplyPropertiesVisitor(context, true), null);
				return context.Values[cnode];
			};
		}

		static bool TryAddValue(BindableObject bindable, BindableProperty property, object value, IServiceProvider serviceProvider, out Exception exception)
		{
			exception = null;

			if (property?.ReturnType?.GenericTypeArguments == null)
				return false;

			if (property.ReturnType.GenericTypeArguments.Length != 1 || !property.ReturnType.GenericTypeArguments[0].IsInstanceOfType(value))
				return false;

			// This might be a collection we can add to; see if we can find an Add method
			var addMethod = GetAllRuntimeMethods(property.ReturnType).FirstOrDefault(mi => mi.Name == "Add" && mi.GetParameters().Length == 1);
			if (addMethod == null)
				return false;

			// If there's an add method, get the collection
			var collection = bindable.GetValue(property);

			// And add the new value to it
			addMethod.Invoke(collection, new[] { value.ConvertTo(addMethod.GetParameters()[0].ParameterType, (Func<TypeConverter>)null, serviceProvider, out exception) });
			return exception == null;
		}

		static IEnumerable<MethodInfo> GetAllRuntimeMethods(Type type)
		{
			return type.GetRuntimeMethods()
				.Concat(type.GetInterfaces().SelectMany(t => t.GetRuntimeMethods()));
		}

		bool TrySetRuntimeName(XmlName propertyName, object source, object value, ValueNode node)
		{
			if (propertyName != XmlName.xName)
				return false;

			var runTimeName = source.GetType().GetCustomAttribute<RuntimeNamePropertyAttribute>();
			if (runTimeName == null)
				return false;

			SetPropertyValue(source, new XmlName("", runTimeName.Name), value, Context.RootElement, node, Context, node);
			return true;
		}
	}
}
