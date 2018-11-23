using System;

namespace Xamarin.Forms
{
	public static class VisualMarker
	{
		public static IVisual MatchParent { get; } = new VisualRendererMarker.MatchParent();
		public static IVisual Default { get; } = new VisualRendererMarker.Default();
		public static IVisual Material { get; } = new VisualRendererMarker.Material();
	}

	public static class VisualRendererMarker
	{
		public sealed class Material : IVisual { internal Material() { } }
		public sealed class Default : IVisual { internal Default() { } }
		internal sealed class MatchParent : IVisual { internal MatchParent() { } }
	}

	[TypeConverter(typeof(VisualTypeConverter))]
	public interface IVisual
	{

	}

	[Xaml.TypeConversion(typeof(IVisual))]
	public class VisualTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				var sc = StringComparison.OrdinalIgnoreCase;
				if (value.Equals(nameof(VisualMarker.MatchParent), sc))
					return VisualMarker.MatchParent;
				else if (value.Equals(nameof(VisualMarker.Material), sc))
					return VisualMarker.Material;
				else
					return VisualMarker.Default;
			}
			throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(IVisual)}");
		}
	}
}