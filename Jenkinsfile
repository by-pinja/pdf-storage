podTemplate(label: 'dotnet', idleMinutes:30,
  containers: [
    containerTemplate(name: 'node', image: 'node:boron', ttyEnabled: true, command: '/bin/sh -c', args: 'cat'),
    containerTemplate(name: 'dotnetWithNode', image: 'ptcos/docker-dotnet-node-sdk:latest', ttyEnabled: true, command: '/bin/sh -c', args: 'cat'),
    containerTemplate(name: 'docker', image: 'ptcos/docker-client:latest', alwaysPullImage: true, ttyEnabled: true, command: '/bin/sh -c', args: 'cat'),
  ]
) {
    def version = '1.0.0'
    def project = 'pdf-storage'
    def branch = (env.BRANCH_NAME)
    def deploymentYaml = "./k8s/${branch}.yaml"

    node('dotnet') {
      stage('Checkout') {
	        checkout scm
      }
      stage('Build') {
        container('node') {
          sh """
            npm install
          """
        }
        container('dotnetWithNode') {
          sh """
            dotnet restore
            dotnet publish -c Release -o out
          """
        }
      }
      stage('Test') {
        container('dotnetWithNode') {
          sh """
          """
        }
      }
      stage('Package') {
          container('docker') {
            docker.withRegistry("https://eu.gcr.io", "gcr:${env.PTCS_DOCKER_REGISTRY_KEY}") {
              //Workaround Jenkins bug https://issues.jenkins-ci.org/browse/JENKINS-31507
              sh """
                echo ${env.PTCS_DOCKER_REGISTRY}
                docker build -t ${env.PTCS_DOCKER_REGISTRY}/$project .
              """

              def image = docker.image("${env.PTCS_DOCKER_REGISTRY}/$project")

              def tag = "$version-$branch-${env.BUILD_NUMBER}"

              image.push("latest-$branch")
              image.push(tag)

              configFileProvider([configFile(fileId: "11564861-9c61-4af0-aadd-aea8c6990876", targetLocation: "/home/jenkins/.kube/config")]) {
                  sh """
                      kubectl apply -f $deploymentYaml --namespace=pdf-storage
                      kubectl set image deployment/pdf-storage-$branch pdf-storage-$branch=eu.gcr.io/ptcs-docker-registry/$project:$tag --namespace=pdf-storage
                  """
              }
          }
        }
      }
    }
  }