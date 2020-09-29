using System;

namespace Xamarin.Forms
{
	public class DragEventArgs : EventArgs
	{
		public DragEventArgs(DataPackage dataPackage)
		{
			Data = dataPackage;
			AcceptedOperation = DataPackageOperation.Copy;
		}

		public DataPackage Data { get; }
		public DataPackageOperation AcceptedOperation { get; set; }
	}
}