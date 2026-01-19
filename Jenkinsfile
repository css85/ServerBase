pipeline {
    agent any

    options {
        skipDefaultCheckout true
    }

    stages {
         stage('checkout') {
            steps {
                checkout scm
            }
        }

        stage('build-1') {
            steps {
                sh "dotnet build ./AuditionM_Server/src/BattleServer"
            }
        }

		stage('build-2') {
            steps {
                sh "dotnet build ./AuditionM_Server/src/FrontEndWeb"
            }
        }

		stage('test-1') {
            steps {
                sh "dotnet test ./AuditionM_Server/test/BattleServer.Tests -v n --blame"
            }
        }

		stage('test-2') {
            steps {
                sh "dotnet test ./AuditionM_Server/test/FrontEndWeb.Tests  -v n --blame"
            }
        }
	}
}