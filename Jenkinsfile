@Library("PTCSLibrary@github-support") _

podTemplate(label: 'dotnet.2.0-with-node',
  containers: [
    containerTemplate(name: 'dotnet-with-node', image: 'ptcos/docker-dotnet-node-sdk:2.0.1', ttyEnabled: true, command: '/bin/sh -c', args: 'cat'),
    containerTemplate(name: 'docker', image: 'ptcos/docker-client:1.1.32', ttyEnabled: true, command: '/bin/sh -c', args: 'cat'),
  ]
) {
    def project = 'pdf-storage'
    def branch = (env.BRANCH_NAME)
    def deploymentYaml = "./k8s/${branch}.yaml"

    node('dotnet.2.0-with-node') {
      stage('Checkout') {
        checkout_with_tags()
      }
      stage('Build') {
        container('dotnet-with-node') {
          sh """
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
          def published = publishContainerToGcr(project, branch);

          if(branch == "master") {
            applyToK8sTestEnv(published);
          }
        }
      }
    }
  }