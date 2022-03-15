using System.Collections.Generic;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class WindowStub : IWindow, IVisualTreeElement
	{
		public WindowStub()
		{

		}

		HashSet<IWindowOverlay> _overlays = new HashSet<IWindowOverlay>();
		List<IVisualTreeElement> _visualChildren = new List<IVisualTreeElement>();

		public IElementHandler Handler { get; set; }

		public IElement Parent { get; set; }

		public IView Content { get; set; }

		public IVisualDiagnosticsOverlay VisualDiagnosticsOverlay { get; }

		public System.Collections.Generic.IReadOnlyCollection<IWindowOverlay> Overlays { get; }

		public string Title { get; set; }
		public bool IsCreated { get; set; }
		public bool IsActivated { get; set; }
		public bool IsDeactivated { get; set; }
		public bool IsDestroyed { get; set; }
		public bool IsResumed { get; set; }
		public bool IsStopped { get; set; }
		public FlowDirection FlowDirection { get; set; }

		public void Activated()
		{
			IsActivated = true;
			IsDeactivated = false;
		}

		public void Created()
		{
			IsCreated = true;
		}

		public void Deactivated()
		{
			IsActivated = false;
			IsDeactivated = false;
		}

		public void Destroying()
		{
			IsDestroyed = true;
			IsCreated = false;
		}

		public void Resumed()
		{
			IsResumed = true;
			IsStopped = false;
		}

		public void Stopped()
		{
			IsStopped = true;
			IsResumed = false;
		}

		public void Backgrounding(IPersistedState state)
		{
		}

		public bool AddOverlay(IWindowOverlay overlay)
		{
			if (overlay is IVisualDiagnosticsOverlay)
				return false;

			// Add the overlay. If it's added, 
			// Initalize the native layer if it wasn't already,
			// and call invalidate so it will be drawn.
			var result = _overlays.Add(overlay);
			if (result)
			{
				overlay.Initialize();
				overlay.Invalidate();
			}

			return result;
		}

		public bool RemoveOverlay(IWindowOverlay overlay)
		{
			if (overlay is IVisualDiagnosticsOverlay)
				return false;

			var result = _overlays.Remove(overlay);
			if (result)
				overlay.Deinitialize();

			return result;
		}

		public bool BackButtonClicked() => true;

		public IReadOnlyList<IVisualTreeElement> GetVisualChildren() => _visualChildren.AsReadOnly();

		public IVisualTreeElement GetVisualParent() => this.Parent as IVisualTreeElement;

		public float RequestDisplayDensity() => 1.0f;

		public virtual void DisplayDensityChanged(float displayDensity) { }
	}
}