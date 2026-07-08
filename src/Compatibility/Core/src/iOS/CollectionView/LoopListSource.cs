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
