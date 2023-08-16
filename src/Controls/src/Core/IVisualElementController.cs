#nullable disable
using System;
using Microsoft.Maui.Controls.Internals;
using static Microsoft.Maui.Controls.VisualElement;

namespace Microsoft.Maui.Controls
{
	/// <summary>For internal use by .NET MAUI.</summary>
	public interface IVisualElementController : IElementController
	{
		/// <summary>For internal use by .NET MAUI.</summary>
		void PlatformSizeChanged();

		/// <summary>For internal use by .NET MAUI.</summary>
		void InvalidateMeasure(InvalidationTrigger trigger);

		/// <summary>For internal use by .NET MAUI.</summary>
		bool Batched { get; }

		/// <summary>For internal use by .NET MAUI.</summary>
		bool DisableLayout { get; set; }

		/// <summary>For internal use by .NET MAUI.</summary>
		EffectiveFlowDirection EffectiveFlowDirection { get; }

		/// <summary>For internal use by .NET MAUI.</summary>
		bool IsInPlatformLayout { get; set; }

		/// <summary>For internal use by .NET MAUI.</summary>
		bool IsPlatformStateConsistent { get; set; }

		/// <summary>For internal use by .NET MAUI.</summary>
		bool IsPlatformEnabled { get; set; }

		/// <summary>For internal use by .NET MAUI.</summary>
		NavigationProxy NavigationProxy { get; }

		/// <summary>For internal use by .NET MAUI.</summary>
		event EventHandler<EventArg<VisualElement>> BatchCommitted;

		/// <summary>For internal use by .NET MAUI.</summary>
		event EventHandler<FocusRequestArgs> FocusChangeRequested;
	}
}