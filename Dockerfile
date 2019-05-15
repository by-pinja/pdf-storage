FROM mcr.microsoft.com/dotnet/core/aspnet:2.2.4-alpine3.8

RUN apk add --no-cache chromium pdftk && \
    apk add --no-cache --repository http://dl-3.alpinelinux.org/alpine/edge/testing/ libgdiplus

WORKDIR /app
COPY out .

EXPOSE 5000

ENTRYPOINT ["dotnet", "Pdf.Storage.dll"]
