using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Maui.Hosting.Internal
{
	internal sealed class RegisteredHandlerServiceTypeSet
	{
		private static readonly ConcurrentDictionary<IMauiHandlersCollection, RegisteredHandlerServiceTypeSet> s_instances = new();

		internal static RegisteredHandlerServiceTypeSet GetInstance(IMauiHandlersCollection collection) =>
			s_instances.GetOrAdd(collection, static _ => new RegisteredHandlerServiceTypeSet());

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

		public Type? ResolveVirtualViewToRegisteredHandlerServiceType(Type type)
		{
			Debug.Assert(typeof(IElement).IsAssignableFrom(type));

			if (_concreteHandlerServiceTypeSet.Contains(type)
				|| _interfaceHandlerServiceTypeSet.Contains(type))
			{
				return type;
			}

			return ResolveVirtualViewFromTypeSet(type, _concreteHandlerServiceTypeSet)
				?? ResolveVirtualViewFromTypeSet(type, _interfaceHandlerServiceTypeSet);
		}

		private static Type? ResolveVirtualViewFromTypeSet(Type type, HashSet<Type> set)
		{
			Type? bestVirtualViewHandlerServiceType = null;

			foreach (Type registeredViewHandlerServiceType in set)
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

			return bestVirtualViewHandlerServiceType;
		}
	}
}