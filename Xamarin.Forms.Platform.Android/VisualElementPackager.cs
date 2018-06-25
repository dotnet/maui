using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.Content;
using Xamarin.Forms.Internals;
using Android.Views;
using AView = Android.Views.View;
using System.Linq;

namespace Xamarin.Forms.Platform.Android
{
	public class VisualElementPackager : IDisposable
	{
		readonly EventHandler<ElementEventArgs> _childAddedHandler;
		readonly EventHandler<ElementEventArgs> _childRemovedHandler;
		readonly EventHandler _childReorderedHandler;
		List<IVisualElementRenderer> _childViews;

		Dictionary<BindableObject, VisualElementPackager> _childPackagers;

		bool _disposed;

		IVisualElementRenderer _renderer;

		VisualElement _element;

		IElementController ElementController => _element;

		public VisualElementPackager(IVisualElementRenderer renderer, VisualElement element = null)
		{
			if (renderer == null)
				throw new ArgumentNullException(nameof(renderer));

			_element = element ?? renderer.Element;
			_childAddedHandler = OnChildAdded;
			_childRemovedHandler = OnChildRemoved;
			_childReorderedHandler = OnChildrenReordered;

			_renderer = renderer;
			_renderer.ElementChanged += (sender, args) => SetElement(args.OldElement, args.NewElement);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (_renderer != null)
			{
				if (_childViews != null)
				{
					_childViews.Clear();
					_childViews = null;
				}

				if (_childPackagers != null)
				{
					foreach (var kvp in _childPackagers)
						kvp.Value.Dispose();

					_childPackagers.Clear();
					_childPackagers = null;
				}

				if (_renderer.Element != null)
				{
					_renderer.Element.ChildAdded -= _childAddedHandler;
					_renderer.Element.ChildRemoved -= _childRemovedHandler;

					_renderer.Element.ChildrenReordered -= _childReorderedHandler;
				}
				_renderer = null;
			}

			_element = null;
		}

		public void Load()
		{
			SetElement(null, _element);
		}

		void AddChild(VisualElement view, IVisualElementRenderer oldRenderer = null, RendererPool pool = null, bool sameChildren = false)
		{
			Performance.Start(out string reference);

			if (CompressedLayout.GetIsHeadless(view))
			{
				var packager = new VisualElementPackager(_renderer, view);
				if (_childPackagers == null)
					_childPackagers = new Dictionary<BindableObject, VisualElementPackager>();
				view.IsPlatformEnabled = true;
				packager.Load();

				_childPackagers[view] = packager;
			}
			else
			{
				if (_childViews == null)
					_childViews = new List<IVisualElementRenderer>();

				IVisualElementRenderer renderer = oldRenderer;
				if (pool != null)
					renderer = pool.GetFreeRenderer(view);
				if (renderer == null || (renderer.View?.Handle ?? IntPtr.Zero) == IntPtr.Zero)
				{
					Performance.Start(reference, "New renderer");
					renderer = Platform.CreateRenderer(view, _renderer.View.Context);
					Performance.Stop(reference, "New renderer");
				}

				if (renderer == oldRenderer)
				{
					renderer.Element?.ClearValue(Platform.RendererProperty);
					renderer.SetElement(view);
				}

				Performance.Start(reference, "Set renderer");
				Platform.SetRenderer(view, renderer);
				Performance.Stop(reference, "Set renderer");

				Performance.Start(reference, "Add view");
				if (!sameChildren)
				{
					(_renderer.View as ViewGroup)?.AddView(renderer.View);
					_childViews.Add(renderer);
				}
				Performance.Stop(reference, "Add view");

				Performance.Stop(reference);
			}
		}
		void EnsureChildOrder()
		{
			for (var i = 0; i < ElementController.LogicalChildren.Count; i++)
			{
				Element child = ElementController.LogicalChildren[i];
				var element = (VisualElement)child;
				if (element != null)
				{
					IVisualElementRenderer r = Platform.GetRenderer(element);
					if (r != null)
						(_renderer.View as ViewGroup)?.BringChildToFront(r.View);
				}
			}
		}

		void OnChildAdded(object sender, ElementEventArgs e)
		{
			var view = e.Element as VisualElement;
			if (view != null)
				AddChild(view);

			if (ElementController.LogicalChildren.LastOrDefault() != view)
				EnsureChildOrder();
		}

		void OnChildRemoved(object sender, ElementEventArgs e)
		{
			Performance.Start(out string reference);
			var view = e.Element as VisualElement;
			if (view != null)
				RemoveChild(view);

			Performance.Stop(reference);
		}

		void OnChildrenReordered(object sender, EventArgs e)
		{
			EnsureChildOrder();
		}

		void RemoveChild(VisualElement view)
		{
			IVisualElementRenderer renderer = Platform.GetRenderer(view);
			if (renderer == null) // child is itself a compressed layout
			{
				if (_childPackagers != null && _childPackagers.TryGetValue (view, out VisualElementPackager packager))
				{
					foreach (var child in view.LogicalChildren)
					{
						if (child is VisualElement ve)
							packager.RemoveChild(ve);
					}
				}
			}
			else
			{
				_childViews.Remove(renderer);
				renderer.View.RemoveFromParent();
				renderer.Dispose();
			}
		}

		void SetElement(VisualElement oldElement, VisualElement newElement)
		{
			Performance.Start(out string reference);

			var sameChildrenTypes = false;

			ReadOnlyCollection<Element> newChildren = null, oldChildren = null;

			RendererPool pool = null;
			if (oldElement != null)
			{
				if (newElement != null)
				{
					sameChildrenTypes = true;

					oldChildren = ((IElementController)oldElement).LogicalChildren;
					newChildren = ((IElementController)newElement).LogicalChildren;
					if (oldChildren.Count == newChildren.Count)
					{
						for (var i = 0; i < oldChildren.Count; i++)
						{
							if (oldChildren[i].GetType() != newChildren[i].GetType())
							{
								sameChildrenTypes = false;
								break;
							}
						}
					}
					else
						sameChildrenTypes = false;
				}

				oldElement.ChildAdded -= _childAddedHandler;
				oldElement.ChildRemoved -= _childRemovedHandler;

				oldElement.ChildrenReordered -= _childReorderedHandler;

				if (!sameChildrenTypes)
				{
					_childViews = new List<IVisualElementRenderer>();
					pool = new RendererPool(_renderer, oldElement);
					pool.ClearChildrenRenderers();
				}
			}

			if (newElement != null)
			{
				Performance.Start(reference, "Setup");
				newElement.ChildAdded += _childAddedHandler;
				newElement.ChildRemoved += _childRemovedHandler;

				newElement.ChildrenReordered += _childReorderedHandler;

				newChildren = newChildren ?? ((IElementController)newElement).LogicalChildren;

				for (var i = 0; i < newChildren.Count; i++)
				{
					IVisualElementRenderer oldRenderer = null;
					if (oldChildren != null && sameChildrenTypes)
						oldRenderer = _childViews[i];

					AddChild((VisualElement)newChildren[i], oldRenderer, pool, sameChildrenTypes);
				}

#if DEBUG
				//if (renderer.Element.LogicalChildren.Any() && renderer.ViewGroup.ChildCount != renderer.Element.LogicalChildren.Count)
				//	throw new InvalidOperationException ("SetElement did not create the correct number of children");
#endif
				EnsureChildOrder();
				Performance.Stop(reference, "Setup");
			}

			Performance.Stop(reference);
		}
	}
}

