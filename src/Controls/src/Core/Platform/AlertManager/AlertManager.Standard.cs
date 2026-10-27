using System;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	internal partial class AlertManager
	{
		private partial IAlertManagerSubscription CreateSubscription(IMauiContext mauiContext)
		{
			return new AlertRequestHelper();
		}

		internal partial class AlertRequestHelper
		{
			public partial void OnActionSheetRequested(Page sender, ActionSheetArguments arguments) { }

			public partial void OnAlertRequested(Page sender, AlertArguments arguments) { }

			public partial void OnPromptRequested(Page sender, PromptArguments arguments) { }

			// TODO: Remove this obsolete method in a future release.
			public partial void OnPageBusy(Page sender, bool enabled) { }
		}
	}
}