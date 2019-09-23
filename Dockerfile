FROM mcr.microsoft.com/dotnet/core/sdk:2.2.401-alpine3.8 as dotnetBuild

COPY ./ /src/
WORKDIR /src/
RUN dotnet publish -c release -o /out

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2.4-alpine3.8

# ! IMPORTANT: Keep chromium version synced with version from package 'PuppeteerSharp'
# and match it with from https://pkgs.alpinelinux.org/packages
RUN \
  echo "http://dl-cdn.alpinelinux.org/alpine/edge/community" >> /etc/apk/repositories \
  && echo "http://dl-cdn.alpinelinux.org/alpine/edge/main" >> /etc/apk/repositories \
  && echo "http://dl-cdn.alpinelinux.org/alpine/edge/testing" >> /etc/apk/repositories \
  && apk --no-cache  update \
  && apk --no-cache  upgrade \
  && apk add --no-cache --virtual .build-deps \
    gifsicle \
    pngquant \
    optipng \
    libjpeg-turbo-utils \
    udev \
    ttf-opensans \
    chromium=76.0.3809.132-r0 \
    libgdiplus \
    pdftk \
  && rm -rf /var/cache/apk/* /tmp/*

# Tells software that it is running in container and have all requirements pre-installed.
ENV RUNNING_IN_CONTAINER=1

ENV ASPNETCORE_ENVIRONMENT=Production

WORKDIR /app
COPY --from=dotnetBuild /out/ /app/

EXPOSE 5000

ENTRYPOINT ["dotnet", "Pdf.Storage.dll"]
