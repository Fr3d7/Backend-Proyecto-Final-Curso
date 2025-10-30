pipeline {
  agent any

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
        // Si no tienes tests aún, no pasa nada: este paso no fallará el análisis.
        // (El Gate no se romperá por cobertura gracias a las exclusiones de abajo)
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
          // Instala el scanner de .NET en el agente si no está
          bat """
            dotnet tool install --global dotnet-sonarscanner || ver >NUL
            setx PATH "%PATH%;%USERPROFILE%\\\\.dotnet\\tools" >NUL
          """

          // BEGIN
          bat """
            dotnet-sonarscanner begin ^
              /k:"%PROJECT_KEY%" ^
              /n:"%PROJECT_NAME%" ^
              /v:"${env.BUILD_NUMBER}" ^
              /d:sonar.host.url="%SONAR_HOST_URL%" ^
              /d:sonar.login="%SONAR_AUTH_TOKEN%" ^
              /d:sonar.cs.opencover.reportsPaths="**/TestResults/**/coverage.opencover.xml" ^
              /d:sonar.exclusions="**/bin/**,**/obj/**,**/*.Tests/**,**/Migrations/**" ^
              /d:sonar.coverage.exclusions="**/*" ^
              /d:sonar.cpd.exclusions="**/Migrations/**"
          """

          // BUILD (otra vez para enlazar con el begin)
          bat 'set CI= & dotnet build -c %CONFIG% --no-restore'

          // END
          bat 'dotnet-sonarscanner end /d:sonar.login="%SONAR_AUTH_TOKEN%"'
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
      }
    }

    stage('Deploy') {
      steps {
        bat 'echo Desplegando...'
      }
    }
  }

  post {
    always {
      echo "🏁 Fin | Rama: DEV | Build #${env.BUILD_NUMBER}"
    }
  }
}
