using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Android.Views;
using Xamarin.Forms.Internals;
using AView = Android.Views.View;

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
			_renderer.ElementChanged += OnElementChanged;

			if (renderer.View is ILayoutChanges layout)
				layout.LayoutChange += OnInitialLayoutChange;
		}

		void OnInitialLayoutChange(object sender, AView.LayoutChangeEventArgs e)
		{
			// this is used to adjust any relative elevations on the child elements that still need to settle
			// the default elevation is set on Button after it's already added to the view hierarchy
			// but this appears to only be the case when the app first starts

			if (sender is ILayoutChanges layout)
				layout.LayoutChange -= OnInitialLayoutChange;

			EnsureChildOrder(true);
		}

		void OnElementChanged(object sender, VisualElementChangedEventArgs e)
			=> SetElement(e.OldElement, e.NewElement);

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

			if (disposing)
			{
				if (_renderer != null)
				{
					_renderer.ElementChanged -= OnElementChanged;

					if (_renderer.Element != null)
					{
						_renderer.Element.ChildAdded -= _childAddedHandler;
						_renderer.Element.ChildRemoved -= _childRemovedHandler;
						_renderer.Element.ChildrenReordered -= _childReorderedHandler;
					}

					if (_renderer.View is ILayoutChanges layout)
						layout.LayoutChange -= OnInitialLayoutChange;

					SetElement(_element, null);

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

					_renderer = null;
				}
			}
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
			}

			Performance.Stop(reference);
		}

		void EnsureChildOrder() => EnsureChildOrder(false);

		void EnsureChildOrder(bool onlyUpdateElevations)
		{
			float elevationToSet = 0;
			for (var i = 0; i < ElementController.LogicalChildren.Count; i++)
			{
				Element child = ElementController.LogicalChildren[i];
				var element = (VisualElement)child;
				if (element != null)
				{
					IVisualElementRenderer r = Platform.GetRenderer(element);
					if (r != null)
					{
						if (Forms.IsLollipopOrNewer)
						{
							var elevation = ElevationHelper.GetElevation(r.View) ?? 0;
							var elementElevation = ElevationHelper.GetElevation(element, r.View.Context);

							if (elementElevation == null)
							{
								if (elevation > elevationToSet)
									elevationToSet = elevation;

								if (r.View.Elevation != elevationToSet)
									r.View.Elevation = elevationToSet;
							}
						}

						if (!onlyUpdateElevations)
							(_renderer.View as ViewGroup)?.BringChildToFront(r.View);
					}
				}
			}
		}

		void OnChildAdded(object sender, ElementEventArgs e)
		{
			var view = e.Element as VisualElement;
			if (view != null)
				AddChild(view);

			int itemCount = ElementController.LogicalChildren.Count;
			if (itemCount <= 1)
				return;

			Element lastChild = ElementController.LogicalChildren[itemCount - 1];

			if (lastChild != view)
			{
				EnsureChildOrder();
				return;
			}

			if (!Forms.IsLollipopOrNewer)
				return;

			Element previousChild = ElementController.LogicalChildren[itemCount - 2];

			IVisualElementRenderer lastRenderer = null;
			IVisualElementRenderer previousRenderer = null;

			if (lastChild is VisualElement last)
				lastRenderer = Platform.GetRenderer(last);

			if (previousChild is VisualElement previous)
				previousRenderer = Platform.GetRenderer(previous);

			if (ElevationHelper.GetElevation(lastRenderer?.View) < ElevationHelper.GetElevation(previousRenderer?.View))
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
				if (_childPackagers != null && _childPackagers.TryGetValue(view, out VisualElementPackager packager))
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

				if (renderer.View.IsAlive())
				{
					renderer.View.RemoveFromParent();
				}

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
				oldElement.ChildAdded -= _childAddedHandler;
				oldElement.ChildRemoved -= _childRemovedHandler;
				oldElement.ChildrenReordered -= _childReorderedHandler;

				oldChildren = ((IElementController)oldElement).LogicalChildren;

				if (newElement != null)
				{
					sameChildrenTypes = true;

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
				else
				{
					for (var i = 0; i < oldChildren.Count; i++)
					{
						RemoveChild((VisualElement)oldChildren[i]);
					}
				}

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
					if (oldChildren != null && sameChildrenTypes && _childViews != null)
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