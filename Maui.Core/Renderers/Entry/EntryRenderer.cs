
namespace System.Maui.Platform
{
	public partial class EntryRenderer
	{
		public static PropertyMapper<ITextInput> EntryMapper = new PropertyMapper<ITextInput>(ViewRenderer.ViewMapper)
		{
			[nameof(IText.Color)] = MapPropertyColor,
			[nameof(IText.Text)] = MapPropertyText,

			[nameof(ITextInput.Placeholder)] = MapPropertyPlaceholder,
			[nameof(ITextInput.PlaceholderColor)] = MapPropertyPlaceholderColor,
		};

		public EntryRenderer() : base(EntryMapper)
		{

		}

		public EntryRenderer(PropertyMapper mapper) : base(mapper ?? EntryMapper)
		{

		}
	}
}
