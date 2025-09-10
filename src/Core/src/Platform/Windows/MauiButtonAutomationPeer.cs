using System.Collections.Generic;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	public partial class MauiButtonAutomationPeer : ButtonAutomationPeer
	{
		public MauiButtonAutomationPeer(Button owner) : base(owner)
		{
		}

		protected override IList<AutomationPeer>? GetChildrenCore()
		{
			return null;
		}

		protected override AutomationPeer? GetLabeledByCore()
		{
			foreach (var item in base.GetChildrenCore())
			{
				if (item is TextBlockAutomationPeer tba)
					return tba;
			}

			return null;
		}
	}
}
