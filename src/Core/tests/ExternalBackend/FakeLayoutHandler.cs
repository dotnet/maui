using System.Collections.Generic;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Microsoft.Maui.ExternalBackend
{
	/// <summary>
	/// A layout handler for a platform that .NET MAUI does not ship support for.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <see cref="ILayoutHandler"/> carries real behavior — <c>Add</c>, <c>Remove</c>, <c>Clear</c>,
	/// <c>Insert</c>, <c>Update</c> and <c>UpdateZIndex</c> — but it also declares an aliased
	/// <c>PlatformView</c>, so an external backend cannot implement it.
	/// <see cref="ILayoutHandler{TPlatformView}"/> carries the same behavior without the alias.
	/// </para>
	/// <para>
	/// The command mapper keys are taken from <see cref="ILayoutHandler"/> via <c>nameof</c>. Referencing
	/// the interface for its member names is always fine; only implementing it is blocked. Using the same
	/// keys is what keeps .NET MAUI Controls' <c>Layout</c> talking to this handler.
	/// </para>
	/// </remarks>
	public class FakeLayoutHandler : ViewHandler<ILayout, FakeNativeLayoutView>, ILayoutHandler<FakeNativeLayoutView>
	{
		public static IPropertyMapper<ILayout, FakeLayoutHandler> Mapper =
			new PropertyMapper<ILayout, FakeLayoutHandler>(ViewMapper);

		public static CommandMapper<ILayout, FakeLayoutHandler> CommandMapper =
			new CommandMapper<ILayout, FakeLayoutHandler>(ViewCommandMapper)
			{
				[nameof(ILayoutHandler.Add)] = MapAdd,
				[nameof(ILayoutHandler.Remove)] = MapRemove,
				[nameof(ILayoutHandler.Clear)] = MapClear,
				[nameof(ILayoutHandler.Insert)] = MapInsert,
				[nameof(ILayoutHandler.Update)] = MapUpdate,
				[nameof(ILayoutHandler.UpdateZIndex)] = MapUpdateZIndex,
			};

		public FakeLayoutHandler()
			: base(Mapper, CommandMapper)
		{
		}

		protected override FakeNativeLayoutView CreatePlatformView() => new FakeNativeLayoutView();

		public void Add(IView view)
		{
			if (GetNativeChild(view) is FakeNativeView child)
			{
				PlatformView.Children.Add(child);
			}
		}

		public void Remove(IView view)
		{
			if (view.Handler?.PlatformView is FakeNativeView child)
			{
				PlatformView.Children.Remove(child);
			}
		}

		public void Clear() => PlatformView.Children.Clear();

		public void Insert(int index, IView view)
		{
			if (GetNativeChild(view) is FakeNativeView child)
			{
				PlatformView.Children.Insert(index, child);
			}
		}

		public void Update(int index, IView view)
		{
			if (GetNativeChild(view) is FakeNativeView child)
			{
				PlatformView.Children[index] = child;
			}
		}

		public void UpdateZIndex(IView view)
		{
			if (view.Handler?.PlatformView is FakeNativeView child)
			{
				List<FakeNativeView> children = PlatformView.Children;
				children.Remove(child);
				children.Insert(GetClampedZIndex(view, children.Count), child);
			}
		}

		FakeNativeView? GetNativeChild(IView view) =>
			view.ToHandler(MauiContext!).PlatformView as FakeNativeView;

		static int GetClampedZIndex(IView view, int count)
		{
			int index = view.ZIndex;

			if (index < 0)
			{
				return 0;
			}

			return index > count ? count : index;
		}

		public static void MapAdd(FakeLayoutHandler handler, ILayout layout, object? arg)
		{
			if (arg is LayoutHandlerUpdate update)
			{
				handler.Add(update.View);
			}
		}

		public static void MapRemove(FakeLayoutHandler handler, ILayout layout, object? arg)
		{
			if (arg is LayoutHandlerUpdate update)
			{
				handler.Remove(update.View);
			}
		}

		public static void MapClear(FakeLayoutHandler handler, ILayout layout, object? arg) =>
			handler.Clear();

		public static void MapInsert(FakeLayoutHandler handler, ILayout layout, object? arg)
		{
			if (arg is LayoutHandlerUpdate update)
			{
				handler.Insert(update.Index, update.View);
			}
		}

		public static void MapUpdate(FakeLayoutHandler handler, ILayout layout, object? arg)
		{
			if (arg is LayoutHandlerUpdate update)
			{
				handler.Update(update.Index, update.View);
			}
		}

		public static void MapUpdateZIndex(FakeLayoutHandler handler, ILayout layout, object? arg)
		{
			if (arg is IView view)
			{
				handler.UpdateZIndex(view);
			}
		}
	}
}
