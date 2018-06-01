[![Build Status](https://jenkins.protacon.cloud/buildStatus/icon?job=www.github.com/pdf-storage/master)](https://jenkins.protacon.cloud/job/www.github.com/job/pdf-storage/job/master/)

# Getting started

```bash
npm install
dotnet run
```

Navigate [http://localhost:5000/doc/](http://localhost:5000/doc/)

## Local development, mocks enabled
At default setup, all mocks are enabled in `appsettings.Development.json`. This way service should start and fuction correctly without any external depencies.

Set `$Env:ASPNETCORE_ENVIRONMENT = "Development"` if run from command line. Visual studio defaults to development environment.
```js
{
	"Mock": {
		"Mq": "true",
		"Db": "true",
        	"GoogleBucket": "true"
	}
}
```

Or overwrite them with environment variables `Mock__Mq = "true"` etc.

On linux set development and install pdftk.
```bash
sudo apt-get -y install pdftk
EXPORT ASPNETCORE_ENVIRONMENT=Development
```

## Run local development database
```bash
docker run --name pdf-storage-postgress -e POSTGRES_PASSWORD=passwordfortesting -it -p 5432:5432 postgres
```

## Hangfire dashboard

Dashboard allows traffic only from localhost. For this reason portforwarding to running container is required.

### Getting correct pod

```bash
kubectl get pod -n pdf-storage

NAME                                  READY     STATUS    RESTARTS   AGE
pdf-storage-master-4075827073-5h77r   1/1       Running   0          6m
```

### Port forwarding
```bash
kubectl -n pdf-storage port-forward pdf-storage-master-4075827073-5h77r 5000
```

Navigate [http://localhost:5000/hangfire](http://localhost:5000/hangfire)

# PDF stores
## Google bucket
Pdf storage supports google bucket for saving pdf binaries.

Mount valid service account file and configure it's path and configure google configurations in appconfig or environment variables.
```json
{
  "PdfStorageType": "googleBucket",
  "googleBucketName": "pdf-storage-master",
  "googleAuthFile": "/path/to/key/google.key.json",
}
```

Example (not valid) service account file, see google service accounts for futher information.
```json
{
  "type": "service_account",
  "project_id": "ptcs-internal",
  "private_key_id": "8349f90611b76043d8bf01ae4cb09835434cb9bb",
  "private_key": "-----BEGIN PRIVATE KEY-----\nKEY_SHOULD_BE_HERE-----END PRIVATE KEY-----\n",
  "client_email": "pdf-storage-master@ptcs-internal.iam.gserviceaccount.com",
  "client_id": "101865608634637923419",
  "auth_uri": "https://accounts.google.com/o/oauth2/auth",
  "token_uri": "https://accounts.google.com/o/oauth2/token",
  "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
  "client_x509_cert_url": "https://www.googleapis.com/robot/v1/metadata/x509/pdf-storage-master%40ptcs-internal.iam.gserviceaccount.com"
}
```

## AWS S3
Configure application to use pdf store:
```json
{
  "PdfStorageType": "awsS3"
}
```

Then configure AWS configuration:
```json
{
  "AwsS3": {
    "AwsS3BucketName": "pdf-storage-master",
    "AccessKey": "thisisaccesskey",
    "SecretKey": "ThisIsSecretKey",
    "AwsServiceURL": "http://localhost:9000",
    "AwsRegion": "EUCentral1"
  }
}
```