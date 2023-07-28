#nullable disable
using System;
using Microsoft.Maui.Controls.Internals;
using static Microsoft.Maui.Controls.VisualElement;

namespace Microsoft.Maui.Controls
{
	/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
	public interface IVisualElementController : IElementController
	{
		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		void PlatformSizeChanged();

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		void InvalidateMeasure(InvalidationTrigger trigger);

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		bool Batched { get; }

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		bool DisableLayout { get; set; }

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		EffectiveFlowDirection EffectiveFlowDirection { get; }

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		bool IsInPlatformLayout { get; set; }

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		bool IsPlatformStateConsistent { get; set; }

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		bool IsPlatformEnabled { get; set; }

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		NavigationProxy NavigationProxy { get; }

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		event EventHandler<EventArg<VisualElement>> BatchCommitted;

		/// <summary>For internal use by the <see cref="Controls"/> platform.</summary>
		event EventHandler<FocusRequestArgs> FocusChangeRequested;
	}
}