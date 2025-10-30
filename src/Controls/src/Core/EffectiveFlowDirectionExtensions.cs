#nullable disable
using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	internal static class EffectiveFlowDirectionExtensions
	{
		internal static EffectiveFlowDirection ToEffectiveFlowDirection(this FlowDirection self, bool isExplicit = false)
		{
			switch (self)
			{
				case FlowDirection.MatchParent:
					return default(EffectiveFlowDirection);


				case FlowDirection.LeftToRight:
					if (isExplicit)
					{
						return EffectiveFlowDirection.Explicit;
					}
					else
					{
						return default(EffectiveFlowDirection);
					}

				case FlowDirection.RightToLeft:
					if (isExplicit)
					{
						return EffectiveFlowDirection.RightToLeft | EffectiveFlowDirection.Explicit;
					}
					else
					{
						return EffectiveFlowDirection.RightToLeft;
					}

				default:
					throw new InvalidOperationException($"Cannot convert {self} to {nameof(EffectiveFlowDirection)}.");
			}
		}

		internal static FlowDirection ToFlowDirection(this EffectiveFlowDirection self)
		{
			if (self.IsLeftToRight())
				return FlowDirection.LeftToRight;
			else
				return FlowDirection.RightToLeft;

			throw new InvalidOperationException($"Cannot convert {self} to {nameof(FlowDirection)}.");
		}

		/// <summary>Returns <see langword="true"/> if the flow direction is right-to-left. Otherwise, returns <see langword="false"/>.</summary>
		/// <param name="self">The enumeration value on which this method operates.</param>
		/// <returns><see langword="true"/> if the flow direction is right-to-left. Otherwise, <see langword="false"/>.</returns>
		public static bool IsRightToLeft(this EffectiveFlowDirection self)
		{
			return (self & EffectiveFlowDirection.RightToLeft) == EffectiveFlowDirection.RightToLeft;
		}

		/// <summary>Returns <see langword="true"/> if the flow direction is left-to-right. Otherwise, returns <see langword="false"/>.</summary>
		/// <param name="self">The enumeration value on which this method operates.</param>
		/// <returns><see langword="true"/> if the flow direction is left-to-right. Otherwise, <see langword="false"/>.</returns>
		public static bool IsLeftToRight(this EffectiveFlowDirection self)
		{
			return (self & EffectiveFlowDirection.RightToLeft) != EffectiveFlowDirection.RightToLeft;
		}

		/// <summary>Returns <see langword="false"/> if the developer set the flow direction on the current object. Returns <see langword="true"/> if the flow direction is inherited or was not set by the developer.</summary>
		/// <param name="self">The enumeration value on which this method operates.</param>
		/// <returns><see langword="false"/> if the developer set the flow direction on the current object, or <see langword="true"/> if the flow direction is inherited or was not set by the developer.</returns>
		public static bool IsImplicit(this EffectiveFlowDirection self)
		{
			return (self & EffectiveFlowDirection.Explicit) != EffectiveFlowDirection.Explicit;
		}

		/// <summary>Returns <see langword="true"/> if the developer set the flow direction on the current object. Returns <see langword="false"/> if the flow direction is inherited or was not set by the developer.</summary>
		/// <param name="self">The enumeration value on which this method operates.</param>
		/// <returns><see langword="true"/> if the developer set the flow direction on the current object, or <see langword="false"/> if the flow direction is inherited or was not set by the developer.</returns>
		public static bool IsExplicit(this EffectiveFlowDirection self)
		{
			return (self & EffectiveFlowDirection.Explicit) == EffectiveFlowDirection.Explicit;
		}
	}
}