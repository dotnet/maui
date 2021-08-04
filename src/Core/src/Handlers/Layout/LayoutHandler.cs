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
		};

		public LayoutHandler() : base(LayoutMapper)
		{

		}

		public LayoutHandler(PropertyMapper? mapper = null) : base(mapper ?? LayoutMapper)
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
	}
}
