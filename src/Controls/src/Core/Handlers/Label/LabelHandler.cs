using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Controls.Core.Handlers
{ 
	public partial class LabelHandler : Microsoft.Maui.Handlers.LabelHandler
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
