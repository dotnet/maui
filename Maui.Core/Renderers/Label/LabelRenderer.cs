using System;

namespace System.Maui.Platform {
	public partial class LabelRenderer {
		public static PropertyMapper<IText> ITextMapper = new PropertyMapper<IText>(ViewRenderer.ViewMapper)
		{
			[nameof(ILabel.Text)] = MapPropertyText,
			[nameof(ILabel.Color)] = MapPropertyColor,
		};

		public static PropertyMapper<ILabel> Mapper = new PropertyMapper<ILabel>(ViewRenderer.ViewMapper)
		{
			[nameof(ILabel.Text)] = MapPropertyText,
			[nameof(ILabel.Color)] = MapPropertyColor,
			[nameof(ILabel.LineHeight)] = MapPropertyLineHeight,
		};

		public LabelRenderer () : base (Mapper)
		{

		}
		public LabelRenderer (PropertyMapper mapper) : base (mapper ?? Mapper)
		{

		}
	}
}
