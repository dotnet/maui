namespace System.Maui.Platform
{
	public partial class EditorRenderer
	{
		public static PropertyMapper<IEditor> EditorMapper = new PropertyMapper<IEditor>(ViewRenderer.ViewMapper)
		{
			[nameof(IEditor.Color)] = MapPropertyColor,
			[nameof(IEditor.Text)] = MapPropertyText,
			[nameof(IEditor.Placeholder)] = MapPropertyPlaceholder,
			[nameof(IEditor.PlaceholderColor)] = MapPropertyPlaceholderColor,
			[nameof(IEditor.AutoSize)] = MapPropertyAutoSize
		};

		public EditorRenderer() : base(EditorMapper)
		{

		}

		public EditorRenderer(PropertyMapper mapper) : base(mapper ?? EditorMapper)
		{

		}
	}
}
