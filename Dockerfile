FROM mcr.microsoft.com/dotnet/core/sdk:2.2.401-alpine3.8 as dotnetBuild

COPY ./ /src/
WORKDIR /src/
RUN dotnet publish -c release -o /out

FROM mcr.microsoft.com/dotnet/core/aspnet:2.2.4-alpine3.8

# # Installs latest Chromium (76) package.
# RUN apk add --no-cache \
#         chromium=76.0.3809.132-r0 \
#         nss \
#         freetype \
#         freetype-dev \
#         harfbuzz \
#         ca-certificates \
#         ttf-freefont \
#         pdftk && \
#     apk add --no-cache --repository http://dl-3.alpinelinux.org/alpine/edge/testing/ libgdiplus

# ! IMPORTANT: Keep chromium version synced with
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

# # Install latest chrome dev package and fonts to support major charsets (Chinese, Japanese, Arabic, Hebrew, Thai and a few others)
# # Note: this installs the necessary libs to make the bundled version of Chromium that Puppeteer
# # installs, work.
# RUN apt-get update && apt-get -f install && apt-get -y install wget gnupg2 apt-utils
# RUN wget -q -O - https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - \
#     && sh -c 'echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google.list' \
#     && apt-get update \
#     && apt-get install -y google-chrome-unstable fonts-ipafont-gothic fonts-wqy-zenhei fonts-thai-tlwg fonts-kacst ttf-freefont \
#       --no-install-recommends \
#     && rm -rf /var/lib/apt/lists/*

# Tells software that it is running in container and have all requirements pre-installed.
ENV RUNNING_IN_CONTAINER=1

ENV ASPNETCORE_ENVIRONMENT=Production

WORKDIR /app
COPY --from=dotnetBuild /out/ /app/

EXPOSE 5000

ENTRYPOINT ["dotnet", "Pdf.Storage.dll"]
