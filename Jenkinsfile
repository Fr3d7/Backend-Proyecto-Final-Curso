pipeline {
  agent any

  options {
    timestamps()
    ansiColor('xterm')
    skipDefaultCheckout(true)
  }

  environment {
    PROJECT_KEY   = 'backend-proyecto-final-DEV'
    PROJECT_NAME  = 'backend-proyecto-final-DEV'
    CONFIG        = 'Release'
  }

  stages {

    stage('Checkout') {
      steps {
        echo "📦 ${env.PROJECT_NAME} | 🧪 DEV"
        checkout([$class: 'GitSCM',
          branches: [[name: '*/DEV']],
          userRemoteConfigs: [[
            url: 'https://github.com/Fr3d7/Backend-Proyecto-Final-Curso.git',
            credentialsId: 'github-creds'
          ]]
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
        bat 'set CI= & dotnet build -c %CONFIG% --no-restore'
      }
    }

    stage('Test (coverage)') {
      when { expression { fileExists('ProyectoAPI/ProyectoAPI.csproj') } }
      steps {
        // Genera cobertura OpenCover para que Sonar la importe
        bat '''
          dotnet test --no-build -c %CONFIG% ^
            /p:CollectCoverage=true ^
            /p:CoverletOutputFormat=opencover ^
            /p:CoverletOutput=TestResults/coverage
        '''
        // Si quieres guardar reportes de test/cobertura en Jenkins, descomenta:
        // junit allowEmptyResults: true, testResults: '**/TestResults/*.trx'
        // publishCoverage adapters: [opencoverAdapter('**/TestResults/**/coverage.opencover.xml')], failNoReports: false
      }
    }

    stage('SonarQube Analysis (.NET)') {
      steps {
        withSonarQubeEnv('sonar-local') {
          // Instala (si no existe) y asegura que esté en el PATH ACTUAL
          bat '''
            dotnet tool install --global dotnet-sonarscanner || ver >NUL
            set PATH=%PATH%;%USERPROFILE%\\.dotnet\\tools
          '''
          // BEGIN
          bat '''
            dotnet-sonarscanner begin ^
              /k:"%PROJECT_KEY%" ^
              /n:"%PROJECT_NAME%" ^
              /v:"${BUILD_NUMBER}" ^
              /d:sonar.host.url="%SONAR_HOST_URL%" ^
              /d:sonar.login="%SONAR_AUTH_TOKEN%" ^
              /d:sonar.cs.opencover.reportsPaths="**/TestResults/**/coverage.opencover.xml" ^
              /d:sonar.exclusions="**/bin/**,**/obj/**,**/*.Tests/**"
          '''
          // Build (y opcionalmente test aquí si prefieres)
          bat 'set CI= & dotnet build -c %CONFIG% --no-restore'
          // END
          bat 'dotnet-sonarscanner end /d:sonar.login="%SONAR_AUTH_TOKEN%"'
        }
      }
    }

    stage('Quality Gate') {
      steps {
        timeout(time: 15, unit: 'MINUTES') {
          // Requiere que el webhook de Sonar apunte a http://TU_IP_LAN:8081/sonarqube-webhook/
          waitForQualityGate abortPipeline: true
        }
      }
    }

    stage('Package artifact') {
      steps {
        bat 'echo Empaquetando artefacto...'
        // TODO: agrega tu comando real de empaquetado (zip, dotnet publish, etc.)
        // ej: bat 'dotnet publish ProyectoAPI/ProyectoAPI.csproj -c %CONFIG% -o out'
        // archiveArtifacts artifacts: 'out/**/*', fingerprint: true
      }
    }

    stage('Deploy') {
      steps {
        bat 'echo Desplegando...'
        // TODO: agrega tu lógica real de despliegue
      }
    }
  }

  post {
    always {
      echo "🏁 Fin | Rama: DEV | Build #${env.BUILD_NUMBER}"
    }
    success {
      echo '✅ Pipeline OK'
    }
    failure {
      echo '❌ Pipeline con errores'
    }
  }
}
