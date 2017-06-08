FROM microsoft/dotnet:1.1-runtime

RUN apt-get update \
    && curl -sL https://deb.nodesource.com/setup_8.x | bash \
    && apt-get -y install nodejs

WORKDIR /app
COPY out .
COPY node_modules node_modules

EXPOSE 5000

ENTRYPOINT ["dotnet", "Pdf.Storage.dll"]
