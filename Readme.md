# Getting started

```bash
npm install
dotnet restore
dotnet build
```

## Configure local test database
```bash
docker run --name pdf-storage-postgress -e POSTGRES_PASSWORD=passwordfortesting -it -p 5432:5432 postgres
```