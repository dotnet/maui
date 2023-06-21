#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	//NOTE: IDEA: review this: merge FROM into a single int (vsm, manual, dynamicR, binding), and CSS into another
	internal readonly struct SetterSpecificity : IComparable<SetterSpecificity>
	{
		public static readonly SetterSpecificity DefaultValue = new(-1, 0, 0, 0, -1, 0, 0, 0);
		public static readonly SetterSpecificity VisualStateSetter = new SetterSpecificity(1, 0, 0, 0, 0, 0, 0, 0);
		public static readonly SetterSpecificity FromBinding = new SetterSpecificity(0, 0, 0, 1, 0, 0, 0, 0);

		public static readonly SetterSpecificity ManualValueSetter = new SetterSpecificity(0, 100, 0, 0, 0, 0, 0, 0);
		public static readonly SetterSpecificity Trigger = new SetterSpecificity(0, 101, 0, 0, 0, 0, 0, 0);

		public static readonly SetterSpecificity DynamicResourceSetter = new SetterSpecificity(0, 0, 1, 0, 0, 0, 0, 0);

		//handler always apply, but are removed when anything else comes in. see SetValueActual
		public static readonly SetterSpecificity FromHandler = new SetterSpecificity(1000, 0, 0, 0, 0, 0, 0, 0);

		//100-n: direct VSM (not from Style), n = max(99, distance between the RD and the target)
		public int Vsm { get; }

		//1: SetValue, SetBinding
		public int Manual { get; }

		//1: DynamicResource
		public int DynamicResource { get; }

		//1: SetBinding
		public int Binding { get; }

		//XAML Style specificity
		//100-n: implicit style, n = max(99, distance between the RD and the target)
		//200-n: RD Style, n = max(99, distance between the RD and the target)
		//200: local style, inline css,
		//300-n: VSM, n = max(99, distance between the RD and the target)
		//300: !important (not implemented)
		public int Style { get; }

		public const int StyleImplicit = 100;
		public const int StyleRD = 200;
		public const int StyleLocal = 200;
		public const int StyleVSM = 300;

		//CSS Specificity, see https://developer.mozilla.org/en-US/docs/Web/CSS/Specificity
		public int Id { get; }
		public int Class { get; }
		public int Type { get; }

		public SetterSpecificity(int vsm, int manual, int dynamicresource, int binding, int style, int id, int @class, int type)
		{
			Vsm = vsm;
			Manual = manual;
			DynamicResource = dynamicresource;
			Binding = binding;
			Style = style;
			Id = id;
			Class = @class;
			Type = type;
		}

		public SetterSpecificity(int style, int id, int @class, int type) : this(0, 0, 0, 0, style, id, @class, type)
		{
		}

		public int CompareTo(SetterSpecificity other)
		{
			//everything coming from Style has lower priority than something that does not
			if (Style != other.Style && Style == 0)
				return 1;
			if (Style != other.Style && other.Style == 0)
				return -1;
			if (Style != other.Style)
				return Style.CompareTo(other.Style);

			if (Vsm != other.Vsm)
				return Vsm.CompareTo(other.Vsm);
			if (Manual != other.Manual)
				return Manual.CompareTo(other.Manual);
			if (DynamicResource != other.DynamicResource)
				return DynamicResource.CompareTo(other.DynamicResource);
			if (Binding != other.Binding)
				return Binding.CompareTo(other.Binding);
			if (Id != other.Id)
				return Id.CompareTo(other.Id);
			if (Class != other.Class)
				return Class.CompareTo(other.Class);
			return Type.CompareTo(other.Type);
		}

		public override bool Equals(object obj) => Equals((SetterSpecificity)obj);

		bool Equals(SetterSpecificity other) => CompareTo(other) == 0;

		public override int GetHashCode() => (Vsm, Manual, DynamicResource, Binding, Style, Id, Class, Type).GetHashCode();

		public static bool operator ==(SetterSpecificity left, SetterSpecificity right) => left.Equals(right);
		public static bool operator !=(SetterSpecificity left, SetterSpecificity right) => !left.Equals(right);
	}
}