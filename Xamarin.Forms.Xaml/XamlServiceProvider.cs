using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Xaml.Internals
{
	public class XamlServiceProvider : IServiceProvider
	{
		readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

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

			IValueConverterProvider = new ValueConverterProvider();
		}

		public XamlServiceProvider() => IValueConverterProvider = new ValueConverterProvider();

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

		[Obsolete]
		[EditorBrowsable(EditorBrowsableState.Never)]
		internal INameScopeProvider INameScopeProvider
		{
			get { return (INameScopeProvider)GetService(typeof(INameScopeProvider)); }
			set { services[typeof(INameScopeProvider)] = value; }
		}
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

	public class SimpleValueTargetProvider : IProvideParentValues, IProvideValueTarget, IReferenceProvider
	{
		readonly object[] objectAndParents;
		readonly object targetProperty;
		readonly INameScope scope;

		[Obsolete("SimpleValueTargetProvider(object[] objectAndParents) is obsolete as of version 2.3.4. Please use SimpleValueTargetProvider(object[] objectAndParents, object targetProperty) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public SimpleValueTargetProvider(object[] objectAndParents) : this(objectAndParents, null)
		{
		}

		[Obsolete("SimpleValueTargetProvider(object[] objectAndParents) is obsolete as of version 3.3.0. Please use SimpleValueTargetProvider(object[] objectAndParents, object targetProperty, NameScope scope) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public SimpleValueTargetProvider(object[] objectAndParents, object targetProperty) : this(objectAndParents, targetProperty, null)
		{
		}

		public SimpleValueTargetProvider(object[] objectAndParents, object targetProperty, INameScope scope)
		{
			if (objectAndParents == null)
				throw new ArgumentNullException(nameof(objectAndParents));
			if (objectAndParents.Length == 0)
				throw new ArgumentException();

			this.objectAndParents = objectAndParents;
			this.targetProperty = targetProperty;
			this.scope = scope;
		}

		IEnumerable<object> IProvideParentValues.ParentObjects => objectAndParents;
		object IProvideValueTarget.TargetObject => objectAndParents[0];
		object IProvideValueTarget.TargetProperty => targetProperty;

		public object FindByName(string name)
		{
			object value;
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
			exception = null;
			var split = qualifiedTypeName.Split(':');
			if (split.Length > 2)
				return null;

			string prefix, name;
			if (split.Length == 2)
			{
				prefix = split[0];
				name = split[1];
			}
			else
			{
				prefix = "";
				name = split[0];
			}

			IXmlLineInfo xmlLineInfo = null;
			if (serviceProvider != null)
			{
				if (serviceProvider.GetService(typeof(IXmlLineInfoProvider)) is IXmlLineInfoProvider lineInfoProvider)
					xmlLineInfo = lineInfoProvider.XmlLineInfo;
			}

			var namespaceuri = namespaceResolver.LookupNamespace(prefix);
			if (namespaceuri == null)
			{
				exception = new XamlParseException($"No xmlns declaration for prefix \"{prefix}\"", xmlLineInfo);
				return null;
			}

			return getTypeFromXmlName(new XmlType(namespaceuri, name, null), xmlLineInfo, currentAssembly, out exception);
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

	[Obsolete]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class NameScopeProvider : INameScopeProvider
	{
		public INameScope NameScope { get; set; }
	}

	public class XmlNamespaceResolver : IXmlNamespaceResolver
	{
		readonly Dictionary<string, string> namespaces = new Dictionary<string, string>();

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
}