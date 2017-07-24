FROM microsoft/dotnet:1.1-runtime

RUN apt-get update \
    && curl -sL https://deb.nodesource.com/setup_8.x | bash \
    && apt-get -y install nodejs \
    && apt-get -y install bzip2 \
    && apt-get -y install libfontconfig

RUN apt-get -y install pdftk

WORKDIR /app
COPY out .
COPY node_modules node_modules

RUN npm rebuild phantomjs-prebuilt

EXPOSE 5000

ENTRYPOINT ["dotnet", "Pdf.Storage.dll"]
