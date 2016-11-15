using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml.Internals;

using static System.String;

namespace Xamarin.Forms.Xaml
{
	internal class ApplyPropertiesVisitor : IXamlNodeVisitor
	{
		public static readonly IList<XmlName> Skips = new List<XmlName>
		{
			XmlName.xKey,
			XmlName.xTypeArguments,
			XmlName.xArguments,
			XmlName.xFactoryMethod,
			XmlName.xName,
			XmlName.xDataType
		};

		public ApplyPropertiesVisitor(HydratationContext context, bool stopOnResourceDictionary = false)
		{
			Context = context;
			StopOnResourceDictionary = stopOnResourceDictionary;
		}

		Dictionary<INode, object> Values {
			get { return Context.Values; }
		}

		HydratationContext Context { get; }

		public bool VisitChildrenFirst {
			get { return true; }
		}

		public bool StopOnDataTemplate {
			get { return true; }
		}

		public bool StopOnResourceDictionary { get; }

		public void Visit(ValueNode node, INode parentNode)
		{
			var parentElement = parentNode as IElementNode;
			var value = Values [node];
			var source = Values [parentNode];

			XmlName propertyName;
			if (TryGetPropertyName(node, parentNode, out propertyName)) {
				if (Skips.Contains(propertyName))
					return;
				if (parentElement.SkipProperties.Contains(propertyName))
					return;
				if (propertyName.NamespaceURI == "http://schemas.openxmlformats.org/markup-compatibility/2006" &&
					propertyName.LocalName == "Ignorable")
					return;
				SetPropertyValue(source, propertyName, value, Context.RootElement, node, Context, node);
			} else if (IsCollectionItem(node, parentNode) && parentNode is IElementNode) {
				// Collection element, implicit content, or implicit collection element.
				var contentProperty = GetContentPropertyName(Context.Types [parentElement].GetTypeInfo());
				if (contentProperty != null) {
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
			var value = Values [node];
			var parentElement = parentNode as IElementNode;
			var markupExtension = value as IMarkupExtension;
			var valueProvider = value as IValueProvider;

			if (markupExtension != null) {
				var serviceProvider = new XamlServiceProvider(node, Context);
				value = markupExtension.ProvideValue(serviceProvider);
			}

			if (valueProvider != null) {
				var serviceProvider = new XamlServiceProvider(node, Context);
				value = valueProvider.ProvideValue(serviceProvider);
			}

			XmlName propertyName = XmlName.Empty;

			//Simplify ListNodes with single elements
			var pList = parentNode as ListNode;
			if (pList != null && pList.CollectionItems.Count == 1) {
				propertyName = pList.XmlName;
				parentNode = parentNode.Parent;
				parentElement = parentNode as IElementNode;
			}

			if (propertyName != XmlName.Empty || TryGetPropertyName(node, parentNode, out propertyName)) {
				if (Skips.Contains(propertyName))
					return;
				if (parentElement.SkipProperties.Contains(propertyName))
					return;

				var source = Values [parentNode];

				if (propertyName == XmlName._CreateContent && source is ElementTemplate)
					SetTemplate(source as ElementTemplate, node);
				else
					SetPropertyValue(source, propertyName, value, Context.RootElement, node, Context, node);
			} else if (IsCollectionItem(node, parentNode) && parentNode is IElementNode) {
				// Collection element, implicit content, or implicit collection element.
				string contentProperty;
				if (typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(Context.Types [parentElement].GetTypeInfo())) {
					var source = Values [parentNode];
					if (!(typeof(ResourceDictionary).IsAssignableFrom(Context.Types [parentElement]))) {
						var addMethod =
							Context.Types [parentElement].GetRuntimeMethods().First(mi => mi.Name == "Add" && mi.GetParameters().Length == 1);
						addMethod.Invoke(source, new [] { value });
					}
				} else if ((contentProperty = GetContentPropertyName(Context.Types [parentElement].GetTypeInfo())) != null) {
					var name = new XmlName(node.NamespaceURI, contentProperty);
					if (Skips.Contains(name))
						return;
					if (parentElement.SkipProperties.Contains(propertyName))
						return;

					var source = Values [parentNode];
					SetPropertyValue(source, name, value, Context.RootElement, node, Context, node);
				}
			} else if (IsCollectionItem(node, parentNode) && parentNode is ListNode) {
				var parentList = (ListNode)parentNode;
				var source = Values [parentNode.Parent];

				if (Skips.Contains(parentList.XmlName))
					return;

				var elementType = source.GetType();
				var localname = parentList.XmlName.LocalName;

				GetRealNameAndType(ref elementType, parentList.XmlName.NamespaceURI, ref localname, Context, node);

				PropertyInfo propertyInfo = null;
				try {
					propertyInfo = elementType.GetRuntimeProperty(localname);
				} catch (AmbiguousMatchException) {
					// Get most derived instance of property
					foreach (var property in elementType.GetRuntimeProperties().Where(prop => prop.Name == localname)) {
						if (propertyInfo == null || propertyInfo.DeclaringType.IsAssignableFrom(property.DeclaringType))
							propertyInfo = property;
					}
				}
				if (propertyInfo == null)
					throw new XamlParseException(string.Format("Property {0} not found", localname), node);
				MethodInfo getter;
				if (!propertyInfo.CanRead || (getter = propertyInfo.GetMethod) == null)
					throw new XamlParseException(string.Format("Property {0} does not have an accessible getter", localname), node);
				IEnumerable collection;
				if ((collection = getter.Invoke(source, new object [] { }) as IEnumerable) == null)
					throw new XamlParseException(string.Format("Property {0} is null or is not IEnumerable", localname), node);
				MethodInfo addMethod;
				if (
					(addMethod =
						collection.GetType().GetRuntimeMethods().First(mi => mi.Name == "Add" && mi.GetParameters().Length == 1)) == null)
					throw new XamlParseException(string.Format("Value of {0} does not have a Add() method", localname), node);

				addMethod.Invoke(collection, new [] { Values [node] });
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
			foreach (var kvp in parentElement.Properties) {
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

		internal static string GetContentPropertyName(TypeInfo typeInfo)
		{
			while (typeInfo != null) {
				var propName = GetContentPropertyName(typeInfo.CustomAttributes);
				if (propName != null)
					return propName;
				typeInfo = typeInfo?.BaseType?.GetTypeInfo();
			}
			return null;
		}

		static string GetContentPropertyName(IEnumerable<CustomAttributeData> attributes)
		{
			var contentAttribute =
				attributes.FirstOrDefault(cad => ContentPropertyAttribute.ContentPropertyTypes.Contains(cad.AttributeType.FullName));
			if (contentAttribute == null || contentAttribute.ConstructorArguments.Count != 1)
				return null;
			if (contentAttribute.ConstructorArguments [0].ArgumentType == typeof(string))
				return (string)contentAttribute.ConstructorArguments [0].Value;
			return null;
		}

		static bool GetRealNameAndType(ref Type elementType, string namespaceURI, ref string localname,
			HydratationContext context, IXmlLineInfo lineInfo)
		{
			var dotIdx = localname.IndexOf('.');
			if (dotIdx > 0) {
				var typename = localname.Substring(0, dotIdx);
				localname = localname.Substring(dotIdx + 1);
				XamlParseException xpe;
				elementType = XamlParser.GetElementType(new XmlType(namespaceURI, typename, null), lineInfo,
					context.RootElement.GetType().GetTypeInfo().Assembly, out xpe);
				if (xpe != null)
					throw xpe;
				return true;
			}
			return false;
		}

		static BindableProperty GetBindableProperty(Type elementType, string localName, IXmlLineInfo lineInfo,
			bool throwOnError = false)
		{
			var bindableFieldInfo =
				elementType.GetFields().FirstOrDefault(fi => fi.Name == localName + "Property" && fi.IsStatic && fi.IsPublic);

			Exception exception = null;
			if (exception == null && bindableFieldInfo == null) {
				exception =
					new XamlParseException(
						string.Format("BindableProperty {0} not found on {1}", localName + "Property", elementType.Name), lineInfo);
			}

			if (exception == null)
				return bindableFieldInfo.GetValue(null) as BindableProperty;
			if (throwOnError)
				throw exception;
			return null;
		}

		public static void SetPropertyValue(object xamlelement, XmlName propertyName, object value, object rootElement, INode node, HydratationContext context, IXmlLineInfo lineInfo)
		{
			var localName = propertyName.LocalName;
			var serviceProvider = new XamlServiceProvider(node, context);
			Exception xpe = null;

			//If it's an attached BP, update elementType and propertyName
			var bpOwnerType = xamlelement.GetType();
			var attached = GetRealNameAndType(ref bpOwnerType, propertyName.NamespaceURI, ref localName, context, lineInfo);
			var property = GetBindableProperty(bpOwnerType, localName, lineInfo, false);

			//If the target is an event, connect
			if (xpe == null && TryConnectEvent(xamlelement, localName, value, rootElement, lineInfo, out xpe))
				return;

			//If Value is DynamicResource and it's a BP, SetDynamicResource
			if (xpe == null && TrySetDynamicResource(xamlelement, property, value, lineInfo, out xpe))
				return;

			//If value is BindingBase, SetBinding
			if (xpe == null && TrySetBinding(xamlelement, property, localName, value, lineInfo, out xpe))
				return;

			//If it's a BindableProberty, SetValue
			if (xpe == null && TrySetValue(xamlelement, property, attached, value, lineInfo, serviceProvider, out xpe))
				return;

			//If we can assign that value to a normal property, let's do it
			if (xpe == null && TrySetProperty(xamlelement, localName, value, lineInfo, serviceProvider, out xpe))
				return;

			//If it's an already initialized property, add to it
			if (xpe == null && TryAddToProperty(xamlelement, localName, value, lineInfo, serviceProvider, out xpe))
				return;

			xpe = xpe ?? new XamlParseException($"Cannot assign property \"{localName}\": Property does not exists, or is not assignable, or mismatching type between value and property", lineInfo);
			if (context.DoNotThrowOnExceptions)
				System.Diagnostics.Debug.WriteLine(xpe.Message);
			else
				throw xpe;
		}

		static bool TryConnectEvent(object element, string localName, object value, object rootElement, IXmlLineInfo lineInfo, out Exception exception)
		{
			exception = null;

			var elementType = element.GetType();
			var eventInfo = elementType.GetRuntimeEvent(localName);
			var stringValue = value as string;

			if (eventInfo == null || IsNullOrEmpty(stringValue))
				return false;

			var methodInfo = rootElement.GetType().GetRuntimeMethods().FirstOrDefault(mi => mi.Name == (string)value);
			if (methodInfo == null) {
				exception = new XamlParseException($"No method {value} found on type {rootElement.GetType()}", lineInfo);
				return false;
			}

			try {
				eventInfo.AddEventHandler(element, methodInfo.CreateDelegate(eventInfo.EventHandlerType, rootElement));
				return true;
			} catch (ArgumentException ae) {
				exception = new XamlParseException($"Method {stringValue} does not have the correct signature", lineInfo, ae);
			}
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

			if (bindable == null) {
				exception = new XamlParseException($"{elementType.Name} is not a BindableObject", lineInfo);
				return false;
			}

			bindable.SetDynamicResource(property, dynamicResource.Key);
			return true;
		}

		static bool TrySetBinding(object element, BindableProperty property, string localName, object value, IXmlLineInfo lineInfo, out Exception exception)
		{
			exception = null;

			var elementType = element.GetType();
			var binding = value as BindingBase;
			var bindable = element as BindableObject;
			var nativeBindingService = DependencyService.Get<INativeBindingService>();

			if (binding == null)
				return false;

			if (bindable != null && property != null) {
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

		static bool TrySetValue(object element, BindableProperty property, bool attached, object value, IXmlLineInfo lineInfo, XamlServiceProvider serviceProvider, out Exception exception)
		{
			exception = null;

			var elementType = element.GetType();
			var bindable = element as BindableObject;
			var nativeBindingService = DependencyService.Get<INativeBindingService>();

			if (property == null)
				return false;

			Func<MemberInfo> minforetriever;
			if (attached)
				minforetriever = () => property.DeclaringType.GetRuntimeMethod("Get" + property.PropertyName, new [] { typeof(BindableObject) });
			else
				minforetriever = () => property.DeclaringType.GetRuntimeProperty(property.PropertyName);
			var convertedValue = value.ConvertTo(property.ReturnType, minforetriever, serviceProvider);

			if (bindable != null) {
				//SetValue doesn't throw on mismatching type, so check before to get a chance to try the property setting or the collection adding
				var nullable = property.ReturnTypeInfo.IsGenericType &&
							   property.ReturnTypeInfo.GetGenericTypeDefinition() == typeof(Nullable<>);
				if ((convertedValue == null && (!property.ReturnTypeInfo.IsValueType || nullable)) ||
					(property.ReturnType.IsInstanceOfType(convertedValue))) {
					bindable.SetValue(property, convertedValue);
					return true;
				}
				return false;
			}

			if (nativeBindingService != null && nativeBindingService.TrySetValue(element, property, convertedValue))
				return true;

			exception = new XamlParseException($"{elementType.Name} is not a BindableObject or does not support setting native BindableProperties", lineInfo);
			return false;
		}

		static bool TrySetProperty(object element, string localName, object value, IXmlLineInfo lineInfo, XamlServiceProvider serviceProvider, out Exception exception)
		{
			exception = null;

			var elementType = element.GetType();
			var propertyInfo = elementType.GetRuntimeProperties().FirstOrDefault(p => p.Name == localName);
			MethodInfo setter;
			if (propertyInfo == null || !propertyInfo.CanWrite || (setter = propertyInfo.SetMethod) == null)
				return false;

			object convertedValue = value.ConvertTo(propertyInfo.PropertyType, () => propertyInfo, serviceProvider);
			if (convertedValue != null && !propertyInfo.PropertyType.IsInstanceOfType(convertedValue))
				return false;

			setter.Invoke(element, new object [] { convertedValue });
			return true;
		}

		static bool TryAddToProperty(object element, string localName, object value, IXmlLineInfo lineInfo, XamlServiceProvider serviceProvider, out Exception exception)
		{
			exception = null;

			var elementType = element.GetType();
			var propertyInfo = elementType.GetRuntimeProperties().FirstOrDefault(p => p.Name == localName);
			MethodInfo getter;
			if (propertyInfo == null || !propertyInfo.CanRead || (getter = propertyInfo.GetMethod) == null)
				return false;

			var collection = getter.Invoke(element, new object [] { }) as IEnumerable;
			if (collection == null)
				return false;

			var addMethod = collection.GetType().GetRuntimeMethods().First(mi => mi.Name == "Add" && mi.GetParameters().Length == 1);
			if (addMethod == null)
				return false;

			addMethod.Invoke(collection, new [] { value.ConvertTo(addMethod.GetParameters() [0].ParameterType, (Func<TypeConverter>)null, serviceProvider) });
			return true;
		}

		void SetTemplate(ElementTemplate dt, INode node)
		{
#pragma warning disable 0612
			((IDataTemplate)dt).LoadTemplate = () => {
#pragma warning restore 0612
				var cnode = node.Clone();
				var context = new HydratationContext { ParentContext = Context, RootElement = Context.RootElement };
				cnode.Accept(new XamlNodeVisitor((n, parent) => n.Parent = parent), node.Parent); //set parents for {StaticResource}
				cnode.Accept(new ExpandMarkupsVisitor(context), null);
				cnode.Accept(new NamescopingVisitor(context), null);
				cnode.Accept(new CreateValuesVisitor(context), null);
				cnode.Accept(new RegisterXNamesVisitor(context), null);
				cnode.Accept(new FillResourceDictionariesVisitor(context), null);
				cnode.Accept(new ApplyPropertiesVisitor(context, true), null);
				return context.Values [cnode];
			};
		}
	}
}