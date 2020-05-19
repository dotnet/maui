using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace System.Maui.Controls
{
	
	public class RootContentPage : ContentPage 
	{
		public RootContentPage (string hierarchy) 
		{
			AutomationId = hierarchy + "PageId";
			Content = new SwapHierachyStackLayout (hierarchy);
		}
	}
}