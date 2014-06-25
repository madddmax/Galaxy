﻿using System.IO;
using Codestellation.Galaxy.Domain;
using Codestellation.Galaxy.ServiceManager.Helpers;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using System.Runtime.Versioning;
using System.Configuration;

namespace Codestellation.Galaxy.ServiceManager.Operations
{
    public class CopyNugetsToRoot: ServiceOperation
    {
        const string libFolder = "lib";
        string _hostPackageName;

        public CopyNugetsToRoot(string targetPath, ServiceApp serviceApp, NugetFeed feed) :
            base(targetPath, serviceApp, feed)
        {
            _hostPackageName = ConfigurationManager.AppSettings["hostPackageName"];
        }

        Version GetNugetDotNetVersion(string installedNugetPath)
        {
            Version result = new Version();

            var libFolderPath = Path.Combine(installedNugetPath, libFolder);
            if (Directory.Exists(libFolderPath))
            {
                foreach(var pair in DotNetVersionHelper.dotNetNugetFolders)
                {
                    var libFolderDotNetVersionedPath = Path.Combine(libFolderPath, pair.Value);

                    if (Directory.Exists(libFolderDotNetVersionedPath))
                        if (result < pair.Key)
                            result = pair.Key;
                }
            }

            // temporary, if no lib/ folder
            if(result.Equals(new Version()))
                result = new Version(4, 5);

            return result;
        }

        private void UnpackPackage(string packagePath, string serviceTargetPath, Version targetDotNetVersion)
        {
            var libFolderPath = Path.Combine(packagePath, libFolder);
            if (Directory.Exists(libFolderPath))
	        {
                var libFolderDotNetVersionedPath = Path.Combine(libFolderPath, DotNetVersionHelper.dotNetNugetFolders[targetDotNetVersion]);
                if(Directory.Exists(libFolderDotNetVersionedPath))
                    // copy from "{package name}/lib/{dotnetversionfolder}/" to "../" dir
         	        CopyDirectoryHelper.DirectoryCopy(libFolderDotNetVersionedPath, serviceTargetPath, true);
                else
                    // copy from "{package name}/lib/" to "../" dir
         	        CopyDirectoryHelper.DirectoryCopy(libFolderPath, serviceTargetPath, true);
            }
            else 
                // copy from "{package name}/" to "../" dir
     	        CopyDirectoryHelper.DirectoryCopy(packagePath, serviceTargetPath, true);
        }
        private void Clean(IEnumerable<String> packageFolders)
        {
            foreach (var packagePath in packageFolders)
            {
                Directory.Delete(packagePath, true);
            }
        }

        public override void Execute()
        {
            try
            {
                string serviceTargetPath = Path.Combine(_targetPath, _serviceApp.DisplayName);

                var packageFolders = Directory.EnumerateDirectories(serviceTargetPath);

                Dictionary<string, Version> packageDotNetVersions = new Dictionary<string, Version>();
                Version hostPackageDotNetVersion = new Version();
                Version packagesDotNetVersionMax = new Version();
               
                foreach (var packagePath in packageFolders)
                {
                    var packageDotNetVersion = GetNugetDotNetVersion(packagePath);

                    if (packagePath.Contains(_hostPackageName))
                        hostPackageDotNetVersion = packageDotNetVersion;
                    else 
                    {
                        packageDotNetVersions.Add(packagePath, packageDotNetVersion);
                        if(packagesDotNetVersionMax < packageDotNetVersion)
                            packagesDotNetVersionMax = packageDotNetVersion;
                    }
                }

                if (packagesDotNetVersionMax > hostPackageDotNetVersion)
                {
                    StoreResult(OperationResult.OR_FAIL, "Found incompatible package with host service application, reason: .NET framework version");
                    return;
                }

                foreach (var packagePath in packageFolders)
                {
                    if (packagePath.Contains(_hostPackageName))
                        // unpacking host app package
                        UnpackPackage(packagePath, serviceTargetPath, hostPackageDotNetVersion);
                    else
                        UnpackPackage(packagePath, serviceTargetPath, packageDotNetVersions[packagePath]);

                }

                Clean(packageFolders);

                StoreResult(OperationResult.OR_OK, "");
            }
            catch (System.Exception ex)
            {
                StoreResult(OperationResult.OR_FAIL, ex.Message);
            }
        }

    }
}
