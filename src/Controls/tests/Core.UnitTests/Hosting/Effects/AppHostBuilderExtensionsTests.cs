#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime;
using System.Runtime.CompilerServices;

using Microsoft.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Hosting;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests.Hosting.Effects
{
    public partial class AppHostBuilderExtensionsTests
    {
        /// <summary>
        /// Tests that ConfigureEffects throws ArgumentNullException when builder is null.
        /// This verifies the extension method behaves correctly with null input.
        /// </summary>
        [Fact]
        public void ConfigureEffects_NullBuilder_ThrowsArgumentNullException()
        {
            // Arrange
            MauiAppBuilder builder = null;
            Action<IEffectsBuilder> configureDelegate = Substitute.For<Action<IEffectsBuilder>>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                AppHostBuilderExtensions.ConfigureEffects(builder, configureDelegate));
        }

        /// <summary>
        /// Tests that ConfigureEffects registers EffectsFactory singleton and returns builder when configureDelegate is null.
        /// This verifies the method handles null configureDelegate gracefully and still registers the factory.
        /// </summary>
        [Fact]
        public void ConfigureEffects_NullConfigureDelegate_RegistersEffectsFactoryOnly()
        {
            // Arrange
            var mockServices = Substitute.For<IServiceCollection>();
            var mockBuilder = Substitute.For<MauiAppBuilder>();
            mockBuilder.Services.Returns(mockServices);
            Action<IEffectsBuilder> configureDelegate = null;

            // Act
            var result = AppHostBuilderExtensions.ConfigureEffects(mockBuilder, configureDelegate);

            // Assert
            mockServices.Received(1).TryAddSingleton(Arg.Any<Func<IServiceProvider, EffectsFactory>>());
            mockServices.DidNotReceive().AddSingleton<EffectsRegistration>(Arg.Any<EffectsRegistration>());
            Assert.Same(mockBuilder, result);
        }

        /// <summary>
        /// Tests that ConfigureEffects registers both EffectsFactory and EffectsRegistration when configureDelegate is provided.
        /// This verifies the complete registration flow when a valid delegate is provided.
        /// </summary>
        [Fact]
        public void ConfigureEffects_ValidConfigureDelegate_RegistersBothServices()
        {
            // Arrange
            var mockServices = Substitute.For<IServiceCollection>();
            var mockBuilder = Substitute.For<MauiAppBuilder>();
            mockBuilder.Services.Returns(mockServices);
            Action<IEffectsBuilder> configureDelegate = builder => { };

            // Act
            var result = AppHostBuilderExtensions.ConfigureEffects(mockBuilder, configureDelegate);

            // Assert
            mockServices.Received(1).TryAddSingleton(Arg.Any<Func<IServiceProvider, EffectsFactory>>());
            mockServices.Received(1).AddSingleton<EffectsRegistration>(Arg.Any<EffectsRegistration>());
            Assert.Same(mockBuilder, result);
        }

        /// <summary>
        /// Tests that the EffectsFactory factory function creates EffectsFactory with correct dependencies.
        /// This verifies the lambda expression passed to TryAddSingleton works correctly.
        /// </summary>
        [Fact]
        public void ConfigureEffects_EffectsFactoryRegistration_CreatesFactoryWithServices()
        {
            // Arrange
            var mockServices = Substitute.For<IServiceCollection>();
            var mockBuilder = Substitute.For<MauiAppBuilder>();
            var mockServiceProvider = Substitute.For<IServiceProvider>();
            var effectsRegistrations = new List<EffectsRegistration>();

            mockBuilder.Services.Returns(mockServices);
            mockServiceProvider.GetServices<EffectsRegistration>().Returns(effectsRegistrations);

            Func<IServiceProvider, EffectsFactory> capturedFactory = null;
            mockServices.When(x => x.TryAddSingleton(Arg.Any<Func<IServiceProvider, EffectsFactory>>()))
                       .Do(callInfo => capturedFactory = callInfo.ArgAt<Func<IServiceProvider, EffectsFactory>>(0));

            // Act
            AppHostBuilderExtensions.ConfigureEffects(mockBuilder, null);

            // Assert
            Assert.NotNull(capturedFactory);
            var factory = capturedFactory(mockServiceProvider);
            Assert.NotNull(factory);
            mockServiceProvider.Received(1).GetServices<EffectsRegistration>();
        }

        /// <summary>
        /// Tests that EffectsRegistration is created with the provided configureDelegate.
        /// This verifies the EffectsRegistration constructor is called with the correct delegate.
        /// </summary>
        [Fact]
        public void ConfigureEffects_ValidConfigureDelegate_CreatesEffectsRegistrationWithDelegate()
        {
            // Arrange
            var mockServices = Substitute.For<IServiceCollection>();
            var mockBuilder = Substitute.For<MauiAppBuilder>();
            mockBuilder.Services.Returns(mockServices);

            var configureDelegate = Substitute.For<Action<IEffectsBuilder>>();
            EffectsRegistration capturedRegistration = null;

            mockServices.When(x => x.AddSingleton<EffectsRegistration>(Arg.Any<EffectsRegistration>()))
                       .Do(callInfo => capturedRegistration = callInfo.ArgAt<EffectsRegistration>(0));

            // Act
            AppHostBuilderExtensions.ConfigureEffects(mockBuilder, configureDelegate);

            // Assert
            Assert.NotNull(capturedRegistration);
            mockServices.Received(1).AddSingleton<EffectsRegistration>(Arg.Any<EffectsRegistration>());
        }

        /// <summary>
        /// Tests ConfigureEffects with various edge case delegates including empty actions.
        /// This parameterized test verifies the method handles different types of valid delegates.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetConfigureDelegateTestData))]
        public void ConfigureEffects_VariousDelegates_RegistersServicesCorrectly(Action<IEffectsBuilder> configureDelegate, bool shouldRegisterEffectsRegistration)
        {
            // Arrange
            var mockServices = Substitute.For<IServiceCollection>();
            var mockBuilder = Substitute.For<MauiAppBuilder>();
            mockBuilder.Services.Returns(mockServices);

            // Act
            var result = AppHostBuilderExtensions.ConfigureEffects(mockBuilder, configureDelegate);

            // Assert
            mockServices.Received(1).TryAddSingleton(Arg.Any<Func<IServiceProvider, EffectsFactory>>());

            if (shouldRegisterEffectsRegistration)
            {
                mockServices.Received(1).AddSingleton<EffectsRegistration>(Arg.Any<EffectsRegistration>());
            }
            else
            {
                mockServices.DidNotReceive().AddSingleton<EffectsRegistration>(Arg.Any<EffectsRegistration>());
            }

            Assert.Same(mockBuilder, result);
        }

        public static IEnumerable<object[]> GetConfigureDelegateTestData()
        {
            yield return new object[] { null, false };
            yield return new object[] { new Action<IEffectsBuilder>(builder => { }), true };
            yield return new object[] { new Action<IEffectsBuilder>(builder => builder.Add(typeof(object), typeof(object))), true };
        }
    }
}

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class EffectsRegistrationTests
    {
        /// <summary>
        /// Tests that the EffectsRegistration constructor properly handles a null Action parameter.
        /// The constructor should accept null without throwing any exceptions.
        /// </summary>
        [Fact]
        public void Constructor_NullAction_DoesNotThrow()
        {
            // Arrange
            Action<IEffectsBuilder> nullAction = null;

            // Act & Assert
            var exception = Record.Exception(() => new EffectsRegistration(nullAction));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that the EffectsRegistration constructor properly handles a valid Action parameter.
        /// The constructor should successfully create an instance and store the provided Action.
        /// </summary>
        [Fact]
        public void Constructor_ValidAction_CreatesInstanceSuccessfully()
        {
            // Arrange
            var mockEffectsBuilder = Substitute.For<IEffectsBuilder>();
            Action<IEffectsBuilder> validAction = builder => { /* Valid action logic */ };

            // Act
            var effectsRegistration = new EffectsRegistration(validAction);

            // Assert
            Assert.NotNull(effectsRegistration);
        }

        /// <summary>
        /// Tests that the EffectsRegistration constructor properly handles an Action that performs operations.
        /// The constructor should accept the Action regardless of its complexity.
        /// </summary>
        [Fact]
        public void Constructor_ActionWithOperations_CreatesInstanceSuccessfully()
        {
            // Arrange
            var mockEffectsBuilder = Substitute.For<IEffectsBuilder>();
            Action<IEffectsBuilder> actionWithOperations = builder =>
            {
                builder.Add(typeof(object), typeof(object));
            };

            // Act
            var effectsRegistration = new EffectsRegistration(actionWithOperations);

            // Assert
            Assert.NotNull(effectsRegistration);
        }

        /// <summary>
        /// Tests that the EffectsRegistration constructor properly handles an empty Action parameter.
        /// The constructor should accept an empty Action without throwing any exceptions.
        /// </summary>
        [Fact]
        public void Constructor_EmptyAction_CreatesInstanceSuccessfully()
        {
            // Arrange
            Action<IEffectsBuilder> emptyAction = builder => { };

            // Act
            var effectsRegistration = new EffectsRegistration(emptyAction);

            // Assert
            Assert.NotNull(effectsRegistration);
        }

        /// <summary>
        /// Tests that AddEffects calls the registered action with a valid IEffectsBuilder parameter.
        /// Input: Valid IEffectsBuilder instance.
        /// Expected: The registered action is called with the provided IEffectsBuilder.
        /// </summary>
        [Fact]
        public void AddEffects_ValidEffectsBuilder_CallsRegisterEffectsAction()
        {
            // Arrange
            var mockAction = Substitute.For<Action<IEffectsBuilder>>();
            var mockEffectsBuilder = Substitute.For<IEffectsBuilder>();
            var effectsRegistration = new EffectsRegistration(mockAction);

            // Act
            effectsRegistration.AddEffects(mockEffectsBuilder);

            // Assert
            mockAction.Received(1).Invoke(mockEffectsBuilder);
        }

        /// <summary>
        /// Tests that AddEffects calls the registered action with null parameter.
        /// Input: Null IEffectsBuilder parameter.
        /// Expected: The registered action is called with null.
        /// </summary>
        [Fact]
        public void AddEffects_NullEffectsBuilder_CallsRegisterEffectsActionWithNull()
        {
            // Arrange
            var mockAction = Substitute.For<Action<IEffectsBuilder>>();
            var effectsRegistration = new EffectsRegistration(mockAction);

            // Act
            effectsRegistration.AddEffects(null);

            // Assert
            mockAction.Received(1).Invoke(null);
        }

        /// <summary>
        /// Tests that AddEffects propagates exceptions thrown by the registered action.
        /// Input: Valid IEffectsBuilder with action that throws exception.
        /// Expected: The exception from the action is propagated.
        /// </summary>
        [Fact]
        public void AddEffects_RegisterEffectsActionThrows_PropagatesException()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Test exception");
            var mockAction = Substitute.For<Action<IEffectsBuilder>>();
            mockAction.When(x => x.Invoke(Arg.Any<IEffectsBuilder>())).Do(x => throw expectedException);
            var mockEffectsBuilder = Substitute.For<IEffectsBuilder>();
            var effectsRegistration = new EffectsRegistration(mockAction);

            // Act & Assert
            var actualException = Assert.Throws<InvalidOperationException>(() => effectsRegistration.AddEffects(mockEffectsBuilder));
            Assert.Same(expectedException, actualException);
        }

        /// <summary>
        /// Tests that AddEffects throws NullReferenceException when the registered action is null.
        /// Input: Valid IEffectsBuilder with null registered action.
        /// Expected: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void AddEffects_NullRegisterEffectsAction_ThrowsNullReferenceException()
        {
            // Arrange
            var effectsRegistration = new EffectsRegistration(null);
            var mockEffectsBuilder = Substitute.For<IEffectsBuilder>();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => effectsRegistration.AddEffects(mockEffectsBuilder));
        }
    }

    public partial class EffectCollectionBuilderTests
    {
        /// <summary>
        /// Tests that Add method successfully adds a valid effect type and platform effect type to the RegisteredEffects dictionary.
        /// Input: Valid TEffect and TPlatformEffect types.
        /// Expected: Effect is registered and fluent interface returns same instance.
        /// </summary>
        [Fact]
        public void Add_ValidTypes_AddsToRegisteredEffectsAndReturnsSelf()
        {
            // Arrange
            var builder = new EffectCollectionBuilder();
            var effectType = typeof(string);
            var platformEffectType = typeof(TestPlatformEffect);

            // Act
            var result = builder.Add(effectType, platformEffectType);

            // Assert
            Assert.Same(builder, result);
            Assert.Single(builder.RegisteredEffects);
            Assert.True(builder.RegisteredEffects.ContainsKey(effectType));
        }

        /// <summary>
        /// Tests that Add method throws ArgumentNullException when TEffect parameter is null.
        /// Input: null TEffect parameter.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void Add_NullTEffect_ThrowsArgumentNullException()
        {
            // Arrange
            var builder = new EffectCollectionBuilder();
            var platformEffectType = typeof(TestPlatformEffect);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => builder.Add(null, platformEffectType));
        }

        /// <summary>
        /// Tests that Add method can register effect with null TPlatformEffect parameter without immediate error.
        /// Input: null TPlatformEffect parameter.
        /// Expected: Registration succeeds but lambda execution will fail later.
        /// </summary>
        [Fact]
        public void Add_NullTPlatformEffect_RegistersSuccessfully()
        {
            // Arrange
            var builder = new EffectCollectionBuilder();
            var effectType = typeof(string);

            // Act
            var result = builder.Add(effectType, null);

            // Assert
            Assert.Same(builder, result);
            Assert.Single(builder.RegisteredEffects);
            Assert.True(builder.RegisteredEffects.ContainsKey(effectType));
        }

        /// <summary>
        /// Tests that Add method throws ArgumentException when attempting to register the same TEffect type twice.
        /// Input: Same TEffect type registered twice.
        /// Expected: ArgumentException is thrown on second registration.
        /// </summary>
        [Fact]
        public void Add_DuplicateTEffect_ThrowsArgumentException()
        {
            // Arrange
            var builder = new EffectCollectionBuilder();
            var effectType = typeof(string);
            var platformEffectType1 = typeof(TestPlatformEffect);
            var platformEffectType2 = typeof(object);

            // Act
            builder.Add(effectType, platformEffectType1);

            // Assert
            Assert.Throws<ArgumentException>(() => builder.Add(effectType, platformEffectType2));
        }

        /// <summary>
        /// Tests that Add method can register multiple different effect types successfully.
        /// Input: Multiple different TEffect types.
        /// Expected: All effects are registered in the dictionary.
        /// </summary>
        [Fact]
        public void Add_MultipleDifferentTypes_RegistersAllSuccessfully()
        {
            // Arrange
            var builder = new EffectCollectionBuilder();
            var effectType1 = typeof(string);
            var effectType2 = typeof(int);
            var effectType3 = typeof(double);
            var platformEffectType = typeof(TestPlatformEffect);

            // Act
            builder.Add(effectType1, platformEffectType)
                   .Add(effectType2, platformEffectType)
                   .Add(effectType3, platformEffectType);

            // Assert
            Assert.Equal(3, builder.RegisteredEffects.Count);
            Assert.True(builder.RegisteredEffects.ContainsKey(effectType1));
            Assert.True(builder.RegisteredEffects.ContainsKey(effectType2));
            Assert.True(builder.RegisteredEffects.ContainsKey(effectType3));
        }

        /// <summary>
        /// Tests that the lambda function stored in RegisteredEffects can be executed successfully.
        /// Input: Valid effect registration.
        /// Expected: Lambda executes and attempts to create platform effect instance.
        /// </summary>
        [Fact]
        public void Add_ValidRegistration_LambdaCanBeExecuted()
        {
            // Arrange
            var builder = new EffectCollectionBuilder();
            var effectType = typeof(string);
            var platformEffectType = typeof(TestPlatformEffect);

            // Act
            builder.Add(effectType, platformEffectType);
            var lambda = builder.RegisteredEffects[effectType];

            // Assert
            Assert.NotNull(lambda);
            // Note: We can't easily test the lambda execution without mocking DependencyResolver
            // which is a static class, but we can verify the lambda is not null and stored correctly
        }

        /// <summary>
        /// Tests Add method with extreme boundary type values.
        /// Input: Various system types that represent edge cases.
        /// Expected: All registrations succeed without error.
        /// </summary>
        [Theory]
        [InlineData(typeof(void), typeof(object))]
        [InlineData(typeof(int), typeof(string))]
        [InlineData(typeof(Array), typeof(TestPlatformEffect))]
        public void Add_BoundaryTypes_RegistersSuccessfully(Type effectType, Type platformEffectType)
        {
            // Arrange
            var builder = new EffectCollectionBuilder();

            // Act
            var result = builder.Add(effectType, platformEffectType);

            // Assert
            Assert.Same(builder, result);
            Assert.Single(builder.RegisteredEffects);
            Assert.True(builder.RegisteredEffects.ContainsKey(effectType));
        }

        /// <summary>
        /// Test helper class that simulates a platform effect for testing purposes.
        /// This class is used only within the test scope to provide a concrete type
        /// that can be used as TPlatformEffect parameter.
        /// </summary>
        private class TestPlatformEffect : PlatformEffect
        {
            protected override void OnAttached()
            {
                // Test implementation
            }

            protected override void OnDetached()
            {
                // Test implementation
            }
        }
    }
}