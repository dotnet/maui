using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

/// <summary>
/// Item model for the layout-performance reproduction.
/// Mirrors the shape of the customer sample (lukaszgrudzinski/MauiPerformanceHacks, Models/FeedCardItem.cs):
/// deliberately variable text length / line counts / optional sub-sections so that no two tiles can share
/// a measured height. That is what defeats uniform cell sizing and forces a real measure pass per item.
/// </summary>
public sealed class FeedItem
{
	public string Seller { get; set; } = string.Empty;
	public string Location { get; set; } = string.Empty;
	public string Status { get; set; } = string.Empty;
	public string Title { get; set; } = string.Empty;
	public double TitleFontSize { get; set; }
	public int TitleMaxLines { get; set; }
	public string Description { get; set; } = string.Empty;
	public double DescriptionFontSize { get; set; }
	public int DescriptionMaxLines { get; set; }
	public bool ShowExtraDetails { get; set; }
	public string ExtraDetails { get; set; } = string.Empty;
	public double PreviewHeight { get; set; }
	public string TagsDisplay { get; set; } = string.Empty;
	public bool ShowInlineCarousel { get; set; }
	public double CarouselHeight { get; set; }
	public IReadOnlyList<string> CarouselItems { get; set; } = Array.Empty<string>();
	public int Progress { get; set; }
	public int Price { get; set; }
	public string Updated { get; set; } = string.Empty;
	public bool ShowMetric4 { get; set; }
	public string Metric4Label { get; set; } = string.Empty;
	public string Metric4Value { get; set; } = string.Empty;
	public Color StatusColor { get; set; } = Colors.SlateGray;

	/// <summary>Pre-flattened footer used by the flattened template variant.</summary>
	public string FooterSummary { get; set; } = string.Empty;
}
