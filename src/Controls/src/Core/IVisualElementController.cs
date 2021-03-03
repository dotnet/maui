using System;
using Microsoft.Maui.Controls.Internals;
using static Microsoft.Maui.Controls.VisualElement;

namespace Microsoft.Maui.Controls
{
	public interface IVisualElementController : IElementController
	{
		void NativeSizeChanged();
		void InvalidateMeasure(InvalidationTrigger trigger);
		bool Batched { get; }
		bool DisableLayout { get; set; }
		EffectiveFlowDirection EffectiveFlowDirection { get; }
		bool IsInNativeLayout { get; set; }
		bool IsNativeStateConsistent { get; set; }
		bool IsPlatformEnabled { get; set; }
		NavigationProxy NavigationProxy { get; }
		event EventHandler<EventArg<VisualElement>> BatchCommitted;
		event EventHandler<FocusRequestArgs> FocusChangeRequested;
	}
}