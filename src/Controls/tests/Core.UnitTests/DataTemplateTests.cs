#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

    public class DataTemplateTests : BaseTestFixture
    {
        [Fact]
        public void CtorInvalid()
        {
            Assert.Throws<ArgumentNullException>(() => new DataTemplate((Func<object>)null));

            Assert.Throws<ArgumentNullException>(() => new DataTemplate((Type)null));
        }

        [Fact]
        public void CreateContent()
        {
            var template = new DataTemplate(() => new MockBindable());
            object obj = template.CreateContent();

            Assert.NotNull(obj);
            Assert.IsType<MockBindable>(obj);
        }

        [Fact]
        public void CreateContentType()
        {
            var template = new DataTemplate(typeof(MockBindable));
            object obj = template.CreateContent();

            Assert.NotNull(obj);
            Assert.IsType<MockBindable>(obj);
        }

        [Fact]
        public void CreateContentValues()
        {
            var template = new DataTemplate(typeof(MockBindable))
            {
                Values = { { MockBindable.TextProperty, "value" } }
            };

            MockBindable bindable = (MockBindable)template.CreateContent();
            Assert.Equal("value", bindable.GetValue(MockBindable.TextProperty));
        }

        [Fact]
        public void CreateContentBindings()
        {
            var template = new DataTemplate(() => new MockBindable())
            {
                Bindings = { { MockBindable.TextProperty, new Binding(".") } }
            };

            MockBindable bindable = (MockBindable)template.CreateContent();
            bindable.BindingContext = "text";
            Assert.Equal("text", bindable.GetValue(MockBindable.TextProperty));
        }

        [Fact]
        public void SetBindingInvalid()
        {
            var template = new DataTemplate(typeof(MockBindable));
            Assert.Throws<ArgumentNullException>(() => template.SetBinding(null, new Binding(".")));
            Assert.Throws<ArgumentNullException>(() => template.SetBinding(MockBindable.TextProperty, null));
        }

        [Fact]
        public void SetBindingOverridesValue()
        {
            var template = new DataTemplate(typeof(MockBindable));
            template.SetValue(MockBindable.TextProperty, "value");
            template.SetBinding(MockBindable.TextProperty, new Binding("."));

            MockBindable bindable = (MockBindable)template.CreateContent();
            Assert.Equal(bindable.GetValue(MockBindable.TextProperty), bindable.BindingContext);

            bindable.BindingContext = "binding";
            Assert.Equal("binding", bindable.GetValue(MockBindable.TextProperty));
        }

        [Fact]
        public void SetValueOverridesBinding()
        {
            var template = new DataTemplate(typeof(MockBindable));
            template.SetBinding(MockBindable.TextProperty, new Binding("."));
            template.SetValue(MockBindable.TextProperty, "value");

            MockBindable bindable = (MockBindable)template.CreateContent();
            Assert.Equal("value", bindable.GetValue(MockBindable.TextProperty));
            bindable.BindingContext = "binding";
            Assert.Equal("value", bindable.GetValue(MockBindable.TextProperty));
        }

        [Fact]
        public void SetValueInvalid()
        {
            var template = new DataTemplate(typeof(MockBindable));
            Assert.Throws<ArgumentNullException>(() => template.SetValue(null, "string"));
        }

        [Fact]
        public void SetValueAndBinding()
        {
            var template = new DataTemplate(typeof(TextCell))
            {
                Bindings = {
                    {TextCell.TextProperty, new Binding ("Text")}
                },
                Values = {
                    {TextCell.TextProperty, "Text"}
                }
            };
            Assert.Throws<InvalidOperationException>(() => template.CreateContent());
        }

        [Fact]
        public void HotReloadTransitionDoesNotCrash()
        {
            // Hot Reload may need to create a template while the content portion isn't ready yet
            // We need to make sure that a call to CreateContent during that time doesn't crash
            var template = new DataTemplate();
            template.CreateContent();
        }

        /// <summary>
        /// Tests that the Id property returns a unique identifier for each DataTemplate instance.
        /// Verifies that the default constructor properly initializes the Id with an incremented value.
        /// Expected result: Each DataTemplate instance should have a unique, positive integer Id.
        /// </summary>
        [Fact]
        public void Id_DefaultConstructor_ReturnsUniqueIdentifier()
        {
            // Arrange
            var template1 = new DataTemplate();
            var template2 = new DataTemplate();

            // Act
            var id1 = template1.Id;
            var id2 = template2.Id;

            // Assert
            Assert.True(id1 > 0);
            Assert.True(id2 > 0);
            Assert.NotEqual(id1, id2);
        }

        /// <summary>
        /// Tests that the Id property returns a unique identifier when using the Type constructor.
        /// Verifies that the Type constructor properly initializes the Id with an incremented value.
        /// Expected result: Each DataTemplate instance should have a unique, positive integer Id.
        /// </summary>
        [Fact]
        public void Id_TypeConstructor_ReturnsUniqueIdentifier()
        {
            // Arrange
            var template1 = new DataTemplate(typeof(Label));
            var template2 = new DataTemplate(typeof(Button));

            // Act
            var id1 = template1.Id;
            var id2 = template2.Id;

            // Assert
            Assert.True(id1 > 0);
            Assert.True(id2 > 0);
            Assert.NotEqual(id1, id2);
        }

        /// <summary>
        /// Tests that the Id property returns a unique identifier when using the Func constructor.
        /// Verifies that the Func constructor properly initializes the Id with an incremented value.
        /// Expected result: Each DataTemplate instance should have a unique, positive integer Id.
        /// </summary>
        [Fact]
        public void Id_FuncConstructor_ReturnsUniqueIdentifier()
        {
            // Arrange
            var template1 = new DataTemplate(() => new Label());
            var template2 = new DataTemplate(() => new Button());

            // Act
            var id1 = template1.Id;
            var id2 = template2.Id;

            // Assert
            Assert.True(id1 > 0);
            Assert.True(id2 > 0);
            Assert.NotEqual(id1, id2);
        }

        /// <summary>
        /// Tests that the Id property returns incrementing values for sequential DataTemplate instances.
        /// Verifies that the underlying counter mechanism works correctly across different constructor types.
        /// Expected result: Each subsequent DataTemplate instance should have an Id that is exactly 1 greater than the previous.
        /// </summary>
        [Fact]
        public void Id_SequentialInstances_ReturnsIncrementingValues()
        {
            // Arrange & Act
            var template1 = new DataTemplate();
            var template2 = new DataTemplate(typeof(Label));
            var template3 = new DataTemplate(() => new Button());

            var id1 = template1.Id;
            var id2 = template2.Id;
            var id3 = template3.Id;

            // Assert
            Assert.Equal(id1 + 1, id2);
            Assert.Equal(id2 + 1, id3);
        }

        /// <summary>
        /// Tests that the Id property maintains uniqueness when DataTemplate instances are created concurrently.
        /// Verifies thread-safe behavior of the internal counter mechanism.
        /// Expected result: All concurrently created DataTemplate instances should have unique Ids with no duplicates.
        /// </summary>
        [Fact]
        public void Id_ConcurrentInstances_ReturnsUniqueIdentifiers()
        {
            // Arrange
            const int instanceCount = 100;
            var templates = new DataTemplate[instanceCount];
            var ids = new int[instanceCount];

            // Act
            Parallel.For(0, instanceCount, i =>
            {
                templates[i] = new DataTemplate();
                ids[i] = templates[i].Id;
            });

            // Assert
            var uniqueIds = ids.Distinct().ToArray();
            Assert.Equal(instanceCount, uniqueIds.Length);
            Assert.All(ids, id => Assert.True(id > 0));
        }

        /// <summary>
        /// Tests that the Id property returns the same value on multiple accesses to the same instance.
        /// Verifies that the Id property is consistent and doesn't change after initialization.
        /// Expected result: Multiple calls to Id on the same instance should return the identical value.
        /// </summary>
        [Fact]
        public void Id_MultipleAccesses_ReturnsSameValue()
        {
            // Arrange
            var template = new DataTemplate();

            // Act
            var id1 = template.Id;
            var id2 = template.Id;
            var id3 = template.Id;

            // Assert
            Assert.Equal(id1, id2);
            Assert.Equal(id2, id3);
        }

        /// <summary>
        /// Tests that the Id property works correctly with null Type parameter edge case.
        /// Verifies behavior when the Type constructor is called with null.
        /// Expected result: Should throw ArgumentNullException due to base constructor validation.
        /// </summary>
        [Fact]
        public void Id_TypeConstructorWithNull_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new DataTemplate((Type)null));
        }

        /// <summary>
        /// Tests that the Id property works correctly with null Func parameter edge case.
        /// Verifies behavior when the Func constructor is called with null.
        /// Expected result: Should throw ArgumentNullException due to base constructor validation.
        /// </summary>
        [Fact]
        public void Id_FuncConstructorWithNull_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new DataTemplate((Func<object>)null));
        }
    }
}