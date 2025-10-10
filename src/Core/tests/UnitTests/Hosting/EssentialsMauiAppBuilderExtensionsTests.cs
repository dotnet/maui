using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using NSubstitute;
using Xunit;

namespace Core.UnitTests
{
    /// <summary>
    /// Tests for the IEssentialsBuilder.AddAppAction method implementation.
    /// </summary>
    public partial class IEssentialsBuilderTests
    {
    }

    /// <summary>
    /// Tests for the EssentialsExtensions.ConfigureEssentials method
    /// </summary>
    public class EssentialsExtensionsTests
    {
    }

    /// <summary>
    /// Unit tests for the EssentialsInitializer class constructor.
    /// </summary>
    public partial class EssentialsInitializerTests
    {
    }
}

namespace Core.UnitTests.Hosting
{
    public partial class EssentialsBuilderTests
    {
    }
}

namespace Microsoft.Maui.Hosting.UnitTests
{
    public partial class EssentialsRegistrationTests
    {
        /// <summary>
        /// Tests that the EssentialsRegistration constructor properly stores a valid Action delegate.
        /// Input: Valid Action&lt;IEssentialsBuilder&gt; delegate.
        /// Expected: Constructor completes successfully and the action can be invoked later.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithValidAction_StoresActionSuccessfully()
        {
            // Arrange
            var mockEssentialsBuilder = Substitute.For<IEssentialsBuilder>();
            var actionInvoked = false;
            Action<IEssentialsBuilder> testAction = builder => { actionInvoked = true; };

            // Act
            var registration = new EssentialsExtensions.EssentialsRegistration(testAction);

            // Assert
            Assert.NotNull(registration);

            // Verify the action was stored by invoking RegisterEssentialsOptions
            registration.RegisterEssentialsOptions(mockEssentialsBuilder);
            Assert.True(actionInvoked);
        }

        /// <summary>
        /// Tests that the EssentialsRegistration constructor handles null Action parameter.
        /// Input: null Action&lt;IEssentialsBuilder&gt; parameter.
        /// Expected: Constructor completes without throwing (allows null storage).
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithNullAction_AllowsNullAction()
        {
            // Arrange
            Action<IEssentialsBuilder> nullAction = null;

            // Act & Assert
            var registration = new EssentialsExtensions.EssentialsRegistration(nullAction);
            Assert.NotNull(registration);
        }

        /// <summary>
        /// Tests that the EssentialsRegistration constructor stores an action that modifies the builder.
        /// Input: Action that calls methods on the IEssentialsBuilder.
        /// Expected: The stored action properly modifies the builder when invoked.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithBuilderModifyingAction_StoresAndExecutesActionCorrectly()
        {
            // Arrange
            var mockEssentialsBuilder = Substitute.For<IEssentialsBuilder>();
            const string testToken = "test-token-123";
            Action<IEssentialsBuilder> builderAction = builder => builder.UseMapServiceToken(testToken);

            // Act
            var registration = new EssentialsExtensions.EssentialsRegistration(builderAction);

            // Assert
            Assert.NotNull(registration);

            // Verify the action was stored and executes properly
            registration.RegisterEssentialsOptions(mockEssentialsBuilder);
            mockEssentialsBuilder.Received(1).UseMapServiceToken(testToken);
        }

        /// <summary>
        /// Tests that the EssentialsRegistration constructor stores an empty action without issues.
        /// Input: Empty Action delegate that performs no operations.
        /// Expected: Constructor stores the action and it can be invoked without side effects.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithEmptyAction_StoresEmptyActionSuccessfully()
        {
            // Arrange
            var mockEssentialsBuilder = Substitute.For<IEssentialsBuilder>();
            Action<IEssentialsBuilder> emptyAction = builder => { }; // Empty action

            // Act
            var registration = new EssentialsExtensions.EssentialsRegistration(emptyAction);

            // Assert
            Assert.NotNull(registration);

            // Verify the empty action can be invoked without issues
            registration.RegisterEssentialsOptions(mockEssentialsBuilder);
            // No specific assertions needed for empty action, just verify no exceptions thrown
        }

        /// <summary>
        /// Tests that RegisterEssentialsOptions calls the stored action with the provided essentials builder.
        /// Verifies normal execution path where the action is invoked correctly.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void RegisterEssentialsOptions_WithValidEssentials_CallsActionWithCorrectParameter()
        {
            // Arrange
            var mockAction = Substitute.For<Action<IEssentialsBuilder>>();
            var mockEssentials = Substitute.For<IEssentialsBuilder>();
            var registration = new EssentialsExtensions.EssentialsRegistration(mockAction);

            // Act
            registration.RegisterEssentialsOptions(mockEssentials);

            // Assert
            mockAction.Received(1).Invoke(mockEssentials);
        }

        /// <summary>
        /// Tests that RegisterEssentialsOptions propagates exceptions thrown by the stored action.
        /// Verifies that exceptions are not swallowed and bubble up to the caller.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void RegisterEssentialsOptions_WhenActionThrowsException_PropagatesException()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Test exception");
            var mockAction = Substitute.For<Action<IEssentialsBuilder>>();
            var mockEssentials = Substitute.For<IEssentialsBuilder>();
            mockAction.When(x => x.Invoke(Arg.Any<IEssentialsBuilder>())).Do(_ => throw expectedException);
            var registration = new EssentialsExtensions.EssentialsRegistration(mockAction);

            // Act & Assert
            var actualException = Assert.Throws<InvalidOperationException>(() =>
                registration.RegisterEssentialsOptions(mockEssentials));
            Assert.Same(expectedException, actualException);
        }

        /// <summary>
        /// Tests that RegisterEssentialsOptions handles null essentials parameter appropriately.
        /// Verifies behavior when null is passed despite non-nullable parameter type.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void RegisterEssentialsOptions_WithNullEssentials_CallsActionWithNull()
        {
            // Arrange
            var mockAction = Substitute.For<Action<IEssentialsBuilder>>();
            var registration = new EssentialsExtensions.EssentialsRegistration(mockAction);

            // Act
            registration.RegisterEssentialsOptions(null);

            // Assert
            mockAction.Received(1).Invoke(null);
        }

        /// <summary>
        /// Tests that RegisterEssentialsOptions can be called multiple times successfully.
        /// Verifies that each call results in the action being invoked with the respective parameter.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void RegisterEssentialsOptions_MultipleCallsWithDifferentEssentials_CallsActionForEachInvocation()
        {
            // Arrange
            var mockAction = Substitute.For<Action<IEssentialsBuilder>>();
            var mockEssentials1 = Substitute.For<IEssentialsBuilder>();
            var mockEssentials2 = Substitute.For<IEssentialsBuilder>();
            var registration = new EssentialsExtensions.EssentialsRegistration(mockAction);

            // Act
            registration.RegisterEssentialsOptions(mockEssentials1);
            registration.RegisterEssentialsOptions(mockEssentials2);

            // Assert
            mockAction.Received(1).Invoke(mockEssentials1);
            mockAction.Received(1).Invoke(mockEssentials2);
            mockAction.Received(2).Invoke(Arg.Any<IEssentialsBuilder>());
        }
    }
}