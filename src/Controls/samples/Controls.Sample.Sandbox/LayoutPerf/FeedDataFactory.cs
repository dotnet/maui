using System.Text;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

/// <summary>
/// Deterministic data generator for the layout-performance reproduction.
/// Ported (shape only, not source) from the customer sample's Helpers/FeedStressDataFactory.cs so that the
/// text-length distribution — and therefore the measure cost distribution — is comparable.
/// </summary>
public static class FeedDataFactory
{
	static readonly string[] s_statuses = { "Live", "Needs review", "Awaiting pickup", "Delayed", "New" };

	static readonly Color[] s_statusColors =
	{
		Color.FromArgb("#2E7D32"), Color.FromArgb("#B26A00"), Color.FromArgb("#3348A8"),
		Color.FromArgb("#8E2A2A"), Color.FromArgb("#0F6B6B"),
	};

	static readonly string[] s_chipPool =
	{
		"priority", "inspection", "photo-proof", "handover", "safety", "qa", "retrofit", "electrical",
		"plumbing", "night-shift", "permit", "compliance", "elevator", "roof", "fixtures", "revision",
	};

	const string TitleChunk = "Renovation package {0} with full timeline, weekly notes, legal checkpoints, budget gates. ";
	const string DescriptionChunk = "This card mirrors a realistic field operations dashboard tile with dense metadata and text-heavy updates.";
	const string DetailChunk = "Escalation context: includes permit timeline, supplier risk, daylight constraints, noise windows, and stakeholder notes.";

	public static List<FeedItem> Create(int count)
	{
		var items = new List<FeedItem>(count);
		var sb = new StringBuilder();

		for (int i = 0; i < count; i++)
		{
			int titleRepeat = 1 + (i % 4);
			int descriptionRepeat = 1 + (i % 8);
			bool showExtra = i % 3 != 0;
			int extraRepeat = 2 + (i % 10);
			bool showCarousel = i % 4 == 0;

			sb.Clear();
			for (int r = 0; r < titleRepeat; r++)
			{
				sb.AppendFormat(System.Globalization.CultureInfo.InvariantCulture, TitleChunk, i + 1);
			}
			string title = sb.ToString();

			sb.Clear();
			for (int r = 0; r < descriptionRepeat; r++)
			{
				sb.Append(DescriptionChunk);
			}
			string description = sb.ToString();

			string extra = string.Empty;
			if (showExtra)
			{
				sb.Clear();
				for (int r = 0; r < extraRepeat; r++)
				{
					sb.Append(DetailChunk);
				}
				extra = sb.ToString();
			}

			sb.Clear();
			int tagCount = 2 + ((i * 5) % 11);
			for (int t = 0; t < tagCount; t++)
			{
				if (t > 0)
				{
					sb.Append("  •  ");
				}

				sb.Append(s_chipPool[(i + t) % s_chipPool.Length]).Append('-').Append((i + t) % 9);
			}
			string tags = sb.ToString();

			string[] carousel = Array.Empty<string>();
			if (showCarousel)
			{
				int carouselCount = 4 + (i % 6);
				carousel = new string[carouselCount];
				for (int c = 0; c < carouselCount; c++)
				{
					carousel[c] = "Milestone " + (c + 1) + ": photo set, vendor memo, and approval notes for zone " + ((i + c) % 12);
				}
			}

			items.Add(new FeedItem
			{
				Seller = "Northwind Studio " + i,
				Location = "Warsaw - District " + (i % 18),
				Status = s_statuses[i % s_statuses.Length],
				StatusColor = s_statusColors[i % s_statusColors.Length],
				Title = title,
				TitleFontSize = 14 + (i % 6),
				TitleMaxLines = 2 + (i % 6),
				Description = description,
				DescriptionFontSize = 11 + (i % 4),
				DescriptionMaxLines = 3 + (i % 10),
				ShowExtraDetails = showExtra,
				ExtraDetails = extra,
				PreviewHeight = 110 + (i % 3) * 30,
				TagsDisplay = tags,
				ShowInlineCarousel = showCarousel,
				CarouselHeight = showCarousel ? 84 : 0,
				CarouselItems = carousel,
				Progress = i % 100,
				Price = 350 + (i * 17) % 4200,
				Updated = "-" + (1 + (i % 59)) + "min",
				ShowMetric4 = i % 2 == 0,
				Metric4Label = "Risk",
				Metric4Value = ((i * 7) % 100) + "%",
				FooterSummary = "Progress " + (i % 100) + "%   Price " + (350 + (i * 17) % 4200) +
					" PLN   Updated -" + (1 + (i % 59)) + "min   Risk " + ((i * 7) % 100) + "%",
			});
		}

		return items;
	}
}
