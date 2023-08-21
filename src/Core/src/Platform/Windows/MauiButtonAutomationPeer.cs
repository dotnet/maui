// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	public class MauiButtonAutomationPeer : ButtonAutomationPeer
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
