using CG.Web.MegaApiClient;
using Mirror2MegaNZ.Logic;
using Mirror2MegaNZ.V2.DomainModel.Commands;
using NLog;
using Polly;
using System;
using System.Collections.Generic;

namespace Mirror2MegaNZ.V2.Logic
{
    /// <summary>
    /// This is the class that execute the command specified in the ICommand items
    /// </summary>
    internal class CommandExecutor
    {
        private readonly IMegaApiClient _megaApiClient;
        private readonly ILogger _logger;

        public CommandExecutor(IMegaApiClient megaApiClient, ILogger logger)
        {
            _megaApiClient = megaApiClient;
            _logger = logger;
        }
          
        public void Execute(IEnumerable<ICommand> commands, 
            IMegaNzItemCollection megaNzItemCollection,
            IFileManager fileManager,
            IProgress<double> progressNotifier)
        {
            foreach(var command in commands)
            {
                _logger.Trace(string.Empty);
                _logger.Trace(command.ToString());

                Policy.Handle<Exception>()
                    .Retry(5, (ex, counter) => {
                        _logger.Trace(string.Empty);   // New Line
                        _logger.Trace("An exception occurred: " + ex.GetType());
                        if( ex is AggregateException)
                        {
                            var aggregateException = (AggregateException)ex;
                            if( aggregateException.InnerExceptions.Count > 0 )
                            {
                                var innerException = aggregateException.InnerExceptions[0];
                                _logger.Trace("Inner exception: " + innerException.GetType());
                                _logger.Trace("Inner exception message: " + innerException.Message);
                            }
                        }
                        _logger.Trace("Retry #" + counter);
                    })
                    .Execute(() =>
                    {
                        command.Execute(_megaApiClient, megaNzItemCollection, fileManager, progressNotifier);
                    });
            }
        }
    }
}
