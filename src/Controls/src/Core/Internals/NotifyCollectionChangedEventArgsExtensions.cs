#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>For internal use by platform renderers.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class NotifyCollectionChangedEventArgsExtensions
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NotifyCollectionChangedEventArgsExtensions.xml" path="//Member[@MemberName='Apply&lt;TFrom&gt;'][1]/Docs/*" />
		public static void Apply<TFrom>(this NotifyCollectionChangedEventArgs self, IList<TFrom> from, IList<object> to)
		{
			self.Apply((o, i, b) => to.Insert(i, o), (o, i) => to.RemoveAt(i), () =>
			{
				to.Clear();
				for (var i = 0; i < from.Count; i++)
					to.Add(from[i]);
			});
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NotifyCollectionChangedEventArgsExtensions.xml" path="//Member[@MemberName='Apply'][1]/Docs/*" />
		public static NotifyCollectionChangedAction Apply(this NotifyCollectionChangedEventArgs self, Action<object, int, bool> insert, Action<object, int> removeAt, Action reset)
		{
			if (self == null)
				throw new ArgumentNullException(nameof(self));
			if (reset == null)
				throw new ArgumentNullException(nameof(reset));
			if (insert == null)
				throw new ArgumentNullException(nameof(insert));
			if (removeAt == null)
				throw new ArgumentNullException(nameof(removeAt));

			switch (self.Action)
			{
				case NotifyCollectionChangedAction.Add:
					if (self.NewStartingIndex < 0)
						goto case NotifyCollectionChangedAction.Reset;

					for (var i = 0; i < self.NewItems.Count; i++)
						insert(self.NewItems[i], i + self.NewStartingIndex, true);

					break;

				case NotifyCollectionChangedAction.Move:
					if (self.NewStartingIndex < 0 || self.OldStartingIndex < 0)
						goto case NotifyCollectionChangedAction.Reset;

					for (var i = 0; i < self.OldItems.Count; i++)
						removeAt(self.OldItems[i], self.OldStartingIndex);

					int insertIndex = self.NewStartingIndex;
					if (self.OldStartingIndex < self.NewStartingIndex)
						insertIndex -= self.OldItems.Count - 1;

					for (var i = 0; i < self.OldItems.Count; i++)
						insert(self.OldItems[i], insertIndex + i, false);

					break;

				case NotifyCollectionChangedAction.Remove:
					if (self.OldStartingIndex < 0)
						goto case NotifyCollectionChangedAction.Reset;

					for (var i = 0; i < self.OldItems.Count; i++)
						removeAt(self.OldItems[i], self.OldStartingIndex);
					break;

				case NotifyCollectionChangedAction.Replace:
					if (self.OldStartingIndex < 0 || self.OldItems.Count != self.NewItems.Count)
						goto case NotifyCollectionChangedAction.Reset;

					for (var i = 0; i < self.OldItems.Count; i++)
					{
						removeAt(self.OldItems[i], i + self.OldStartingIndex);
						insert(self.NewItems[i], i + self.OldStartingIndex, true);
					}
					break;

				case NotifyCollectionChangedAction.Reset:
					reset();
					return NotifyCollectionChangedAction.Reset;
			}

			return self.Action;
		}

		/// <summary>For internal use by platform renderers.</summary>
		/// <param name="e">Internal parameter for platform use.</param>
		/// <param name="count">Internal parameter for platform use.</param>
		/// <returns>For internal use by the Microsoft.Maui.Controls platform.</returns>
		public static NotifyCollectionChangedEventArgsEx WithCount(this NotifyCollectionChangedEventArgs e, int count)
		{
			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					return new NotifyCollectionChangedEventArgsEx(count, NotifyCollectionChangedAction.Add, e.NewItems, e.NewStartingIndex);

				case NotifyCollectionChangedAction.Remove:
					return new NotifyCollectionChangedEventArgsEx(count, NotifyCollectionChangedAction.Remove, e.OldItems, e.OldStartingIndex);

				case NotifyCollectionChangedAction.Move:
					return new NotifyCollectionChangedEventArgsEx(count, NotifyCollectionChangedAction.Move, e.OldItems, e.NewStartingIndex, e.OldStartingIndex);

				case NotifyCollectionChangedAction.Replace:
					return new NotifyCollectionChangedEventArgsEx(count, NotifyCollectionChangedAction.Replace, e.NewItems, e.OldItems, e.OldStartingIndex);

				default:
					return new NotifyCollectionChangedEventArgsEx(count, NotifyCollectionChangedAction.Reset);
			}
		}
	}
}
