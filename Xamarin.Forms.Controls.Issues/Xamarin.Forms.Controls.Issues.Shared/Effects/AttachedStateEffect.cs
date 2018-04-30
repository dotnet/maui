using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Effects
{

	[Preserve(AllMembers = true)]
	public class AttachedStateEffect : RoutingEffect
	{
		public const string EffectName = "AttachedStateEffect";
		public string ElementIdentifier { get; private set; }
		public AttachedState State { get; private set; } = AttachedState.Unknown;
		public event EventHandler StateChanged;

		public AttachedStateEffect() : base($"{Issues.Effects.ResolutionGroupName}.{EffectName}") { }

		public AttachedStateEffect(Element element) : this()
		{
			ElementIdentifier = element.AutomationId ?? element.ClassId;
		}

		public void Attached(Element Element)
		{
			ElementIdentifier = Element.AutomationId ?? Element.ClassId;

			if (State != AttachedStateEffect.AttachedState.Unknown)
			{
				throw new InvalidOperationException($"Invalid State: {State} expected {AttachedStateEffect.AttachedState.Unknown}");
			}

			State = AttachedStateEffect.AttachedState.Attached;
			StateChanged?.Invoke(this, EventArgs.Empty);
		}

		public void Detached(Element Element)
		{
			if (State != AttachedStateEffect.AttachedState.Attached)
			{
				throw new InvalidOperationException($"Invalid State: {State} expected {AttachedStateEffect.AttachedState.Attached}");
			}

			State = AttachedStateEffect.AttachedState.Detached;
			StateChanged?.Invoke(this, EventArgs.Empty);
		}

		public enum AttachedState
		{
			Unknown,
			Attached,
			Detached
		}
	}
}
