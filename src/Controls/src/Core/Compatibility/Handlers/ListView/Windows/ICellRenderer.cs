// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public interface ICellRenderer : IRegisterable
	{
		Microsoft.UI.Xaml.DataTemplate GetTemplate(Cell cell);
	}
}