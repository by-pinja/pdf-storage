FROM microsoft/dotnet:1.1-runtime

RUN apt-get update && apt-get -y install nodejs && alias node=nodejs

WORKDIR /app
COPY out .

EXPOSE 5000

ENTRYPOINT ["dotnet", "Pdf.Storage.dll"]
