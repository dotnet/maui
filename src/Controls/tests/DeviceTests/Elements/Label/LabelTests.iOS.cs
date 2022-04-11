using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LabelTests
	{
		UILabel GetPlatformLabel(LabelHandler labelHandler) =>
			(UILabel)labelHandler.PlatformView;

		UILineBreakMode GetPlatformLineBreakMode(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).LineBreakMode;

		int GetPlatformMaxLines(LabelHandler labelHandler) =>
 			(int)GetPlatformLabel(labelHandler).Lines;
	}
}
