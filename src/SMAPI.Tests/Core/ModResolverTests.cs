using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Framework;
using StardewModdingAPI.Framework.ModLoading;
using StardewModdingAPI.Toolkit;
using StardewModdingAPI.Toolkit.Framework.ModData;
using StardewModdingAPI.Toolkit.Serialization.Models;
using StardewModdingAPI.Toolkit.Utilities.PathLookups;
using SemanticVersion = StardewModdingAPI.SemanticVersion;

namespace SMAPI.Tests.Core;

/// <summary>Unit tests for <see cref="ModResolver"/>.</summary>
[TestFixture]
public class ModResolverTests
{
    /*********
    ** Unit tests
    *********/
    /****
    ** ReadManifests
    ****/
    [Test(Description = "Assert that the resolver correctly returns an empty list if there are no mods installed.")]
    public void ReadBasicManifest_NoMods_ReturnsEmptyList()
    {
        // arrange
        string rootFolder = this.GetTempFolderPath();
        Directory.CreateDirectory(rootFolder);

        // act
        IModMetadata[] mods = new ModResolver().ReadManifests(new ModToolkit(), rootFolder, new ModDatabase(), useCaseInsensitiveFilePaths: true).ToArray();

        // assert
        mods.Should().BeEmpty("it should match number of mods input");

        // cleanup
        Directory.Delete(rootFolder, recursive: true);
    }

    [Test(Description = "Assert that the resolver correctly returns a failed metadata if there's an empty mod folder.")]
    public void ReadBasicManifest_EmptyModFolder_ReturnsFailedManifest()
    {
        // arrange
        string rootFolder = this.GetTempFolderPath();
        string modFolder = Path.Combine(rootFolder, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(modFolder);

        // act
        IModMetadata[] mods = new ModResolver().ReadManifests(new ModToolkit(), rootFolder, new ModDatabase(), useCaseInsensitiveFilePaths: true).ToArray();
        IModMetadata? mod = mods.FirstOrDefault();

        // assert
        mods.Should().HaveCount(1, "it should match number of mods input");
        mod!.Status.Should().Be(ModMetadataStatus.Failed);
        mod.Error.Should().NotBeNull();

        // cleanup
        Directory.Delete(rootFolder, recursive: true);
    }

    [Test(Description = "Assert that the resolver correctly reads manifest data from a randomized file.")]
    public void ReadBasicManifest_CanReadFile()
    {
        // create manifest data
        IDictionary<string, object> originalDependency = new Dictionary<string, object>
        {
            [nameof(IManifestDependency.UniqueID)] = Sample.String()
        };
        IDictionary<string, object> original = new Dictionary<string, object>
        {
            [nameof(IManifest.Name)] = Sample.String(),
            [nameof(IManifest.Author)] = Sample.String(),
            [nameof(IManifest.Version)] = new SemanticVersion(Sample.Int(), Sample.Int(), Sample.Int(), Sample.String()),
            [nameof(IManifest.Description)] = Sample.String(),
            [nameof(IManifest.UniqueID)] = $"{Sample.String()}.{Sample.String()}",
            [nameof(IManifest.EntryDll)] = $"{Sample.String()}.dll",
            [nameof(IManifest.MinimumApiVersion)] = $"{Sample.Int()}.{Sample.Int()}.{Sample.Int()}-{Sample.String()}",
            [nameof(IManifest.MinimumGameVersion)] = $"{Sample.Int()}.{Sample.Int()}.{Sample.Int()}-{Sample.String()}",
            [nameof(IManifest.Dependencies)] = new[] { originalDependency },
            ["ExtraString"] = Sample.String(),
            ["ExtraInt"] = Sample.Int()
        };

        // write to filesystem
        string rootFolder = this.GetTempFolderPath();
        string modFolder = Path.Combine(rootFolder, Guid.NewGuid().ToString("N"));
        string filename = Path.Combine(modFolder, "manifest.json");
        Directory.CreateDirectory(modFolder);
        File.WriteAllText(filename, JsonConvert.SerializeObject(original));

        // act
        IModMetadata[] mods = new ModResolver().ReadManifests(new ModToolkit(), rootFolder, new ModDatabase(), useCaseInsensitiveFilePaths: true).ToArray();
        IModMetadata? mod = mods.FirstOrDefault();

        // assert
        mods.Should().HaveCount(1, "it should match number of mods input");
        mod.Should().NotBeNull();
        mod!.DataRecord.Should().BeNull("we didn't provide one");
        mod.DirectoryPath.Should().Be(modFolder);
        mod.Error.Should().BeNull();
        mod.Status.Should().Be(ModMetadataStatus.Found);

        mod.DisplayName.Should().Be((string)original[nameof(IManifest.Name)], mod.DisplayName);
        mod.Manifest.Name.Should().Be((string)original[nameof(IManifest.Name)], mod.Manifest.Name);
        mod.Manifest.ExtraFields.Should()
            .NotBeNull()
            .And.HaveCount(2)
            .And.ContainKeys("ExtraString", "ExtraInt");
        mod.Manifest.ExtraFields["ExtraString"].Should().Be(original["ExtraString"]);
        mod.Manifest.ExtraFields["ExtraInt"].Should().Be(original["ExtraInt"]);

        mod.Manifest.Dependencies.Should()
            .NotBeNull()
            .And.HaveCount(1);
        mod.Manifest.Dependencies[0].Should().NotBeNull();
        mod.Manifest.Dependencies[0].UniqueID.Should().Be((string)originalDependency[nameof(IManifestDependency.UniqueID)]);

        // cleanup
        Directory.Delete(rootFolder, recursive: true);
    }

    /****
    ** ValidateManifests
    ****/
    [Test(Description = "Assert that validation doesn't fail if there are no mods installed.")]
    public void ValidateManifests_NoMods_DoesNothing()
    {
        new ModResolver().ValidateManifests(Array.Empty<ModMetadata>(), apiVersion: new SemanticVersion("1.0.0"), gameVersion: new SemanticVersion("1.0.0"), getUpdateUrl: _ => null, getFileLookup: this.GetFileLookup, validateFilesExist: false);
    }

    [Test(Description = "Assert that validation skips manifests that have already failed without calling any other properties.")]
    public void ValidateManifests_Skips_Failed()
    {
        // arrange
        Mock<IModMetadata> mock = this.GetMetadata("Mod A");
        mock.Setup(p => p.Status).Returns(ModMetadataStatus.Failed);

        // act
        new ModResolver().ValidateManifests([mock.Object], apiVersion: new SemanticVersion("1.0.0"), gameVersion: new SemanticVersion("1.0.0"), getUpdateUrl: _ => null, getFileLookup: this.GetFileLookup, validateFilesExist: false);

        // assert
        mock.VerifyGet(p => p.Status, Times.Once, "The validation did not check the manifest status.");
    }

    [Test(Description = "Assert that validation fails if the mod has 'assume broken' status.")]
    public void ValidateManifests_ModStatus_AssumeBroken_Fails()
    {
        // arrange
        Mock<IModMetadata> mock = this.GetMetadata("Mod A", [], allowStatusChange: true);
        mock.Setup(p => p.DataRecord).Returns(() => new ModDataRecordVersionedFields(this.GetModDataRecord())
        {
            Status = ModStatus.AssumeBroken
        });

        // act
        new ModResolver().ValidateManifests([mock.Object], apiVersion: new SemanticVersion("1.0.0"), gameVersion: new SemanticVersion("1.0.0"), getUpdateUrl: _ => null, getFileLookup: this.GetFileLookup, validateFilesExist: false);

        // assert
        mock.Verify(p => p.SetStatus(ModMetadataStatus.Failed, It.IsAny<ModFailReason>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once, "The validation did not fail the metadata.");
    }

    [Test(Description = "Assert that validation fails when the minimum API version is higher than the current SMAPI version.")]
    public void ValidateManifests_MinimumApiVersion_Fails()
    {
        // arrange
        Mock<IModMetadata> mock = this.GetMetadata("Mod A", [], allowStatusChange: true);
        mock.Setup(p => p.Manifest).Returns(this.GetManifest(minimumApiVersion: "1.1"));

        // act
        new ModResolver().ValidateManifests([mock.Object], apiVersion: new SemanticVersion("1.0.0"), gameVersion: new SemanticVersion("1.0.0"), getUpdateUrl: _ => null, getFileLookup: this.GetFileLookup, validateFilesExist: false);

        // assert
        mock.Verify(p => p.SetStatus(ModMetadataStatus.Failed, It.IsAny<ModFailReason>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once, "The validation did not fail the metadata.");
    }

    [Test(Description = "Assert that validation fails when the minimum game version is higher than the current Stardew Valley version.")]
    public void ValidateManifests_MinimumGameVersion_Fails()
    {
        // arrange
        Mock<IModMetadata> mock = this.GetMetadata("Mod A", [], allowStatusChange: true);
        mock.Setup(p => p.Manifest).Returns(this.GetManifest(minimumGameVersion: "1.6.9"));

        // act
        new ModResolver().ValidateManifests([mock.Object], apiVersion: new SemanticVersion("1.0.0"), gameVersion: new SemanticVersion("1.0.0"), getUpdateUrl: _ => null, getFileLookup: this.GetFileLookup, validateFilesExist: false);

        // assert
        mock.Verify(p => p.SetStatus(ModMetadataStatus.Failed, It.IsAny<ModFailReason>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once, "The validation did not fail the metadata.");
    }

    [Test(Description = "Assert that validation fails when the manifest references a DLL that does not exist.")]
    public void ValidateManifests_MissingEntryDLL_Fails()
    {
        // arrange
        string directoryPath = this.GetTempFolderPath();
        Mock<IModMetadata> mock = this.GetMetadata(this.GetManifest(id: "Mod A", version: "1.0", entryDll: "Missing.dll"), allowStatusChange: true, directoryPath: directoryPath);
        Directory.CreateDirectory(directoryPath);

        // act
        new ModResolver().ValidateManifests([mock.Object], apiVersion: new SemanticVersion("1.0.0"), gameVersion: new SemanticVersion("1.0.0"), getUpdateUrl: _ => null, getFileLookup: this.GetFileLookup);

        // assert
        mock.Verify(p => p.SetStatus(ModMetadataStatus.Failed, It.IsAny<ModFailReason>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once, "The validation did not fail the metadata.");

        // cleanup
        Directory.Delete(directoryPath);
    }

    [Test(Description = "Assert that validation fails when multiple mods have the same unique ID.")]
    public void ValidateManifests_DuplicateUniqueId_Fails()
    {
        // arrange
        Mock<IModMetadata> modA = this.GetMetadata("Mod A", [], allowStatusChange: true);
        Mock<IModMetadata> modB = this.GetMetadata(this.GetManifest(id: "Mod A", name: "Mod B", version: "1.0"), allowStatusChange: true);

        // act
        new ModResolver().ValidateManifests([modA.Object, modB.Object], apiVersion: new SemanticVersion("1.0.0"), gameVersion: new SemanticVersion("1.0.0"), getUpdateUrl: _ => null, getFileLookup: this.GetFileLookup, validateFilesExist: false);

        // assert
        modA.Verify(p => p.SetStatus(ModMetadataStatus.Failed, ModFailReason.Duplicate, It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce, "The validation did not fail the first mod with a unique ID.");
        modB.Verify(p => p.SetStatus(ModMetadataStatus.Failed, ModFailReason.Duplicate, It.IsAny<string>(), It.IsAny<string>()), Times.AtLeastOnce, "The validation did not fail the second mod with a unique ID.");
    }

    [Test(Description = "Assert that validation fails when the manifest references a DLL that does not exist.")]
    public void ValidateManifests_Valid_Passes()
    {
        // set up manifest
        IManifest manifest = this.GetManifest();

        // create DLL
        string modFolder = Path.Combine(this.GetTempFolderPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(modFolder);
        File.WriteAllText(Path.Combine(modFolder, manifest.EntryDll!), "");

        // arrange
        Mock<IModMetadata> mock = new(MockBehavior.Strict);
        mock.Setup(p => p.Status).Returns(ModMetadataStatus.Found);
        mock.Setup(p => p.DataRecord).Returns(this.GetModDataRecordVersionedFields());
        mock.Setup(p => p.Manifest).Returns(manifest);
        mock.Setup(p => p.DirectoryPath).Returns(modFolder);

        // act
        new ModResolver().ValidateManifests([mock.Object], apiVersion: new SemanticVersion("1.0.0"), gameVersion: new SemanticVersion("1.0.0"), getUpdateUrl: _ => null, getFileLookup: this.GetFileLookup);

        // assert
        // if Moq doesn't throw a method-not-setup exception, the validation didn't override the status.

        // cleanup
        Directory.Delete(modFolder, recursive: true);
    }

    /****
    ** ProcessDependencies
    ****/
    [Test(Description = "Assert that processing dependencies doesn't fail if there are no mods installed.")]
    public void ProcessDependencies_NoMods_DoesNothing()
    {
        // act
        IModMetadata[] mods = new ModResolver().ProcessDependencies(Array.Empty<IModMetadata>(), new ModDatabase()).ToArray();

        // assert
        mods.Should().BeEmpty("it should match number of mods input");
    }

    [Test(Description = "Assert that processing dependencies doesn't change the order if there are no mod dependencies.")]
    public void ProcessDependencies_NoDependencies_DoesNothing()
    {
        // arrange
        // A B C
        Mock<IModMetadata> modA = this.GetMetadata("Mod A");
        Mock<IModMetadata> modB = this.GetMetadata("Mod B");
        Mock<IModMetadata> modC = this.GetMetadata("Mod C");

        // act
        IModMetadata[] mods = new ModResolver().ProcessDependencies([modA.Object, modB.Object, modC.Object], new ModDatabase()).ToArray();

        // assert
        mods.Should().HaveCount(3, "it should match number of mods input");
        mods[0].Should().BeSameAs(modA.Object, "the load order shouldn't change with no dependencies");
        mods[1].Should().BeSameAs(modB.Object, "the load order shouldn't change with no dependencies");
        mods[2].Should().BeSameAs(modC.Object, "the load order shouldn't change with no dependencies");
    }

    [Test(Description = "Assert that processing dependencies skips mods that have already failed without calling any other properties.")]
    public void ProcessDependencies_Skips_Failed()
    {
        // arrange
        Mock<IModMetadata> mock = new(MockBehavior.Strict);
        mock.Setup(p => p.Status).Returns(ModMetadataStatus.Failed);

        // act
        new ModResolver().ProcessDependencies([mock.Object], new ModDatabase());

        // assert
        mock.VerifyGet(p => p.Status, Times.Once, "The validation did not check the manifest status.");
    }

    [Test(Description = "Assert that simple dependencies are reordered correctly.")]
    public void ProcessDependencies_Reorders_SimpleDependencies()
    {
        // arrange
        // A ◀── B
        // ▲     ▲
        // │     │
        // └─ C ─┘
        Mock<IModMetadata> modA = this.GetMetadata("Mod A");
        Mock<IModMetadata> modB = this.GetMetadata("Mod B", dependencies: ["Mod A"]);
        Mock<IModMetadata> modC = this.GetMetadata("Mod C", dependencies: ["Mod A", "Mod B"]);

        // act
        IModMetadata[] mods = new ModResolver().ProcessDependencies([modC.Object, modA.Object, modB.Object], new ModDatabase()).ToArray();

        // assert
        mods.Should().HaveCount(3, "it should match number of mods input");
        mods[0].Should().BeSameAs(modA.Object, "mod A should be first since the other mods depend on it");
        mods[1].Should().BeSameAs(modB.Object, "mod B should be second since it needs mod A, and is needed by mod C");
        mods[2].Should().BeSameAs(modC.Object, "mod C should be third since it needs both mod A and mod B");
    }

    [Test(Description = "Assert that simple dependency chains are reordered correctly.")]
    public void ProcessDependencies_Reorders_DependencyChain()
    {
        // arrange
        // A ◀── B ◀── C ◀── D
        Mock<IModMetadata> modA = this.GetMetadata("Mod A");
        Mock<IModMetadata> modB = this.GetMetadata("Mod B", dependencies: ["Mod A"]);
        Mock<IModMetadata> modC = this.GetMetadata("Mod C", dependencies: ["Mod B"]);
        Mock<IModMetadata> modD = this.GetMetadata("Mod D", dependencies: ["Mod C"]);

        // act
        IModMetadata[] mods = new ModResolver().ProcessDependencies([modC.Object, modA.Object, modB.Object, modD.Object], new ModDatabase()).ToArray();

        // assert
        mods.Should().HaveCount(4, "it should match number of mods input");
        mods[0].Should().BeSameAs(modA.Object, "mod A should be first since it's needed by mod B");
        mods[1].Should().BeSameAs(modB.Object, "mod B should be second since it needs mod A, and is needed by mod C");
        mods[2].Should().BeSameAs(modC.Object, "mod C should be third since it needs mod B, and is needed by mod D");
        mods[3].Should().BeSameAs(modD.Object, "mod D should be fourth since it needs mod C");
    }

    [Test(Description = "Assert that overlapping dependency chains are reordered correctly.")]
    public void ProcessDependencies_Reorders_OverlappingDependencyChain()
    {
        // arrange
        // A ◀── B ◀── C ◀── D
        //       ▲     ▲
        //       │     │
        //       E ◀── F
        Mock<IModMetadata> modA = this.GetMetadata("Mod A");
        Mock<IModMetadata> modB = this.GetMetadata("Mod B", dependencies: ["Mod A"]);
        Mock<IModMetadata> modC = this.GetMetadata("Mod C", dependencies: ["Mod B"]);
        Mock<IModMetadata> modD = this.GetMetadata("Mod D", dependencies: ["Mod C"]);
        Mock<IModMetadata> modE = this.GetMetadata("Mod E", dependencies: ["Mod B"]);
        Mock<IModMetadata> modF = this.GetMetadata("Mod F", dependencies: ["Mod C", "Mod E"]);

        // act
        IModMetadata[] mods = new ModResolver().ProcessDependencies([modC.Object, modA.Object, modB.Object, modD.Object, modF.Object, modE.Object], new ModDatabase()).ToArray();

        // assert
        mods.Should().HaveCount(6, "it should match number of mods input");
        mods[0].Should().BeSameAs(modA.Object, "mod A should be first since it's needed by mod B");
        mods[1].Should().BeSameAs(modB.Object, "mod B should be second since it needs mod A, and is needed by mod C");
        mods[2].Should().BeSameAs(modC.Object, "mod C should be third since it needs mod B, and is needed by mod D");
        mods[3].Should().BeSameAs(modD.Object, "mod D should be fourth since it needs mod C");
        mods[4].Should().BeSameAs(modE.Object, "mod E should be fifth since it needs mod B, but is specified after C which also needs mod B");
        mods[5].Should().BeSameAs(modF.Object, "mod F should be last since it needs mods E and C");
    }

    [Test(Description = "Assert that mods with circular dependency chains are skipped, but any other mods are loaded in the correct order.")]
    public void ProcessDependencies_Skips_CircularDependentMods()
    {
        // arrange
        // A ◀── B ◀── C ──▶ D
        //             ▲     │
        //             │     ▼
        //             └──── E
        Mock<IModMetadata> modA = this.GetMetadata("Mod A");
        Mock<IModMetadata> modB = this.GetMetadata("Mod B", dependencies: ["Mod A"]);
        Mock<IModMetadata> modC = this.GetMetadata("Mod C", dependencies: ["Mod B", "Mod D"], allowStatusChange: true);
        Mock<IModMetadata> modD = this.GetMetadata("Mod D", dependencies: ["Mod E"], allowStatusChange: true);
        Mock<IModMetadata> modE = this.GetMetadata("Mod E", dependencies: ["Mod C"], allowStatusChange: true);

        // act
        IModMetadata[] mods = new ModResolver().ProcessDependencies([modC.Object, modA.Object, modB.Object, modD.Object, modE.Object], new ModDatabase()).ToArray();

        // assert
        mods.Should().HaveCount(5, "it should match number of mods input");
        mods[0].Should().BeSameAs(modA.Object, "mod A should be first since it's needed by mod B");
        mods[1].Should().BeSameAs(modB.Object, "mod B should be second since it needs mod A");
        modC.Verify(p => p.SetStatus(ModMetadataStatus.Failed, It.IsAny<ModFailReason>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once, "Mod C was expected to fail since it's part of a dependency loop.");
        modD.Verify(p => p.SetStatus(ModMetadataStatus.Failed, It.IsAny<ModFailReason>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once, "Mod D was expected to fail since it's part of a dependency loop.");
        modE.Verify(p => p.SetStatus(ModMetadataStatus.Failed, It.IsAny<ModFailReason>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once, "Mod E was expected to fail since it's part of a dependency loop.");
    }

    [Test(Description = "Assert that dependencies are sorted correctly even if some of the mods failed during metadata loading.")]
    public void ProcessDependencies_WithSomeFailedMods_Succeeds()
    {
        // arrange
        // A ◀── B ◀── C   D (failed)
        Mock<IModMetadata> modA = this.GetMetadata("Mod A");
        Mock<IModMetadata> modB = this.GetMetadata("Mod B", dependencies: ["Mod A"]);
        Mock<IModMetadata> modC = this.GetMetadata("Mod C", dependencies: ["Mod B"], allowStatusChange: true);
        Mock<IModMetadata> modD = new(MockBehavior.Strict);
        modD.Setup(p => p.Manifest).Returns<IManifest>(null!); // deliberately testing null handling
        modD.Setup(p => p.Status).Returns(ModMetadataStatus.Failed);

        // act
        IModMetadata[] mods = new ModResolver().ProcessDependencies([modC.Object, modA.Object, modB.Object, modD.Object], new ModDatabase()).ToArray();

        // assert
        mods.Should().HaveCount(4, "it should match number of mods input");
        mods[0].Should().BeSameAs(modD.Object, "mod D should be first since it was already failed");
        mods[1].Should().BeSameAs(modA.Object, "mod A should be second since it's needed by mod B");
        mods[2].Should().BeSameAs(modB.Object, "mod B should be third since it needs mod A, and is needed by mod C");
        mods[3].Should().BeSameAs(modC.Object, "mod C should be fourth since it needs mod B, and is needed by mod D");
    }

    [Test(Description = "Assert that dependencies are failed if they don't meet the minimum version.")]
    public void ProcessDependencies_WithMinVersions_FailsIfNotMet()
    {
        // arrange
        // A 1.0 ◀── B (need A 1.1)
        Mock<IModMetadata> modA = this.GetMetadata(this.GetManifest(id: "Mod A", version: "1.0"));
        Mock<IModMetadata> modB = this.GetMetadata(this.GetManifest(id: "Mod B", version: "1.0", dependencies: [new ManifestDependency("Mod A", "1.1")]), allowStatusChange: true);

        // act
        IModMetadata[] mods = new ModResolver().ProcessDependencies([modA.Object, modB.Object], new ModDatabase()).ToArray();

        // assert
        mods.Should().HaveCount(2, "it should match number of mods input");
        modB.Verify(p => p.SetStatus(ModMetadataStatus.Failed, It.IsAny<ModFailReason>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once, "Mod B unexpectedly didn't fail even though it needs a newer version of Mod A.");
    }

    [Test(Description = "Assert that dependencies are accepted if they meet the minimum version.")]
    public void ProcessDependencies_WithMinVersions_SucceedsIfMet()
    {
        // arrange
        // A 1.0 ◀── B (need A 1.0-beta)
        Mock<IModMetadata> modA = this.GetMetadata(this.GetManifest(id: "Mod A", version: "1.0"));
        Mock<IModMetadata> modB = this.GetMetadata(this.GetManifest(id: "Mod B", version: "1.0", dependencies: [new ManifestDependency("Mod A", "1.0-beta")]), allowStatusChange: false);

        // act
        IModMetadata[] mods = new ModResolver().ProcessDependencies([modA.Object, modB.Object], new ModDatabase()).ToArray();

        // assert
        mods.Should().HaveCount(2, "it should match number of mods input");
        mods[0].Should().BeSameAs(modA.Object, "mod A should be first since it's needed by mod B");
        mods[1].Should().BeSameAs(modB.Object, "mod B should be second since it needs mod A");
    }

    [Test(Description = "Assert that optional dependencies are sorted correctly if present.")]
    public void ProcessDependencies_IfOptional()
    {
        // arrange
        // A ◀── B
        Mock<IModMetadata> modA = this.GetMetadata(this.GetManifest(id: "Mod A", version: "1.0"));
        Mock<IModMetadata> modB = this.GetMetadata(this.GetManifest(id: "Mod B", version: "1.0", dependencies: [new ManifestDependency("Mod A", "1.0", required: false)]), allowStatusChange: false);

        // act
        IModMetadata[] mods = new ModResolver().ProcessDependencies([modB.Object, modA.Object], new ModDatabase()).ToArray();

        // assert
        mods.Should().HaveCount(2, "it should match number of mods input");
        mods[0].Should().BeSameAs(modA.Object, "mod A should be first since it's needed by mod B");
        mods[1].Should().BeSameAs(modB.Object, "mod B should be second since it needs mod A");
    }

    [Test(Description = "Assert that optional dependencies are accepted if they're missing.")]
    public void ProcessDependencies_IfOptional_SucceedsIfMissing()
    {
        // arrange
        // A ◀── B where A doesn't exist
        Mock<IModMetadata> modB = this.GetMetadata(this.GetManifest(id: "Mod B", version: "1.0", dependencies: [new ManifestDependency("Mod A", "1.0", required: false)]), allowStatusChange: false);

        // act
        IModMetadata[] mods = new ModResolver().ProcessDependencies([modB.Object], new ModDatabase()).ToArray();

        // assert
        mods.Should().HaveCount(1, "should match number of mods input");
        mods[0].Should().BeSameAs(modB.Object, "mod B should be first since it's the only mod");
    }


    /*********
    ** Private methods
    *********/
    /// <summary>Get a generated folder path in the temp folder. This folder isn't created automatically.</summary>
    private string GetTempFolderPath()
    {
        return Path.Combine(Path.GetTempPath(), "smapi-unit-tests", Guid.NewGuid().ToString("N"));
    }

    /// <summary>Get a file lookup for a given directory.</summary>
    /// <param name="rootDirectory">The full path to the directory.</param>
    private IFileLookup GetFileLookup(string rootDirectory)
    {
        return MinimalFileLookup.GetCachedFor(rootDirectory);
    }

    /// <summary>Get a randomized basic manifest.</summary>
    /// <param name="id">The <see cref="IManifest.UniqueID"/> value, or <c>null</c> for a generated value.</param>
    /// <param name="name">The <see cref="IManifest.Name"/> value, or <c>null</c> for a generated value.</param>
    /// <param name="version">The <see cref="IManifest.Version"/> value, or <c>null</c> for a generated value.</param>
    /// <param name="entryDll">The <see cref="IManifest.EntryDll"/> value, or <c>null</c> for a generated value.</param>
    /// <param name="contentPackForId">The <see cref="IManifest.ContentPackFor"/> value.</param>
    /// <param name="minimumApiVersion">The <see cref="IManifest.MinimumApiVersion"/> value.</param>
    /// <param name="minimumGameVersion">The <see cref="IManifest.MinimumGameVersion"/> value.</param>
    /// <param name="dependencies">The <see cref="IManifest.Dependencies"/> value.</param>
    private Manifest GetManifest(string? id = null, string? name = null, string? version = null, string? entryDll = null, string? contentPackForId = null, string? minimumApiVersion = null, string? minimumGameVersion = null, IManifestDependency[]? dependencies = null)
    {
        return new Manifest(
            uniqueId: id ?? $"{Sample.String()}.{Sample.String()}",
            name: name ?? id ?? Sample.String(),
            author: Sample.String(),
            description: Sample.String(),
            version: version != null ? new SemanticVersion(version) : new SemanticVersion(Sample.Int(), Sample.Int(), Sample.Int(), Sample.String()),
            entryDll: entryDll ?? $"{Sample.String()}.dll",
            contentPackFor: contentPackForId != null ? new ManifestContentPackFor(contentPackForId, null) : null,
            minimumApiVersion: minimumApiVersion != null ? new SemanticVersion(minimumApiVersion) : null,
            minimumGameVersion: minimumGameVersion != null ? new SemanticVersion(minimumGameVersion) : null,
            dependencies: dependencies ?? [],
            updateKeys: []
        );
    }

    /// <summary>Get a randomized basic manifest.</summary>
    /// <param name="uniqueId">The mod's name and unique ID.</param>
    private Mock<IModMetadata> GetMetadata(string uniqueId)
    {
        return this.GetMetadata(this.GetManifest(uniqueId, "1.0"));
    }

    /// <summary>Get a randomized basic manifest.</summary>
    /// <param name="uniqueId">The mod's name and unique ID.</param>
    /// <param name="dependencies">The dependencies this mod requires.</param>
    /// <param name="allowStatusChange">Whether the code being tested is allowed to change the mod status.</param>
    private Mock<IModMetadata> GetMetadata(string uniqueId, string[] dependencies, bool allowStatusChange = false)
    {
        IManifest manifest = this.GetManifest(id: uniqueId, version: "1.0", dependencies: dependencies.Select(dependencyID => (IManifestDependency)new ManifestDependency(dependencyID, null as ISemanticVersion)).ToArray());
        return this.GetMetadata(manifest, allowStatusChange);
    }

    /// <summary>Get a randomized basic manifest.</summary>
    /// <param name="manifest">The mod manifest.</param>
    /// <param name="allowStatusChange">Whether the code being tested is allowed to change the mod status.</param>
    /// <param name="directoryPath">The directory path the mod metadata should be pointed at, or <c>null</c> to generate a fake path.</param>
    private Mock<IModMetadata> GetMetadata(IManifest manifest, bool allowStatusChange = false, string? directoryPath = null)
    {
        directoryPath ??= this.GetTempFolderPath();

        Mock<IModMetadata> mod = new(MockBehavior.Strict);
        mod.Setup(p => p.DataRecord).Returns(this.GetModDataRecordVersionedFields());
        mod.Setup(p => p.Status).Returns(ModMetadataStatus.Found);
        mod.Setup(p => p.DisplayName).Returns(manifest.UniqueID);
        mod.Setup(p => p.DirectoryPath).Returns(directoryPath);
        mod.Setup(p => p.Manifest).Returns(manifest);
        mod.Setup(p => p.HasId(It.IsAny<string>())).Returns((string id) => manifest.UniqueID == id);
        mod.Setup(p => p.GetUpdateKeys(It.IsAny<bool>())).Returns([]);
        mod.Setup(p => p.GetRelativePathWithRoot()).Returns(directoryPath);
        if (allowStatusChange)
        {
            mod
                .Setup(p => p.SetStatus(It.IsAny<ModMetadataStatus>(), It.IsAny<ModFailReason>(), It.IsAny<string>(), It.IsAny<string>()))
                .Callback<ModMetadataStatus, ModFailReason, string, string>((status, failReason, message, errorDetails) => Console.WriteLine($"<{manifest.UniqueID} changed status: [{status}] {message}\n{failReason}\n{errorDetails}"))
                .Returns(mod.Object);
        }
        return mod;
    }

    /// <summary>Generate a default mod data record.</summary>
    private ModDataRecord GetModDataRecord()
    {
        return new("Default Display Name", new ModDataModel("Sample ID", null, ModWarning.None, false));
    }

    /// <summary>Generate a default mod data versioned fields instance.</summary>
    private ModDataRecordVersionedFields GetModDataRecordVersionedFields()
    {
        return new ModDataRecordVersionedFields(this.GetModDataRecord());
    }
}
