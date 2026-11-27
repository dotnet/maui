using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Maui.Hosting.Internal
{
	internal sealed class RegisteredHandlerServiceTypeSet
	{
		private readonly HashSet<Type> _concreteHandlerServiceTypeSet = new();
		private readonly HashSet<Type> _interfaceHandlerServiceTypeSet = new();

		public void Add(Type virtualViewType)
		{
			if (virtualViewType.IsInterface)
			{
				_interfaceHandlerServiceTypeSet.Add(virtualViewType);
			}
			else
			{
				_concreteHandlerServiceTypeSet.Add(virtualViewType);
			}
		}

		public Type? ResolveVirtualViewToRegisteredHandlerServiceType(Type type) =>
			ResolveConcreteHandlerServiceType(type) ?? ResolveInterfaceHandlerServiceType(type);

		/// <summary>
		/// Returns the most specific registered non-interface service type assignable from <paramref name="type"/>.
		/// </summary>
		public Type? ResolveConcreteHandlerServiceType(Type type)
		{
			Debug.Assert(typeof(IElement).IsAssignableFrom(type));

			if (_concreteHandlerServiceTypeSet.Contains(type))
				return type;

			return ResolveVirtualViewFromTypeSet(type, _concreteHandlerServiceTypeSet);
		}

		/// <summary>
		/// Returns the most specific registered interface service type assignable from <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The requested virtual view type.</param>
		/// <param name="defaultInterface">
		/// An unregistered interface that competes with the registered interfaces as if it had been registered,
		/// representing a built-in interface default.
		/// </param>
		public Type? ResolveInterfaceHandlerServiceType(Type type, Type? defaultInterface = null)
		{
			Debug.Assert(typeof(IElement).IsAssignableFrom(type));

			if (_interfaceHandlerServiceTypeSet.Contains(type) || type == defaultInterface)
				return type;

			return ResolveVirtualViewFromTypeSet(type, _interfaceHandlerServiceTypeSet, defaultInterface);
		}

		public bool Contains(Type serviceType) =>
			_concreteHandlerServiceTypeSet.Contains(serviceType) || _interfaceHandlerServiceTypeSet.Contains(serviceType);

		private static Type? ResolveVirtualViewFromTypeSet(Type type, HashSet<Type> set, Type? additionalCandidate = null)
		{
			Type? bestVirtualViewHandlerServiceType = null;

			foreach (Type registeredViewHandlerServiceType in set)
			{
				Consider(registeredViewHandlerServiceType);
			}

			if (additionalCandidate is not null)
			{
				Consider(additionalCandidate);
			}

			return bestVirtualViewHandlerServiceType;

			void Consider(Type registeredViewHandlerServiceType)
			{
				if (registeredViewHandlerServiceType.IsAssignableFrom(type))
				{
					if (bestVirtualViewHandlerServiceType is null || bestVirtualViewHandlerServiceType.IsAssignableFrom(registeredViewHandlerServiceType))
					{
						bestVirtualViewHandlerServiceType = registeredViewHandlerServiceType;
					}
					else if (!registeredViewHandlerServiceType.IsAssignableFrom(bestVirtualViewHandlerServiceType))
					{
						// This exception can be thrown when the Element implements two interfaces that aren't derived from each other
						// which both have a registered image source service to them.
						// For example, consider this setup:
						//	class MyElement : ILargeElement, IVisibleElement { ... }
						//
						//	handlersCollection.AddHandler<ILargeElement, LargeElementHandler>();
						//  handlersCollection.AddHandler<IVisibleElement, VisibleElementHandler>();
						//
						// 	  var handler = Handler.GetHandler(typeof(MyElement));
						//	ambiguous match: both LargeElementHandler and VisibleElementHandler are registered for MyImageSource

						throw new InvalidOperationException($"Unable to find a single {nameof(IElementHandler)} corresponding to {type}. There is an ambiguous match between {bestVirtualViewHandlerServiceType} and {registeredViewHandlerServiceType}.");
					}
				}
			}
		}
	}
}
