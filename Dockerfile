FROM mcr.microsoft.com/dotnet/core/runtime:2.2

RUN apt-get update \
    && apt-get -y install gnupg \
    && curl -sL https://deb.nodesource.com/setup_8.x | bash \
    && apt-get -y install nodejs \
    && apt-get -y install bzip2 \
    && apt-get -y install libfontconfig \
    && apt-get -y install pdftk

WORKDIR /app
COPY out .
COPY node_modules node_modules

RUN npm rebuild phantomjs-prebuilt

EXPOSE 5000

ENTRYPOINT ["dotnet", "Pdf.Storage.dll"]
