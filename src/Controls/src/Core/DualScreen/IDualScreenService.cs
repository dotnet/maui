using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.DualScreen
{
	internal interface IDualScreenService
	{
		event EventHandler OnScreenChanged;
		bool IsSpanned { get; }
		bool IsLandscape { get; }
		Rectangle GetHinge();
		Size ScaledScreenSize { get; }
		Point? GetLocationOnScreen(VisualElement visualElement);
		object WatchForChangesOnLayout(VisualElement visualElement, Action action);
		void StopWatchingForChangesOnLayout(VisualElement visualElement, object handle);
		Task<int> GetHingeAngleAsync();

		event EventHandler<FoldEventArgs> OnLayoutChanged;
	}

	public class FoldEventArgs : System.EventArgs
	{
		public bool isSeparating { get; set; }
		public Rectangle FoldingFeatureBounds { get; set; }
		public Rectangle WindowBounds { get; set; }
		public override string ToString()
		{
			return $"FoldEventArgs:: isSeparating: {isSeparating} FoldingFeatureBounds: {FoldingFeatureBounds} WindowBounds: {WindowBounds}";
		}
	}
}
