namespace Microsoft.Maui.Controls.Handlers
{
	public partial class LabelHandler : Maui.Handlers.LabelHandler
	{
		public static PropertyMapper<Label, LabelHandler> ControlsLabelMapper = new PropertyMapper<Label, LabelHandler>(LabelMapper)
		{
			[nameof(Label.TextType)] = MapTextType,
#if __IOS__
			[nameof(Label.TextDecorations)] = MapLabelTextDecorations
#endif
		};

		public LabelHandler() : base(ControlsLabelMapper) { }

		public LabelHandler(PropertyMapper mapper = null) : base(mapper ?? ControlsLabelMapper) { }
	}
}
