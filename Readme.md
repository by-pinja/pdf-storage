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