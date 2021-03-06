﻿using System;
using System.IO;
using System.Linq;
using Codestellation.Galaxy.Domain;
using Codestellation.Galaxy.ServiceManager.Operations;
using Codestellation.Galaxy.Tests.Content;
using Codestellation.Quarks.IO;
using NUnit.Framework;

namespace Codestellation.Galaxy.Tests.DeploymentAndOperations.OperationsTests
{
    [TestFixture]
    public class ClearBinariesTests
    {
        private string _nugetFeedFolder;
        private string _basePath;

        private DeploymentTaskContext _context;

        [SetUp]
        public void Init()
        {
            _nugetFeedFolder = Path.Combine(Environment.CurrentDirectory, "testnuget");
            Folder.EnsureDeleted(_nugetFeedFolder);
            Folder.EnsureExists(_nugetFeedFolder);

            var version10 = new Version(1, 0);

            TestPackages.CopyHostPackageTo(_nugetFeedFolder);
            TestPackages.CopyTest10To(_nugetFeedFolder);

            _basePath = Folder.Combine(_nugetFeedFolder, "extracted");

            var packageDetails = new PackageDetails("Codestellation.Galaxy.Host", _nugetFeedFolder, version10);
            var installPackage = new InstallPackage(_basePath, packageDetails, FileList.Empty);

            var stringWriter = new StringWriter();
            _context = new DeploymentTaskContext(stringWriter);
            installPackage.Execute(_context);
        }

        [Test]
        public void Deletes_files_exclude_specified_file()
        {
            //given
            var fileToSkip = "Codestellation.Galaxy.Host.exe.config";

            var keepOnUpdate = new FileList(new[] { fileToSkip });

            var operation = new ClearBinaries(_basePath, keepOnUpdate);

            //when
            operation.Execute(_context);

            //then
            var file = Directory
                .GetFiles(_basePath, "*.*", SearchOption.AllDirectories)
                .Select(Path.GetFileName)
                .SingleOrDefault();

            Assert.That(file, Is.StringEnding(fileToSkip));
        }

        [Test]
        public void Deletes_folders_exclude_specified_folder()
        {
            //given
            var folderToSkip = "database";
            var aFileInFolder = "read.me";

            var folderPath = Path.Combine(_basePath, folderToSkip);
            Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, aFileInFolder);

            File.WriteAllText(filePath, "Please, please! Do not delete me!");

            var keepOnUpdate = new FileList(new[] { folderToSkip });

            var operation = new ClearBinaries(_basePath, keepOnUpdate);

            //when
            operation.Execute(_context);

            //then
            Assert.That(Directory.Exists(folderPath), Is.True, "Directory was removed");
            Assert.That(File.Exists(filePath), Is.True, "File in directory was removed");
        }

        [TearDown]
        public void Cleanup()
        {
            Folder.EnsureDeleted(_nugetFeedFolder);
        }

        [Test]
        public void Should_not_fail_if_directory_not_found()
        {
            //given
            var fakePath = Folder.Combine(_nugetFeedFolder, "this_folder_does_not_exists");
            var operation = new ClearBinaries(fakePath, FileList.Empty);

            //when
            //then
            Assert.DoesNotThrow(() => operation.Execute(_context));
        }
    }
}