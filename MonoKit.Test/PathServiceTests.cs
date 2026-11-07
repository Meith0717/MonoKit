using MonoKit.App.Core.IO;

namespace MonoKit.Test;

public class PathServiceTests
{
    private enum TestPath
    {
        Saves
    }

    private static string NewTempRoot()
        => Path.Combine(Path.GetTempPath(), "monokit-test-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void RegisterPath_Then_GetPath_ReturnsCombinedPath()
    {
        var root = NewTempRoot();
        try
        {
            var paths = new PathService<TestPath>(root);
            paths.RegisterPath(TestPath.Saves, "saves");

            var expected = Path.Combine(root, "saves");
            Assert.Equal(expected, paths.GetPath(TestPath.Saves));
            Assert.True(Directory.Exists(expected));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void GetFilePath_ForUnregisteredId_Throws()
    {
        var root = NewTempRoot();
        try
        {
            var paths = new PathService<TestPath>(root);
            Assert.Throws<InvalidOperationException>(() => paths.GetFilePath(TestPath.Saves, "a.txt"));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
