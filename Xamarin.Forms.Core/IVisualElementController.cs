using System;
using Xamarin.Forms.Internals;
using static Xamarin.Forms.VisualElement;

namespace Xamarin.Forms
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