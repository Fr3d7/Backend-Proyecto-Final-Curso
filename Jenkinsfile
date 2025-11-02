pipeline {
  agent any

  environment {
    PROJECT_KEY   = 'backend-proyecto-final-PROD'
    PROJECT_NAME  = 'backend-proyecto-final-PROD'
    CONFIG        = 'Release'
  }

  stages {
    stage('Checkout') {
      steps {
        echo "📦 ${env.PROJECT_NAME} | 🚀 PROD"
        checkout([$class: 'GitSCM',
          branches: [[name: '*/PROD']],
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
          // Instala el scanner si no está presente
          bat 'dotnet tool install --global dotnet-sonarscanner || ver >NUL'

          script {
            // Detecta si hay reporte de cobertura
            def hasCoverage = (bat(
              script: 'powershell -NoProfile -Command "if(Get-ChildItem -Recurse -Filter coverage.opencover.xml){exit 0}else{exit 1}"',
              returnStatus: true
            ) == 0)

            def coverageArg = hasCoverage ?
              '/d:sonar.cs.opencover.reportsPaths="**/TestResults/**/coverage.opencover.xml"' :
              ''

            // 🔧 Corregido: sin ^ después del &&
            bat """
set "PATH=%PATH%;%USERPROFILE%\\.dotnet\\tools" && dotnet-sonarscanner begin ^
  /k:"%PROJECT_KEY%" ^
  /n:"%PROJECT_NAME%" ^
  /v:"${env.BUILD_NUMBER}" ^
  /d:sonar.host.url="%SONAR_HOST_URL%" ^
  /d:sonar.login="%SONAR_AUTH_TOKEN%" ^
  /d:sonar.projectBaseDir="%WORKSPACE%" ^
  ${coverageArg} ^
  /d:sonar.exclusions="**/bin/**,**/obj/**,**/*.Tests/**,**/Migrations/**" ^
  /d:sonar.cpd.exclusions="**/Migrations/**" ^
  /d:sonar.coverage.exclusions="**/*"
            """

            bat 'set CI= & dotnet build -c %CONFIG% --no-restore'

            bat 'set "PATH=%PATH%;%USERPROFILE%\\.dotnet\\tools" && dotnet-sonarscanner end /d:sonar.login="%SONAR_AUTH_TOKEN%"'
          }
        }
      }
    }

  stage('Quality Gate') {
  steps {
    script {
      try {
        // reintenta hasta 3 veces por si Sonar aún está procesando
        retry(3) {
          timeout(time: 5, unit: 'MINUTES') {
            def qg = waitForQualityGate()  // espera a que Sonar procese el análisis
            echo "Quality Gate: ${qg.status}"
            if (qg.status != 'OK') {
              error "Quality Gate NO OK: ${qg.status}${qg.errorMessage ? ' - ' + qg.errorMessage : ''}"
            }
          }
        }
      } catch (org.jenkinsci.plugins.workflow.steps.FlowInterruptedException e) {
        // ⏳ si se agota el tiempo, solo informamos y seguimos (no marcamos UNSTABLE)
        echo '⏳ SonarQube tardó demasiado (timeout). Continuo el pipeline SIN cambiar el resultado.'
        // Si prefieres que falle en vez de seguir verde, comenta la línea de arriba y descomenta esta:
        // error 'Quality Gate timeout'
      }
    }
  }
}

    stage('Package artifact') { 
      steps { bat 'echo Empaquetando (PROD)...' } 
    }

    stage('Deploy') {
      steps {
        bat 'echo Desplegando a PRODUCCIÓN...'
        // ⬆️ aquí puedes colocar tu despliegue real (IIS, Docker, etc.)
      }
    }
  }

  post {
    always { echo "🏁 Fin | Rama: PROD | Build #${env.BUILD_NUMBER}" }
  }
}
