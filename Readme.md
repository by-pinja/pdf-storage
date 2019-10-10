[![Docker pulls](https://img.shields.io/docker/pulls/ptcos/pdf-storage.svg)](https://hub.docker.com/r/ptcos/pdf-storage/)
[![Build Status](https://jenkins.protacon.cloud/buildStatus/icon?job=www.github.com/pdf-storage/master)](https://jenkins.protacon.cloud/job/www.github.com/job/pdf-storage/job/master/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

# PDF-STORAGE

PDF storage is designed to be a universally usable PDF generator from HTML templates.

Key aspects:

- The consumer of the API doesn't have to care how PDFs, templates and so on are persisted - 
  the storage will handle that part.
- A simple API which takes HTML and templating data and responds with
  a final PDF file URL.
- Supports merging PDFs as one.
- Can handle very large amounts of generated PDFs and prioritize files which require
  a faster output (files which are currently opened if the queue is long) before everything else.
- Supports AWS, Azure and Google services and data stores.

For further details, see the [API description document](ApiDescription.md).

## Testing locally in docker

Easiest way to run and test application is to start in with docker

```bash
docker run -it -p 5000:5000 -e ASPNETCORE_ENVIRONMENT=Development ptcos/pdf-storage
```

Navigate to [http://localhost:5000/doc](http://localhost:5000/doc)

## Running locally for development

Install .NET core SDK.

```bash
dotnet run --environment=Development
```

The default development setup mocks all external dependencies.

Navigate to [http://localhost:5000/doc/](http://localhost:5000/doc/).

The following headers must be included in API calls:

```
Authorization: ApiKey apikeyfortesting
Content-Type: application/json-patch+json
```

## Local development with mocks enabled

The default `Development` environment setup enables all mocks in `appsettings.Development.json`.
This way the service should start and function correctly without any external dependencies.

```json
{
  "DbType": "inMemory",
  "MqType": "inMemory",
  "PdfStorageType": "inMemory"
}
```

These can be overwritten with environment variables: `PdfStorageType = "inMemory"` etc.

On Linux (Debian) set development, install pdftk and chromium.

```bash
sudo apt-get -y install pdftk chromium
```

On Windows, Chrome is required.

## Run a local development database

PostgreSQL:

```bash
docker run --name pdf-storage-postgress -e POSTGRES_PASSWORD=passwordfortesting -it -p 5432:5432 postgres
```

Connect with `User ID=postgres;Password=passwordfortesting;Host=localhost;Port=5432;Database=pdfstorage;Pooling=true;`.

SqlServer:

```bash
docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=testpassword1#!' -p 1433:1433 --name sql1 -d mcr.microsoft.com/mssql/server:2017-latest
```

Connect with `Server=localhost,1433;Database=pdf-storage;User=sa;Password=testpassword1#!`

## Hangfire dashboard

Dashboard allows traffic only from localhost on production builds.

For this reason, port forwarding to the running container is required (in Kubernetes).

```bash
kubectl -n pdf-storage port-forward pdf-storage-master-4075827073-5h77r 5000
```

Navigate (via forwarded port) to [http://localhost:5000/hangfire](http://localhost:5000/hangfire).

## Other technologies

The localhost requirement applies, but access methods may vary.

## PDF stores

### Google bucket

PDF storage supports Google bucket for saving PDF binaries.

Mount a valid service account file, configure its path and Google configurations in appconfig or environment variables.

```json
{
  "PdfStorageType": "googleBucket",
  "GoogleCloud": {
    "GoogleBucketName": "pdf-storage-master",
    "GoogleAuthFile": "/path/to/key/google.key.json"
}
```

Example (not valid) service account file, see Google service accounts for further information.

```json
{
  "type": "service_account",
  "project_id": "some-project",
  "private_key_id": "1234",
  "private_key": "-----BEGIN PRIVATE KEY-----\nKEY_SHOULD_BE_HERE-----END PRIVATE KEY-----\n",
  "client_email": "pdf-storage-master@some-project.com",
  "client_id": "1234",
  "auth_uri": "https://accounts.google.com/o/oauth2/auth",
  "token_uri": "https://accounts.google.com/o/oauth2/token",
  "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
  "client_x509_cert_url": "https://www.googleapis.com/robot/v1/metadata/x509/pdf-storage-master%40some-project.some-project.com"
}
```

### AWS S3

Configure application to use AWS S3 store.

```json
{
  "PdfStorageType": "awsS3",
  "AwsS3": {
    "AwsS3BucketName": "pdf-storage-master",
    "AccessKey": "thisisaccesskey",
    "SecretKey": "ThisIsSecretKey",
    "AwsServiceURL": "http://localhost:9000",
    "AwsRegion": "EUCentral1"
  }
}
```

### Azure storage

PDF storage supports Azure storage accounts as storage.

```json
{
  "PdfStorageType": "azureStorage",
  "AzureStorage": {
    "StorageConnectionString": "DefaultEndpointsProtocol=https;AccountName=[your_account];AccountKey=[your_key];EndpointSuffix=core.windows.net",
    "ContainerName": "pdf-storage"
}
```

## Migrations

There is a special script for migrations since multiple database engines are supported.

```powershell
./AddOrRemoveMigrations.ps1 -MigrationName "DescriptionForMigration"
```

### Removing the latest migration

```powershell
./AddOrRemoveMigrations.ps1 -Operation Remove -MigrationName "DescriptionForMigration"
```

Useful for development.