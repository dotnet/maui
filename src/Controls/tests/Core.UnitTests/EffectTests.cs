using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class EffectTests : BaseTestFixture
	{
		[Fact]
		public void ResolveSetsId()
		{
			string id = "Unknown";
			var effect = Effect.Resolve(id);
			Assert.Equal(id, effect.ResolveId);
		}

		[Fact]
		public void UnknownIdReturnsNullEffect()
		{
			var effect = Effect.Resolve("Foo");
			Assert.IsType<NullEffect>(effect);
		}

		[Fact]
		public void SendAttachedSetsFlag()
		{
			var effect = Effect.Resolve("Foo");
			effect.SendAttached();
			Assert.True(effect.IsAttached);
		}

		[Fact]
		public void SendDetachedUnsetsFlag()
		{
			var effect = Effect.Resolve("Foo");
			effect.SendAttached();
			effect.SendDetached();
			Assert.False(effect.IsAttached);
		}

		[Fact]
		public void EffectLifecyclePreProvider()
		{
			var effect = new CustomEffect();
			var element = new Label();

			element.Effects.Add(effect);
			((IVisualElementController)element).EffectControlProvider = new EffectControlProvider();

			Assert.True(effect.IsAttached);
			Assert.True(effect.OnAttachedCalled);
			Assert.True(effect.Registered);
			Assert.False(effect.OnDetachedCalled);

			element.Effects.Remove(effect);
			Assert.True(effect.OnDetachedCalled);
		}

		[Fact]
		public void EffectLifecyclePostProvider()
		{
			var effect = new CustomEffect();
			var element = new Label();

			((IVisualElementController)element).EffectControlProvider = new EffectControlProvider();
			element.Effects.Add(effect);

			Assert.True(effect.IsAttached);
			Assert.True(effect.OnAttachedCalled);
			Assert.True(effect.Registered);
			Assert.False(effect.OnDetachedCalled);

			element.Effects.Remove(effect);
			Assert.True(effect.OnDetachedCalled);
		}

		[Fact]
		public void EffectsClearDetachesEffect()
		{
			var effect = new CustomEffect();
			var element = new Label();

			((IVisualElementController)element).EffectControlProvider = new EffectControlProvider();
			element.Effects.Add(effect);

			element.Effects.Clear();

			Assert.True(effect.OnDetachedCalled);
		}

		class EffectControlProvider : IEffectControlProvider
		{
			public void RegisterEffect(Effect effect)
			{
				var e = effect as CustomEffect;
				if (e != null)
					e.Registered = true;
			}
		}

		class CustomEffect : Effect
		{
			public bool OnAttachedCalled;
			public bool OnDetachedCalled;
			public bool Registered;

			protected override void OnAttached()
			{
				OnAttachedCalled = true;
			}

			protected override void OnDetached()
			{
				OnDetachedCalled = true;
			}
		}
	}
}
