podTemplate(label: 'dotnet', idleMinutes:30,
  containers: [
    containerTemplate(name: 'dotnet', image: 'microsoft/dotnet:1.1.1-sdk', ttyEnabled: true, command: '/bin/sh -c', args: 'cat'),
    containerTemplate(name: 'docker', image: 'ptcos/docker-client:latest', alwaysPullImage: true, ttyEnabled: true, command: '/bin/sh -c', args: 'cat')
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

              configFileProvider([configFile(fileId: "888b53ec-9509-46e1-8cb5-953f4de43f58", targetLocation: "/home/jenkins/.kube/config")]) {
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