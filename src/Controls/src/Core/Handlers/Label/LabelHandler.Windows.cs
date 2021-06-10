using System;
using System.Collections.Generic;
using System.Text;

namespace Controls.Core.Handlers.Shell
{
	public partial class LabelHandler : Microsoft.Maui.Handlers.LabelHandler
	{
		public static void MapTextType(LabelHandler handler, Label label) =>
				handler.NativeView?.UpdateText(label);
	}
}
