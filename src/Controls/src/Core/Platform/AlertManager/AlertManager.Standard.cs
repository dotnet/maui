using System;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Platform
{
	public partial class AlertManager
	{
		private partial IAlertManagerSubscription CreateSubscription(IMauiContext mauiContext)
		{
			throw new NotImplementedException();
		}

		internal partial class AlertRequestHelper
		{
			public partial void OnActionSheetRequested(Page sender, ActionSheetArguments arguments) { }

			public partial void OnAlertRequested(Page sender, AlertArguments arguments) { }

			public partial void OnPromptRequested(Page sender, PromptArguments arguments) { }

			// TODO: This method is obsolete in .NET 10 and will be removed in .NET 11.
			public partial void OnPageBusy(Page sender, bool enabled) { }
		}
	}
}