library 'jenkins-ptcs-library@0.6.1'

podTemplate(label: pod.label,
  containers: pod.templates + [
    containerTemplate(name: 'dotnet', image: 'mcr.microsoft.com/dotnet/core/sdk:2.2.203-alpine3.8', ttyEnabled: true, command: '/bin/sh -c', args: 'cat'),
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
            apk add --no-cache chromium pdftk
            apk add --no-cache --repository http://dl-3.alpinelinux.org/alpine/edge/testing/ libgdiplus
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
