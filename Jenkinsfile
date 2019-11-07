library 'jenkins-ptcs-library@2.1.0'

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
      stage('Build') {
        container('dotnet') {
          sh """
            dotnet build
          """
        }
      }
      stage('Package & Test') {
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
