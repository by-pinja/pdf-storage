FROM mcr.microsoft.com/dotnet/core/sdk:2.2.401-alpine3.8 as dotnetBuild

COPY ./ /src/
WORKDIR /src/
RUN dotnet publish -c release -o /out

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2.4-alpine3.8

RUN apk add --no-cache chromium pdftk && \
    apk add --no-cache --repository http://dl-3.alpinelinux.org/alpine/edge/testing/ libgdiplus

WORKDIR /app
COPY --from=dotnetBuild /out/ /app/

EXPOSE 5000

ENTRYPOINT ["dotnet", "Pdf.Storage.dll"]
