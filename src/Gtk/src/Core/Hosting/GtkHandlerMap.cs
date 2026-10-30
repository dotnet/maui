using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Gtk.Handlers;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Gtk.Hosting
{
	/// <summary>
	/// Maps stock MAUI handler types to their GTK subclasses and rewrites handler registrations accordingly.
	/// </summary>
	/// <remarks>
	/// <para>The stock handlers on the platform-less assemblies throw from <c>CreatePlatformView</c>; each GTK handler subclasses
	/// its stock handler. Controls registers concrete pairs such as <c>Label -&gt; LabelHandler</c>; <see cref="Apply"/> (called from
	/// <see cref="GtkMauiAppBuilderExtensions.UseMauiGtkCore"/>) replaces every registration whose implementation type is a mapped stock type.</para>
	/// <para>The map is open: other backend assemblies add their own pairs with <see cref="Add"/> (for example from a module
	/// initializer or their <c>Use...</c> extension) without touching this file. Core's own entries are in
	/// <c>GtkHandlerMap.Core.cs</c>.</para>
	/// </remarks>
	public static partial class GtkHandlerMap
	{
		static readonly object s_gate = new();
		static readonly Dictionary<Type, Type> s_stockToGtk = new();
		static readonly Dictionary<Type, Type> s_defaultViewHandlers = new();

		static GtkHandlerMap()
		{
			AddCoreEntries();
			AddControlEntries();
			AddNavigationEntries();
			AddWebViewEntries();
		}

		/// <summary>Maps <paramref name="stockHandlerType"/> to <paramref name="gtkHandlerType"/>.</summary>
		/// <param name="stockHandlerType">The stock handler type registered by MAUI.</param>
		/// <param name="gtkHandlerType">The GTK handler; it must derive from <paramref name="stockHandlerType"/>.</param>
		public static void Add(Type stockHandlerType, Type gtkHandlerType)
		{
			if (stockHandlerType is null)
				throw new ArgumentNullException(nameof(stockHandlerType));
			if (gtkHandlerType is null)
				throw new ArgumentNullException(nameof(gtkHandlerType));
			if (!stockHandlerType.IsAssignableFrom(gtkHandlerType))
				throw new ArgumentException($"{gtkHandlerType} must derive from {stockHandlerType}.", nameof(gtkHandlerType));

			lock (s_gate)
				s_stockToGtk[stockHandlerType] = gtkHandlerType;
		}

		/// <summary>Maps <typeparamref name="TStock"/> to <typeparamref name="TGtk"/>.</summary>
		/// <typeparam name="TStock">The stock handler type.</typeparam>
		/// <typeparam name="TGtk">The GTK handler type.</typeparam>
		public static void Add<TStock, TGtk>()
			where TStock : IElementHandler
			where TGtk : TStock =>
			Add(typeof(TStock), typeof(TGtk));

		/// <summary>
		/// Declares the GTK handler used for a core virtual-view interface (for example <c>IButton</c>) when nothing else is registered
		/// for it, so apps that use only Core (no Controls) get handlers.
		/// </summary>
		/// <param name="virtualViewType">The virtual view type, typically a Core interface.</param>
		/// <param name="gtkHandlerType">The GTK handler.</param>
		public static void AddDefault(Type virtualViewType, Type gtkHandlerType)
		{
			if (virtualViewType is null)
				throw new ArgumentNullException(nameof(virtualViewType));
			if (gtkHandlerType is null)
				throw new ArgumentNullException(nameof(gtkHandlerType));

			lock (s_gate)
				s_defaultViewHandlers[virtualViewType] = gtkHandlerType;
		}

		/// <summary>Looks up the GTK handler type for a stock handler type.</summary>
		/// <param name="stockHandlerType">The stock handler type.</param>
		/// <param name="gtkHandlerType">The mapped type.</param>
		public static bool TryGetGtkHandler(Type stockHandlerType, out Type gtkHandlerType)
		{
			lock (s_gate)
				return s_stockToGtk.TryGetValue(stockHandlerType, out gtkHandlerType!);
		}

		/// <summary>
		/// Rewrites the registrations in <paramref name="handlers"/> whose implementation type is a mapped stock handler, then
		/// registers the default handlers for core interfaces that have none.
		/// </summary>
		/// <param name="handlers">The handler collection being built.</param>
		public static void Apply(IMauiHandlersCollection handlers)
		{
			if (handlers is null)
				throw new ArgumentNullException(nameof(handlers));

			Dictionary<Type, Type> map;
			Dictionary<Type, Type> defaults;
			lock (s_gate)
			{
				map = new Dictionary<Type, Type>(s_stockToGtk);
				defaults = new Dictionary<Type, Type>(s_defaultViewHandlers);
			}

			for (int i = 0; i < handlers.Count; i++)
			{
				var descriptor = handlers[i];
				if (descriptor.ImplementationType is { } implementation && map.TryGetValue(implementation, out var gtk))
					handlers[i] = new ServiceDescriptor(descriptor.ServiceType, gtk, descriptor.Lifetime);
			}

			PreferMostDerived(handlers);

			foreach (var pair in defaults)
				handlers.TryAddHandler(pair.Key, pair.Value);
		}

		// The last registration of a service type wins. A stock registration rewritten to a GTK handler must not shadow an explicit registration
		// of a more specific handler for the same view (for example a Controls-aware subclass), whatever the order of the registrations:
		// move such a registration after the rewritten one.
		static void PreferMostDerived(IMauiHandlersCollection handlers)
		{
			for (int i = 0; i < handlers.Count; i++)
			{
				var current = handlers[i];
				if (current.ImplementationType is not { } currentType)
					continue;

				for (int j = i + 1; j < handlers.Count; j++)
				{
					var later = handlers[j];
					if (later.ServiceType == current.ServiceType && later.ImplementationType is { } laterType
						&& laterType != currentType && laterType.IsAssignableFrom(currentType))
					{
						// current is more specific than the later registration: current must win.
						handlers.RemoveAt(i);
						handlers.Add(current);
						i--;
						break;
					}
				}
			}
		}
	}
}
