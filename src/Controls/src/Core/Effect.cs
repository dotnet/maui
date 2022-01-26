using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Effect.xml" path="Type[@FullName='Microsoft.Maui.Controls.Effect']/Docs" />
	public abstract class Effect
	{
		internal Effect()
		{
		}

		internal PlatformEffect PlatformEffect { get; set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/Effect.xml" path="//Member[@MemberName='Element']/Docs" />
		public Element Element { get; internal set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/Effect.xml" path="//Member[@MemberName='IsAttached']/Docs" />
		public bool IsAttached { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/Effect.xml" path="//Member[@MemberName='ResolveId']/Docs" />
		public string ResolveId { get; internal set; }

		#region Statics

		/// <include file="../../docs/Microsoft.Maui.Controls/Effect.xml" path="//Member[@MemberName='Resolve']/Docs" />
		public static Effect Resolve(string name)
		{
			Effect result = null;
			if (Internals.Registrar.Effects.TryGetValue(name, out Type effectType))
			{
				result = (Effect)DependencyResolver.ResolveOrCreate(effectType);
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