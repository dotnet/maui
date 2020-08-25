using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace System.Maui
{
	public static class Registrar
	{
		public static Registrar<IFrameworkElement, IViewRenderer> Handlers { get; private set; }
		public static Registrar<Type, Type> Registered { get; private set; }
		public static Dictionary<string, Type> Effects { get; } = new Dictionary<string, Type>();
		//public static Dictionary<string, IList<StyleSheets.StylePropertyAttribute>> StyleProperties { get; } = new Dictionary<string, IList<StyleSheets.StylePropertyAttribute>>();

		static Registrar()
		{
			Handlers = new Registrar<IFrameworkElement, IViewRenderer> ();
		}
	}
	public class Registrar<TType, TTypeRender>
	{
		internal Dictionary<Type, Type> Handler = new Dictionary<Type, Type>();

		public void Register<TView, TRender>()
			where TView : TType
				where TRender : TTypeRender
		{
			Register(typeof(TView), typeof(TRender));
		}

		public void Register(Type view, Type handler)
		{
			Handler[view] = handler;
		}
		public TTypeRender GetHandler<T>()
		{
			return GetHandler(typeof(T));
		}

		internal List<KeyValuePair<Type,Type>> GetViewType(Type type) =>
			Handler.Where(x => isType(x.Value,type)).ToList();
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
				var renderer = getRenderer(t);
				if (renderer != null)
					return renderer;
			}
			return default(TTypeRender);
		}

		public Type GetRendererType(Type type)
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
				if (Handler.TryGetValue(t, out var returnType))
					return returnType;
			}
			return null;
		}

		TTypeRender getRenderer(Type t)
		{
			if (!Handler.TryGetValue(t, out var renderer))
				return default(TTypeRender);
			try
			{
				var newObject = Activator.CreateInstance(renderer);
				return (TTypeRender)newObject;
			}
			catch (Exception ex)
			{
				if (Debugger.IsAttached)
					throw ex;
			}

			return default;
		}
	}
}
