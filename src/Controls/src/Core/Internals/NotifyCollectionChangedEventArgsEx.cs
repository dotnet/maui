#nullable disable
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>For internal use by platform renderers.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class NotifyCollectionChangedEventArgsEx : NotifyCollectionChangedEventArgs
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NotifyCollectionChangedEventArgsEx.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action) : base(action)
		{
			Count = count;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NotifyCollectionChangedEventArgsEx.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, IList changedItems) : base(action, changedItems)
		{
			Count = count;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NotifyCollectionChangedEventArgsEx.xml" path="//Member[@MemberName='.ctor'][4]/Docs/*" />
		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, IList newItems, IList oldItems) : base(action, newItems, oldItems)
		{
			Count = count;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NotifyCollectionChangedEventArgsEx.xml" path="//Member[@MemberName='.ctor'][8]/Docs/*" />
		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, IList newItems, IList oldItems, int startingIndex) : base(action, newItems, oldItems, startingIndex)
		{
			Count = count;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NotifyCollectionChangedEventArgsEx.xml" path="//Member[@MemberName='.ctor'][5]/Docs/*" />
		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, IList changedItems, int startingIndex) : base(action, changedItems, startingIndex)
		{
			Count = count;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NotifyCollectionChangedEventArgsEx.xml" path="//Member[@MemberName='.ctor'][9]/Docs/*" />
		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, IList changedItems, int index, int oldIndex) : base(action, changedItems, index, oldIndex)
		{
			Count = count;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NotifyCollectionChangedEventArgsEx.xml" path="//Member[@MemberName='.ctor'][3]/Docs/*" />
		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, object changedItem) : base(action, changedItem)
		{
			Count = count;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NotifyCollectionChangedEventArgsEx.xml" path="//Member[@MemberName='.ctor'][6]/Docs/*" />
		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, object changedItem, int index) : base(action, changedItem, index)
		{
			Count = count;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NotifyCollectionChangedEventArgsEx.xml" path="//Member[@MemberName='.ctor'][10]/Docs/*" />
		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, object changedItem, int index, int oldIndex) : base(action, changedItem, index, oldIndex)
		{
			Count = count;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NotifyCollectionChangedEventArgsEx.xml" path="//Member[@MemberName='.ctor'][7]/Docs/*" />
		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, object newItem, object oldItem) : base(action, newItem, oldItem)
		{
			Count = count;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/NotifyCollectionChangedEventArgsEx.xml" path="//Member[@MemberName='.ctor'][11]/Docs/*" />
		public NotifyCollectionChangedEventArgsEx(int count, NotifyCollectionChangedAction action, object newItem, object oldItem, int index) : base(action, newItem, oldItem, index)
		{
			Count = count;
		}

		/// <summary>For internal use by platform renderers.</summary>
		public int Count { get; private set; }
	}
}