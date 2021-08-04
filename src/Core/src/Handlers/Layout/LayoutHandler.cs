#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Handlers
{
	public partial class LayoutHandler : ILayoutHandler
	{
		public static PropertyMapper<ILayout> LayoutMapper = new(ViewMapper)
		{
		};

		public static CommandMapper<ILayout, ILayoutHandler> LayoutCommandMapper = new(ViewCommandMapper)
		{
			[nameof(ILayoutHandler.Add)] = MapAdd,
			[nameof(ILayoutHandler.Remove)] = MapRemove,
			[nameof(ILayoutHandler.Clear)] = MapClear,
			[nameof(ILayoutHandler.Insert)] = MapInsert,
			[nameof(ILayoutHandler.Update)] = MapUpdate,
		};

		public LayoutHandler() : base(LayoutMapper, LayoutCommandMapper)
		{

		}

		public LayoutHandler(PropertyMapper? mapper = null, CommandMapper? commandMapper = null) 
			: base(mapper ?? LayoutMapper, commandMapper ?? LayoutCommandMapper)
		{

		}

		public static void MapAdd(ILayoutHandler handler, ILayout layout, object? arg)
		{
			if (arg is not IView view)
			{
				throw new ArgumentException($"{nameof(arg)} must be of type {nameof(IView)}");
			}

			handler.Add(view);
		}

		public static void MapRemove(ILayoutHandler handler, ILayout layout, object? arg)
		{
			if (arg is not IView view)
			{
				throw new ArgumentException($"{nameof(arg)} must be of type {nameof(IView)}");
			}

			handler.Remove(view);
		}

		public static void MapInsert(ILayoutHandler handler, ILayout layout, object? arg)
		{
			if (arg is ValueTuple<int, IView> insertArgs)
			{
				(int index, IView view) = insertArgs;

				handler.Insert(index, view);
			}
		}

		public static void MapClear(ILayoutHandler handler, ILayout layout, object? arg)
		{
			handler.Clear();
		}

		private static void MapUpdate(ILayoutHandler handler, ILayout layout, object? arg)
		{
			if (arg is ValueTuple<int, IView> updateArgs)
			{
				(int index, IView view) = updateArgs;

				handler.Update(index, view);
			}
		}
	}
}
