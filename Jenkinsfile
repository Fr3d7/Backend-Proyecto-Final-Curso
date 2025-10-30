pipeline {
  agent any

  options {
    skipDefaultCheckout(true)
    ansiColor('xterm')
    timestamps()
  }

  parameters {
    choice(name: 'TARGET_BRANCH', choices: ['DEV', 'QA', 'PROD'], description: 'Solo para jobs NO multibranch.')
  }

  environment {
    // Detecta la rama: Multibranch > parámetro > nombre del job
    RAW_BRANCH  = "${env.BRANCH_NAME ?: (params.TARGET_BRANCH ?: (env.JOB_BASE_NAME?.toLowerCase()?.contains('prod') ? 'PROD' : (env.JOB_BASE_NAME?.toLowerCase()?.contains('qa') ? 'QA' : 'DEV')))}"
    PIPE_BRANCH = "${RAW_BRANCH.toUpperCase()}"
    PROJECT_KEY = "frontend-proyecto-final-${PIPE_BRANCH}"

    // Usa el tool configurado en "Global Tool Configuration"
    // (asegúrate de que exista un tool Sonar llamado exactamente 'sonar-scanner-win')
    SCANNER_HOME = tool 'sonar-scanner-win'

    REPO_URL    = "https://github.com/Fr3d7/Backend-Proyecto-Final-Curso.git"
  }

  stages {
    stage('Checkout') {
      steps {
        script {
          echo "📦 Proyecto: ${env.PROJECT_KEY} | 🧵 Rama destino: ${env.PIPE_BRANCH}"
        }
        checkout([$class: 'GitSCM',
          branches: [[name: "*/${PIPE_BRANCH}"]],
          userRemoteConfigs: [[url: "${env.REPO_URL}", credentialsId: 'github-creds']]
        ])
      }
    }

    stage('Install dependencies') {
      steps {
        bat 'npm ci || npm install'
      }
    }

    stage('Run tests (coverage)') {
      steps {
        // No falla si no hay tests; si existen, genera coverage
        bat 'npm test -- --coverage --watchAll=false --ci --passWithNoTests'
      }
    }

    stage('Build app') {
      steps {
        bat '''
          set CI=
          npm run build
        '''
      }
    }

    stage('SonarQube Analysis') {
      steps {
        withCredentials([string(credentialsId: 'sonarqube-token', variable: 'SONAR_TOKEN')]) {
          withSonarQubeEnv('sonar-local') {
            // Aviso si no existe coverage/lcov.info (no falla)
            bat 'if not exist coverage\\lcov.info (echo "WARN: no coverage\\lcov.info")'
            bat """
              "${SCANNER_HOME}\\bin\\sonar-scanner.bat" ^
                -Dsonar.projectKey=${PROJECT_KEY} ^
                -Dsonar.projectName=${PROJECT_KEY} ^
                -Dsonar.projectVersion=${BUILD_NUMBER} ^
                -Dsonar.projectBaseDir=%WORKSPACE% ^
                -Dsonar.sources=src ^
                -Dsonar.tests=src ^
                -Dsonar.test.inclusions=**/*.test.js,**/*.spec.js ^
                -Dsonar.sourceEncoding=UTF-8 ^
                -Dsonar.javascript.lcov.reportPaths=coverage/lcov.info ^
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
        archiveArtifacts artifacts: 'build/**', fingerprint: true
      }
    }

    stage('Deploy') {
      when {
        expression { return env.PIPE_BRANCH == 'QA' || env.PIPE_BRANCH == 'PROD' }
      }
      steps {
        script {
          def deployPath = (env.PIPE_BRANCH == 'PROD') ? 'C:\\deploy\\frontend' : 'C:\\deploy\\frontend-qa'
          bat """
            if not exist ${deployPath} mkdir ${deployPath}
            xcopy /E /I /Y build ${deployPath}
          """
          echo "🚀 Desplegado a ${deployPath} para rama ${env.PIPE_BRANCH}"
        }
      }
    }
  }

  post {
    always {
      echo "🏁 Fin | Rama: ${env.PIPE_BRANCH} | Build: #${env.BUILD_NUMBER}"
    }
  }
}
