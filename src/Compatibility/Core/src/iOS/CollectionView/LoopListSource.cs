//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	internal class LoopListSource : ListSource, ILoopItemsViewSource
	{
		const int LoopBy = 3;

		public LoopListSource(IEnumerable<object> enumerable, bool loop) : base(enumerable)
		{
			Loop = loop;
		}

		public LoopListSource(IEnumerable enumerable, bool loop)
		{
			Loop = loop;

			foreach (object item in enumerable)
			{
				Add(item);
			}
		}

		public bool Loop { get; set; }

		public int LoopCount => Loop ? Count * LoopBy : Count;
	}
}
