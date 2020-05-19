using System;

namespace System.Maui
{
	public interface ITableViewController
	{
		event EventHandler ModelChanged;

		ITableModel Model { get; }
	}
}
