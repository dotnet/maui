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
		event EventHandler<FoldableHingeAngleChangedEventArgs> HingeAngleChanged;
		bool IsSpanned { get; }
		bool IsLandscape { get; }
		Rect GetHinge();
		Rect GetHinge(VisualElement visualElement);
		bool IsLandscapeFor(VisualElement visualElement);
		Size GetScaledScreenSize(VisualElement visualElement);
		Size ScaledScreenSize { get; }
		Point? GetLocationOnScreen(VisualElement visualElement);
		Task<int> GetHingeAngleAsync();
		void StartMonitoring(VisualElement visualElement);
		void StopMonitoring(VisualElement visualElement);

		event EventHandler<FoldEventArgs> OnLayoutChanged;
	}

	internal sealed class FoldableHingeAngleChangedEventArgs : EventArgs
	{
		public FoldableHingeAngleChangedEventArgs(double hingeAngleInDegrees)
		{
			HingeAngleInDegrees = hingeAngleInDegrees;
		}

		public double HingeAngleInDegrees { get; }
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
