using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml.Internals;

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
			XmlName.xName
		};

		public ApplyPropertiesVisitor(HydratationContext context, bool stopOnResourceDictionary = false)
		{
			Context = context;
			StopOnResourceDictionary = stopOnResourceDictionary;
		}

		Dictionary<INode, object> Values
		{
			get { return Context.Values; }
		}

		HydratationContext Context { get; }

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
			var parentElement = parentNode as IElementNode;
			var value = Values[node];
			var source = Values[parentNode];

			XmlName propertyName;
			if (TryGetPropertyName(node, parentNode, out propertyName))
			{
				if (Skips.Contains(propertyName))
					return;
				if (parentElement.SkipProperties.Contains(propertyName))
					return;
				if (parentElement.SkipPrefix(node.NamespaceResolver.LookupPrefix(propertyName.NamespaceURI)))
					return;
				if (propertyName.NamespaceURI == "http://schemas.openxmlformats.org/markup-compatibility/2006" &&
				    propertyName.LocalName == "Ignorable")
				{
					(parentNode.IgnorablePrefixes ?? (parentNode.IgnorablePrefixes = new List<string>())).AddRange(
						(value as string).Split(','));
					return;
				}
				SetPropertyValue(source, propertyName, value, Context.RootElement, node, Context, node);
			}
			else if (IsCollectionItem(node, parentNode) && parentNode is IElementNode)
			{
				// Collection element, implicit content, or implicit collection element.
				var contentProperty = GetContentPropertyName(Context.Types[parentElement].GetTypeInfo());
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
			if (node.SkipPrefix(node.NamespaceResolver.LookupPrefix(node.NamespaceURI)))
				return;

			var value = Values[node];
			var parentElement = parentNode as IElementNode;
			var markupExtension = value as IMarkupExtension;
			var valueProvider = value as IValueProvider;

			if (markupExtension != null)
			{
				var serviceProvider = new XamlServiceProvider(node, Context);
				value = markupExtension.ProvideValue(serviceProvider);
			}

			if (valueProvider != null)
			{
				var serviceProvider = new XamlServiceProvider(node, Context);
				value = valueProvider.ProvideValue(serviceProvider);
			}

			XmlName propertyName;
			if (TryGetPropertyName(node, parentNode, out propertyName))
			{
				if (Skips.Contains(propertyName))
					return;
				if (parentElement.SkipProperties.Contains(propertyName))
					return;

				var source = Values[parentNode];

				if (propertyName == XmlName._CreateContent && source is ElementTemplate)
					SetTemplate(source as ElementTemplate, node);
				else
					SetPropertyValue(source, propertyName, value, Context.RootElement, node, Context, node);
			}
			else if (IsCollectionItem(node, parentNode) && parentNode is IElementNode)
			{
				// Collection element, implicit content, or implicit collection element.
				string contentProperty;
				if (typeof (IEnumerable).GetTypeInfo().IsAssignableFrom(Context.Types[parentElement].GetTypeInfo()))
				{
					var source = Values[parentNode];
					if (!(typeof (ResourceDictionary).IsAssignableFrom(Context.Types[parentElement])))
					{
						var addMethod =
							Context.Types[parentElement].GetRuntimeMethods().First(mi => mi.Name == "Add" && mi.GetParameters().Length == 1);
						addMethod.Invoke(source, new[] { value });
					}
				}
				else if ((contentProperty = GetContentPropertyName(Context.Types[parentElement].GetTypeInfo())) != null)
				{
					var name = new XmlName(node.NamespaceURI, contentProperty);
					if (Skips.Contains(name))
						return;
					if (parentElement.SkipProperties.Contains(propertyName))
						return;

					var source = Values[parentNode];
					SetPropertyValue(source, name, value, Context.RootElement, node, Context, node);
				}
			}
			else if (IsCollectionItem(node, parentNode) && parentNode is ListNode)
			{
				var parentList = (ListNode)parentNode;
				var source = Values[parentNode.Parent];

				if (Skips.Contains(parentList.XmlName))
					return;

				var elementType = source.GetType();
				var localname = parentList.XmlName.LocalName;

				GetRealNameAndType(ref elementType, parentList.XmlName.NamespaceURI, ref localname, Context, node);

				PropertyInfo propertyInfo = null;
				try
				{
					propertyInfo = elementType.GetRuntimeProperty(localname);
				}
				catch (AmbiguousMatchException)
				{
					// Get most derived instance of property
					foreach (var property in elementType.GetRuntimeProperties().Where(prop => prop.Name == localname))
					{
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
				if ((collection = getter.Invoke(source, new object[] { }) as IEnumerable) == null)
					throw new XamlParseException(string.Format("Property {0} is null or is not IEnumerable", localname), node);
				MethodInfo addMethod;
				if (
					(addMethod =
						collection.GetType().GetRuntimeMethods().First(mi => mi.Name == "Add" && mi.GetParameters().Length == 1)) == null)
					throw new XamlParseException(string.Format("Value of {0} does not have a Add() method", localname), node);

				addMethod.Invoke(collection, new[] { Values[node] });
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

		internal static string GetContentPropertyName(TypeInfo typeInfo)
		{
			while (typeInfo != null)
			{
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
			if (contentAttribute.ConstructorArguments[0].ArgumentType == typeof (string))
				return (string)contentAttribute.ConstructorArguments[0].Value;
			return null;
		}

		static bool GetRealNameAndType(ref Type elementType, string namespaceURI, ref string localname,
			HydratationContext context, IXmlLineInfo lineInfo)
		{
			var dotIdx = localname.IndexOf('.');
			if (dotIdx > 0)
			{
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
			if (exception == null && bindableFieldInfo == null)
			{
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

		public static void SetPropertyValue(object xamlelement, XmlName propertyName, object value, object rootElement,
			INode node, HydratationContext context, IXmlLineInfo lineInfo)
		{
			var elementType = xamlelement.GetType();
			var localname = propertyName.LocalName;

			var serviceProvider = new XamlServiceProvider(node, context);

			//If it's an attached BP, update elementType and propertyName
			var attached = GetRealNameAndType(ref elementType, propertyName.NamespaceURI, ref localname, context, lineInfo);

			//If the target is an event, connect
			var eventInfo = elementType.GetRuntimeEvent(localname);
			if (eventInfo != null && value is string)
			{
				var methodInfo = rootElement.GetType().GetRuntimeMethods().FirstOrDefault(mi => mi.Name == (string)value);
				if (methodInfo == null) {
					var xpe = new XamlParseException (string.Format ("No method {0} found on type {1}", value, rootElement.GetType ()), lineInfo);
					if (context.DoNotThrowOnExceptions) {
						System.Diagnostics.Debug.WriteLine (xpe.Message);
						return;
					} else
						throw xpe;
				}
				try
				{
					eventInfo.AddEventHandler(xamlelement, methodInfo.CreateDelegate(eventInfo.EventHandlerType, rootElement));
				}
				catch (ArgumentException)
				{
					var xpe = new XamlParseException (string.Format ("Method {0} does not have the correct signature", value), lineInfo);
					if (context.DoNotThrowOnExceptions)
						System.Diagnostics.Debug.WriteLine (xpe.Message);
					else
						throw xpe;
				}

				return;
			}

			var property = GetBindableProperty(elementType, localname, lineInfo, false);

			//If Value is DynamicResource and it's a BP, SetDynamicResource
			if (value is DynamicResource && property != null)
			{
				if (!(xamlelement.GetType()).GetTypeInfo().IsSubclassOf(typeof (BindableObject)))
					throw new XamlParseException(string.Format("{0} is not a BindableObject", xamlelement.GetType().Name), lineInfo);
				((BindableObject)xamlelement).SetDynamicResource(property, ((DynamicResource)value).Key);
				return;
			}

			//If value is BindingBase, and target is a BindableProperty, SetBinding
			if (value is BindingBase && property != null)
			{
				if (!(xamlelement.GetType()).GetTypeInfo().IsSubclassOf(typeof (BindableObject)))
					throw new XamlParseException(string.Format("{0} is not a BindableObject", xamlelement.GetType().Name), lineInfo);

				((BindableObject)xamlelement).SetBinding(property, value as BindingBase);
				return;
			}

			//If it's a BindableProberty, SetValue
			if (property != null)
			{
				if (!(xamlelement.GetType()).GetTypeInfo().IsSubclassOf(typeof (BindableObject)))
					throw new XamlParseException(string.Format("{0} is not a BindableObject", xamlelement.GetType().Name), lineInfo);
				Func<MemberInfo> minforetriever;
				if (attached)
					minforetriever = () => elementType.GetRuntimeMethod("Get" + localname, new[] { typeof (BindableObject) });
				else
					minforetriever = () => elementType.GetRuntimeProperty(localname);

				var convertedValue = value.ConvertTo(property.ReturnType, minforetriever, serviceProvider);

				//SetValue doesn't throw on mismatching type, so check before to get a chance to try the property setting or the collection adding
				var nullable = property.ReturnTypeInfo.IsGenericType &&
				               property.ReturnTypeInfo.GetGenericTypeDefinition() == typeof (Nullable<>);
				if ((convertedValue == null && (!property.ReturnTypeInfo.IsValueType || nullable)) ||
				    (property.ReturnType.IsInstanceOfType(convertedValue)))
				{
					((BindableObject)xamlelement).SetValue(property, convertedValue);
					return;
				}
			}

			var exception = new XamlParseException(
				String.Format("No Property of name {0} found", propertyName.LocalName), lineInfo);

			//If we can assign that value to a normal property, let's do it
			var propertyInfo = elementType.GetRuntimeProperties().FirstOrDefault(p => p.Name == localname);
			MethodInfo setter;
			if (propertyInfo != null && propertyInfo.CanWrite && (setter = propertyInfo.SetMethod) != null)
			{
				object convertedValue = value.ConvertTo(propertyInfo.PropertyType, () => propertyInfo, serviceProvider);
				if (convertedValue == null || propertyInfo.PropertyType.IsInstanceOfType(convertedValue))
				{
					try
					{
						setter.Invoke(xamlelement, new[] { convertedValue });
						return;
					}
					catch (ArgumentException)
					{
					}
				}
				else
				{
					exception = new XamlParseException(
						String.Format("Cannot assign property \"{0}\": type mismatch between \"{1}\" and \"{2}\"", propertyName.LocalName,
							value.GetType(), propertyInfo.PropertyType), lineInfo);
				}
			}

			//If it's an already initialized property, add to it
			MethodInfo getter;
			if (propertyInfo != null && propertyInfo.CanRead && (getter = propertyInfo.GetMethod) != null)
			{
				IEnumerable collection;
				MethodInfo addMethod;
				if ((collection = getter.Invoke(xamlelement, new object[] { }) as IEnumerable) != null
				    &&
				    (addMethod =
					    collection.GetType().GetRuntimeMethods().First(mi => mi.Name == "Add" && mi.GetParameters().Length == 1)) !=
				    null)
				{
					addMethod.Invoke(collection,
					                 new[] { value.ConvertTo(addMethod.GetParameters()[0].ParameterType, (Func<TypeConverter>)null, serviceProvider) });
					return;
				}
			}

			if (context.DoNotThrowOnExceptions)
				System.Diagnostics.Debug.WriteLine (exception.Message);
			else
				throw exception;
		}

		void SetTemplate(ElementTemplate dt, INode node)
		{
#pragma warning disable 0612
			((IDataTemplate)dt).LoadTemplate = () =>
			{
#pragma warning restore 0612
				var context = new HydratationContext { ParentContext = Context, RootElement = Context.RootElement };
				node.Accept(new ExpandMarkupsVisitor(context), null);
				node.Accept(new NamescopingVisitor(context), null);
				node.Accept(new CreateValuesVisitor(context), null);
				node.Accept(new RegisterXNamesVisitor(context), null);
				node.Accept(new FillResourceDictionariesVisitor(context), null);
				node.Accept(new ApplyPropertiesVisitor(context, true), null);
				return context.Values[node];
			};
		}
	}
}