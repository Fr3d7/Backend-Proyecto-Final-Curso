pipeline {
  agent any

  options {
    skipDefaultCheckout(true)
    ansiColor('xterm')
    timestamps()
  }

  parameters {
    // Útil si NO usas multibranch. En multibranch se ignora.
    choice(name: 'TARGET_BRANCH', choices: ['DEV','QA','PROD'], description: 'Rama a construir (solo jobs no multibranch).')
  }

  environment {
    // Detecta rama: BRANCH_NAME (multibranch) -> param -> nombre del job
    RAW_BRANCH   = "${env.BRANCH_NAME ?: (params.TARGET_BRANCH ?: (env.JOB_BASE_NAME?.toLowerCase()?.contains('prod') ? 'PROD' : (env.JOB_BASE_NAME?.toLowerCase()?.contains('qa') ? 'QA' : 'DEV')))}"
    PIPE_BRANCH  = "${RAW_BRANCH.toUpperCase()}"                     // DEV | QA | PROD
    PROJECT_KEY  = "backend-proyecto-final-${PIPE_BRANCH}"           // Clave Sonar por rama
    REPO_URL     = "https://github.com/Fr3d7/Backend-Proyecto-Final-Curso.git"
    SCANNER_HOME = tool 'sonar-scanner-win'                          // Nombre del Global Tool en Jenkins
    COVERAGE_ARG = ""                                                // Se llena sólo si hay tests
  }

  stages {

    stage('Checkout') {
      steps {
        echo "📦 ${env.PROJECT_KEY} | 🧵 ${env.PIPE_BRANCH}"
        checkout([$class: 'GitSCM',
          branches: [[name: "*/${env.PIPE_BRANCH}"]],
          userRemoteConfigs: [[url: "${env.REPO_URL}", credentialsId: 'github-creds']]
        ])
      }
    }

    stage('Restore') {
      steps {
        bat 'dotnet restore'
      }
    }

    stage('Build') {
      steps {
        // Limpia variable CI para evitar warnings de .NET en pipelines
        bat 'set CI=& dotnet build -c Release --no-restore'
      }
    }

    stage('Test (coverage)') {
      when {
        expression { fileExists('ProyectoAPI.Tests/ProyectoAPI.Tests.csproj') }
      }
      steps {
        bat '''
          dotnet test ProyectoAPI.Tests\\ProyectoAPI.Tests.csproj ^
            -c Release --no-build ^
            /p:CollectCoverage=true ^
            /p:CoverletOutputFormat=opencover ^
            /p:CoverletOutput=TestResults\\coverage\\
        '''
        script {
          // Sólo añadimos este flag si hubo tests
          env.COVERAGE_ARG = "-Dsonar.cs.opencover.reportsPaths=**/coverage.opencover.xml"
        }
      }
    }

    stage('SonarQube Analysis') {
      steps {
        withCredentials([string(credentialsId: 'sonarqube-token', variable: 'SONAR_TOKEN')]) {
          withSonarQubeEnv('sonar-local') { // Nombre de tu Server en Jenkins > Configure System
            bat """
              "${SCANNER_HOME}\\bin\\sonar-scanner.bat" ^
                -Dsonar.projectKey=${PROJECT_KEY} ^
                -Dsonar.projectName=${PROJECT_KEY} ^
                -Dsonar.projectVersion=${BUILD_NUMBER} ^
                -Dsonar.sources=. ^
                -Dsonar.exclusions=**/bin/**,**/obj/**,**/*.Tests/** ^
                -Dsonar.sourceEncoding=UTF-8 ^
                ${env.COVERAGE_ARG} ^
                -Dsonar.token=%SONAR_TOKEN%
            """
          }
        }
      }
    }

    stage('Quality Gate') {
      steps {
        timeout(time: 10, unit: 'MINUTES') {
          waitForQualityGate abortPipeline: true
        }
      }
    }

    stage('Package artifact') {
      steps {
        bat 'dotnet publish ProyectoAPI\\ProyectoAPI.csproj -c Release -o publish'
        archiveArtifacts artifacts: 'publish/**', fingerprint: true
      }
    }

    stage('Deploy') {
      when {
        expression { env.PIPE_BRANCH == 'QA' || env.PIPE_BRANCH == 'PROD' }
      }
      steps {
        script {
          def target = (env.PIPE_BRANCH == 'PROD') ? 'C:\\deploy\\api' : 'C:\\deploy\\api-qa'
          bat """
            if not exist "${target}" mkdir "${target}"
            xcopy /E /I /Y publish "${target}"
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
