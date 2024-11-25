using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Xml;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Xaml.Internals
{
	public class XamlServiceProvider : IServiceProvider
	{
		readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

		static IValueConverterProvider defaultValueConverterProvider = new ValueConverterProvider();

		[RequiresUnreferencedCode(TrimmerConstants.XamlRuntimeParsingNotSupportedWarning)]
#if !NETSTANDARD
		[RequiresDynamicCode(TrimmerConstants.XamlRuntimeParsingNotSupportedWarning)]
#endif
		internal XamlServiceProvider(INode node, HydrationContext context)
		{
			if (context != null && node != null && node.Parent != null && context.Values.TryGetValue(node.Parent, out object targetObject))
				IProvideValueTarget = new XamlValueTargetProvider(targetObject, node, context, null);
			if (context != null)
				IRootObjectProvider = new XamlRootObjectProvider(context.RootElement);
			if (context != null && node != null)
			{
				IXamlTypeResolver = new XamlTypeResolver(node.NamespaceResolver, XamlParser.GetElementType, context.RootAssembly);
				Add(typeof(IReferenceProvider), new ReferenceProvider(node));
			}

			if (node is IXmlLineInfo xmlLineInfo)
				IXmlLineInfoProvider = new XmlLineInfoProvider(xmlLineInfo);

			IValueConverterProvider = defaultValueConverterProvider;

			if (node is IElementNode elementNode)
				Add(typeof(IXamlDataTypeProvider), new XamlDataTypeProvider(elementNode, context));
		}

		public XamlServiceProvider() => IValueConverterProvider = defaultValueConverterProvider;

		internal IProvideValueTarget IProvideValueTarget
		{
			get => (IProvideValueTarget)GetService(typeof(IProvideValueTarget));
			set => services[typeof(IProvideValueTarget)] = value;
		}

		internal IXamlTypeResolver IXamlTypeResolver
		{
			get => (IXamlTypeResolver)GetService(typeof(IXamlTypeResolver));
			set => services[typeof(IXamlTypeResolver)] = value;
		}

		internal IRootObjectProvider IRootObjectProvider
		{
			get => (IRootObjectProvider)GetService(typeof(IRootObjectProvider));
			set => services[typeof(IRootObjectProvider)] = value;
		}

		internal IXmlLineInfoProvider IXmlLineInfoProvider
		{
			get => (IXmlLineInfoProvider)GetService(typeof(IXmlLineInfoProvider));
			set => services[typeof(IXmlLineInfoProvider)] = value;
		}

		internal IValueConverterProvider IValueConverterProvider
		{
			get => (IValueConverterProvider)GetService(typeof(IValueConverterProvider));
			set => services[typeof(IValueConverterProvider)] = value;
		}

		public object GetService(Type serviceType) => services.TryGetValue(serviceType, out var service) ? service : null;

		public void Add(Type type, object service) => services.Add(type, service);
	}

	class XamlValueTargetProvider : IProvideParentValues, IProvideValueTarget
	{
		public XamlValueTargetProvider(object targetObject, INode node, HydrationContext context, object targetProperty)
		{
			Context = context;
			Node = node;
			TargetObject = targetObject;
			TargetProperty = targetProperty;
		}

		INode Node { get; }

		HydrationContext Context { get; }
		public object TargetObject { get; }
		public object TargetProperty { get; internal set; } = null;

		IEnumerable<object> IProvideParentValues.ParentObjects
		{
			get
			{
				if (Node == null || Context == null)
					yield break;
				var n = Node;
				var context = Context;
				while (n.Parent != null && context != null)
				{
					if (n.Parent is IElementNode)
					{
						if (context.Values.TryGetValue(n.Parent, out var obj))
							yield return obj;
						else
						{
							context = context.ParentContext;
							continue;
						}
					}
					n = n.Parent;
				}
			}
		}
	}

#nullable enable
	public class ValueTargetProvider : IProvideValueTarget
	{
		private object targetObject;
		private object targetProperty;

		public ValueTargetProvider(object targetObject, object targetProperty)
		{
			this.targetObject = targetObject;
			this.targetProperty = targetProperty;
		}
		object IProvideValueTarget.TargetObject => targetObject;
		object IProvideValueTarget.TargetProperty => targetProperty;
	}
#nullable restore

	public class SimpleValueTargetProvider : IProvideParentValues, IProvideValueTarget, IReferenceProvider
	{
		readonly object[] objectAndParents;
		readonly object targetProperty;
		readonly INameScope[] scopes;

		[Obsolete("Use the other ctor")]
		public SimpleValueTargetProvider(object[] objectAndParents, object targetProperty, INameScope scope)
			: this(objectAndParents, targetProperty, new INameScope[] { scope }, false)
		{
		}

		public SimpleValueTargetProvider(object[] objectAndParents, object targetProperty, INameScope[] scopes, bool notused)
		{
			if (objectAndParents == null)
				throw new ArgumentNullException(nameof(objectAndParents));
			if (objectAndParents.Length == 0)
				throw new ArgumentException();

			this.objectAndParents = objectAndParents;
			this.targetProperty = targetProperty;
			this.scopes = scopes;
		}

		IEnumerable<object> IProvideParentValues.ParentObjects => objectAndParents;
		object IProvideValueTarget.TargetObject => objectAndParents[0];
		object IProvideValueTarget.TargetProperty => targetProperty;

		public object FindByName(string name)
		{
			object value;
			if (scopes != null)
				foreach (var scope in scopes)
					if ((value = scope?.FindByName(name)) != null)
						return value;

			for (var i = 0; i < objectAndParents.Length; i++)
			{
				if (!(objectAndParents[i] is BindableObject bo))
					continue;
				if (!(NameScope.GetNameScope(bo) is INameScope ns))
					continue;
				if ((value = ns.FindByName(name)) != null)
					return value;
			}
			return null;
		}
	}

	public class XamlTypeResolver : IXamlTypeResolver
	{
		readonly Assembly currentAssembly;
		readonly GetTypeFromXmlName getTypeFromXmlName;
		readonly IXmlNamespaceResolver namespaceResolver;

		[RequiresUnreferencedCode(TrimmerConstants.XamlRuntimeParsingNotSupportedWarning)]
#if !NETSTANDARD
		[RequiresDynamicCode(TrimmerConstants.XamlRuntimeParsingNotSupportedWarning)]
#endif
		public XamlTypeResolver(IXmlNamespaceResolver namespaceResolver, Assembly currentAssembly)
			: this(namespaceResolver, XamlParser.GetElementType, currentAssembly)
		{
		}

		internal XamlTypeResolver(IXmlNamespaceResolver namespaceResolver, GetTypeFromXmlName getTypeFromXmlName,
			Assembly currentAssembly)
		{
			this.currentAssembly = currentAssembly;
			this.namespaceResolver = namespaceResolver ?? throw new ArgumentNullException();
			this.getTypeFromXmlName = getTypeFromXmlName ?? throw new ArgumentNullException();
		}

		Type IXamlTypeResolver.Resolve(string qualifiedTypeName, IServiceProvider serviceProvider)
		{
			var type = Resolve(qualifiedTypeName, serviceProvider, out XamlParseException e);
			if (e != null)
				throw e;
			return type;
		}

		bool IXamlTypeResolver.TryResolve(string qualifiedTypeName, out Type type)
		{
			type = Resolve(qualifiedTypeName, null, out XamlParseException exception);
			return exception == null;
		}

		internal bool TryResolve(XmlType xmlType, out Type type)
		{
			XamlParseException exception;
			type = getTypeFromXmlName(xmlType, null, currentAssembly, out exception);
			return exception == null;
		}

		Type Resolve(string qualifiedTypeName, IServiceProvider serviceProvider, out XamlParseException exception)
		{
			IXmlLineInfo xmlLineInfo = null;
			if (serviceProvider != null)
			{
				if (serviceProvider.GetService(typeof(IXmlLineInfoProvider)) is IXmlLineInfoProvider lineInfoProvider)
					xmlLineInfo = lineInfoProvider.XmlLineInfo;
			}

			var xmlType = TypeArgumentsParser.ParseSingle(qualifiedTypeName, namespaceResolver, xmlLineInfo);
			return getTypeFromXmlName(xmlType, xmlLineInfo, currentAssembly, out exception);
		}

		internal delegate Type GetTypeFromXmlName(XmlType xmlType, IXmlLineInfo xmlInfo, Assembly currentAssembly, out XamlParseException exception);
	}

	class XamlRootObjectProvider : IRootObjectProvider
	{
		public XamlRootObjectProvider(object rootObject) => RootObject = rootObject;

		public object RootObject { get; }
	}

	public class XmlLineInfoProvider : IXmlLineInfoProvider
	{
		public XmlLineInfoProvider(IXmlLineInfo xmlLineInfo) => XmlLineInfo = xmlLineInfo;

		public IXmlLineInfo XmlLineInfo { get; }
	}

	class ReferenceProvider : IReferenceProvider
	{
		readonly INode _node;
		internal ReferenceProvider(INode node) => _node = node;

		public object FindByName(string name)
		{
			var n = _node;
			while (n != null)
			{
				object value;
				if ((value = (n as IElementNode)?.NameScopeRef.NameScope?.FindByName(name)) != null)
					return value;
				n = n.Parent;
			}
			return null;
		}
	}

	public class XmlNamespaceResolver : IXmlNamespaceResolver
	{
		readonly Dictionary<string, string> namespaces = new Dictionary<string, string>(StringComparer.Ordinal);

		public IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope) => throw new NotImplementedException();

		public string LookupNamespace(string prefix)
		{
			if (namespaces.TryGetValue(prefix, out var result))
				return result;
			return null;
		}

		public string LookupPrefix(string namespaceName) => throw new NotImplementedException();
		public void Add(string prefix, string ns) => namespaces.Add(prefix, ns);
	}

	class XamlDataTypeProvider : IXamlDataTypeProvider
	{
		[RequiresUnreferencedCode(TrimmerConstants.XamlRuntimeParsingNotSupportedWarning)]
#if !NETSTANDARD
		[RequiresDynamicCode(TrimmerConstants.XamlRuntimeParsingNotSupportedWarning)]
#endif
		public XamlDataTypeProvider(IElementNode node, HydrationContext context)
		{
			Context = context;
			

			static IElementNode GetParent(IElementNode node)
			{
				return node switch
				{
					{ Parent: ListNode { Parent: IElementNode parentNode } } => parentNode,
					{ Parent: IElementNode parentNode } => parentNode,
					_ => null,
				};
			}

			static bool IsBindingContextBinding(IElementNode node)
			{
				if (   node.TryGetPropertyName(node.Parent, out XmlName name)
					&& name.NamespaceURI == ""
					&& name.LocalName == nameof(BindableObject.BindingContext))
					return true;
				return false;
			}

			static bool DoesNotInheritDataType(IElementNode node, HydrationContext context)
			{
				if (   node.TryGetPropertyName(node.Parent, out XmlName name)
					&& node.Parent is IElementNode parent
					&& XamlParser.GetElementType(parent.XmlType, 
												 new XmlLineInfo(((IXmlLineInfo)node).LineNumber, ((IXmlLineInfo)node).LinePosition), 
												 context.RootElement.GetType().Assembly, out var xpe) is Type parentType
					&& parentType.GetRuntimeProperties().FirstOrDefault(p => p.Name == name.LocalName) is PropertyInfo propertyInfo
					&& propertyInfo.CustomAttributes.Any(ca => ca.AttributeType == typeof(DoesNotInheritDataTypeAttribute)))
				{								
					return true;
				}
				return false;
			}

			INode dataTypeNode = null;
			IElementNode n = node as IElementNode;

			// Special handling for BindingContext={Binding ...}
			// The order of checks is:
			// - x:DataType on the binding itself
			// - SKIP looking for x:DataType on the parent
			// - continue looking for x:DataType on the parent's parent...
			IElementNode skipNode = null;
			if (IsBindingContextBinding(node))
			{
				skipNode = GetParent(node);
			}

			while (n != null)
			{
				
				if (n != skipNode && n.Properties.TryGetValue(XmlName.xDataType, out dataTypeNode))
				{
					break;
				}
				if (DoesNotInheritDataType(n, context))
				{					
					break;
				}
				n = GetParent(n);
			}
			if (dataTypeNode is ValueNode valueNode)
				BindingDataType = valueNode.Value as string;
		}
		public string BindingDataType { get; }
		public HydrationContext Context { get; }
	}
}
