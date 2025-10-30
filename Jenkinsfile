pipeline {
  agent any

  options { skipDefaultCheckout(true); ansiColor('xterm'); timestamps() }

  parameters {
    choice(name: 'TARGET_BRANCH', choices: ['DEV','QA','PROD'], description: 'Solo para jobs NO multibranch.')
  }

  environment {
    RAW_BRANCH   = "${env.BRANCH_NAME ?: (params.TARGET_BRANCH ?: (env.JOB_BASE_NAME?.toLowerCase()?.contains('prod') ? 'PROD' : (env.JOB_BASE_NAME?.toLowerCase()?.contains('qa') ? 'QA' : 'DEV')))}"
    PIPE_BRANCH  = "${RAW_BRANCH.toUpperCase()}"
    PROJECT_KEY  = "backend-proyecto-final-${PIPE_BRANCH}"
    REPO_URL     = "https://github.com/Fr3d7/Backend-Proyecto-Final-Curso.git"
    SCANNER_HOME = tool 'sonar-scanner-win'          // Configurado en Global Tool Configuration
    COVERAGE_ARG = ""                                 // Se llenará si hay tests
  }

  stages {
    stage('Checkout'){ steps {
      echo "📦 ${env.PROJECT_KEY} | 🧵 ${env.PIPE_BRANCH}"
      checkout([$class:'GitSCM',
        branches:[[name:"*/${PIPE_BRANCH}"]],
        userRemoteConfigs:[[url:"${env.REPO_URL}", credentialsId:'github-creds']]
      ])
    }}

    stage('Restore'){ steps { bat 'dotnet restore' } }

    stage('Build'){ steps { bat 'set CI=& dotnet build -c Release --no-restore' } }

    // ---- Tests condicionales (solo si existe el proyecto de pruebas) ----
    stage('Test (coverage)'){
      when { expression { fileExists('ProyectoAPI.Tests/ProyectoAPI.Tests.csproj') } }
      steps {
        bat '''
          dotnet test ProyectoAPI.Tests\\ProyectoAPI.Tests.csproj ^
            -c Release --no-build ^
            /p:CollectCoverage=true ^
            /p:CoverletOutputFormat=opencover ^
            /p:CoverletOutput=TestResults\\coverage\\
        '''
        script {
          env.COVERAGE_ARG = "-Dsonar.cs.opencover.reportsPaths=**/coverage.opencover.xml"
        }
      }
    }

    stage('SonarQube Analysis'){
      steps {
        withCredentials([string(credentialsId:'sonarqube-token', variable:'SONAR_TOKEN')]) {
          withSonarQubeEnv('sonar-local') {
            bat """
              "${SCANNER_HOME}\\bin\\sonar-scanner.bat" ^
                -Dsonar.projectKey=${PROJECT_KEY} ^
                -Dsonar.projectName=${PROJECT_KEY} ^
                -Dsonar.projectVersion=${BUILD_NUMBER} ^
                -Dsonar.sources=. ^
                -Dsonar.exclusions=**/bin/**,**/obj/**,**/*.Tests/** ^
                -Dsonar.sourceEncoding=UTF-8 ^
                ${COVERAGE_ARG} ^
                -Dsonar.token=%SONAR_TOKEN%
            """
          }
        }
      }
    }

    stage('Quality Gate'){ steps {
      timeout(time: 10, unit: 'MINUTES') { waitForQualityGate abortPipeline: true }
    }}

    stage('Package artifact'){ steps {
      bat 'dotnet publish ProyectoAPI\\ProyectoAPI.csproj -c Release -o publish'
      archiveArtifacts artifacts: 'publish/**', fingerprint: true
    }}

    stage('Deploy'){
      when { expression { env.PIPE_BRANCH=='QA' || env.PIPE_BRANCH=='PROD' } }
      steps {
        script {
          def target = (env.PIPE_BRANCH=='PROD') ? 'C:\\deploy\\api' : 'C:\\deploy\\api-qa'
          bat """
            if not exist ${target} mkdir ${target}
            xcopy /E /I /Y publish ${target}
          """
          echo "🚀 Desplegado a ${target}"
        }
      }
    }
  }

  post {
    always {
      echo "🏁 Fin | Rama: ${env.PIPE_BRANCH} | Build #${env.BUILD_NUMBER}"
    }
  }
}
