// FileHandler.cs 
// Copyright (c) 2023-2025 Thierry Meiers 
// All rights reserved.

using System;
using System.IO;

namespace Engine.Core
{
    public class FileHandler
    {
        public readonly string RootPath;
        public readonly string GameSavePath;
        public readonly string ConfigsSavePath;
        public readonly string ImagedSavePath;

        public readonly string SettingsFilePath;
        public readonly string GameSaveFilePath;

        public FileHandler(string rootDir)
        {
            string documentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            RootPath = Path.Combine(documentPath, rootDir);
            GameSavePath = Path.Combine(RootPath, "saves");
            ConfigsSavePath = Path.Combine(RootPath, "config");
            ImagedSavePath = Path.Combine(RootPath, "images");
            CreateDirectory(RootPath);
            CreateDirectory(GameSavePath);
            CreateDirectory(ConfigsSavePath);
            CreateDirectory(ImagedSavePath);
            SettingsFilePath = Path.Combine(ConfigsSavePath, "settings.json");
            GameSaveFilePath = Path.Combine(GameSavePath, "save.sav");
        }

        public static void CreateDirectory(string path)
        {
            if (Directory.Exists(path)) return;
            Directory.CreateDirectory(path);
        }

        public static void DeleteDirectory(string path)
        {
            if (!Directory.Exists(path)) return;
            Directory.Delete(path, true);
        }

        public static bool DirectoryExist(string path)
            => Directory.Exists(path);

        public static void CreateFile(string path, string content)
        {
            using StreamWriter writer = new StreamWriter(path, false);
            writer.Write(content);
        }

        public static void DeleteFile(string path)
        {
            if (!FileExist(path)) return;
            File.Delete(path);
        }

        public static string ReadFile(string path)
        {
            if (!File.Exists(path)) return null;
            using StreamReader streamReader = new(path);
            return streamReader.ReadToEnd();
        }

        public static bool FileExist(string path)
            => File.Exists(path);

        public static string[] GetAllFilesInDirectory(string path, SearchOption searchOption)
            => Directory.GetFiles(path, "*.*", searchOption);

        public static string[] GetDirectoriesInDirectory(string path, SearchOption searchOption)
            => Directory.GetDirectories(path, "*.*", searchOption);
    }
}
