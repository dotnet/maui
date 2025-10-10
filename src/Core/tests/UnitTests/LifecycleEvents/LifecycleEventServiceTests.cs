using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.LifecycleEvents;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.LifecycleEvents.UnitTests
{
    public class LifecycleEventServiceTests
    {
        /// <summary>
        /// Tests that the constructor completes successfully when null registrations are provided.
        /// Input: null registrations collection.
        /// Expected: Constructor completes without throwing an exception.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithNullRegistrations_DoesNotThrow()
        {
            // Arrange
            IEnumerable<LifecycleEventRegistration> registrations = null;

            // Act & Assert
            var exception = Record.Exception(() => new LifecycleEventService(registrations));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that the constructor completes successfully when an empty registrations collection is provided.
        /// Input: empty registrations collection.
        /// Expected: Constructor completes without throwing an exception.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithEmptyRegistrations_DoesNotThrow()
        {
            // Arrange
            var registrations = new List<LifecycleEventRegistration>();

            // Act & Assert
            var exception = Record.Exception(() => new LifecycleEventService(registrations));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that the constructor calls AddRegistration exactly once when a single registration is provided.
        /// Input: collection with one LifecycleEventRegistration.
        /// Expected: The registration action is called once with the service instance.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithSingleRegistration_CallsAddRegistrationOnce()
        {
            // Arrange
            var mockAction = Substitute.For<Action<ILifecycleBuilder>>();
            var registration = new LifecycleEventRegistration(mockAction);
            var registrations = new List<LifecycleEventRegistration> { registration };

            // Act
            var service = new LifecycleEventService(registrations);

            // Assert
            mockAction.Received(1).Invoke(service);
        }

        /// <summary>
        /// Tests that the constructor calls AddRegistration for each registration when multiple registrations are provided.
        /// Input: collection with multiple LifecycleEventRegistration instances.
        /// Expected: Each registration action is called exactly once with the service instance.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithMultipleRegistrations_CallsAddRegistrationForEach()
        {
            // Arrange
            var mockAction1 = Substitute.For<Action<ILifecycleBuilder>>();
            var mockAction2 = Substitute.For<Action<ILifecycleBuilder>>();
            var mockAction3 = Substitute.For<Action<ILifecycleBuilder>>();

            var registration1 = new LifecycleEventRegistration(mockAction1);
            var registration2 = new LifecycleEventRegistration(mockAction2);
            var registration3 = new LifecycleEventRegistration(mockAction3);

            var registrations = new List<LifecycleEventRegistration> { registration1, registration2, registration3 };

            // Act
            var service = new LifecycleEventService(registrations);

            // Assert
            mockAction1.Received(1).Invoke(service);
            mockAction2.Received(1).Invoke(service);
            mockAction3.Received(1).Invoke(service);
        }

        /// <summary>
        /// Tests that the constructor propagates exceptions thrown by registration actions.
        /// Input: registration that throws an exception when AddRegistration is called.
        /// Expected: The exception is propagated to the caller.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithRegistrationThatThrows_PropagatesException()
        {
            // Arrange
            var expectedException = new InvalidOperationException("Test exception");
            var mockAction = Substitute.For<Action<ILifecycleBuilder>>();
            mockAction.When(x => x.Invoke(Arg.Any<ILifecycleBuilder>())).Do(x => throw expectedException);

            var registration = new LifecycleEventRegistration(mockAction);
            var registrations = new List<LifecycleEventRegistration> { registration };

            // Act & Assert
            var actualException = Assert.Throws<InvalidOperationException>(() => new LifecycleEventService(registrations));
            Assert.Same(expectedException, actualException);
        }

        /// <summary>
        /// Tests that the constructor handles a collection with duplicate registrations correctly.
        /// Input: collection containing the same registration instance multiple times.
        /// Expected: The registration action is called for each occurrence in the collection.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithDuplicateRegistrations_CallsAddRegistrationForEachOccurrence()
        {
            // Arrange
            var mockAction = Substitute.For<Action<ILifecycleBuilder>>();
            var registration = new LifecycleEventRegistration(mockAction);
            var registrations = new List<LifecycleEventRegistration> { registration, registration };

            // Act
            var service = new LifecycleEventService(registrations);

            // Assert
            mockAction.Received(2).Invoke(service);
        }

        /// <summary>
        /// Tests that the constructor processes registrations in order when multiple registrations are provided.
        /// Input: collection with multiple registrations that track call order.
        /// Expected: Registrations are processed in the order they appear in the collection.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithMultipleRegistrations_ProcessesInOrder()
        {
            // Arrange
            var callOrder = new List<int>();
            var mockAction1 = Substitute.For<Action<ILifecycleBuilder>>();
            var mockAction2 = Substitute.For<Action<ILifecycleBuilder>>();

            mockAction1.When(x => x.Invoke(Arg.Any<ILifecycleBuilder>())).Do(x => callOrder.Add(1));
            mockAction2.When(x => x.Invoke(Arg.Any<ILifecycleBuilder>())).Do(x => callOrder.Add(2));

            var registration1 = new LifecycleEventRegistration(mockAction1);
            var registration2 = new LifecycleEventRegistration(mockAction2);
            var registrations = new List<LifecycleEventRegistration> { registration1, registration2 };

            // Act
            var service = new LifecycleEventService(registrations);

            // Assert
            Assert.Equal(new[] { 1, 2 }, callOrder);
        }
    }
}
