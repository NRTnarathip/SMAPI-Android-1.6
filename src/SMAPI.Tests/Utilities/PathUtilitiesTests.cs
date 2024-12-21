using System.Diagnostics.CodeAnalysis;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using StardewModdingAPI.Toolkit.Utilities;

namespace SMAPI.Tests.Utilities;

/// <summary>Unit tests for <see cref="PathUtilities"/>.</summary>
[TestFixture]
[SuppressMessage("ReSharper", "StringLiteralTypo", Justification = "These are standard game install paths.")]
internal class PathUtilitiesTests
{
    /*********
    ** Sample data
    *********/
    /// <summary>Sample paths used in unit tests.</summary>
    public static readonly SamplePath[] SamplePaths = [
        // Windows absolute path
        new(
            OriginalPath: @"C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley",

            Segments: new[] { "C:", "Program Files (x86)", "Steam", "steamapps", "common", "Stardew Valley" },
            SegmentsLimit3: new [] { "C:", "Program Files (x86)", @"Steam\steamapps\common\Stardew Valley" },

            NormalizedOnWindows: @"C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley",
            NormalizedOnUnix: @"C:/Program Files (x86)/Steam/steamapps/common/Stardew Valley"
        ),

        // Windows absolute path (with trailing slash)
        new(
            OriginalPath: @"C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\",

            Segments: new[] { "C:", "Program Files (x86)", "Steam", "steamapps", "common", "Stardew Valley" },
            SegmentsLimit3: new [] { "C:", "Program Files (x86)", @"Steam\steamapps\common\Stardew Valley\" },

            NormalizedOnWindows: @"C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\",
            NormalizedOnUnix: @"C:/Program Files (x86)/Steam/steamapps/common/Stardew Valley/"
        ),

        // Windows relative path
        new(
            OriginalPath: @"Content\Characters\Dialogue\Abigail",

            Segments: new [] { "Content", "Characters", "Dialogue", "Abigail" },
            SegmentsLimit3: new [] { "Content", "Characters", @"Dialogue\Abigail" },

            NormalizedOnWindows: @"Content\Characters\Dialogue\Abigail",
            NormalizedOnUnix: @"Content/Characters/Dialogue/Abigail"
        ),

        // Windows relative path (with directory climbing)
        new(
            OriginalPath: @"..\..\Content",

            Segments: new [] { "..", "..", "Content" },
            SegmentsLimit3: new [] { "..", "..", "Content" },

            NormalizedOnWindows: @"..\..\Content",
            NormalizedOnUnix: @"../../Content"
        ),

        // Windows UNC path
        new(
            OriginalPath: @"\\unc\path",

            Segments: new [] { "unc", "path" },
            SegmentsLimit3: new [] { "unc", "path" },

            NormalizedOnWindows: @"\\unc\path",
            NormalizedOnUnix: "/unc/path" // there's no good way to normalize this on Unix since UNC paths aren't supported; path normalization is meant for asset names anyway, so this test only ensures it returns some sort of sane value
        ),

        // Linux absolute path
        new(
            OriginalPath: @"/home/.steam/steam/steamapps/common/Stardew Valley",

            Segments: new [] { "home", ".steam", "steam", "steamapps", "common", "Stardew Valley" },
            SegmentsLimit3: new [] { "home", ".steam", "steam/steamapps/common/Stardew Valley" },

            NormalizedOnWindows: @"\home\.steam\steam\steamapps\common\Stardew Valley",
            NormalizedOnUnix: @"/home/.steam/steam/steamapps/common/Stardew Valley"
        ),

        // Linux absolute path (with trailing slash)
        new(
            OriginalPath: @"/home/.steam/steam/steamapps/common/Stardew Valley/",

            Segments: new [] { "home", ".steam", "steam", "steamapps", "common", "Stardew Valley" },
            SegmentsLimit3: new [] { "home", ".steam", "steam/steamapps/common/Stardew Valley/" },

            NormalizedOnWindows: @"\home\.steam\steam\steamapps\common\Stardew Valley\",
            NormalizedOnUnix: @"/home/.steam/steam/steamapps/common/Stardew Valley/"
        ),

        // Linux absolute path (with ~)
        new(
            OriginalPath: @"~/.steam/steam/steamapps/common/Stardew Valley",

            Segments: new [] { "~", ".steam", "steam", "steamapps", "common", "Stardew Valley" },
            SegmentsLimit3: new [] { "~", ".steam", "steam/steamapps/common/Stardew Valley" },

            NormalizedOnWindows: @"~\.steam\steam\steamapps\common\Stardew Valley",
            NormalizedOnUnix: @"~/.steam/steam/steamapps/common/Stardew Valley"
        ),

        // Linux relative path
        new(
            OriginalPath: @"Content/Characters/Dialogue/Abigail",

            Segments: new [] { "Content", "Characters", "Dialogue", "Abigail" },
            SegmentsLimit3: new [] { "Content", "Characters", "Dialogue/Abigail" },

            NormalizedOnWindows: @"Content\Characters\Dialogue\Abigail",
            NormalizedOnUnix: @"Content/Characters/Dialogue/Abigail"
        ),

        // Linux relative path (with directory climbing)
        new(
            OriginalPath: @"../../Content",

            Segments: new [] { "..", "..", "Content" },
            SegmentsLimit3: new [] { "..", "..", "Content" },

            NormalizedOnWindows: @"..\..\Content",
            NormalizedOnUnix: @"../../Content"
        ),

        // Mixed directory separators
        new(
            OriginalPath: @"C:\some/mixed\path/separators",

            Segments: new [] { "C:", "some", "mixed", "path", "separators" },
            SegmentsLimit3: new [] { "C:", "some", @"mixed\path/separators" },

            NormalizedOnWindows: @"C:\some\mixed\path\separators",
            NormalizedOnUnix: @"C:/some/mixed/path/separators"
        )
    ];


    /*********
    ** Unit tests
    *********/
    /****
    ** GetSegments
    ****/
    [Test(Description = $"Assert that {nameof(PathUtilities.GetSegments)} splits paths correctly.")]
    [TestCaseSource(nameof(PathUtilitiesTests.SamplePaths))]
    public void GetSegments(SamplePath path)
    {
        // act
        string[] segments = PathUtilities.GetSegments(path.OriginalPath);

        // assert
        path.Segments.Should()
            .HaveCount(segments.Length)
            .And.ContainInOrder(segments);
    }

    [Test(Description = $"Assert that {nameof(PathUtilities.GetSegments)} splits paths correctly when given a limit.")]
    [TestCaseSource(nameof(PathUtilitiesTests.SamplePaths))]
    public void GetSegments_WithLimit(SamplePath path)
    {
        // act
        string[] segments = PathUtilities.GetSegments(path.OriginalPath, 3);

        // assert
        path.SegmentsLimit3.Should()
            .HaveCount(segments.Length)
            .And.ContainInOrder(segments);
    }

    /****
    ** NormalizeAssetName
    ****/
    [Test(Description = $"Assert that {nameof(PathUtilities.NormalizeAssetName)} normalizes paths correctly.")]
    [TestCaseSource(nameof(PathUtilitiesTests.SamplePaths))]
    public void NormalizeAssetName(SamplePath path)
    {
        if (Path.IsPathRooted(path.OriginalPath) || path.OriginalPath.StartsWith('/') || path.OriginalPath.StartsWith('\\'))
            Assert.Ignore("Absolute paths can't be used as asset names.");

        // act
        string normalized = PathUtilities.NormalizeAssetName(path.OriginalPath);

        // assert
        normalized.Should().Be(path.NormalizedOnUnix); // MonoGame uses the Linux format
    }

    /****
    ** NormalizePath
    ****/
    [Test(Description = $"Assert that {nameof(PathUtilities.NormalizePath)} normalizes paths correctly.")]
    [TestCaseSource(nameof(PathUtilitiesTests.SamplePaths))]
    public void NormalizePath(SamplePath path)
    {
        // act
        string normalized = PathUtilities.NormalizePath(path.OriginalPath);

        // assert
        normalized.Should().Be(
#if SMAPI_FOR_WINDOWS
            path.NormalizedOnWindows
#else
                path.NormalizedOnUnix
#endif
        );
    }

    /****
    ** GetRelativePath
    ****/
    [Test(Description = $"Assert that {nameof(PathUtilities.GetRelativePath)} returns the expected values.")]
#if SMAPI_FOR_WINDOWS
    [TestCase(
        @"C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley",
        @"C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\Automate",
        ExpectedResult = @"Mods\Automate"
    )]
    [TestCase(
        @"C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\Automate",
        @"C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Content",
        ExpectedResult = @"..\..\Content"
    )]
    [TestCase(
        @"C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Mods\Automate",
        @"D:\another-drive",
        ExpectedResult = @"D:\another-drive"
    )]
    [TestCase(
        @"\\parent\unc",
        @"\\parent\unc\path\to\child",
        ExpectedResult = @"path\to\child"
    )]
    [TestCase(
        @"C:\same\path",
        @"C:\same\path",
        ExpectedResult = @"."
    )]
    [TestCase(
        @"C:\parent",
        @"C:\PARENT\child",
        ExpectedResult = @"child"
    )]
#else
    [TestCase(
        @"~/.steam/steam/steamapps/common/Stardew Valley",
        @"~/.steam/steam/steamapps/common/Stardew Valley/Mods/Automate",
        ExpectedResult = @"Mods/Automate"
    )]
    [TestCase(
        @"~/.steam/steam/steamapps/common/Stardew Valley/Mods/Automate",
        @"~/.steam/steam/steamapps/common/Stardew Valley/Content",
        ExpectedResult = @"../../Content"
    )]
    [TestCase(
        @"~/.steam/steam/steamapps/common/Stardew Valley/Mods/Automate",
        @"/mnt/another-drive",
        ExpectedResult = @"/mnt/another-drive"
    )]
    [TestCase(
        @"~/same/path",
        @"~/same/path",
        ExpectedResult = @"."
    )]
    [TestCase(
        @"~/parent",
        @"~/PARENT/child",
        ExpectedResult = @"child" // note: incorrect on Linux and sometimes macOS, but not worth the complexity of detecting whether the filesystem is case-sensitive for SMAPI's purposes
    )]
#endif
    public string GetRelativePath(string sourceDir, string targetPath)
    {
        return PathUtilities.GetRelativePath(sourceDir, targetPath);
    }

    /****
    ** IsSlug
    ****/
    [Test(Description = $"Assert that {nameof(PathUtilities.IsSlug)} returns the expected values.")]
    [TestCase("example", ExpectedResult = true)]
    [TestCase("example2", ExpectedResult = true)]
    [TestCase("ex-ample", ExpectedResult = true)]
    [TestCase("ex_ample", ExpectedResult = true)]
    [TestCase("ex.ample", ExpectedResult = true)]
    [TestCase("ex-ample---text", ExpectedResult = true)]
    [TestCase("eXAMple", ExpectedResult = true)]
    [TestCase("example-例子-text", ExpectedResult = true)]

    [TestCase("  example", ExpectedResult = false)]
    [TestCase("example  ", ExpectedResult = false)]
    [TestCase("exa  mple", ExpectedResult = false)]
    [TestCase("exa - mple", ExpectedResult = false)]
    [TestCase("exam!ple", ExpectedResult = false)]
    [TestCase("example?", ExpectedResult = false)]
    [TestCase("#example", ExpectedResult = false)]
    [TestCase("example~", ExpectedResult = false)]
    [TestCase("example/", ExpectedResult = false)]
    [TestCase("example\\", ExpectedResult = false)]
    [TestCase("example|", ExpectedResult = false)]
    public bool IsSlug(string input)
    {
        return PathUtilities.IsSlug(input);
    }

    /****
    ** GetSlug
    ****/
    [Test(Description = $"Assert that {nameof(PathUtilities.CreateSlug)} returns the expected values.")]
    [TestCase("example", ExpectedResult = "example")]
    [TestCase("example2", ExpectedResult = "example2")]
    [TestCase("ex-ample", ExpectedResult = "ex-ample")]
    [TestCase("ex_ample", ExpectedResult = "ex_ample")]
    [TestCase("ex.ample", ExpectedResult = "ex.ample")]
    [TestCase("ex-ample---text", ExpectedResult = "ex-ample-text")]
    [TestCase("eXAMple", ExpectedResult = "eXAMple")]
    [TestCase("example-例子-text", ExpectedResult = "example-例子-text")]

    [TestCase("  example", ExpectedResult = "example")]
    [TestCase("example  ", ExpectedResult = "example-")]
    [TestCase("exa  mple", ExpectedResult = "exa-mple")]
    [TestCase("exa - mple", ExpectedResult = "exa-mple")]
    [TestCase("exam!ple", ExpectedResult = "exam-ple")]
    [TestCase("example?", ExpectedResult = "example-")]
    [TestCase("#example", ExpectedResult = "example")]
    [TestCase("example~", ExpectedResult = "example-")]
    [TestCase("example/", ExpectedResult = "example-")]
    [TestCase("example\\", ExpectedResult = "example-")]
    [TestCase("example|", ExpectedResult = "example-")]
    public string CreateSlug(string input)
    {
        return PathUtilities.CreateSlug(input);
    }


    /*********
    ** Private classes
    *********/
    /// <summary>A sample path in multiple formats.</summary>
    /// <param name="OriginalPath">The original path to pass to the <see cref="PathUtilities"/>.</param>
    /// <param name="Segments">The normalized path segments.</param>
    /// <param name="SegmentsLimit3">The normalized path segments, if we stop segmenting after the second one.</param>
    /// <param name="NormalizedOnWindows">The normalized form on Windows.</param>
    /// <param name="NormalizedOnUnix">The normalized form on Linux or macOS.</param>
    public record SamplePath(string OriginalPath, string[] Segments, string[] SegmentsLimit3, string NormalizedOnWindows, string NormalizedOnUnix)
    {
        public override string ToString()
        {
            return this.OriginalPath;
        }
    }
}
