using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.UITest;

namespace Xamarin.Forms.Core.UITests
{
    internal static class RunningApp
	{
		public static IApp App;
	
		public static void Restart () 
		{
			App = null;
		}
	}
}
