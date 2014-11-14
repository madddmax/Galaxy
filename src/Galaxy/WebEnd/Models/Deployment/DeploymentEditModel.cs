﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Codestellation.Galaxy.Domain;
using Nejdb.Bson;

namespace Codestellation.Galaxy.WebEnd.Models.Deployment
{
    public class DeploymentEditModel
    {
        public ObjectId Id { get; set; }

        [Display(Name = "Feed", Prompt = "<Select feed>")]
        public ObjectId FeedId { get; set; }

        public IReadOnlyCollection<KeyValuePair<ObjectId, string>> AllFeeds { get; set; }

        [Display(Name = "Package", Prompt = "Package Id")]
        public string PackageId { get; set; }

        [Display(Name = "Service group", Prompt = "Service group")]
        public string Group { get; set; }

        [Display(Name = "Instance name", Prompt = "Instance name")]
        public string InstanceName { get; set; }

        [Display(Name = "Keep on update", Prompt = "files, folders, allowed file masks")]
        public string KeepOnUpdate { get; set; }

        public DeploymentEditModel()
        {
            
        }

        public DeploymentEditModel(Domain.Deployment deployment)
        {
            Id = deployment.Id;

            InstanceName = deployment.InstanceName;

            FeedId = deployment.FeedId;
            PackageId = deployment.PackageId;

            KeepOnUpdate = (deployment.KeepOnUpdate ?? new FileList(new string[0])).ToString();

            Group = deployment.Group;
        }

        public Domain.Deployment ToDeployment()
        {
            var deployment = new Domain.Deployment
            {
                Id = Id
            };

            Update(deployment);
            return deployment;
        }

        public void Update(Domain.Deployment deployment)
        {
            deployment.InstanceName = InstanceName;
            deployment.FeedId = FeedId;
            deployment.PackageId = PackageId;
            deployment.Group = Group;
            deployment.KeepOnUpdate = (FileList)KeepOnUpdate;
        }

    }
}