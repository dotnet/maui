using Microsoft.Maui.Handlers;
using static Controls.Core.Platform.Android.Extensions.TextViewExtensions;

namespace Microsoft.Maui.Controls
{
	public partial class Label
	{
		public static void RemapForControls()
		{
			// Adjust the mappings to preserve Controls.Label legacy behaviors
			// ILabel does not include the TextType property, so we map it here to handle HTML text

			IPropertyMapper<ILabel, LabelHandler> ControlsLabelMapper = new PropertyMapper<Label, LabelHandler>(LabelHandler.LabelMapper)
			{
				[nameof(TextType)] = MapTextType,
				[nameof(Text)] = MapText,
			};

			LabelHandler.LabelMapper = ControlsLabelMapper;
		}

		public static void MapTextType(LabelHandler handler, Label label)
		{
			handler.NativeView?.UpdateText(label);
		}

		public static void MapText(LabelHandler handler, Label label)
		{
			handler.NativeView?.UpdateText(label);
		}
	}
}
