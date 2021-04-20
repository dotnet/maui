#nullable enable
namespace Microsoft.Maui.Handlers
{
	public partial class FrameHandler
	{
		public static PropertyMapper<IFrame, FrameHandler> FrameMapper = new PropertyMapper<IFrame, FrameHandler>(ViewHandler.ViewMapper)
		{
			[nameof(IFrame.Content)] = MapContent,
			[nameof(IFrame.BackgroundColor)] = MapBackgroundColor,
			[nameof(IFrame.BorderColor)] = MapBorderColor,
			[nameof(IFrame.HasShadow)] = MapHasShadow,
			[nameof(IFrame.CornerRadius)] = MapCornerRadius
		};

		public FrameHandler() : base(FrameMapper)
		{

		}

		public FrameHandler(PropertyMapper mapper) : base(mapper ?? FrameMapper)
		{

		}
	}
}