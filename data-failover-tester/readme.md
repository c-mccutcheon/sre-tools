# Data failover testing tool

## Purpose

This tool was created to ingest a continuous stream of records into a chosen provider, whilst running a consumer on a different thread to return the total number of records. The operations will run in parallel until either the process is terminated or a default timeout is reached.

The tool can be used when testing out functionality such as **CosmosDB** manual-failover operations to check data continues to be written and read even in the event of region failovers.

## Available Providers

**CosmosDB** - The tool will ingest CosmosDB Documents to a provided CosmosDB Account whilst simultaneously reading them.
**SQL** - Will be supported soon.

## How to use

The tool is compiled as a command-line utility which has several options depending on the **Provider** used. Help is available by using the --help command, either on the overall tool, or each individual provider. You can use this tool to continuously produce and consume data whilst testing extraneous
operations on Azure resources, such as manually invoked failover operations (CosmosDB) or region outages (Azure SQL).

### Examples

- **Help** Shows tool help
  
    ```command
    > data-failover-tester --help
    Usage: data-failover-tester [command] [options]

    Options:
    -?|-h|--help  Show help information

    Commands:
    cosmosdb
    sql

    Run 'data-failover-tester [command] -?|-h|--help' for more information about a command.

- **CosmosDB** Shows CosmosDB provider help

    ```command
    > data-failover-tester cosmosdb --help
    Usage: data-failover-tester cosmosdb [options] <dbname> <dbcollectionname> <dbserver> <dbidentity> <dbpassword> <delete>

    Arguments:
    dbname            The database name to generate for the test.
    dbcollectionname  The collection name to generate for the test.
    dbserver          The database server. Should be a URI and will use port 10255 by default.
    dbidentity        The database server identity. Must have read/write access.
    dbpassword        The database server password. This should be the read/write access key.
    delete            Deletes all data resources before the test run.

    Options:
    -?|-h|--help      Show help information

- **CosmosDB** Execute CosmosDB Provider

    ```command
    > data-failover-tester cosmosdb failover-testing test-collection your-mongo-server.mongo.cosmos.azure.com failover-testing-eu databasepassword2094820 true
    [11:14]Beginning operations...

    [11:14]#### Settings ####
    [11:14]Timeout(s) :: 30
    [11:14]Delete data:: True
    [11:14]Created or Fetched Database: failover-testing/test-collection

    [11:14]Dropped Database Collection: SRETestCollection

    [11:14]Created or Fetched Database: failover-testing/test-collection

    [11:14]Data consumer thread... :: read document count 0
    [11:14]Data consumer thread... :: read document count 0
    [11:14]Data consumer thread... :: read document count 0
    [11:15]Data consumer thread... :: read document count 0
    [11:15]Data producer thread... :: session e8aa62a0-33a2-46e3-8d71-3aeb777f4f90 inserted document count 1
    [11:15]Data consumer thread... :: read document count 1
    [11:15]Data producer thread... :: session e8aa62a0-33a2-46e3-8d71-3aeb777f4f90 inserted document count 2
    [11:15]Data producer thread... :: session e8aa62a0-33a2-46e3-8d71-3aeb777f4f90 inserted document count 3
    [11:15]Data consumer thread... :: read document count 3
    [11:15]Data producer thread... :: session e8aa62a0-33a2-46e3-8d71-3aeb777f4f90 inserted document count 4
    [11:15]Data producer thread... :: session e8aa62a0-33a2-46e3-8d71-3aeb777f4f90 inserted document count 5
    ...
    ...
    [11:15]Data producer thread... :: session e8aa62a0-33a2-46e3-8d71-3aeb777f4f90 inserted document count 150
    [11:15]Dropped Database Collection: test-collection
    [11:15]End of thread barrier or default timeout. Cleaned up resources.
