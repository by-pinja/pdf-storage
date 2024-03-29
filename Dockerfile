# ! IMPORTANT: Keep chromium version synced with version from package 'PuppeteerSharp'
# and match it with from https://tracker.debian.org/pkg/chromium
# Download the install packages and place them in the pkg/ folder and update chromium_version here accordingly
ARG chromium_version=119.0.6045.199-1~deb12u1

FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
ARG chromium_version

COPY ./ /src/

RUN apt-get update
RUN apt-get install -y --no-install-recommends \
    /src/pkg/chromium-common_${chromium_version}_amd64.deb \
    /src/pkg/chromium_${chromium_version}_amd64.deb \
    pngquant \
    gifsicle \
    optipng \
    fonts-open-sans \
    fonts-liberation \
    libjpeg-turbo-progs \
    libgdiplus \
    qpdf \
    locales

WORKDIR /src/

RUN dotnet publish -c release -o /out

ENV PuppeteerChromiumPath=/usr/bin/chromium

RUN dotnet test

FROM mcr.microsoft.com/dotnet/aspnet:8.0
ARG chromium_version

COPY --from=build /src/pkg/ /tmp/pkg/

RUN apt-get update && apt-get install -y --no-install-recommends \
        /tmp/pkg/chromium-common_${chromium_version}_amd64.deb \
        /tmp/pkg/chromium_${chromium_version}_amd64.deb \
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
    && rm /tmp/pkg/*.deb

# Tells software that it is running in container and have all requirements pre-installed.
ENV PuppeteerChromiumPath=/usr/bin/chromium

ENV ASPNETCORE_ENVIRONMENT=Production

WORKDIR /app
COPY --from=build /out/ /app/

ENV ASPNETCORE_URLS=http://+:80;http://+:5000;

EXPOSE 5000

# dump-init fixes zombie (defunct) process problem with chrome
ENTRYPOINT ["dumb-init", "dotnet", "Pdf.Storage.dll"]
