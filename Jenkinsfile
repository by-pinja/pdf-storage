library 'jenkins-ptcs-library@1.0.0'

podTemplate(label: pod.label,
  containers: pod.templates + [
    containerTemplate(name: 'dotnet', image: 'mcr.microsoft.com/dotnet/core/sdk:2.2.401-alpine3.8', ttyEnabled: true, command: '/bin/sh -c', args: 'cat'),
  ]
) {
    def project = 'pdf-storage'

    node(pod.label) {
      stage('Checkout') {
         checkout scm
      }
      stage('Prepare') {
        container('dotnet') {
          sh """
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
              pdftk
          """
        }
      }
      stage('Build') {
        container('dotnet') {
          sh """
            dotnet build
          """
        }
      }
      stage('Test') {
        container('dotnet') {
          sh """
            export PuppeteerChromiumPath=/usr/bin/chromium-browser
            dotnet test
          """
        }
      }
      stage('Package') {
        container('docker') {
          def publishedImage = publishContainerToGcr(project);
          publishTagToDockerhub(project);
          if(env.BRANCH_NAME == "master") {
              updateImageToK8sTestEnv(publishedImage)
          }
        }
      }
    }
  }
