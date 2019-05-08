FROM mcr.microsoft.com/dotnet/core/aspnet:2.2.4-alpine3.8

RUN apk add --no-cache chromium pdftk

WORKDIR /app
COPY out .

ENV ASPNETCORE_ENVIRONMENT=Development

EXPOSE 5000

ENTRYPOINT ["dotnet", "Pdf.Storage.dll"]
