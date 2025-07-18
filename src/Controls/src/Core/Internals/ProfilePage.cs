#nullable disable
#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Internals
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/ProfileDatum.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.ProfileDatum']/Docs/*" />
	public class ProfileDatum
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/ProfileDatum.xml" path="//Member[@MemberName='Name']/Docs/*" />
		public string Name;
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/ProfileDatum.xml" path="//Member[@MemberName='Id']/Docs/*" />
		public string Id;
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/ProfileDatum.xml" path="//Member[@MemberName='Ticks']/Docs/*" />
		public long Ticks;
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/ProfileDatum.xml" path="//Member[@MemberName='SubTicks']/Docs/*" />
		public long SubTicks;
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/ProfileDatum.xml" path="//Member[@MemberName='Depth']/Docs/*" />
		public int Depth;
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/ProfileDatum.xml" path="//Member[@MemberName='Path']/Docs/*" />
		public string Path;
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/ProfileDatum.xml" path="//Member[@MemberName='Line']/Docs/*" />
		public int Line;
	}

	/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/ContentPageEx.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.ContentPageEx']/Docs/*" />
	public static class ContentPageEx
	{
		const long MS = TimeSpan.TicksPerMillisecond;
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/ContentPageEx.xml" path="//Member[@MemberName='Data']/Docs/*" />
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

