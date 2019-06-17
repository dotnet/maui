using System.Collections;
using System.Collections.Generic;

namespace Xamarin.Forms.Platform.UWP
{
	internal class ItemTemplateEnumerator : IEnumerable, IEnumerator
	{
		readonly DataTemplate _formsDataTemplate;
		readonly IEnumerator _innerEnumerator;

		public ItemTemplateEnumerator(IEnumerable itemsSource, DataTemplate formsDataTemplate)
		{
			_formsDataTemplate = formsDataTemplate;
			_innerEnumerator = itemsSource.GetEnumerator();
		}
		public IEnumerator GetEnumerator()
		{
			return this;
		}

		public bool MoveNext()
		{
			var moveNext = _innerEnumerator.MoveNext();
			
			if (moveNext)
			{
				Current = new ItemTemplatePair(_formsDataTemplate, _innerEnumerator.Current);
			}

			return moveNext;
		}

		public void Reset()
		{
			_innerEnumerator.Reset();
		}

		public object Current { get; private set; }
	}
}