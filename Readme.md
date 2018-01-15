# Getting started

```bash
npm install
dotnet run
```

Navigate [http://localhost:5000/doc/](http://localhost:5000/doc/)

## Local development, mocks enabled
Create `appsettings.localdev.json` in root directory with content, this file is ignored in git:
```js
{
	"Mock": {
		"Mq": "true",
		"Db": "true",
		"GoogleBucket": "false"
	}
}
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