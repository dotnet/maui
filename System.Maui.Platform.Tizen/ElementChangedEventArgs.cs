using System;

namespace Xamarin.Forms.Platform.Tizen
{
	public class ElementChangedEventArgs<TElement> : EventArgs where TElement : Element
	{
		/// <summary>
		/// Holds the old element which is about to be replaced by a new element
		/// </summary>
		/// <value>An TElement instance representing the old element just being replaced</value>
		public TElement OldElement
		{
			get;
			private set;
		}

		/// <summary>
		/// Holds the new element which will replace the old element
		/// </summary>
		/// <value>An TElement instance representing the new element to be used from now on</value>
		public TElement NewElement
		{
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Xamarin.Forms.Platform.Tizen.ElementChangedEventArgs`1"/> class.
		/// </summary>
		/// <param name="oldElement">The old element which will be replaced by a newElement momentarily.</param>
		/// <param name="newElement">The new element, taking place of an old element.</param>
		public ElementChangedEventArgs(TElement oldElement, TElement newElement)
		{
			this.OldElement = oldElement;
			this.NewElement = newElement;
		}
	}
}
