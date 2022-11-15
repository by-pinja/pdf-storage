# ! IMPORTANT: Keep chromium version synced with version from package 'PuppeteerSharp'
# and match it with from https://pkgs.alpinelinux.org/packages
ARG chromium_version=104.0.5112.79-1~deb11u1

FROM mcr.microsoft.com/dotnet/sdk:6.0 as build
ARG chromium_version

RUN apt-get update
RUN apt -y install \
    locales \
    libgdiplus
RUN sed -i 's/^# *\(fi_FI.UTF-8\)/\1/' /etc/locale.gen
RUN locale-gen

COPY ./ /src/
WORKDIR /src/

RUN dotnet publish -c release -o /out

RUN dotnet test

FROM mcr.microsoft.com/dotnet/aspnet:6.0
ARG chromium_version

COPY --from=build /src/pkg/ /tmp/pkg/
RUN apt-get update
RUN apt-get install -y \
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
    dumb-init

RUN apt-get clean
RUN rm /tmp/pkg/*.deb

# Tells software that it is running in container and have all requirements pre-installed.
ENV PuppeteerChromiumPath=/usr/bin/chromium

ENV ASPNETCORE_ENVIRONMENT=Production

WORKDIR /app
COPY --from=build /out/ /app/

EXPOSE 5000

# dump-init fixes zombie (defunct) process problem with chrome
ENTRYPOINT ["dumb-init", "dotnet", "Pdf.Storage.dll"]
