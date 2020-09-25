using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

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
	{
		internal Dictionary<Type, Type> _handler = new Dictionary<Type, Type>();

		public void Register<TView, TRender>()
			where TView : TType
				where TRender : TTypeRender
		{
			Register(typeof(TView), typeof(TRender));
		}

		public void Register(Type view, Type handler)
		{
			_handler[view] = handler;
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
			Type baseType = type.BaseType;

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

			return default!;
		}

		public Type? GetRendererType(Type type)
		{
			List<Type> types = new List<Type> { type };
			Type baseType = type.BaseType;

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

			return null;
		}

		TTypeRender GetRenderer(Type type)
		{
			if (!_handler.TryGetValue(type, out var handler))
				return default!;
			try
			{
				var newObject = Activator.CreateInstance(handler);
				return (TTypeRender)newObject;
			}
			catch (Exception ex)
			{
				if (Debugger.IsAttached)
					throw ex;
			}

			throw new ArgumentException($"No Handler found for type: {type}", nameof(type));
		}
	}
}