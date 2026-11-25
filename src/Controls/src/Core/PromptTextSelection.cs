#nullable enable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Specifies a preset selection for the initial text in a prompt.</summary>
	public enum PromptTextSelectionPreset
	{
		/// <summary>Selects all text actually displayed in the prompt.</summary>
		All,

		/// <summary>Selects no text and places the caret at the end of the displayed text.</summary>
		None
	}

	/// <summary>Describes a one-time initial text selection in a prompt.</summary>
	/// <remarks>
	/// Coordinates are UTF-16 code-unit offsets. Ranges are clamped to the text actually displayed.
	/// For fixed ranges, a zero length places the caret at <see cref="Start"/>.
	/// Check <see cref="IsAll"/> and <see cref="IsNone"/> before using the range coordinates.
	/// A null selection places the caret at the end on Mac Catalyst and Windows and preserves native defaults on other platforms.
	/// </remarks>
	public sealed class PromptTextSelection : IEquatable<PromptTextSelection>
	{
		enum SelectionKind
		{
			Range,
			All,
			None
		}

		readonly SelectionKind _kind;

		/// <summary>Creates a range or caret position for the initial text in a prompt.</summary>
		/// <param name="start">The nonnegative UTF-16 offset at which selection begins.</param>
		/// <param name="length">The nonnegative number of UTF-16 code units to select.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="start"/> or <paramref name="length"/> is negative.</exception>
		public PromptTextSelection(int start, int length)
		{
			if (start < 0)
				throw new ArgumentOutOfRangeException(nameof(start));
			if (length < 0)
				throw new ArgumentOutOfRangeException(nameof(length));

			Start = start;
			Length = length;
		}

		PromptTextSelection(SelectionKind kind)
		{
			_kind = kind;
		}

		/// <summary>Gets the enum-valued preset for selecting all displayed text.</summary>
		public static PromptTextSelectionPreset All => PromptTextSelectionPreset.All;

		/// <summary>Gets the enum-valued preset for placing the caret at the end without selecting text.</summary>
		public static PromptTextSelectionPreset None => PromptTextSelectionPreset.None;

		/// <summary>Gets the requested starting offset, or zero when <see cref="IsAll"/> or <see cref="IsNone"/> is true.</summary>
		public int Start { get; }

		/// <summary>Gets the requested length, or zero when <see cref="IsAll"/> or <see cref="IsNone"/> is true.</summary>
		public int Length { get; }

		/// <summary>Gets whether this request selects all displayed text rather than a fixed range.</summary>
		public bool IsAll => _kind == SelectionKind.All;

		/// <summary>Gets whether this request places the caret at the end of the displayed text.</summary>
		public bool IsNone => _kind == SelectionKind.None;

		/// <inheritdoc/>
		public bool Equals(PromptTextSelection? other) =>
			other is not null && Start == other.Start && Length == other.Length && _kind == other._kind;

		/// <inheritdoc/>
		public override bool Equals(object? obj) => obj is PromptTextSelection other && Equals(other);

		/// <inheritdoc/>
		public override int GetHashCode() => unchecked((Start * 397 ^ Length) * 397 ^ (int)_kind);

		internal static PromptTextSelection FromPreset(PromptTextSelectionPreset initialSelection) =>
			initialSelection switch
			{
				PromptTextSelectionPreset.All => new PromptTextSelection(SelectionKind.All),
				PromptTextSelectionPreset.None => new PromptTextSelection(SelectionKind.None),
				_ => throw new ArgumentOutOfRangeException(nameof(initialSelection))
			};

		internal (int Start, int Length) GetRange(int textLength)
		{
			if (IsAll)
				return (0, textLength);
			if (IsNone)
				return (textLength, 0);

			var start = Math.Min(Start, textLength);
			return (start, Math.Min(Length, textLength - start));
		}
	}
}
