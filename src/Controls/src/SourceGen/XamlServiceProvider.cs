#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen;

internal class XamlServiceProvider : IServiceProvider
{
	private INode _node;
	private SourceGenContext _context;

	public XamlServiceProvider(INode node, SourceGenContext context)
	{
		_node = node;
		_context = context;
	}

	readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

	public object? GetService(Type serviceType) => services.TryGetValue(serviceType, out var service) ? service : null;

	public void Add(Type type, object service) => services.Add(type, service);
}