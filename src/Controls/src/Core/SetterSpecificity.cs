#nullable disable
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Defines a setter specificity
	/// </summary>
	/// <remarks>
	/// We still can refine the specificities, but here is how they're compared right now:
	/// - DefaultValue has the lowest priority
	/// - Everything coming from a Style is low priority
	/// - Binding, DynamicResource, Manual (in that order)
	/// - Values set from VSM have a higher priority
	/// 
	/// Then everything coming from the Handlers has a special priority. it is always applied, but is overridden by almost everything else
	/// </remarks>
	internal readonly struct SetterSpecificity
	{
		const byte ExtrasVsm = 0x01;
		const byte ExtrasHandler = 0xFF;

		public const ushort ManualTriggerBaseline = 2;

		public const ushort StyleImplicit = 0x080;
		public const ushort StyleLocal = 0x100;

		public static readonly SetterSpecificity DefaultValue = new SetterSpecificity(0);
		public static readonly SetterSpecificity VisualStateSetter = new SetterSpecificity(ExtrasVsm, 0, 0, 0, 0, 0, 0, 0);
		public static readonly SetterSpecificity FromBinding = new SetterSpecificity(0, 0, 0, 1, 0, 0, 0, 0);

		public static readonly SetterSpecificity ManualValueSetter = new SetterSpecificity(0, 1, 0, 0, 0, 0, 0, 0);
		public static readonly SetterSpecificity Trigger = new SetterSpecificity(0, ManualTriggerBaseline, 0, 0, 0, 0, 0, 0);

		public static readonly SetterSpecificity DynamicResourceSetter = new SetterSpecificity(0, 0, 1, 0, 0, 0, 0, 0);

		// handler always apply, but are removed when anything else comes in. see SetValueActual
		public static readonly SetterSpecificity FromHandler = new SetterSpecificity(0xFF, 0, 0, 0, 0, 0, 0, 0);

		// We store all information in one single UInt64 value to have the fastest comparison possible
		readonly ulong _value;


		public bool IsDefault => _value == 0ul;
		public bool IsHandler => _value == 0xFFFFFFFFFFFFFFFF;
		public bool IsVsm => (_value & 0x0100000000000000) != 0;
		public bool IsVsmImplicit => (_value & 0x0000000004000000) != 0;
		public bool IsManual => ((_value >> 28) & 0xFFFF) == 1;
		public ushort TriggerIndex => GetTriggerIndex();
		public bool IsDynamicResource => ((_value >> 24) & 0x02) != 0;
		public bool IsBinding => ((_value >> 24) & 0x01) != 0;
		public (ushort Style, byte Id, byte Class, byte Type) StyleInfo => GetStyleInfo();

		ushort GetTriggerIndex()
		{
			var manual = (ushort)((_value >> 28) & 0xFFFF);
			if (manual <= 1)
				return 0;
			return (ushort)(manual - 2);
		}

		(ushort Style, byte Id, byte Class, byte Type) GetStyleInfo()
		{
			var style = (ushort)((_value >> 44) & 0xFFF);
			if (style == 0xFFF)
				return default;
			return (style, (byte)((_value >> 16) & 0xFF), (byte)((_value >> 8) & 0xFF), (byte)(_value & 0xFF));
		}


		/// <summary>
		/// Creates a new setter specificity
		/// </summary>
		/// <param name="extras">
		/// Specifies special setter sources <br />
		/// - 1: from VSM <br />
		/// - 0xFF: from Handler
		/// </param>
		/// <param name="manual">
		/// Determines manual specificity, also covers triggers <br />
		/// - 0: not manual <br />
		/// - 1..100: manual <br />
		/// - 101..N: triggers <br />
		/// </param>
		/// <param name="isDynamicResource">Set to 1 when value comes from dynamic resource, otherwise 0</param>
		/// <param name="isBinding">Set to 1 when value comes from binding, otherwise 0</param>
		/// <param name="style">
		/// XAML Style specificity <br />
		/// - 0: not from Style <br />
		/// - 127: base implicit style
		/// - 128-n: implicit style, n = max(99, distance between the RD and the target) <br />
		/// - 255: base local style
		/// - 256-n: local style, inline css, <br />
		/// </param>
		/// <param name="id">
		/// CSS Id Specificity <br />
		/// See https://developer.mozilla.org/en-US/docs/Web/CSS/Specificity
		/// </param>
		/// <param name="class">
		/// CSS Class Specificity <br />
		/// See https://developer.mozilla.org/en-US/docs/Web/CSS/Specificity
		/// </param>
		/// <param name="type">
		/// CSS Type Specificity <br />
		/// See https://developer.mozilla.org/en-US/docs/Web/CSS/Specificity
		/// </param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public SetterSpecificity(byte extras, ushort manual, byte isDynamicResource, byte isBinding, ushort style, byte id, byte @class, byte type)
		{
			// Handlers are special, they win on everything else
			if (extras == ExtrasHandler)
			{
				_value = 0xFFFFFFFFFFFFFFFF;
				return;
			}

			// If no style is set, set it to a value which supersedes any other style value
			if (style == 0)
			{
				style = 0xFFF;
				id = @class = type = 0xFF;
			}

			// Priority order:
			//                       64bit ulong value
			// 1. VSM                0x0100000000000000
			// 2. Style              0x00FFF00000000000
			// 3. Manual(& Trigger)  0x00000FFFF0000000
			// 4. Implicit VSM       0x0000000004000000
			// 4. DynamicResource    0x0000000002000000
			// 5. Binding            0x0000000001000000
			// 6. Id                 0x0000000000FF0000
			// 7. Class              0x000000000000FF00
			// 8. Type               0x00000000000000FF

			var implicitVsm = 0;
			var vsm = extras == ExtrasVsm ? 0x01 : 0;
			var binding = isBinding > 0 ? 0x01 : 0;
			var dynamicResource = isDynamicResource > 0 ? 0x02 : 0;

			// Implicit style VSM has less priority than manually set values
			// See https://github.com/dotnet/maui/issues/18103
			const int styleImplicitUpperBound = StyleLocal - 1;
			if (vsm != 0 && style < styleImplicitUpperBound)
			{
				implicitVsm = 0x04;
				vsm = 0;
			}

			_value = type
					 | (ulong)@class << 8
					 | (ulong)id << 16
					 | (ulong)(implicitVsm | dynamicResource | binding) << 24
					 | (ulong)manual << 28
					 | (ulong)style << 44
					 | (ulong)vsm << 56
				;
		}

		public SetterSpecificity(ushort style, byte id, byte @class, byte type) : this(0, 0, 0, 0, style, id, @class, type)
		{
		}

		public SetterSpecificity()
		{
			// When no parameter have been specified for the specificity, just use the lowest value possible
			// This value is still higher than the DefaultValue, so it will be applied
			_value = 1;
		}

		/// <summary>
		/// Special private constructor to create DefaultValue specificity
		/// </summary>
		SetterSpecificity(ulong value)
		{
			_value = value;
		}

		public SetterSpecificity CopyStyle(byte extras, ushort manual, byte isDynamicResource, byte isBinding)
		{
			return new SetterSpecificity(
				extras,
				manual,
				isDynamicResource,
				isBinding,
				style: (ushort)((_value >> 44) & 0xFFF),
				id: (byte)((_value >> 16) & 0xFF),
				@class: (byte)((_value >> 8) & 0xFF),
				type: (byte)(_value & 0xFF));
		}

		public SetterSpecificity AsBaseStyle()
		{
			return new SetterSpecificity(_value - 0x0000100000000000);
		}

		public override bool Equals(object obj) => obj is SetterSpecificity s && s._value == _value;
		public override int GetHashCode() => _value.GetHashCode();

		public static bool operator <(SetterSpecificity left, SetterSpecificity right) => left._value < right._value;
		public static bool operator >(SetterSpecificity left, SetterSpecificity right) => left._value > right._value;
		public static bool operator >=(SetterSpecificity left, SetterSpecificity right) => left._value >= right._value;
		public static bool operator <=(SetterSpecificity left, SetterSpecificity right) => left._value <= right._value;
		public static bool operator ==(SetterSpecificity left, SetterSpecificity right) => left._value == right._value;
		public static bool operator !=(SetterSpecificity left, SetterSpecificity right) => left._value != right._value;
	}
}
