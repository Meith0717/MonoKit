// AppHost - FileUtils.cs
// Copyright (c) 2026 Thierry Meiers
// All rights reserved.
// AI-assisted development using Mistral Vibe Code;
// all output reviewed and adapted by the author.

using System.IO;

namespace MonoKit.App.Core.IO;

public class FileUtils
{
    /// <summary>Creates a directory unless it already exists.</summary>
    public static void CreateDirectory(string path)
    {
        if (Directory.Exists(path) || path == null)
            return;
        Directory.CreateDirectory(path);
    }

    /// <summary>Recursively deletes a directory if it exists.</summary>
    public static void DeleteDirectory(string path)
    {
        if (!Directory.Exists(path))
            return;
        Directory.Delete(path, true);
    }

    /// <summary>Checks whether the directory exists on disk.</summary>
    public static bool DirectoryExist(string path)
    {
        return Directory.Exists(path);
    }

    /// <summary>Writes content to a file, overwriting any existing data.</summary>
    public static void CreateFile(string path, string content)
    {
        using var writer = new StreamWriter(path, false);
        writer.Write(content);
    }

    /// <summary>Deletes a file if it exists.</summary>
    public static void DeleteFile(string path)
    {
        if (!FileExist(path))
            return;
        File.Delete(path);
    }

    /// <summary>Reads a file's full text, or null when missing.</summary>
    public static string ReadFile(string path)
    {
        if (!File.Exists(path))
            return null;
        using StreamReader streamReader = new(path);
        return streamReader.ReadToEnd();
    }

    /// <summary>Checks whether the file exists on disk.</summary>
    public static bool FileExist(string path)
    {
        return File.Exists(path);
    }

    /// <summary>Lists files in a directory with the given search scope.</summary>
    public static string[] GetAllFilesInDirectory(string path, SearchOption searchOption)
    {
        return Directory.GetFiles(path, "*.*", searchOption);
    }

    /// <summary>Lists subdirectories with the given search scope.</summary>
    public static string[] GetDirectoriesInDirectory(string path, SearchOption searchOption)
    {
        return Directory.GetDirectories(path, "*.*", searchOption);
    }
}
