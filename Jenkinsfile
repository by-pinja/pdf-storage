@Library("PTCSLibrary@savpek") _

podTemplate(label: 'dotnet.1.1.2-with-node', idleMinutes:30,
  containers: [
    containerTemplate(name: 'dotnet-with-node', image: 'ptcos/docker-dotnet-node-sdk:1.1.2', ttyEnabled: true, command: '/bin/sh -c', args: 'cat'),
    containerTemplate(name: 'docker', image: 'ptcos/docker-client:1.1.18', ttyEnabled: true, command: '/bin/sh -c', args: 'cat'),
  ]
) {
    def project = 'pdf-storage'
    def branch = (env.BRANCH_NAME)
    def deploymentYaml = "./k8s/${branch}.yaml"

    node('dotnet.1.1.2-with-node') {
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

          //   docker.withRegistry("https://eu.gcr.io", "gcr:${env.PTCS_DOCKER_REGISTRY_KEY}") {
          //     //Workaround Jenkins bug https://issues.jenkins-ci.org/browse/JENKINS-31507
          //     sh """
          //       echo ${env.PTCS_DOCKER_REGISTRY}
          //       docker build -t ${env.PTCS_DOCKER_REGISTRY}/$project .
          //     """

          //     def image = docker.image("${env.PTCS_DOCKER_REGISTRY}/$project")

          //     def tag = "$branch-${env.BUILD_NUMBER}"

          //     image.push("latest-$branch")
          //     image.push(tag)

          //     if (env.GIT_TAG_NAME) {
          //       image.push(env.GIT_TAG_NAME)
          //     }
          // }
          println published
          println published['image']
          println published['tag']

          configFileProvider([configFile(fileId: "cf3149e9-c9d2-486d-a965-61e64d458a4a", targetLocation: "/home/jenkins/.kube/config")]) {
            sh """
                kubectl set image deployment/pdf-storage-$branch pdf-storage-$branch=eu.gcr.io/ptcs-docker-registry/$project:$tag --namespace=eventale
            """
          }
        }
      }
    }
  }