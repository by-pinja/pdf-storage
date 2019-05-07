library 'jenkins-ptcs-library@0.5.0'

podTemplate(label: pod.label,
  containers: pod.templates + [
    containerTemplate(name: 'dotnet', image: 'mcr.microsoft.com/dotnet/core/sdk:2.2', ttyEnabled: true, command: '/bin/sh -c', args: 'cat')
  ]
) {
    def project = 'pdf-storage'

    node(pod.label) {
      stage('Checkout') {
         checkout scm
      }
      stage('SettingUpDepencies')
      {
        container('dotnet') {
          sh """
            apt-get update

            apt-get install -y curl software-properties-common pdftk
            curl -sL https://deb.nodesource.com/setup_10.x -o nodesource_setup.sh

            apt-get -y install  nodejs
            npm install
          """
        }
      }
      stage('Build') {
        container('dotnet') {
          sh """
            dotnet restore
            dotnet publish -c Release -o out
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
          if(env.BRANCH_NAME == "master") {
              updateImageToK8sTestEnv(publishedImage)
          }
        }
      }
    }
  }
