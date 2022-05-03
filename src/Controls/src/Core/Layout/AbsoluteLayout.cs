using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/AbsoluteLayout.xml" path="Type[@FullName='Microsoft.Maui.Controls.AbsoluteLayout']/Docs" />
	public class AbsoluteLayout : Layout, IAbsoluteLayout
	{
		readonly Dictionary<IView, AbsoluteLayoutInfo> _viewInfo = new();
		readonly AbsoluteList _children;

		public AbsoluteLayout()
		{
			this._children = new AbsoluteList(this);
		}

		protected override ILayoutManager CreateLayoutManager()
		{
			return new AbsoluteLayoutManager(this);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/AbsoluteLayout.xml" path="//Member[@MemberName='AutoSize']/Docs" />
		public static double AutoSize = -1;

		#region Attached Properties

		/// <include file="../../../docs/Microsoft.Maui.Controls/AbsoluteLayout.xml" path="//Member[@MemberName='LayoutFlagsProperty']/Docs" />
		public static readonly BindableProperty LayoutFlagsProperty = BindableProperty.CreateAttached("LayoutFlags",
			typeof(AbsoluteLayoutFlags), typeof(AbsoluteLayout), AbsoluteLayoutFlags.None);

		/// <include file="../../../docs/Microsoft.Maui.Controls/AbsoluteLayout.xml" path="//Member[@MemberName='LayoutBoundsProperty']/Docs" />
		public static readonly BindableProperty LayoutBoundsProperty = BindableProperty.CreateAttached("LayoutBounds",
			typeof(Rect), typeof(AbsoluteLayout), new Rect(0, 0, AutoSize, AutoSize), propertyChanged: LayoutBoundsPropertyChanged);

		static void LayoutBoundsPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (bindable is View view && view.Parent is Maui.ILayout layout)
			{
				layout.InvalidateMeasure();
			}
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/AbsoluteLayout.xml" path="//Member[@MemberName='GetLayoutFlags'][1]/Docs" />
		public static AbsoluteLayoutFlags GetLayoutFlags(BindableObject bindable)
		{
			return (AbsoluteLayoutFlags)bindable.GetValue(LayoutFlagsProperty);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/AbsoluteLayout.xml" path="//Member[@MemberName='GetLayoutBounds'][1]/Docs" />
		[System.ComponentModel.TypeConverter(typeof(BoundsTypeConverter))]
		public static Rect GetLayoutBounds(BindableObject bindable)
		{
			return (Rect)bindable.GetValue(LayoutBoundsProperty);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/AbsoluteLayout.xml" path="//Member[@MemberName='SetLayoutFlags'][1]/Docs" />
		public static void SetLayoutFlags(BindableObject bindable, AbsoluteLayoutFlags flags)
		{
			bindable.SetValue(LayoutFlagsProperty, flags);
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/AbsoluteLayout.xml" path="//Member[@MemberName='SetLayoutBounds'][1]/Docs" />
		public static void SetLayoutBounds(BindableObject bindable, Rect bounds)
		{
			bindable.SetValue(LayoutBoundsProperty, bounds);
		}

		#endregion

		/// <include file="../../../docs/Microsoft.Maui.Controls/AbsoluteLayout.xml" path="//Member[@MemberName='GetLayoutFlags'][2]/Docs" />
		public AbsoluteLayoutFlags GetLayoutFlags(IView view)
		{
			return view switch
			{
				BindableObject bo => (AbsoluteLayoutFlags)bo.GetValue(LayoutFlagsProperty),
				_ => _viewInfo[view].LayoutFlags,
			};
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/AbsoluteLayout.xml" path="//Member[@MemberName='GetLayoutBounds'][2]/Docs" />
		public Rect GetLayoutBounds(IView view)
		{
			return view switch
			{
				BindableObject bo => (Rect)bo.GetValue(LayoutBoundsProperty),
				_ => _viewInfo[view].LayoutBounds,
			};
		}

		public void SetLayoutFlags(IView view, AbsoluteLayoutFlags flags)
		{
			switch (view)
			{
				case BindableObject bo:
					bo.SetValue(LayoutFlagsProperty, flags);
					break;
				default:
					_viewInfo[view].LayoutFlags = flags;
					break;
			}
		}

		public void SetLayoutBounds(IView view, Rect bounds)
		{
			switch (view)
			{
				case BindableObject bo:
					bo.SetValue(LayoutBoundsProperty, bounds);
					break;
				default:
					_viewInfo[view].LayoutBounds = bounds;
					break;
			}
		}

		protected override void OnAdd(int index, IView view)
		{
			if (view is not BindableObject)
			{
				_viewInfo[view] = new AbsoluteLayoutInfo();
			}

			base.OnAdd(index, view);
		}

		protected override void OnClear()
		{
			_viewInfo.Clear();
			base.OnClear();
		}

		protected override void OnRemove(int index, IView view)
		{
			_viewInfo.Remove(view);
			base.OnRemove(index, view);
		}

		protected override void OnInsert(int index, IView view)
		{
			if (view is not BindableObject)
			{
				_viewInfo[view] = new AbsoluteLayoutInfo();
			}

			base.OnInsert(index, view);
		}

		protected override void OnUpdate(int index, IView view, IView oldView)
		{
			_viewInfo.Remove(oldView);

			if (view is not BindableObject)
			{
				_viewInfo[view] = new AbsoluteLayoutInfo();
			}

			base.OnUpdate(index, view, oldView);
		}

		public new IAbsoluteList<IView> Children
		{
			get { return _children; }
		}

		class AbsoluteLayoutInfo
		{
			public AbsoluteLayoutFlags LayoutFlags { get; set; }
			public Rect LayoutBounds { get; set; }
		}

		public interface IAbsoluteList<T> : IList<IView> where T : IView
		{
			void Add(View view, Rect bounds, AbsoluteLayoutFlags flags = AbsoluteLayoutFlags.None);

			void Add(View view, Point position);
		}

		class AbsoluteList : IAbsoluteList<IView>
		{
			private AbsoluteLayout _parent;

			public AbsoluteList(AbsoluteLayout parent)
			{
				_parent = parent;
			}

			public IView this[int index]
			{
				get
				{
					return _parent[index];
				}
				set
				{
					_parent[index] = value;
				}
			}

			public int Count
			{
				get
				{
					return _parent.Count;
				}
			}

			public bool IsReadOnly
			{
				get
				{
					return _parent.IsReadOnly;
				}
			}

			public void Add(IView item)
			{
				_parent.Add(item);
			}

			public void Add(View view, Rect bounds, AbsoluteLayoutFlags flags = AbsoluteLayoutFlags.None)
			{
				AbsoluteLayout.SetLayoutBounds(view, bounds);
				_parent.SetLayoutFlags(view, flags);
				_parent.Add(view);
			}

			public void Add(View view, Point position)
			{
				AbsoluteLayout.SetLayoutBounds(view, new Rect(position.X, position.Y, AutoSize, AutoSize));
				_parent.Add(view);
			}

			public void Clear()
			{
				_parent.Clear();
			}

			public bool Contains(IView item)
			{
				return _parent.Contains(item);
			}

			public void CopyTo(IView[] array, int arrayIndex)
			{
				_parent.CopyTo(array, arrayIndex);
			}

			public IEnumerator<IView> GetEnumerator()
			{
				return _parent.GetEnumerator();
			}

			public int IndexOf(IView item)
			{
				return _parent.IndexOf(item);
			}

			public void Insert(int index, IView item)
			{
				_parent.Insert(index, item);
			}

			public bool Remove(IView item)
			{
				return _parent.Remove(item);
			}

			public void RemoveAt(int index)
			{
				_parent.RemoveAt(index);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return _parent.GetEnumerator();
			}
		}
	}
}
