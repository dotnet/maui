#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class IndicatorViewHandler
	{
		public static PropertyMapper<IIndicatorView, IndicatorViewHandler> IndicatorViewMapper = new(ViewMapper)
		{
			//[nameof(ITitledElement.Title)] = MapTitle,
			//[nameof(IContentView.Content)] = MapContent,
		};

		public IndicatorViewHandler(PropertyMapper? mapper = null) : base(mapper ?? IndicatorViewMapper)
		{

		}
	}
}
