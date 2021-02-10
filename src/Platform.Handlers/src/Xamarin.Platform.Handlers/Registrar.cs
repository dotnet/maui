using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Xamarin.Platform
{
	public static class Registrar
	{
		public static Registrar<IFrameworkElement, IViewHandler> Handlers { get; private set; }

		static Registrar()
		{
			Handlers = new Registrar<IFrameworkElement, IViewHandler>();
		}
	}

	public class Registrar<TType, TTypeRender>
		where TTypeRender : class
	{
		internal Dictionary<Type, Type> _handler = new Dictionary<Type, Type>();
		internal Dictionary<Type, Func<Type, IViewHandler>> _handlerFactories = new Dictionary<Type, Func<Type, IViewHandler>>();

		public void Register<TView, TRender>()
			where TView : TType
			where TRender : TTypeRender
		{
			Register(typeof(TView), typeof(TRender));
		}

		public void Register(Type view, Type handler)
		{
			_handlerFactories.Remove(view);
			_handler[view] = handler;
		}

		public void Register(Type view, Func<Type, IViewHandler> factory)
		{
			_handler.Remove(view);
			_handlerFactories[view] = factory;
		}

		public void Register<TView>(Func<Type, IViewHandler> factory)
			where TView : TType
		{
			Register(typeof(TView), factory);
		}

		public TTypeRender GetHandler<T>()
		{
			return GetHandler(typeof(T));
		}

		internal List<KeyValuePair<Type, Type>> GetViewType(Type type) =>
			_handler.Where(x => isType(x.Value, type)).ToList();

		bool isType(Type type, Type type2)
		{
			if (type == type2)
				return true;

			if (!type.IsGenericType)
				return false;

			var paramerter = type.GetGenericArguments();
			return paramerter[0] == type2;
		}

		public TTypeRender GetHandler(Type type)
		{
			List<Type> types = new List<Type> { type };

			Type? baseType = type.BaseType;

			while (baseType != null)
			{
				types.Add(baseType);
				baseType = baseType.BaseType;
			}

			foreach (var t in types)
			{
				var renderer = GetRenderer(t);
				if (renderer != null)
					return renderer;
			}

			throw new Exception($"Handler not found for {type}");
		}

		public Type GetRendererType(Type type)
		{
			List<Type> types = new List<Type> { type };
			Type? baseType = type.BaseType;

			while (baseType != null)
			{
				types.Add(baseType);
				baseType = baseType.BaseType;
			}

			foreach (var t in types)
			{
				if (_handler.TryGetValue(t, out var returnType))
					return returnType;
			}

			throw new Exception($"Renderer Type not found  {type}");
		}

		TTypeRender? GetRenderer(Type type)
		{
			if (_handlerFactories.TryGetValue(type, out var handlerFactory))
			{
				var newObject = handlerFactory?.Invoke(type) as TTypeRender;
				if (newObject != null)
					return newObject;
			}

			if (!_handler.TryGetValue(type, out var handler))
			{
				return default(TTypeRender);
			}

			try
			{
				var newObject = Activator.CreateInstance(handler);

				if(newObject == null)
					throw new ArgumentException($"No Handler found for type: {type}", nameof(type));

				return (TTypeRender)newObject;
			}
			catch (Exception)
			{
				if (Debugger.IsAttached)
					throw;
			}

			return default(TTypeRender);
		}
	}
}