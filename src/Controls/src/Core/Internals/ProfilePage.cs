#nullable disable
#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>Stores profiling data for a single measured operation.</summary>
	public class ProfileDatum
	{
		/// <summary>The name of the profiled operation.</summary>
		public string Name;
		/// <summary>Optional identifier for the operation.</summary>
		public string Id;
		/// <summary>Total ticks for this operation.</summary>
		public long Ticks;
		/// <summary>Ticks spent in sub-operations.</summary>
		public long SubTicks;
		/// <summary>Nesting depth of this operation.</summary>
		public int Depth;
		/// <summary>Source file path.</summary>
		public string Path;
		/// <summary>Source line number.</summary>
		public int Line;
	}

	/// <summary>Extension methods for displaying profiling data on a ContentPage.</summary>
	public static class ContentPageEx
	{
		const long MS = TimeSpan.TicksPerMillisecond;
		/// <summary>The collected profile data.</summary>
		public static List<ProfileDatum> Data = new List<ProfileDatum>();

		private static void ASSERT(bool condition)
		{
			if (!condition)
				throw new Exception("assert");
		}

		private static long GatherTicks(ref int i)
		{
			var current = Data[i++];
			var depth = current.Depth;
			var total = current.Ticks;

			while (i < Data.Count)
			{
				var next = Data[i];
				if (next.Depth < depth)
					break;

				if (next.Depth > depth)
				{
					current.SubTicks = GatherTicks(ref i);
					ASSERT(current.Ticks >= current.SubTicks);
					continue;
				}

				i++;
				current = next;
				total += current.Ticks;
			}

			return total;
		}

		private static void AppendProfile(StringBuilder sb, long profiledMs, bool showZeros)
		{
			foreach (var datum in Data)
			{
				var depth = datum.Depth;

				var name = datum.Name;
				if (datum.Id != null)
					name += $" ({datum.Id})";

				var ticksMs = datum.Ticks / MS;
				var exclusiveTicksMs = (datum.Ticks - datum.SubTicks) / MS;

				var percentage = (int)Math.Round(ticksMs / (double)profiledMs * 100);
				if (!showZeros && percentage == 0)
					continue;

				var line = $"{name} = {ticksMs}ms";
				if (exclusiveTicksMs != ticksMs)
					line += $" ({exclusiveTicksMs}ms)";
				line += $", {percentage}%";

				sb.AppendLine("".PadLeft(depth * 2) + line);
			}
		}

		/// <param name="page">The page parameter.</param>
		public static void LoadProfile(this ContentPage page)
		{
			Profile.Stop();
			foreach (var datum in Profile.Data)
			{
				Data.Add(new ProfileDatum()
				{
					Name = datum.Name,
					Id = datum.Id,
					Depth = datum.Depth,
					Ticks = datum.Ticks < 0 ? 0L : (long)datum.Ticks
				});
			}
			var i = 0;
			var profiledMs = GatherTicks(ref i) / MS;

			var scrollView = new ScrollView();
			var label = new Label();

			var controls = new Grid();
			var buttonA = new Button() { Text = "0s", HeightRequest = 62 };
			controls.Add(buttonA);

			var grid = new Grid
			{
				RowDefinitions = new RowDefinitionCollection {
				new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
				new RowDefinition { Height = GridLength.Auto },
			},
				ColumnDefinitions = new ColumnDefinitionCollection {
				new ColumnDefinition { Width = GridLength.Star },
			}
			};
			page.Content = grid;
			grid.Add(scrollView, 0, 0);
			grid.Add(controls, 0, 1);

			scrollView.Content = label;

			var showZeros = false;

			Action update = delegate ()
			{
				var sb = new StringBuilder();
				sb.AppendLine($"Profiled: {profiledMs}ms");
				AppendProfile(sb, profiledMs, showZeros);
				label.Text = sb.ToString();
			};
			buttonA.Clicked += delegate
			{ showZeros = !showZeros; update(); };

			update();
		}
	}
}

