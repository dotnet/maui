using System;
using System.Collections.Generic;
using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	public sealed class RendererPool
	{
		readonly Dictionary<Type, Stack<IVisualElementRenderer>> _freeRenderers = new Dictionary<Type, Stack<IVisualElementRenderer>>();

		readonly VisualElement _oldElement;

		readonly IVisualElementRenderer _parent;

		public RendererPool(IVisualElementRenderer renderer, VisualElement oldElement)
		{
			if (renderer == null)
				throw new ArgumentNullException("renderer");

			if (oldElement == null)
				throw new ArgumentNullException("oldElement");

			_oldElement = oldElement;
			_parent = renderer;
		}

		public void ClearChildrenRenderers()
		{
			if (((IElementController)_parent.Element).LogicalChildren.Count == 0)
				return;
			ClearChildrenRenderers(_oldElement);
		}

		public IVisualElementRenderer GetFreeRenderer(VisualElement view)
		{
			if (view == null)
				throw new ArgumentNullException("view");

			Type rendererType = Internals.Registrar.Registered.GetHandlerTypeForObject(view) ?? typeof(ViewRenderer);

			Stack<IVisualElementRenderer> renderers;
			if (!_freeRenderers.TryGetValue(rendererType, out renderers) || renderers.Count == 0)
				return null;

			IVisualElementRenderer renderer = renderers.Pop();
			renderer.SetElement(view);
			return renderer;
		}

		void ClearChildrenRenderers(VisualElement view)
		{
			if (view == null)
				return;

			foreach (Element logicalChild in ((IElementController)view).LogicalChildren)
			{
				var child = logicalChild as VisualElement;

				if (child != null)
				{
					IVisualElementRenderer renderer = Platform.GetRenderer(child);

					if (renderer == null)
						continue;

					if (renderer.View.IsDisposed())
						continue;

					if (renderer.View.Parent != _parent.View)
						continue;

					renderer.View.RemoveFromParent();

					Platform.SetRenderer(child, null);

					PushRenderer(renderer);
				}
			}

			var viewGroup = _parent.View as ViewGroup;

			if (viewGroup != null && viewGroup.ChildCount != 0)
				viewGroup.RemoveAllViews();
		}

		void PushRenderer(IVisualElementRenderer renderer)
		{
			var reflectableType = renderer as System.Reflection.IReflectableType;
			var rendererType = reflectableType != null ? reflectableType.GetTypeInfo().AsType() : renderer.GetType();

			Stack<IVisualElementRenderer> renderers;
			if (!_freeRenderers.TryGetValue(rendererType, out renderers))
				_freeRenderers[rendererType] = renderers = new Stack<IVisualElementRenderer>();

			renderers.Push(renderer);
		}
	}
}