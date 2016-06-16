using System;
using System.Windows;
using System.Windows.Controls;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class VisualElementPackager
	{
		readonly Panel _panel;
		readonly IVisualElementRenderer _renderer;
		bool _loaded;

		public VisualElementPackager(IVisualElementRenderer renderer)
		{
			if (renderer == null)
				throw new ArgumentNullException("renderer");

			_panel = renderer.ContainerElement as Panel;
			if (_panel == null)
				throw new ArgumentException("Renderer's container element must be a Panel or Panel subclass");

			_renderer = renderer;
		}

		IElementController ElementController => _renderer.Element as IElementController;

		public void Load()
		{
			if (_loaded)
				return;

			_loaded = true;
			_renderer.Element.ChildAdded += HandleChildAdded;
			_renderer.Element.ChildRemoved += HandleChildRemoved;
			_renderer.Element.ChildrenReordered += HandleChildrenReordered;

			foreach (Element child in ElementController.LogicalChildren)
				HandleChildAdded(_renderer.Element, new ElementEventArgs(child));
		}

		void EnsureZIndex()
		{
			for (var index = 0; index < ElementController.LogicalChildren.Count; index++)
			{
				var child = (VisualElement)ElementController.LogicalChildren[index];
				IVisualElementRenderer r = Platform.GetRenderer(child);
				if (r == null)
					continue;
				// Even though this attached property is defined on Canvas, it actually works on all Panels
				// Why? Microsoft.
				Canvas.SetZIndex(r.ContainerElement, index + 1);
			}
		}

		void HandleChildAdded(object sender, ElementEventArgs e)
		{
			var view = e.Element as VisualElement;

			if (view == null)
				return;

			IVisualElementRenderer renderer;
			Platform.SetRenderer(view, renderer = Platform.CreateRenderer(view));

			_panel.Children.Add(renderer.ContainerElement);

			EnsureZIndex();
		}

		void HandleChildRemoved(object sender, ElementEventArgs e)
		{
			var view = e.Element as VisualElement;

			if (view == null)
				return;

			var renderer = Platform.GetRenderer(view) as UIElement;

			if (renderer != null)
				_panel.Children.Remove(renderer);

			EnsureZIndex();
		}

		void HandleChildrenReordered(object sender, EventArgs e)
		{
			EnsureZIndex();
		}
	}
}