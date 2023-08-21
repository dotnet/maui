// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Triggers
{
	public class NumericValidationTriggerAction : TriggerAction<Entry>
	{
		protected override void Invoke(Entry entry)
		{
			bool isValid = double.TryParse(entry.Text, out var result);
			entry.TextColor = isValid ? Colors.Transparent : Colors.Red;
		}
	}
}
