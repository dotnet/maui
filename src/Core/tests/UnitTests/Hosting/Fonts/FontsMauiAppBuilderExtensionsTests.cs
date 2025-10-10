using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Hosting.Internal;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Hosting.UnitTests
{
    public partial class FontInitializerTests
    {
        /// <summary>
        /// Tests that Initialize does nothing when _fontsRegistrations is null.
        /// Expected result: No operations should be performed and no exceptions thrown.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Initialize_WhenFontsRegistrationsIsNull_DoesNothing()
        {
            // Arrange
            var fontRegistrar = Substitute.For<IFontRegistrar>();
            var fontInitializer = new FontsMauiAppBuilderExtensions.FontInitializer(null, fontRegistrar);
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act
            fontInitializer.Initialize(serviceProvider);

            // Assert
            fontRegistrar.DidNotReceive().Register(Arg.Any<string>(), Arg.Any<string>());
            fontRegistrar.DidNotReceive().Register(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Assembly>());
        }

        /// <summary>
        /// Tests that Initialize creates FontCollection but registers no fonts when _fontsRegistrations is empty.
        /// Expected result: FontCollection is created but no fonts are registered.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Initialize_WhenFontsRegistrationsIsEmpty_RegistersNoFonts()
        {
            // Arrange
            var fontRegistrar = Substitute.For<IFontRegistrar>();
            var emptyRegistrations = new List<FontsMauiAppBuilderExtensions.FontsRegistration>();
            var fontInitializer = new FontsMauiAppBuilderExtensions.FontInitializer(emptyRegistrations, fontRegistrar);
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act
            fontInitializer.Initialize(serviceProvider);

            // Assert
            fontRegistrar.DidNotReceive().Register(Arg.Any<string>(), Arg.Any<string>());
            fontRegistrar.DidNotReceive().Register(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Assembly>());
        }

        /// <summary>
        /// Tests that Initialize registers fonts with null Assembly using the 2-parameter Register method.
        /// Expected result: Fonts with null Assembly are registered using Register(filename, alias).
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Initialize_WhenFontHasNullAssembly_CallsTwoParameterRegister()
        {
            // Arrange
            var fontRegistrar = Substitute.For<IFontRegistrar>();
            var registrations = new List<FontsMauiAppBuilderExtensions.FontsRegistration>
            {
                new FontsMauiAppBuilderExtensions.FontsRegistration(fonts =>
                {
                    fonts.Add(new FontDescriptor("font1.ttf", "Font1", null));
                    fonts.Add(new FontDescriptor("font2.ttf", "Font2", null));
                })
            };
            var fontInitializer = new FontsMauiAppBuilderExtensions.FontInitializer(registrations, fontRegistrar);
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act
            fontInitializer.Initialize(serviceProvider);

            // Assert
            fontRegistrar.Received(1).Register("font1.ttf", "Font1");
            fontRegistrar.Received(1).Register("font2.ttf", "Font2");
            fontRegistrar.DidNotReceive().Register(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Assembly>());
        }

        /// <summary>
        /// Tests that Initialize registers fonts with non-null Assembly using the 3-parameter Register method.
        /// Expected result: Fonts with Assembly are registered using Register(filename, alias, assembly).
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Initialize_WhenFontHasAssembly_CallsThreeParameterRegister()
        {
            // Arrange
            var fontRegistrar = Substitute.For<IFontRegistrar>();
            var assembly = Assembly.GetExecutingAssembly();
            var registrations = new List<FontsMauiAppBuilderExtensions.FontsRegistration>
            {
                new FontsMauiAppBuilderExtensions.FontsRegistration(fonts =>
                {
                    fonts.Add(new FontDescriptor("font1.ttf", "Font1", assembly));
                    fonts.Add(new FontDescriptor("font2.ttf", null, assembly));
                })
            };
            var fontInitializer = new FontsMauiAppBuilderExtensions.FontInitializer(registrations, fontRegistrar);
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act
            fontInitializer.Initialize(serviceProvider);

            // Assert
            fontRegistrar.Received(1).Register("font1.ttf", "Font1", assembly);
            fontRegistrar.Received(1).Register("font2.ttf", null, assembly);
            fontRegistrar.DidNotReceive().Register(Arg.Any<string>(), Arg.Any<string>());
        }

        /// <summary>
        /// Tests that Initialize handles mixed fonts with both null and non-null Assembly values.
        /// Expected result: Fonts are registered using appropriate methods based on Assembly value.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Initialize_WhenMixedAssemblyValues_CallsAppropriateRegisterMethods()
        {
            // Arrange
            var fontRegistrar = Substitute.For<IFontRegistrar>();
            var assembly = Assembly.GetExecutingAssembly();
            var registrations = new List<FontsMauiAppBuilderExtensions.FontsRegistration>
            {
                new FontsMauiAppBuilderExtensions.FontsRegistration(fonts =>
                {
                    fonts.Add(new FontDescriptor("nullAssembly.ttf", "NullAlias", null));
                    fonts.Add(new FontDescriptor("withAssembly.ttf", "WithAlias", assembly));
                    fonts.Add(new FontDescriptor("anotherNull.ttf", null, null));
                })
            };
            var fontInitializer = new FontsMauiAppBuilderExtensions.FontInitializer(registrations, fontRegistrar);
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act
            fontInitializer.Initialize(serviceProvider);

            // Assert
            fontRegistrar.Received(1).Register("nullAssembly.ttf", "NullAlias");
            fontRegistrar.Received(1).Register("anotherNull.ttf", null);
            fontRegistrar.Received(1).Register("withAssembly.ttf", "WithAlias", assembly);
        }

        /// <summary>
        /// Tests that Initialize processes multiple FontsRegistration objects correctly.
        /// Expected result: All FontsRegistration objects are processed and their fonts registered.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Initialize_WhenMultipleRegistrations_ProcessesAllRegistrations()
        {
            // Arrange
            var fontRegistrar = Substitute.For<IFontRegistrar>();
            var assembly = Assembly.GetExecutingAssembly();
            var registrations = new List<FontsMauiAppBuilderExtensions.FontsRegistration>
            {
                new FontsMauiAppBuilderExtensions.FontsRegistration(fonts =>
                {
                    fonts.Add(new FontDescriptor("reg1font1.ttf", "Reg1Font1", null));
                }),
                new FontsMauiAppBuilderExtensions.FontsRegistration(fonts =>
                {
                    fonts.Add(new FontDescriptor("reg2font1.ttf", "Reg2Font1", assembly));
                    fonts.Add(new FontDescriptor("reg2font2.ttf", "Reg2Font2", null));
                }),
                new FontsMauiAppBuilderExtensions.FontsRegistration(fonts =>
                {
                    fonts.Add(new FontDescriptor("reg3font1.ttf", null, assembly));
                })
            };
            var fontInitializer = new FontsMauiAppBuilderExtensions.FontInitializer(registrations, fontRegistrar);
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act
            fontInitializer.Initialize(serviceProvider);

            // Assert
            fontRegistrar.Received(1).Register("reg1font1.ttf", "Reg1Font1");
            fontRegistrar.Received(1).Register("reg2font2.ttf", "Reg2Font2");
            fontRegistrar.Received(1).Register("reg2font1.ttf", "Reg2Font1", assembly);
            fontRegistrar.Received(1).Register("reg3font1.ttf", null, assembly);
        }

        /// <summary>
        /// Tests that Initialize handles edge case where registration action adds no fonts.
        /// Expected result: No fonts are registered when registration action doesn't add any fonts.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Initialize_WhenRegistrationAddsNoFonts_RegistersNoFonts()
        {
            // Arrange
            var fontRegistrar = Substitute.For<IFontRegistrar>();
            var registrations = new List<FontsMauiAppBuilderExtensions.FontsRegistration>
            {
                new FontsMauiAppBuilderExtensions.FontsRegistration(fonts =>
                {
                    // Do nothing - add no fonts
                })
            };
            var fontInitializer = new FontsMauiAppBuilderExtensions.FontInitializer(registrations, fontRegistrar);
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act
            fontInitializer.Initialize(serviceProvider);

            // Assert
            fontRegistrar.DidNotReceive().Register(Arg.Any<string>(), Arg.Any<string>());
            fontRegistrar.DidNotReceive().Register(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Assembly>());
        }

        /// <summary>
        /// Tests that Initialize handles fonts with empty and whitespace filenames.
        /// Expected result: Fonts with empty/whitespace filenames are still registered.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Initialize_WhenFontHasEmptyOrWhitespaceFilename_StillRegisters()
        {
            // Arrange
            var fontRegistrar = Substitute.For<IFontRegistrar>();
            var registrations = new List<FontsMauiAppBuilderExtensions.FontsRegistration>
            {
                new FontsMauiAppBuilderExtensions.FontsRegistration(fonts =>
                {
                    fonts.Add(new FontDescriptor("", "EmptyFilename", null));
                    fonts.Add(new FontDescriptor("   ", "WhitespaceFilename", null));
                })
            };
            var fontInitializer = new FontsMauiAppBuilderExtensions.FontInitializer(registrations, fontRegistrar);
            var serviceProvider = Substitute.For<IServiceProvider>();

            // Act
            fontInitializer.Initialize(serviceProvider);

            // Assert
            fontRegistrar.Received(1).Register("", "EmptyFilename");
            fontRegistrar.Received(1).Register("   ", "WhitespaceFilename");
        }
    }
}
