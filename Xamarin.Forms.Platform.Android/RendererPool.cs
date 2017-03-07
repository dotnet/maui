using System;
using System.Collections.Generic;

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

			Type rendererType = Internals.Registrar.Registered.GetHandlerType(view.GetType()) ?? typeof(ViewRenderer);

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

					if (renderer.ViewGroup.Parent != _parent.ViewGroup)
						continue;

					renderer.ViewGroup.RemoveFromParent();

					Platform.SetRenderer(child, null);
					PushRenderer(renderer);
				}
			}

			if (_parent.ViewGroup.ChildCount != 0)
				_parent.ViewGroup.RemoveAllViews();
		}

		void PushRenderer(IVisualElementRenderer renderer)
		{
			Type rendererType = renderer.GetType();

			Stack<IVisualElementRenderer> renderers;
			if (!_freeRenderers.TryGetValue(rendererType, out renderers))
				_freeRenderers[rendererType] = renderers = new Stack<IVisualElementRenderer>();

			renderers.Push(renderer);
		}
	}
}