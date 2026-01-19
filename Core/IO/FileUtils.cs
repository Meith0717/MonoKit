// FileUtils.cs
// Copyright (c) 2023-2026 Thierry Meiers
// All rights reserved.
// Portions generated or assisted by AI.

using System.IO;

namespace MonoKit.Core.IO;

public class FileUtils
{
    public static void CreateDirectory(string path)
    {
        if (Directory.Exists(path))
            return;
        Directory.CreateDirectory(path);
    }

    public static void DeleteDirectory(string path)
    {
        if (!Directory.Exists(path))
            return;
        Directory.Delete(path, true);
    }

    public static bool DirectoryExist(string path)
    {
        return Directory.Exists(path);
    }

    public static void CreateFile(string path, string content)
    {
        using var writer = new StreamWriter(path, false);
        writer.Write(content);
    }

    public static void DeleteFile(string path)
    {
        if (!FileExist(path))
            return;
        File.Delete(path);
    }

    public static string ReadFile(string path)
    {
        if (!File.Exists(path))
            return null;
        using StreamReader streamReader = new(path);
        return streamReader.ReadToEnd();
    }

    public static bool FileExist(string path)
    {
        return File.Exists(path);
    }

    public static string[] GetAllFilesInDirectory(string path, SearchOption searchOption)
    {
        return Directory.GetFiles(path, "*.*", searchOption);
    }

    public static string[] GetDirectoriesInDirectory(string path, SearchOption searchOption)
    {
        return Directory.GetDirectories(path, "*.*", searchOption);
    }
}
