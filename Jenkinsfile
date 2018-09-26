library 'jenkins-ptcs-library@0.2.5'

podTemplate(label: pod.label,
  containers: pod.templates + [
    containerTemplate(name: 'dotnet-with-node', image: 'ptcos/docker-dotnet-node-sdk:2.0.1', ttyEnabled: true, command: '/bin/sh -c', args: 'cat')
  ]
) {
    def project = 'pdf-storage'

    node(pod.label) {
      stage('Checkout') {
         checkout scm
      }
      stage('Build') {
        container('dotnet-with-node') {
          sh """
            apt-get update
            apt-get -y install pdftk
            npm install
            dotnet restore
            dotnet publish -c Release -o out
          """
        }
      }
      stage('Test') {
        container('dotnet-with-node') {
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
