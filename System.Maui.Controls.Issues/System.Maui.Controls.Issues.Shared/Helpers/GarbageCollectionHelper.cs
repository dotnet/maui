using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms.Controls.Issues
{
	public static class GarbageCollectionHelper
	{
		public static void Collect()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();

			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
	}
}
