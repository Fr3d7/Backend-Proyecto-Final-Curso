pipeline {
  agent any

  tools {
    // Asegúrate de tener .NET 8 instalado en el agente
    // y el SonarScanner for .NET como dotnet tool (ver notas abajo).
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
        bat "dotnet restore"
      }
    }

    stage('Build') {
      steps {
        bat "set CI= & dotnet build -c %CONFIG% --no-restore"
      }
    }

    stage('Test (coverage)') {
      when { expression { fileExists('ProyectoAPI/ProyectoAPI.csproj') } }
      steps {
        // Genera cobertura OpenCover para que Sonar la lea
        bat """
          dotnet test --no-build -c %CONFIG% ^
            /p:CollectCoverage=true ^
            /p:CoverletOutputFormat=opencover ^
            /p:CoverletOutput=TestResults/coverage
        """
      }
    }

    stage('SonarQube Analysis (.NET)') {
      steps {
        withSonarQubeEnv('sonar-local') {
          // SonarScanner for .NET usa begin/build/test/end
          bat """
            dotnet tool install --global dotnet-sonarscanner || ver >NUL
            setx PATH "%PATH%;%USERPROFILE%\\\\.dotnet\\tools" >NUL
          """
          bat """
            dotnet-sonarscanner begin ^
              /k:"%PROJECT_KEY%" ^
              /n:"%PROJECT_NAME%" ^
              /v:"${env.BUILD_NUMBER}" ^
              /d:sonar.host.url="%SONAR_HOST_URL%" ^
              /d:sonar.login="%SONAR_AUTH_TOKEN%" ^
              /d:sonar.cs.opencover.reportsPaths="**/TestResults/**/coverage.opencover.xml" ^
              /d:sonar.exclusions="**/bin/**,**/obj/**,**/*.Tests/**"
          """
          bat "set CI= & dotnet build -c %CONFIG% --no-restore"
          // (Si ya corriste tests arriba, puedes omitirlos aquí)
          bat """
            dotnet-sonarscanner end /d:sonar.login="%SONAR_AUTH_TOKEN%"
          """
        }
      }
    }

    stage('Quality Gate') {
      steps {
        timeout(time: 15, unit: 'MINUTES') {
          waitForQualityGate abortPipeline: true
        }
      }
    }

    stage('Package artifact') {
      steps {
        bat 'echo Empaquetando...'
        // agrega tu empaquetado real aquí
      }
    }

    stage('Deploy') {
      steps {
        bat 'echo Desplegando...'
        // agrega tu deploy real aquí
      }
    }
  }

  post {
    always {
      echo "🏁 Fin | Rama: DEV | Build #${env.BUILD_NUMBER}"
    }
  }
}
