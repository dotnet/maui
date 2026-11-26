#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <summary>A native resting position of a bottom sheet and the detents that resolve to it.</summary>
	internal sealed class BottomSheetStop
	{
		public BottomSheetStop(double height, BottomSheetDetent detent)
		{
			Height = height;
			Detents.Add(detent);
		}

		/// <summary>The height of the sheet at this position, in the unit the resolver was given.</summary>
		public double Height { get; }

		/// <summary>The detents that resolve to this position, in declaration order.</summary>
		public List<BottomSheetDetent> Detents { get; } = new List<BottomSheetDetent>(1);

		/// <summary>The detent to report for this position: <paramref name="current"/> when it rests here, otherwise the first declared one.</summary>
		public BottomSheetDetent ReportedDetent(BottomSheetDetent? current) =>
			current is not null && Detents.Contains(current) ? current : Detents[0];
	}

	/// <summary>Resolves bottom sheet detents to the ordered, distinct resting positions the native sheets need.</summary>
	internal static class BottomSheetDetentResolver
	{
		internal const double MediumFraction = 0.5;

		/// <summary>Resolves the height of one detent, limited to <paramref name="available"/>.</summary>
		public static double ResolveHeight(BottomSheetDetent detent, double available, Func<double>? measureContent)
		{
			if (available <= 0 || double.IsNaN(available) || double.IsInfinity(available))
				return 0;

			double height = detent.Kind switch
			{
				BottomSheetDetentKind.Medium => available * MediumFraction,
				BottomSheetDetentKind.Large => available,
				BottomSheetDetentKind.Fraction => available * detent.Value,
				BottomSheetDetentKind.Height => detent.Value,
				BottomSheetDetentKind.FitContent => measureContent?.Invoke() ?? available,
				_ => available,
			};

			if (double.IsNaN(height))
				return available;

			// Content can measure to nothing, for example before it loads; the sheet still needs a height.
			return Math.Min(Math.Max(height, 1), available);
		}

		/// <summary>
		/// Resolves <paramref name="detents"/> to resting positions ordered from smallest to largest. Detents whose heights are within
		/// <paramref name="tolerance"/> of each other share one position, so a native sheet never gets two positions it can't tell apart.
		/// </summary>
		public static List<BottomSheetStop> Resolve(IReadOnlyList<BottomSheetDetent> detents, double available, Func<double>? measureContent, double tolerance)
		{
			var resolved = new List<(BottomSheetDetent Detent, double Height, int Order)>(detents.Count);
			double? contentHeight = null;
			Func<double>? measure = measureContent is null ? null : () => contentHeight ??= measureContent();

			for (int i = 0; i < detents.Count; i++)
			{
				var detent = detents[i];
				if (detent is null || ContainsDetent(resolved, detent))
					continue;

				resolved.Add((detent, ResolveHeight(detent, available, measure), i));
			}

			resolved.Sort((a, b) =>
			{
				int byHeight = a.Height.CompareTo(b.Height);
				return byHeight != 0 ? byHeight : a.Order.CompareTo(b.Order);
			});

			var stops = new List<BottomSheetStop>(resolved.Count);
			foreach (var (detent, height, _) in resolved)
			{
				var last = stops.Count > 0 ? stops[stops.Count - 1] : null;
				if (last is not null && height - last.Height <= tolerance)
					last.Detents.Add(detent);
				else
					stops.Add(new BottomSheetStop(height, detent));
			}

			return stops;
		}

		/// <summary>
		/// Measures the height the content of <paramref name="page"/> needs at <paramref name="width"/>, including its padding.
		/// Only a <see cref="ContentPage"/> has a content height; other pages fill the available height.
		/// </summary>
		public static double MeasureContentHeight(Page page, double width)
		{
			if (page is ContentPage contentPage && width > 0)
				return ((IContentView)contentPage).MeasureContent(width, double.PositiveInfinity).Height;

			return double.PositiveInfinity;
		}

		/// <summary>
		/// Returns the index of the position <paramref name="selected"/> rests at: the smallest when it is <see langword="null"/>,
		/// and -1 when it is not a detent of the sheet.
		/// </summary>
		public static int IndexOf(IReadOnlyList<BottomSheetStop> stops, BottomSheetDetent? selected)
		{
			if (selected is null)
				return 0;

			for (int i = 0; i < stops.Count; i++)
			{
				if (stops[i].Detents.Contains(selected))
					return i;
			}

			return -1;
		}

		static bool ContainsDetent(List<(BottomSheetDetent Detent, double Height, int Order)> resolved, BottomSheetDetent detent)
		{
			foreach (var item in resolved)
			{
				if (item.Detent.Equals(detent))
					return true;
			}

			return false;
		}
	}
}
