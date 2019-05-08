FROM mcr.microsoft.com/dotnet/core/runtime:2.2.4-alpine3.9

RUN apk add --update --no-cache pdftk chromium

WORKDIR /app
COPY out .

EXPOSE 5000

ENTRYPOINT ["dotnet", "Pdf.Storage.dll"]
