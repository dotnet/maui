// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//allow usage of required and record in netstandard2.0

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime;
using System.Runtime.CompilerServices;
using NUnit.Framework;


namespace Microsoft.Maui.Controls.SourceGen.UnitTests
{
#if !NET7_0_OR_GREATER
    /// <summary>
    /// Unit tests for the CompilerFeatureRequiredAttribute constructor.
    /// </summary>
    [TestFixture]
    public partial class CompilerFeatureRequiredAttributeTests
    {
        /// <summary>
        /// Tests CompilerFeatureRequiredAttribute constructor with valid feature names.
        /// Should correctly assign the feature name to the FeatureName property.
        /// </summary>
        [TestCase("TestFeature")]
        [TestCase("AnotherFeature")]
        [TestCase("Feature123")]
        [TestCase("Feature_With_Underscores")]
        [TestCase("Feature-With-Dashes")]
        [TestCase("Feature.With.Dots")]
        [TestCase("UPPERCASE_FEATURE")]
        [TestCase("lowercase_feature")]
        [TestCase("MixedCaseFeature")]
        public void CompilerFeatureRequiredAttribute_ValidFeatureName_SetsFeatureNameProperty(string featureName)
        {
            // Act
            var attribute = new CompilerFeatureRequiredAttribute(featureName);

            // Assert
            Assert.AreEqual(featureName, attribute.FeatureName);
        }

        /// <summary>
        /// Tests CompilerFeatureRequiredAttribute constructor with predefined constant values.
        /// Should correctly assign the constant values to the FeatureName property.
        /// </summary>
        [TestCase("RefStructs")]
        [TestCase("RequiredMembers")]
        public void CompilerFeatureRequiredAttribute_PredefinedConstants_SetsFeatureNameProperty(string constantValue)
        {
            // Act
            var attribute = new CompilerFeatureRequiredAttribute(constantValue);

            // Assert
            Assert.AreEqual(constantValue, attribute.FeatureName);
        }

        /// <summary>
        /// Tests CompilerFeatureRequiredAttribute constructor with class constants.
        /// Should correctly assign the class constant values to the FeatureName property.
        /// </summary>
        [Test]
        public void CompilerFeatureRequiredAttribute_RefStructsConstant_SetsFeatureNameProperty()
        {
            // Act
            var attribute = new CompilerFeatureRequiredAttribute(CompilerFeatureRequiredAttribute.RefStructs);

            // Assert
            Assert.AreEqual(CompilerFeatureRequiredAttribute.RefStructs, attribute.FeatureName);
        }

        /// <summary>
        /// Tests CompilerFeatureRequiredAttribute constructor with RequiredMembers constant.
        /// Should correctly assign the RequiredMembers constant value to the FeatureName property.
        /// </summary>
        [Test]
        public void CompilerFeatureRequiredAttribute_RequiredMembersConstant_SetsFeatureNameProperty()
        {
            // Act
            var attribute = new CompilerFeatureRequiredAttribute(CompilerFeatureRequiredAttribute.RequiredMembers);

            // Assert
            Assert.AreEqual(CompilerFeatureRequiredAttribute.RequiredMembers, attribute.FeatureName);
        }

        /// <summary>
        /// Tests CompilerFeatureRequiredAttribute constructor with empty string.
        /// Should correctly assign empty string to the FeatureName property.
        /// </summary>
        [Test]
        public void CompilerFeatureRequiredAttribute_EmptyString_SetsFeatureNameProperty()
        {
            // Arrange
            string featureName = "";

            // Act
            var attribute = new CompilerFeatureRequiredAttribute(featureName);

            // Assert
            Assert.AreEqual("", attribute.FeatureName);
        }

        /// <summary>
        /// Tests CompilerFeatureRequiredAttribute constructor with whitespace-only strings.
        /// Should correctly assign whitespace strings to the FeatureName property.
        /// </summary>
        [TestCase(" ")]
        [TestCase("\t")]
        [TestCase("\n")]
        [TestCase("\r")]
        [TestCase("   ")]
        [TestCase("\t\n\r")]
        public void CompilerFeatureRequiredAttribute_WhitespaceString_SetsFeatureNameProperty(string featureName)
        {
            // Act
            var attribute = new CompilerFeatureRequiredAttribute(featureName);

            // Assert
            Assert.AreEqual(featureName, attribute.FeatureName);
        }

        /// <summary>
        /// Tests CompilerFeatureRequiredAttribute constructor with strings containing special characters.
        /// Should correctly assign special character strings to the FeatureName property.
        /// </summary>
        [TestCase("Feature@#$%")]
        [TestCase("Feature with spaces")]
        [TestCase("Feature\nWith\nNewlines")]
        [TestCase("Feature\tWith\tTabs")]
        [TestCase("Feature\"With\"Quotes")]
        [TestCase("Feature'With'SingleQuotes")]
        [TestCase("Feature\\With\\Backslashes")]
        [TestCase("Feature/With/Slashes")]
        [TestCase("Feature<With>Brackets")]
        [TestCase("Feature[With]SquareBrackets")]
        [TestCase("Feature{With}CurlyBraces")]
        [TestCase("Feature(With)Parentheses")]
        [TestCase("Feature|With|Pipes")]
        [TestCase("Feature&With&Ampersands")]
        [TestCase("Feature*With*Asterisks")]
        [TestCase("Feature+With+Plus")]
        [TestCase("Feature=With=Equals")]
        [TestCase("Feature?With?QuestionMarks")]
        [TestCase("Feature!With!ExclamationMarks")]
        [TestCase("Feature~With~Tildes")]
        [TestCase("Feature`With`Backticks")]
        [TestCase("Feature^With^Carets")]
        public void CompilerFeatureRequiredAttribute_SpecialCharacters_SetsFeatureNameProperty(string featureName)
        {
            // Act
            var attribute = new CompilerFeatureRequiredAttribute(featureName);

            // Assert
            Assert.AreEqual(featureName, attribute.FeatureName);
        }

        /// <summary>
        /// Tests CompilerFeatureRequiredAttribute constructor with Unicode characters.
        /// Should correctly assign Unicode strings to the FeatureName property.
        /// </summary>
        [TestCase("FeatureΩ")]
        [TestCase("Feature™")]
        [TestCase("Feature©")]
        [TestCase("Feature®")]
        [TestCase("Feature€")]
        [TestCase("Feature中文")]
        [TestCase("Feature🚀")]
        [TestCase("Feature😀")]
        public void CompilerFeatureRequiredAttribute_UnicodeCharacters_SetsFeatureNameProperty(string featureName)
        {
            // Act
            var attribute = new CompilerFeatureRequiredAttribute(featureName);

            // Assert
            Assert.AreEqual(featureName, attribute.FeatureName);
        }

        /// <summary>
        /// Tests CompilerFeatureRequiredAttribute constructor with very long string.
        /// Should correctly assign very long string to the FeatureName property.
        /// </summary>
        [Test]
        public void CompilerFeatureRequiredAttribute_VeryLongString_SetsFeatureNameProperty()
        {
            // Arrange
            string featureName = new string('A', 10000);

            // Act
            var attribute = new CompilerFeatureRequiredAttribute(featureName);

            // Assert
            Assert.AreEqual(featureName, attribute.FeatureName);
            Assert.AreEqual(10000, attribute.FeatureName.Length);
        }

        /// <summary>
        /// Tests CompilerFeatureRequiredAttribute constructor with string containing null character.
        /// Should correctly assign string with null character to the FeatureName property.
        /// </summary>
        [Test]
        public void CompilerFeatureRequiredAttribute_StringWithNullCharacter_SetsFeatureNameProperty()
        {
            // Arrange
            string featureName = "Feature\0WithNull";

            // Act
            var attribute = new CompilerFeatureRequiredAttribute(featureName);

            // Assert
            Assert.AreEqual(featureName, attribute.FeatureName);
        }

        /// <summary>
        /// Tests that IsOptional property has default value when not explicitly set.
        /// Should have default value of false for IsOptional property.
        /// </summary>
        [Test]
        public void CompilerFeatureRequiredAttribute_Constructor_IsOptionalDefaultsFalse()
        {
            // Arrange
            string featureName = "TestFeature";

            // Act
            var attribute = new CompilerFeatureRequiredAttribute(featureName);

            // Assert
            Assert.IsFalse(attribute.IsOptional);
        }
    }
#endif // !NET7_0_OR_GREATER
}
