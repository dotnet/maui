// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiPicker : NoCaretField
	{
		readonly HashSet<string> _enableActions;

		public MauiPicker(UIPickerView? uIPickerView)
		{
			UIPickerView = uIPickerView;

			string[] actions = { "copy:", "select:", "selectAll:" };
			_enableActions = new HashSet<string>(actions);
		}

		[UnconditionalSuppressMessage("Memory", "MA0002", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		public UIPickerView? UIPickerView { get; set; }

		public override bool CanPerform(Selector action, NSObject? withSender)
			=> _enableActions.Contains(action.Name);
	}
}
