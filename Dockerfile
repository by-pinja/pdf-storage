# ! IMPORTANT: Keep chromium version synced with version from package 'PuppeteerSharp'
# and match it with from https://pkgs.alpinelinux.org/packages
ARG chromium_version=77.0.3865.120-r0

FROM mcr.microsoft.com/dotnet/core/sdk:3.1.100-alpine3.10 as dotnetBuild
ARG chromium_version

RUN \
  echo "http://dl-cdn.alpinelinux.org/alpine/edge/community" >> /etc/apk/repositories &&\
  echo "http://dl-cdn.alpinelinux.org/alpine/edge/main" >> /etc/apk/repositories && \
  echo "http://dl-cdn.alpinelinux.org/alpine/edge/testing" >> /etc/apk/repositories && \
  apk --no-cache  update && \
  apk --no-cache  upgrade && \
  apk add --no-cache \
    gifsicle \
    pngquant \
    optipng \
    libjpeg-turbo-utils \
    udev \
    ttf-opensans \
    chromium=${chromium_version} \
    libgdiplus \
    qpdf

COPY ./ /src/
WORKDIR /src/

ENV PuppeteerChromiumPath=/usr/bin/chromium-browser

RUN dotnet publish -c release -o /out

RUN dotnet test

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.0-alpine3.10
ARG chromium_version

RUN \
  echo "http://dl-cdn.alpinelinux.org/alpine/edge/community" >> /etc/apk/repositories &&\
  echo "http://dl-cdn.alpinelinux.org/alpine/edge/main" >> /etc/apk/repositories && \
  echo "http://dl-cdn.alpinelinux.org/alpine/edge/testing" >> /etc/apk/repositories && \
  apk --no-cache  update && \
  apk --no-cache  upgrade && \
  apk add --no-cache \
    gifsicle \
    pngquant \
    optipng \
    libjpeg-turbo-utils \
    udev \
    ttf-opensans \
    chromium=${chromium_version} \
    libgdiplus \
    qpdf

# Tells software that it is running in container and have all requirements pre-installed.
ENV PuppeteerChromiumPath=/usr/bin/chromium-browser

ENV ASPNETCORE_ENVIRONMENT=Production

WORKDIR /app
COPY --from=dotnetBuild /out/ /app/

EXPOSE 5000

ENTRYPOINT ["dotnet", "Pdf.Storage.dll"]
