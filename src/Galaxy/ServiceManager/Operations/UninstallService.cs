﻿using Codestellation.Galaxy.ServiceManager.Helpers;
using System.IO;

namespace Codestellation.Galaxy.ServiceManager.Operations
{
    public class UninstallService : IOperation
    {
        private readonly string _serviceFolder;
        private readonly string _hostFileName;
        private readonly string _instance;

        public UninstallService(string serviceFolder, string hostFileName, string instance)

        {
            _serviceFolder = serviceFolder;
            _hostFileName = hostFileName;
            _instance = instance;
        }

        public void Execute(DeploymentTaskContext context)
        {
            var exePath = Path.Combine(_serviceFolder, _hostFileName);

            var exeParams = string.Format("uninstall -instance:{0}", _instance);

            context.BuildLog.WriteLine("Executing '{0} {1}'", exePath, exeParams);

            string error;
            var result = ProcessStarter.ExecuteWithParams(exePath, exeParams, out error);

            context.BuildLog.WriteLine("Exe output:");
            context.BuildLog.WriteLine(result);

            context.BuildLog.WriteLine("Exe error:");
            context.BuildLog.WriteLine(error);
        }
    }
}
