using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Core.ExternalBackend.TestSupport
{
	/// <summary>
	/// Stands in for the root platform view type of a backend that ships outside of .NET MAUI.
	/// </summary>
	public class ExternalPlatformView
	{
		/// <summary>
		/// Gets the group this view is currently parented to, or <see langword="null"/> when it is detached.
		/// </summary>
		public ExternalViewGroup? Parent { get; private set; }

		internal void SetParent(ExternalViewGroup? parent) => Parent = parent;
	}

	/// <summary>
	/// Stands in for a platform view that can host children.
	/// </summary>
	public class ExternalViewGroup : ExternalPlatformView
	{
		readonly List<ExternalPlatformView> _children = new();

		/// <summary>
		/// Gets the children currently hosted by this group, in order.
		/// </summary>
		public IReadOnlyList<ExternalPlatformView> Children => _children;

		/// <summary>
		/// Appends <paramref name="child"/> to this group.
		/// </summary>
		public void Add(ExternalPlatformView child)
		{
			_ = child ?? throw new ArgumentNullException(nameof(child));

			_children.Add(child);
			child.SetParent(this);
		}

		/// <summary>
		/// Inserts <paramref name="child"/> into this group at <paramref name="index"/>.
		/// </summary>
		public void Insert(int index, ExternalPlatformView child)
		{
			_ = child ?? throw new ArgumentNullException(nameof(child));

			_children.Insert(index, child);
			child.SetParent(this);
		}

		/// <summary>
		/// Removes <paramref name="child"/> from this group.
		/// </summary>
		public bool Remove(ExternalPlatformView child)
		{
			if (child is null || !_children.Remove(child))
			{
				return false;
			}

			child.SetParent(null);
			return true;
		}

		/// <summary>
		/// Returns the index of <paramref name="child"/>, or -1 when it is not a child of this group.
		/// </summary>
		public int IndexOf(ExternalPlatformView child) => _children.IndexOf(child);
	}

	/// <summary>
	/// Stands in for the wrapper view an external backend would install as a container view in order to
	/// render gradient or image backgrounds, clips and shadows.
	/// </summary>
	public sealed class ExternalWrapperView : ExternalViewGroup, IDisposable
	{
		/// <summary>
		/// Gets or sets the single view wrapped by this container.
		/// </summary>
		public ExternalPlatformView? Content
		{
			get => Children.Count > 0 ? Children[0] : null;
			set
			{
				var current = Content;
				if (ReferenceEquals(current, value))
				{
					return;
				}

				if (current is not null)
				{
					Remove(current);
				}

				if (value is not null)
				{
					Add(value);
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether <see cref="Dispose"/> has been called.
		/// </summary>
		public bool IsDisposed { get; private set; }

		/// <inheritdoc/>
		public void Dispose()
		{
			Content = null;
			IsDisposed = true;
		}
	}
}
