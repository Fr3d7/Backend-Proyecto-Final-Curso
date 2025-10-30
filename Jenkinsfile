pipeline {
  agent any

  environment {
    // 👇 Ajusta estos dos si usas otro nombre en SonarQube
    PROJECT_KEY   = 'backend-proyecto-final-PROD'
    PROJECT_NAME  = 'backend-proyecto-final-PROD'

    CONFIG        = 'Release'
  }

  stages {
    stage('Checkout') {
      steps {
        echo "📦 ${env.PROJECT_NAME} | 🚀 PROD"
        checkout([$class: 'GitSCM',
          branches: [[name: '*/PROD']],                 // ← rama PROD
          userRemoteConfigs: [[
            url: 'https://github.com/Fr3d7/Backend-Proyecto-Final-Curso.git',
            credentialsId: 'github-creds'
          ]]
        ])
      }
    }

    stage('Restore') {
      steps { bat 'dotnet restore' }
    }

    stage('Build') {
      steps { bat 'set CI= & dotnet build -c %CONFIG% --no-restore' }
    }

    stage('Test (coverage)') {
      when { expression { fileExists('ProyectoAPI/ProyectoAPI.csproj') } }
      steps {
        // Si no hay tests, no pasa nada; el siguiente stage ya maneja la ausencia del reporte.
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
          // Instala el scanner de .NET si hace falta
          bat """
            dotnet tool install --global dotnet-sonarscanner || ver >NUL
            setx PATH "%PATH%;%USERPROFILE%\\\\.dotnet\\tools" >NUL
          """

          script {
            // ¿Existe al menos un coverage.opencover.xml?
            def hasCoverage = (bat(
              script: 'powershell -NoProfile -Command "if(Get-ChildItem -Recurse -Filter coverage.opencover.xml){exit 0}else{exit 1}"',
              returnStatus: true
            ) == 0)

            // Construye los args dinámicos para cobertura (solo si hay reporte)
            def coverageArg = hasCoverage ?
              '/d:sonar.cs.opencover.reportsPaths="**/TestResults/**/coverage.opencover.xml"' :
              ''

            // BEGIN
            bat """
              dotnet-sonarscanner begin ^
                /k:"%PROJECT_KEY%" ^
                /n:"%PROJECT_NAME%" ^
                /v:"${env.BUILD_NUMBER}" ^
                /d:sonar.host.url="%SONAR_HOST_URL%" ^
                /d:sonar.login="%SONAR_AUTH_TOKEN%" ^
                ${coverageArg} ^
                /d:sonar.exclusions="**/bin/**,**/obj/**,**/*.Tests/**,**/Migrations/**" ^
                /d:sonar.cpd.exclusions="**/Migrations/**" ^
                /d:sonar.coverage.exclusions="**/*"
            """

            // BUILD (necesario entre begin/end)
            bat 'set CI= & dotnet build -c %CONFIG% --no-restore'

            // END
            bat 'dotnet-sonarscanner end /d:sonar.login="%SONAR_AUTH_TOKEN%"'
          }
        }
      }
    }

    stage('Quality Gate') {
      steps {
        timeout(time: 1, unit: 'MINUTES') {
          waitForQualityGate abortPipeline: true
        }
      }
    }

    stage('Package artifact') { steps { bat 'echo Empaquetando (PROD)...' } }

    stage('Deploy') {
      steps {
        bat 'echo Desplegando a PRODUCCIÓN...'
        // ⬆️ coloca aquí tu despliegue real (IIS, Docker, servicio Windows, etc.)
      }
    }
  }

  post {
    always { echo "🏁 Fin | Rama: PROD | Build #${env.BUILD_NUMBER}" }
  }
}
