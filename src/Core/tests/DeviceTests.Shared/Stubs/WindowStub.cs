using System.Collections.Generic;
using Microsoft.Maui.Graphics;

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

		public void FrameChanged(Rect frame)
		{
			X = frame.X;
			Y = frame.Y;
			Width = frame.Width;
			Height = frame.Height;
		}

		public double X { get; set; } = double.NaN;

		public double Y { get; set; } = double.NaN;

		public double Width { get; set; } = double.NaN;

		public double Height { get; set; } = double.NaN;

		public double MinimumWidth { get; set; } = double.NaN;

		public double MaximumWidth { get; set; } = double.NaN;

		public double MinimumHeight { get; set; } = double.NaN;

		public double MaximumHeight { get; set; } = double.NaN;

		public virtual void Activated()
		{
			IsActivated = true;
			IsDeactivated = false;
		}

		public virtual void Created()
		{
			IsCreated = true;
		}

		public virtual void Deactivated()
		{
			IsActivated = false;
			IsDeactivated = false;
		}

		public virtual void Destroying()
		{
			IsDestroyed = true;
			IsCreated = false;
		}

		public virtual void Resumed()
		{
			IsResumed = true;
			IsStopped = false;
		}

		public virtual void Stopped()
		{
			IsStopped = true;
			IsResumed = false;
		}

		public virtual void Backgrounding(IPersistedState state)
		{
		}

		public virtual bool AddOverlay(IWindowOverlay overlay)
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

		public virtual bool RemoveOverlay(IWindowOverlay overlay)
		{
			if (overlay is IVisualDiagnosticsOverlay)
				return false;

			var result = _overlays.Remove(overlay);
			if (result)
				overlay.Deinitialize();

			return result;
		}

		public virtual bool BackButtonClicked() => true;

		public IReadOnlyList<IVisualTreeElement> GetVisualChildren() => _visualChildren.AsReadOnly();

		public IVisualTreeElement GetVisualParent() => this.Parent as IVisualTreeElement;

		public virtual float RequestDisplayDensity() => 1.0f;

		public virtual void DisplayDensityChanged(float displayDensity) { }
	}
}