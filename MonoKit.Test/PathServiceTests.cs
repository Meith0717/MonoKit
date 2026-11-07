// AppHost.Test - PathServiceTests.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using MonoKit.App.Core.IO;

namespace MonoKit.Test;

public class PathServiceTests
{
    private enum TestPath : byte
    {
        Saves,
    }

    private static string NewTempRoot() =>
        Path.Combine(Path.GetTempPath(), "monokit-test-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void RegisterPath_Then_GetPath_ReturnsCombinedPath()
    {
        var root = NewTempRoot();
        try
        {
            var paths = new PathService(root);
            paths.RegisterPath((byte)TestPath.Saves, "saves");

            var expected = Path.Combine(root, "saves");
            Assert.Equal(expected, paths.GetPath((byte)TestPath.Saves));
            Assert.True(Directory.Exists(expected));
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void GetFilePath_ForUnregisteredId_Throws()
    {
        var root = NewTempRoot();
        try
        {
            var paths = new PathService(root);
            Assert.Throws<InvalidOperationException>(() =>
                paths.GetFilePath((byte)TestPath.Saves, "a.txt")
            );
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }
}
