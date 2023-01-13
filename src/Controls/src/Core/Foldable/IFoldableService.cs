#nullable disable
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Foldable
{
	internal interface IFoldableService
	{
		event EventHandler OnScreenChanged;
		bool IsSpanned { get; }
		bool IsLandscape { get; }
		Rect GetHinge();
		Size ScaledScreenSize { get; }
		Point? GetLocationOnScreen(VisualElement visualElement);
		Task<int> GetHingeAngleAsync();

		event EventHandler<FoldEventArgs> OnLayoutChanged;
	}

	public class FoldEventArgs : System.EventArgs
	{
		public bool isSeparating { get; set; }
		public Rect FoldingFeatureBounds { get; set; }
		public Rect WindowBounds { get; set; }
		public override string ToString()
		{
			return $"FoldEventArgs:: isSeparating: {isSeparating} FoldingFeatureBounds: {FoldingFeatureBounds} WindowBounds: {WindowBounds}";
		}
	}
}
