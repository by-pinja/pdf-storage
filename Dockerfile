# ! IMPORTANT: Keep chrome_version synced with the version package 'PuppeteerSharp' expects
# (PuppeteerSharp.BrowserData.Chrome.DefaultBuildId). Chrome for Testing builds are listed at
# https://googlechromelabs.github.io/chrome-for-testing/
# The image uses the 'chrome-headless-shell' build, which is enough for headless pdf generation
# and keeps the image small. Download that build, not 'chrome', when updating the version.
ARG chrome_version=154.0.8037.57

# Runtime libraries chrome-headless-shell needs on Ubuntu 24.04 (noble).
ARG chrome_deps="ca-certificates libasound2t64 libatk-bridge2.0-0t64 libatk1.0-0t64 libatspi2.0-0t64 libdbus-1-3 libexpat1 libgbm1 libglib2.0-0t64 libnspr4 libnss3 libx11-6 libxcb1 libxcomposite1 libxdamage1 libxext6 libxfixes3 libxkbcommon0 libxrandr2"

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS chrome
ARG chrome_version

RUN apt-get update && apt-get install -y --no-install-recommends unzip \
    && curl -fsSL -o /tmp/chrome.zip https://storage.googleapis.com/chrome-for-testing-public/${chrome_version}/linux64/chrome-headless-shell-linux64.zip \
    && unzip -q /tmp/chrome.zip -d /opt \
    && mv /opt/chrome-headless-shell-linux64 /opt/chrome \
    && rm /tmp/chrome.zip

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG chrome_deps

RUN apt-get update && apt-get install -y --no-install-recommends \
    ${chrome_deps} \
    pngquant \
    gifsicle \
    optipng \
    fonts-open-sans \
    fonts-liberation \
    libjpeg-turbo-progs \
    libgdiplus \
    qpdf \
    locales

COPY --from=chrome /opt/chrome/ /opt/chrome/

COPY ./ /src/

WORKDIR /src/

RUN dotnet publish -c release -o /out

ENV PuppeteerChromiumPath=/opt/chrome/chrome-headless-shell

RUN dotnet test

FROM mcr.microsoft.com/dotnet/aspnet:10.0
ARG chrome_deps

RUN apt-get update && apt-get install -y --no-install-recommends \
        ${chrome_deps} \
        pngquant \
        gifsicle \
        optipng \
        fonts-open-sans \
        fonts-liberation \
        libjpeg-turbo-progs \
        libgdiplus \
        qpdf \
        dumb-init \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/*

COPY --from=chrome /opt/chrome/ /opt/chrome/

# Tells software that it is running in container and have all requirements pre-installed.
ENV PuppeteerChromiumPath=/opt/chrome/chrome-headless-shell

ENV ASPNETCORE_ENVIRONMENT=Production

WORKDIR /app
COPY --from=build /out/ /app/

ENV ASPNETCORE_URLS=http://+:80;http://+:5000;

EXPOSE 5000

# dump-init fixes zombie (defunct) process problem with chrome
ENTRYPOINT ["dumb-init", "dotnet", "Pdf.Storage.dll"]
