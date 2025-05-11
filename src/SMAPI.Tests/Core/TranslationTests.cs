using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Framework.ModHelpers;
using StardewModdingAPI.Framework.ModLoading;
using StardewModdingAPI.Toolkit.Serialization.Models;
using StardewValley;

namespace SMAPI.Tests.Core;

/// <summary>Unit tests for <see cref="TranslationHelper"/> and <see cref="Translation"/>.</summary>
[TestFixture]
public class TranslationTests
{
    /*********
    ** Data
    *********/
    /// <summary>Sample translation text for unit tests.</summary>
    public static string?[] Samples = [null, "", "  ", "boop", "  boop  "];

    /// <summary>The locale to check for unit tests.</summary>
    public const string TestLocale = "en";

    /// <summary>A translation key which exists in both <c>default.json</c> and <c>en.json</c>.</summary>
    public const string DefaultAndLocalKey = "key A";

    /// <summary>A translation key which only exists in <c>en.json</c>.</summary>
    public const string LocalKey = "key B";

    /// <summary>A translation key which only exists in <c>default.json</c>.</summary>
    public const string DefaultKey = "key C";


    /*********
    ** Unit tests
    *********/
    /****
    ** Translation helper
    ****/
    [Test(Description = "Assert that the translation helper correctly handles no translations.")]
    public void Helper_HandlesNoTranslations()
    {
        // arrange
        var data = new Dictionary<string, IDictionary<string, string>>();
        ITranslationHelper helper = this.GetSampleHelper(data);

        // act
        Translation translation = helper.Get("key");
        Translation[] translationList = helper.GetTranslations().ToArray();

        // assert
        helper.Locale.Should().Be(TranslationTests.TestLocale);
        helper.LocaleEnum.Should().Be(LocalizedContentManager.LanguageCode.en);
        translationList.Should().NotBeNull().And.BeEmpty();

        translation.Should().NotBeNull();
        translation.ToString().Should().Be(this.GetPlaceholderText("key"));
    }

    [Test(Description = $"Assert that {nameof(ITranslationHelper.ContainsKey)} returns the expected value.")]
    [TestCase(TranslationTests.DefaultAndLocalKey, ExpectedResult = true)]
    [TestCase(TranslationTests.LocalKey, ExpectedResult = true)]
    [TestCase(TranslationTests.DefaultKey, ExpectedResult = true)]
    [TestCase("missing-key", ExpectedResult = false)]
    public bool Helper_ContainsKey(string key)
    {
        // arrange
        ITranslationHelper helper = this.GetSampleHelper();

        // act & assert
        return helper.ContainsKey(key);
    }

    [Test(Description = $"Assert that {nameof(ITranslationHelper.GetKeys)} returns the expected keys.")]
    public void Helper_GetKeys()
    {
        // arrange
        string[] expectedKeys = [TranslationTests.DefaultKey, TranslationTests.DefaultAndLocalKey, TranslationTests.LocalKey];
        ITranslationHelper helper = this.GetSampleHelper();

        // act
        IEnumerable<string> actualKeys = helper.GetKeys();

        // assert
        actualKeys.Should().BeEquivalentTo(expectedKeys);
    }

    [Test(Description = "Assert that the translation helper returns the expected translations correctly.")]
    public void Helper_GetTranslations_ReturnsExpectedText()
    {
        // arrange
        var expected = this.GetExpectedTranslations();
        TranslationHelper helper = this.GetSampleHelper();

        // act
        var actual = new Dictionary<string, Translation[]?>();
        foreach (string locale in expected.Keys)
        {
            this.AssertSetLocale(helper, locale, LocalizedContentManager.LanguageCode.en);
            actual[locale] = helper.GetTranslations().ToArray();
        }

        // assert
        foreach (string locale in expected.Keys)
        {
            actual[locale].Should()
                .NotBeNull($"the translations for {locale} should be set")
                .And.BeEquivalentTo(expected[locale], $"the translations for {locale} should match the input values");
        }
    }

    [Test(Description = "Assert that the translations returned by the helper has the expected text.")]
    public void Helper_Get_ReturnsExpectedText()
    {
        // arrange
        var expected = this.GetExpectedTranslations();
        TranslationHelper helper = this.GetSampleHelper();

        // act
        var actual = new Dictionary<string, Translation[]>();
        foreach (string locale in expected.Keys)
        {
            this.AssertSetLocale(helper, locale, LocalizedContentManager.LanguageCode.en);

            List<Translation> translations = [];
            foreach (Translation translation in expected[locale])
                translations.Add(helper.Get(translation.Key));
            actual[locale] = translations.ToArray();
        }

        // assert
        foreach (string locale in expected.Keys)
        {
            actual[locale].Should()
                .NotBeNull($"the translations for {locale} should be set")
                .And.BeEquivalentTo(expected[locale], $"the translations for {locale} should match the input values");
        }
    }

    /****
    ** Translation
    ****/
    [Test(Description = "Assert that HasValue returns the expected result for various inputs.")]
    [TestCase(null, ExpectedResult = false)]
    [TestCase("", ExpectedResult = false)]
    [TestCase("  ", ExpectedResult = true)]
    [TestCase("boop", ExpectedResult = true)]
    [TestCase("  boop  ", ExpectedResult = true)]
    public bool Translation_HasValue(string? text)
    {
        return new Translation("pt-BR", "key", text).HasValue();
    }

    [Test(Description = "Assert that the translation's ToString method returns the expected text for various inputs.")]
    public void Translation_ToString([ValueSource(nameof(TranslationTests.Samples))] string? text)
    {
        // act
        Translation translation = new("pt-BR", "key", text);

        // assert
        if (!string.IsNullOrEmpty(text))
            translation.ToString().Should().Be(text, "the translation should match the valid input");
        else
            translation.ToString().Should().Be(this.GetPlaceholderText("key"), "the translation should match the placeholder given a null or empty input");
    }

    [Test(Description = "Assert that the translation's implicit string conversion returns the expected text for various inputs.")]
    public void Translation_ImplicitStringConversion([ValueSource(nameof(TranslationTests.Samples))] string? text)
    {
        // act
        Translation translation = new("pt-BR", "key", text);

        // assert
        if (!string.IsNullOrEmpty(text))
            ((string?)translation).Should().Be(text, "the translation should match the valid input");
        else
            ((string?)translation).Should().Be(this.GetPlaceholderText("key"), "the translation should match the placeholder given a null or empty input");
    }

    [Test(Description = "Assert that the translation returns the expected text for a translation containing gender switch blocks.")]
    [TestCase("Hello ${lad^lass^there}$ on this fine ${male¦female¦other}$ day.", Gender.Male, ExpectedResult = "Hello lad on this fine male day.")]
    [TestCase("Hello ${lad^lass^there}$ on this fine ${male¦female¦other}$ day.", Gender.Female, ExpectedResult = "Hello lass on this fine female day.")]
    [TestCase("Hello ${lad^lass^there}$ on this fine ${male¦female¦other}$ day.", Gender.Undefined, ExpectedResult = "Hello there on this fine other day.")]
    public string Translation_WithGenderSwitchBlocks(string text, Gender gender)
    {
        // arrange
        Translation translation = new("pt-BR", "key", text)
        {
            ForceGender = () => gender
        };

        // assert
        return translation.ToString();
    }

    [Test(Description = "Assert that the translation returns the expected text given a use-placeholder setting.")]
    public void Translation_UsePlaceholder([Values(true, false)] bool value, [ValueSource(nameof(TranslationTests.Samples))] string? text)
    {
        // act
        Translation translation = new Translation("pt-BR", "key", text).UsePlaceholder(value);

        // assert
        if (!string.IsNullOrEmpty(text))
            translation.ToString().Should().Be(text, "the translation should match the valid input");
        else if (!value)
            translation.ToString().Should().Be(text, "the translation should return the text as-is given a null or empty input with the placeholder disabled");
        else
            translation.ToString().Should().Be(this.GetPlaceholderText("key"), "the translation should match the placeholder given a null or empty input with the placeholder enabled");
    }

    [Test(Description = "Assert that the translation returns the expected text after setting the default.")]
    public void Translation_Default([ValueSource(nameof(TranslationTests.Samples))] string? text, [ValueSource(nameof(TranslationTests.Samples))] string? @default)
    {
        // act
        Translation translation = new Translation("pt-BR", "key", text).Default(@default);

        // assert
        if (!string.IsNullOrEmpty(text))
            translation.ToString().Should().Be(text, "the translation should match the valid base text");
        else if (!string.IsNullOrEmpty(@default))
            translation.ToString().Should().Be(@default, "the translation should match the default text, given a null or empty base text and valid default.");
        else
            translation.ToString().Should().Be(this.GetPlaceholderText("key"), translation.ToString(), "the translation should match the placeholder, given a null or empty base text and no default text");
    }

    [Test(Description = "Assert that the translation returns the expected text after setting the default to a value containing tokens.")]
    public void Translation_Default_WithTokens()
    {
        // act
        Translation translation = new Translation("pt-BR", "key", null).Default("The {{token}} is {{value}}.").Tokens(new { token = "token value", value = "some value" });

        // assert
        translation.ToString().Should().Be("The token value is some value.");
    }

    [Test(Description = "Assert that the translation returns the expected text after setting the default to a value containing gender switch blocks.")]
    [TestCase("Hello ${lad^lass^there}$ on this fine ${male¦female¦other}$ day.", Gender.Male, ExpectedResult = "Hello lad on this fine male day.")]
    [TestCase("Hello ${lad^lass^there}$ on this fine ${male¦female¦other}$ day.", Gender.Female, ExpectedResult = "Hello lass on this fine female day.")]
    [TestCase("Hello ${lad^lass^there}$ on this fine ${male¦female¦other}$ day.", Gender.Undefined, ExpectedResult = "Hello there on this fine other day.")]
    public string Translation_Default_WithGenderSwitchBlocks(string placeholder, Gender gender)
    {
        // arrange
        Translation translation = new("pt-BR", "key", null)
        {
            ForceGender = () => gender
        };
        translation = translation.Default(placeholder);

        // assert
        return translation.ToString();
    }

    /****
    ** Translation tokens
    ****/
    [Test(Description = "Assert that multiple translation tokens are replaced correctly regardless of the token structure.")]
    public void Translation_Tokens([Values("anonymous object", "class", "IDictionary<string, object>", "IDictionary<string, string>")] string structure)
    {
        // arrange
        string start = Guid.NewGuid().ToString("N");
        string middle = Guid.NewGuid().ToString("N");
        string end = Guid.NewGuid().ToString("N");
        const string input = "{{start}} tokens are properly replaced (including {{middle}} {{  MIDdlE}}) {{end}}";
        string expected = $"{start} tokens are properly replaced (including {middle} {middle}) {end}";

        // act
        Translation translation = new("pt-BR", "key", input);
        switch (structure)
        {
            case "anonymous object":
                translation = translation.Tokens(new { start, middle, end });
                break;

            case "class":
                translation = translation.Tokens(new TokenModel(start, middle, end));
                break;

            case "IDictionary<string, object>":
                translation = translation.Tokens(new Dictionary<string, object> { ["start"] = start, ["middle"] = middle, ["end"] = end });
                break;

            case "IDictionary<string, string>":
                translation = translation.Tokens(new Dictionary<string, string> { ["start"] = start, ["middle"] = middle, ["end"] = end });
                break;

            default:
                throw new NotSupportedException($"Unknown structure '{structure}'.");
        }

        // assert
        translation.ToString().Should().Be(expected);
    }

    [Test(Description = "Assert that the translation can replace tokens in all valid formats.")]
    [TestCase("{{value}}", "value")]
    [TestCase("{{ value }}", "value")]
    [TestCase("{{value       }}", "value")]
    [TestCase("{{ the_value }}", "the_value")]
    [TestCase("{{ the.value_here }}", "the.value_here")]
    [TestCase("{{ the_value-here.... }}", "the_value-here....")]
    [TestCase("{{ tHe_vALuE-HEre.... }}", "tHe_vALuE-HEre....")]
    public void Translation_Tokens_ValidFormats(string text, string key)
    {
        // arrange
        string value = Guid.NewGuid().ToString("N");

        // act
        Translation translation = new Translation("pt-BR", "key", text).Tokens(new Dictionary<string, object> { [key] = value });

        // assert
        translation.ToString().Should().Be(value);
    }

    [Test(Description = "Assert that translation tokens are case-insensitive and surrounding-whitespace-insensitive.")]
    [TestCase("{{value}}", "value")]
    [TestCase("{{VaLuE}}", "vAlUe")]
    [TestCase("{{VaLuE   }}", "   vAlUe")]
    public void Translation_Tokens_KeysAreNormalized(string text, string key)
    {
        // arrange
        string value = Guid.NewGuid().ToString("N");

        // act
        Translation translation = new Translation("pt-BR", "key", text).Tokens(new Dictionary<string, object> { [key] = value });

        // assert
        translation.ToString().Should().Be(value);
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Set a translation helper's locale and assert that it was set correctly.</summary>
    /// <param name="helper">The translation helper to change.</param>
    /// <param name="locale">The expected locale.</param>
    /// <param name="localeEnum">The expected game language code.</param>
    private void AssertSetLocale(TranslationHelper helper, string locale, LocalizedContentManager.LanguageCode localeEnum)
    {
        helper.SetLocale(locale, localeEnum);
        helper.Locale.Should().Be(locale);
        helper.LocaleEnum.Should().Be(localeEnum);
    }

    /// <summary>Get sample raw translations to input.</summary>
    private IDictionary<string, IDictionary<string, string>> GetSampleData()
    {
        return new Dictionary<string, IDictionary<string, string>>
        {
            ["default"] = new Dictionary<string, string>
            {
                [TranslationTests.DefaultAndLocalKey] = "default A",
                [TranslationTests.DefaultKey] = "default C"
            },
            [TranslationTests.TestLocale] = new Dictionary<string, string>
            {
                [TranslationTests.DefaultAndLocalKey] = "en A",
                [TranslationTests.LocalKey] = "en B"
            },
            ["en-US"] = new Dictionary<string, string>(),
            ["zzz"] = new Dictionary<string, string>
            {
                [TranslationTests.DefaultAndLocalKey] = "zzz A"
            }
        };
    }

    /// <summary>Get the expected translation output given <see cref="TranslationTests.GetSampleData"/>, based on the expected locale fallback.</summary>
    private IDictionary<string, Translation[]> GetExpectedTranslations()
    {
        var expected = new Dictionary<string, Translation[]>
        {
            ["default"] =
            [
                new Translation("default", TranslationTests.DefaultAndLocalKey, "default A"),
                new Translation("default", TranslationTests.DefaultKey, "default C")
            ],
            [TranslationTests.TestLocale] =
            [
                new Translation(TranslationTests.TestLocale, TranslationTests.DefaultAndLocalKey, "en A"),
                new Translation(TranslationTests.TestLocale, TranslationTests.LocalKey, "en B"),
                new Translation(TranslationTests.TestLocale, TranslationTests.DefaultKey, "default C")
            ],
            ["zzz"] =
            [
                new Translation("zzz", TranslationTests.DefaultAndLocalKey, "zzz A"),
                new Translation("zzz", TranslationTests.DefaultKey, "default C")
            ]
        };
        expected["en-us"] = expected[TranslationTests.TestLocale].ToArray();
        return expected;
    }

    /// <summary>Get a translation helper with sample data matching <see cref="GetSampleData"/>.</summary>
    private TranslationHelper GetSampleHelper()
    {
        return this.GetSampleHelper(this.GetSampleData());
    }

    /// <summary>Get a translation helper with sample data matching <see cref="GetSampleData"/>.</summary>
    /// <param name="data">The translation data to use.</param>
    private TranslationHelper GetSampleHelper(IDictionary<string, IDictionary<string, string>> data)
    {
        return new TranslationHelper(this.CreateModMetadata(), TranslationTests.TestLocale, LocalizedContentManager.LanguageCode.en).SetTranslations(data);
    }

    /// <summary>Get the default placeholder text when a translation is missing.</summary>
    /// <param name="key">The translation key.</param>
    private string GetPlaceholderText(string key)
    {
        return string.Format(Translation.PlaceholderText, key);
    }

    /// <summary>Create a fake mod manifest.</summary>
    private IModMetadata CreateModMetadata()
    {
        string id = $"smapi.unit-tests.fake-mod-{Guid.NewGuid():N}";

        string tempPath = Path.Combine(Path.GetTempPath(), id);
        return new ModMetadata(
            displayName: "Mod Display Name",
            directoryPath: tempPath,
            rootPath: tempPath,
            manifest: new Manifest(
                uniqueId: id,
                name: "Mod Name",
                author: "Mod Author",
                description: "Mod Description",
                version: new SemanticVersion(1, 0, 0)
            ),
            dataRecord: null,
            isIgnored: false
        );
    }


    /*********
    ** Test models
    *********/
    /// <summary>A model used to test token support.</summary>
    [SuppressMessage("ReSharper", "NotAccessedField.Local", Justification = "Used dynamically via translation helper.")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local", Justification = "Used dynamically via translation helper.")]
    private class TokenModel
    {
        /*********
        ** Accessors
        *********/
        /// <summary>A sample token property.</summary>
        public string Start { get; }

        /// <summary>A sample token property.</summary>
        public string Middle { get; }

        /// <summary>A sample token field.</summary>
        public string End;


        /*********
        ** public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="start">A sample token property.</param>
        /// <param name="middle">A sample token field.</param>
        /// <param name="end">A sample token property.</param>
        public TokenModel(string start, string middle, string end)
        {
            this.Start = start;
            this.Middle = middle;
            this.End = end;
        }
    }
}
