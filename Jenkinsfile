library 'jenkins-ptcs-library@6.1'

podTemplate(label: pod.label,
  containers: pod.templates + [
    containerTemplate(name: 'dotnet', image: 'mcr.microsoft.com/dotnet/sdk:8.0', ttyEnabled: true, command: '/bin/sh -c', args: 'cat'),
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
              updateImageToTestEnvs(publishedImage)
          }
        }
      }
    }
  }
