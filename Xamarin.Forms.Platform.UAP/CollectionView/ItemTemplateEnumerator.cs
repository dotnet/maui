using System.Collections;

namespace Xamarin.Forms.Platform.UWP
{
	internal class ItemTemplateEnumerator : IEnumerable, IEnumerator
	{
		readonly DataTemplate _formsDataTemplate;
		readonly IEnumerator _innerEnumerator;
		readonly BindableObject _container;

		public ItemTemplateEnumerator(IEnumerable itemsSource, DataTemplate formsDataTemplate, BindableObject container)
		{
			_formsDataTemplate = formsDataTemplate;
			_container = container;
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
				Current = new ItemTemplateContext(_formsDataTemplate, _innerEnumerator.Current, _container);
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