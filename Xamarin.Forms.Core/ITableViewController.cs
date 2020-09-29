using System;

namespace Xamarin.Forms
{
	public interface ITableViewController
	{
		event EventHandler ModelChanged;

		ITableModel Model { get; }
	}
}