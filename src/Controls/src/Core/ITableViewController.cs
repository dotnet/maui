using System;

namespace Microsoft.Maui.Controls
{
	public interface ITableViewController
	{
		event EventHandler ModelChanged;

		ITableModel Model { get; }
	}
}
