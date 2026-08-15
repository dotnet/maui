using System;
using CoreGraphics;
using UIKit;

namespace Microsoft.Maui.Platform;

internal readonly record struct SafeAreaPadding(double Left, double Right, double Top, double Bottom)
{
	public static SafeAreaPadding Empty { get; } = new(0, 0, 0, 0);

	public bool IsEmpty { get; } = Left == 0 && Right == 0 && Top == 0 && Bottom == 0;
	public double HorizontalThickness { get; } = Left + Right;
	public double VerticalThickness { get; } = Top + Bottom;

	public CGRect InsetRect(CGRect bounds)
	{
		if (IsEmpty)
		{
			return bounds;
		}

		return new CGRect(
			bounds.Left + Left,
			bounds.Top + Top,
			bounds.Width - HorizontalThickness,
			bounds.Height - VerticalThickness);
	}

	public CGRect ToCGRect() =>
		new((nfloat)Top, (nfloat)Left, (nfloat)Bottom, (nfloat)Right);

	/// <summary>
	/// Compares two SafeAreaPadding values at device-pixel resolution.
	/// Sub-pixel differences (e.g., 0.001pt from animation noise) that map to the same
	/// physical pixel are treated as equal, preventing unnecessary layout invalidation cycles.
	/// </summary>
	public bool EqualsAtPixelLevel(SafeAreaPadding other)
	{
		var scale = (double)UIScreen.MainScreen.Scale;
		return RoundToPixel(Left, scale) == RoundToPixel(other.Left, scale)
			&& RoundToPixel(Right, scale) == RoundToPixel(other.Right, scale)
			&& RoundToPixel(Top, scale) == RoundToPixel(other.Top, scale)
			&& RoundToPixel(Bottom, scale) == RoundToPixel(other.Bottom, scale);
	}

	static double RoundToPixel(double value, double scale)
		=> Math.Round(value * scale, MidpointRounding.AwayFromZero);
}

internal static class SafeAreaInsetsExtensions
{
	public static SafeAreaPadding ToSafeAreaInsets(this UIEdgeInsets insets)
	{
		// Filters out negligible floating-point values from UIKit that may cause layout issues (e.g., 3.5527136788005009e-15).
		const double tolerance = 1e-14;

		static double ApplyTolerance(double value) => Math.Abs(value) < tolerance ? 0 : value;

		return new(
			ApplyTolerance(insets.Left),
			ApplyTolerance(insets.Right),
			ApplyTolerance(insets.Top),
			ApplyTolerance(insets.Bottom)
		);
	}

	/// <summary>
	/// Returns which edges (0=Left, 1=Top, 2=Right, 3=Bottom) are already handled by a parent
	/// <see cref="MauiView"/> with a real, non-zero resolved inset, performing a single ancestor
	/// walk only when <paramref name="blockedEdgesCacheValid"/> is false. Shared by
	/// <see cref="MauiView"/> and <see cref="MauiScrollView"/> so both get identical per-edge
	/// (rather than all-or-nothing) parent-blocking behavior — a parent that only handles Top
	/// must not also suppress a descendant's independent Bottom inset, and vice versa (#34563).
	///
	/// The result is written into the caller-owned <paramref name="blockedEdges"/> array and
	/// reused across layout passes via <paramref name="blockedEdgesCacheValid"/> until the
	/// caller invalidates it (e.g. on SafeAreaInsetsDidChange/InvalidateSafeArea/MovedToWindow),
	/// so this walk only runs once per invalidation cycle instead of on every layout pass.
	/// </summary>
	/// <param name="startingView">The view whose ancestors should be walked.</param>
	/// <param name="blockedEdges">A caller-owned, length-4 array to populate in place.</param>
	/// <param name="blockedEdgesCacheValid">
	/// Whether <paramref name="blockedEdges"/> already holds a valid result. Set to true before
	/// returning.
	/// </param>
	internal static bool[] ResolveParentBlockedEdges(this UIView startingView, bool[] blockedEdges, ref bool blockedEdgesCacheValid)
	{
		if (blockedEdgesCacheValid)
			return blockedEdges;

		Array.Clear(blockedEdges, 0, blockedEdges.Length);
		int resolvedCount = 0;

		startingView.FindParent(x =>
		{
			if (x is not MauiView mv || !mv.RespondsToSafeArea())
				return false;

			for (int edge = 0; edge < 4; edge++)
			{
				if (!blockedEdges[edge] &&
					mv.GetSafeAreaRegionForEdge(edge) != SafeAreaRegions.None &&
					mv.GetSafeAreaComponentForEdge(edge) != 0)
				{
					blockedEdges[edge] = true;
					resolvedCount++;
				}
			}

			// Stop walking once all 4 edges are resolved
			return resolvedCount == 4;
		});

		blockedEdgesCacheValid = true;
		return blockedEdges;
	}
}