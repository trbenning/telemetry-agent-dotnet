// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Diagnostics;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Exceptions;
using Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Runtime;

namespace Microsoft.Azure.IoTSolutions.IoTStreamAnalytics.Services.Helpers
{
    public interface IDocumentWriter
    {
        Task OpenAsync(IStorageServiceConfig config);
        Task WriteAsync(Document doc);
    }

    public class DocumentWriter : IDocumentWriter
    {
        private readonly ILogger logger;

        private IDocumentClient client;
        private string connString;
        private string databaseId;
        private string collectionId;
        private string databaseLink;
        private string collectionLink;
        private RequestOptions options;

        public DocumentWriter(ILogger logger)
        {
            this.logger = logger;
        }

        public async Task OpenAsync(IStorageServiceConfig config)
        {
            connString = config.DocumentDbConnString;
            databaseId = config.DocumentDbDatabase;
            collectionId = config.DocumentDbCollection;
            databaseLink = $"/dbs/{databaseId}";
            collectionLink = $"/dbs/{databaseId}/colls/{collectionId}";
            options = new RequestOptions
            {
                OfferThroughput = config.DocumentDbRUs,
                ConsistencyLevel = ConsistencyLevel.Eventual
            };

            Uri uri;
            string key;
            ParseConnString(connString, out uri, out key);

            client = new DocumentClient(uri, key, ConnectionPolicy.Default, ConsistencyLevel.Eventual);

            await CreateDatabaseIfNotExistsAsync();
            await CreateCollectionIfNotExistsAsync();
        }

        public async Task WriteAsync(Document doc)
        {
            try
            {
                await client.CreateDocumentAsync(collectionLink, doc, options, true);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode != HttpStatusCode.Conflict)
                {
                    logger.Error("Error while writing message", () => new { e });
                    // TODO: fix, otherwise message gets lost. When the service
                    // fails to write the message to storage, it should either retry
                    // or stop processing the following messages. The current behavior
                    // of logging the error and moving on, means that in case of
                    // error, the message has not been stored.
                    // see https://github.com/Azure/telemetry-agent-java/issues/35
                }
            }
        }

        private static void ParseConnString(string connString, out Uri uri, out string key)
        {
            var match = Regex.Match(connString, "^AccountEndpoint=(?<endpoint>.*);AccountKey=(?<key>.*);$");
            if (!match.Success)
            {
                throw new InvalidConfigurationException("Invalid connection string for Cosmos DB");
            }

            uri = new Uri(match.Groups["endpoint"].Value);
            key = match.Groups["key"].Value;
        }

        private async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await client.ReadDatabaseAsync(databaseLink, options);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode != HttpStatusCode.NotFound)
                {
                    logger.Error("Error while getting DocumentDb database", () => new { e });
                    throw;
                }

                await CreateDatabaseAsync();
            }
            catch (Exception e)
            {
                logger.Error("Error while getting DocumentDb database", () => new { e });
                throw;
            }
        }

        private async Task CreateCollectionIfNotExistsAsync()
        {
            try
            {
                await client.ReadDocumentCollectionAsync(collectionLink, options);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode != HttpStatusCode.NotFound)
                {
                    logger.Error("Error while getting DocumentDb collection", () => new { e });
                    throw;
                }

                await CreateCollectionAsync();
            }
            catch (Exception e)
            {
                logger.Error("Error while getting DocumentDb collection", () => new { e });
                throw;
            }
        }

        private async Task CreateDatabaseAsync()
        {
            try
            {
                logger.Info($"Creating DocumentDb database: {databaseId}", () => { });
                var database = new Database
                {
                    Id = databaseId
                };
                await client.CreateDatabaseAsync(database, options);
            }
            catch (Exception e)
            {
                logger.Error("Error while creating DocumentDb database", () => new { e });
                throw;
            }
        }

        private async Task CreateCollectionAsync()
        {
            try
            {
                logger.Info($"Creating DocumentDb collection: {collectionId}", () => { });
                var collection = new DocumentCollection
                {
                    Id = collectionId,
                    IndexingPolicy = new IndexingPolicy(Index.Range(DataType.String, -1))
                    {
                        IndexingMode = IndexingMode.Consistent
                    },
                    PartitionKey = new PartitionKeyDefinition
                    {
                        Paths = new Collection<string> { "/id" }
                    }
                };

                await client.CreateDocumentCollectionAsync(databaseLink, collection, options);
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode != HttpStatusCode.Conflict)
                {
                    logger.Error("Error while getting DocumentDb collection", () => new { e });
                    throw;
                }

                logger.Warn("Another process already created the collection", () => new { e });
            }
            catch (Exception e)
            {
                logger.Error("Error while creating DocumentDb collection", () => new { e });
                throw;
            }
        }
    }
}
