﻿using System;
using System.IO;
using System.Linq;

namespace Codestellation.Galaxy.Infrastructure
{
    public static class Folder
    {
        public static string BasePath
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }

        public static string Combine(params string[] folders)
        {
            return Path.Combine(folders);
        }

        public static void Delete(string folder)
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
        }

        public static void Ensure(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

        }

        public static string[] EnumerateFiles(string logFolder)
        {
            return Directory.EnumerateFiles(logFolder).ToArray();
        }
    }
}