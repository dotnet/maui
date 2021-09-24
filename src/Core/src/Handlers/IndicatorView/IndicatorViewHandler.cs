#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler
	{
		public static PropertyMapper<IIndicatorView, IndicatorViewHandler> IndicatorViewMapper = new(ViewMapper)
		{
			[nameof(IIndicatorView.Count)] = MapCount,
			[nameof(IIndicatorView.Position)] = MapPosition,
			[nameof(IIndicatorView.HideSingle)] = MapHideSingle,
			[nameof(IIndicatorView.MaximumVisible)] = MapMaximumVisible,
			[nameof(IIndicatorView.IndicatorSize)] = MapIndicatorSize,
			[nameof(IIndicatorView.IndicatorColor)] = MapIndicatorColor,
			[nameof(IIndicatorView.SelectedIndicatorColor)] = MapSelectedIndicatorColor,
			[nameof(IIndicatorView.IndicatorsShape)] = MapIndicatorShape
		};

		public IndicatorViewHandler() : base(IndicatorViewMapper)
		{
		}

		public IndicatorViewHandler(PropertyMapper mapper) : base(mapper ?? IndicatorViewMapper)
		{
		}
	}
}
