﻿using CG.Web.MegaApiClient;
using Mirror2MegaNZ.DomainModel;
using NLog;

namespace Mirror2MegaNZ.Logic
{
    /// <summary>
    /// This class compute the actions to be executed to align remote nodes structure with
    /// the local nodes structure
    /// </summary>
    public class Synchronizer
    {
        private readonly IMegaApiClient _megaClient;
        private readonly IFileManager _fileManager;
        private readonly Cleaner _cleaner;
        private readonly Updater _updater;

        public Synchronizer(IMegaApiClient client, IFileManager fileManager)
        {
            _megaClient = client;
            _fileManager = fileManager;
            _cleaner = new Cleaner(_megaClient);
            _updater = new Updater(_megaClient, _fileManager);
        }

        public void SyncronizeFolder(LocalNode localRoot, MegaNZTreeNode remoteRoot, ILogger logger)
        {
            logger.Trace("Cleanin up remote repository");
            _cleaner.CleanUp(localRoot, remoteRoot, logger);

            logger.Trace("Updating remote repository");
            _updater.Update(localRoot, remoteRoot, logger);
        }
    }
}