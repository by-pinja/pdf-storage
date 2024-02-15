library 'jenkins-ptcs-library@6.0.1'

podTemplate(label: pod.label,
  containers: pod.templates + [
    containerTemplate(name: 'dotnet', image: 'mcr.microsoft.com/dotnet/sdk:8.0', ttyEnabled: true, command: '/bin/sh -c', args: 'cat'),
  ]
) {
    def project = 'pdf-storage'

    node(pod.label) {
      stage('Checkout') {
          def branchHead = 'refs/heads/'+env.BRANCH_NAME

          if(utils.isTagBuild())
          {
            branchHead = 'refs/tags/'+env.TAG_NAME
          }

          // LFS pull requires manually written scm step.
          checkout([  $class: 'GitSCM',
              branches: [[name: branchHead]],
                  doGenerateSubmoduleConfigurations: false,
                  extensions: [
                      [$class: 'GitLFSPull'],
                      [$class: 'CheckoutOption', timeout: 20],
                      [$class: 'CloneOption',
                              depth: 0,
                              noTags: false,
                              reference: '/other/optional/local/reference/clone',
                              shallow: false,
                              timeout: 120]
                  ],
                  submoduleCfg: [],
                  userRemoteConfigs: [
                      // Yeah hardcoding repository url here sucks balls big time...
                      // But it seems it isn't available at pod.
                      [credentialsId: 'jenkins_ci_account_for_github', url: 'https://github.com/by-pinja/pdf-storage.git']
                  ]
              ])
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
