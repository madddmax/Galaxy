﻿using System.IO;
using System.Linq;
using Codestellation.Emisstar;
using Codestellation.Galaxy.Domain;
using Codestellation.Galaxy.ServiceManager.Operations;
using Codestellation.Quarks.DateAndTime;
using Codestellation.Quarks.IO;

namespace Codestellation.Galaxy.ServiceManager
{
    public class TaskBuilder
    {
        private readonly OperationBuilder _operations;
        private readonly IPublisher _publisher;

        public TaskBuilder(OperationBuilder operationBuilder, IPublisher publisher)
        {
            _operations = operationBuilder;
            _publisher = publisher;
        }

        public DeploymentTask DeployServiceTask(Deployment deployment, NugetFeed deploymentFeed)
        {
            return CreateDeployTask("UpdateService", deployment)
                .Add(_operations.StopService(deployment, true))
                .Add(_operations.BackupService(deployment))
                .Add(_operations.ClearBinaries(deployment))
                .Add(_operations.EnsureFolders())
                .Add(_operations.InstallPackage(deployment, deploymentFeed, deployment.KeepOnUpdate.Clone()))
                .Add(_operations.DeployHostConfig(deployment))
                .Add(_operations.GetConfigSample(deployment))
                .Add(_operations.DeployServiceConfig(deployment))
                .Add(_operations.StartService(deployment));
        }

        public DeploymentTask InstallServiceTask(Deployment deployment)
        {
            return CreateDeployTask("InstallService", deployment)
                .Add(_operations.InstallService(deployment));
        }

        public DeploymentTask UninstallServiceTask(Deployment deployment)
        {
            return CreateDeployTask("UninstallService", deployment)
                .Add(_operations.StopService(deployment, true))
                .Add(_operations.UninstallService(deployment));
        }

        public DeploymentTask StartServiceTask(Deployment deployment)
        {
            var startServiceTask = CreateDeployTask("StartService", deployment)
                .Add(_operations.StartService(deployment));
            startServiceTask.Context.SetValue(DeploymentTaskContext.ForceStartService, true);
            return startServiceTask;
        }

        public DeploymentTask StopServiceTask(Deployment deployment)
        {
            return CreateDeployTask("StopService", deployment)
                .Add(_operations.StopService(deployment, false));
        }

        private DeploymentTask CreateDeployTask(string name, Deployment deployment, Stream logStream = null)
        {
            var actualLogStream = logStream ?? BuildDefaultLogStream(name, deployment);
            var streamWriter = new StreamWriter(actualLogStream);

            var context = new DeploymentTaskContext(streamWriter);
            context
                .SetValue(DeploymentTaskContext.TaskName, name)
                .SetValue(DeploymentTaskContext.DeploymentId, deployment.Id)
                .SetValue(DeploymentTaskContext.PublisherKey, _publisher)
                .SetValue(DeploymentTaskContext.LogStream, actualLogStream)
                .SetValue(DeploymentTaskContext.Folders, deployment.ServiceFolders.HostFolders.ToArray())
                .SetValue(DeploymentTaskContext.Config, deployment.Config);

            return new DeploymentTask(context);
        }

        private static FileStream BuildDefaultLogStream(string name, Deployment deployment)
        {
            var deployLogFolder = deployment.GetDeployLogFolder();
            Folder.EnsureExists(deployLogFolder);

            var filename = $"{name}.{Clock.UtcNow.ToLocalTime():yyyy-MM-dd_HH.mm.ss}.log";
            var fullPath = Path.Combine(deployLogFolder, filename);
            var defaultStream = File.Open(fullPath, FileMode.Create, FileAccess.Write);
            return defaultStream;
        }

        public DeploymentTask RestoreFromBackup(Deployment deployment, string backupFolder)
        {
            return CreateDeployTask("Restore From Backup", deployment)
                .Add(_operations.StopService(deployment, true))
                .Add(_operations.BackupService(deployment))
                .Add(_operations.ClearBinaries(deployment, FileList.Empty))
                .Add(_operations.RestoreFrom(deployment, backupFolder));
        }

        public DeploymentTask DeleteDeploymentTask(Deployment deployment)
        {
            return CreateDeployTask("DeleteDeployment", deployment, new MemoryStream(1024)) //We are going to delete directory where logs would be written. That's why we hack it!
                .Add(_operations.StopService(deployment, skipIfNotFound: true))
                .Add(_operations.UninstallService(deployment, skipIfNotFound: true))
                .Add(_operations.DeleteFolders(deployment))
                .Add(_operations.UninstallPackage(deployment))
                .Add(_operations.PublishDeletedEvent(deployment));
        }
    }
}