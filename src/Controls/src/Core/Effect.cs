#nullable disable
using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	/// <summary>A collection of styles and properties that can be added to an element at run time.</summary>
	public abstract class Effect
	{
		internal Effect()
		{
		}

		internal PlatformEffect PlatformEffect { get; set; }

		/// <summary>Gets the element to which the style is attached.</summary>
		public Element Element { get; internal set; }

		/// <summary>Gets a value that tells whether the effect is attached to an element.</summary>
		public bool IsAttached { get; private set; }

#pragma warning disable CS1734 // XML comment on 'Effect.ResolveId' has a paramref tag for 'name', but there is no parameter by that name
		/// <summary>Gets the ID that is used to resolve this effect at runtime.</summary>
		/// <remarks>Developers must supply a name to
		/// This property returns the string that developers pass to</remarks>
#pragma warning restore CS1734
		public string ResolveId { get; internal set; }

		#region Statics

		/// <summary>Returns an <see cref="Microsoft.Maui.Controls.Effect"/> for the specified name, which is of the form <c>ResolutionGroupName.ExportEffect</c>.</summary>
		/// <param name="name">The name of the effect to get.</param>
		/// <returns>The uniquely identified effect.</returns>
		public static Effect Resolve(string name)
		{
			Effect result = null;
			if (Internals.Registrar.Effects.TryGetValue(name, out var effectType))
			{
				result = (Effect)DependencyResolver.ResolveOrCreate(effectType.Type);
			}

			if (result == null)
				result = new NullEffect();
			result.ResolveId = name;
			return result;
		}

		#endregion

		// Received after Control/Container/Element made valid
		protected abstract void OnAttached();

		// Received after Control/Container made invalid
		protected abstract void OnDetached();

		internal virtual void ClearEffect()
		{
			if (IsAttached)
				SendDetached();
			Element = null;
		}

		internal virtual void SendAttached()
		{
			if (IsAttached)
				return;
			OnAttached();
			IsAttached = true;
			PlatformEffect?.SendAttached();
		}

		internal virtual void SendDetached()
		{
			if (!IsAttached)
				return;
			OnDetached();
			IsAttached = false;
			PlatformEffect?.SendDetached();
		}

		internal virtual void SendOnElementPropertyChanged(PropertyChangedEventArgs args)
		{
		}
	}
}