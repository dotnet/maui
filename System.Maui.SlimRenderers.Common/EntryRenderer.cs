
using System.Maui.Platform;

namespace System.Maui.Platform
{
	public partial class EntryRenderer
	{
		public static PropertyMapper<ITextInput> EntryMapper = new PropertyMapper<ITextInput>(ViewRenderer.ViewMapper)
		{
			[nameof(IText.Text)] = MapPropertyText,

			//[nameof(ITextInput.Placeholder)] = MapPropertyPlaceholder,
			//[nameof(ITextInput.PlaceholderColor)] = MapPropertyPlaceholderColor,
		};

		public EntryRenderer() : base(EntryMapper)
		{

		}
	}
}
