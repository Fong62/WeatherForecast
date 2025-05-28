pipeline {
    agent any
    environment {
	PATH = "/var/lib/jenkins/sonar-scanner/bin:/usr/local/bin:$PATH"
        //DOCKERHUB_CREDENTIALS = credentials('dockerhub-creds')  // Sử dụng Docker Hub credentials
        SONAR_TOKEN = credentials('sonar-token')                // SonarQube token
        //KUBECONFIG = credentials('kubeconfig')                  // Kubernetes config
        DOCKER_IMAGE = "fong62/weatherforecast"                       // Tên image Docker
    }
    stages {
        stage('Checkout Code') {
            steps {
                git url: 'https://github.com/Fong62/WeatherForecast.git', 
                     branch: 'main',
                     credentialsId: 'github-credentials'
            }
        }

        stage('Restore') {
            steps {
                script {
                    def cachePath = "${env.WORKSPACE}/.nuget_packages_cache"

                    if (fileExists(cachePath)) {
                        echo "Restore cache to .nuget/packages"
                        sh "rm -rf ~/.nuget/packages"
                        sh "mkdir -p ~/.nuget"
                        sh "cp -r ${cachePath} ~/.nuget/packages"
                    }

                    // Thực hiện restore nuget packages
                    sh 'dotnet restore'
                    
                    echo "Save .nuget/packages to cache"
                    sh "rm -rf ${cachePath}"
                    sh "mkdir -p ${cachePath}"
                    sh "cp -r ~/.nuget/packages/* ${cachePath}/"
                }
            }
        }
        
        stage('Build & Test') {
            steps {
                sh '''
                dotnet build WeatherForecast.sln --configuration Release --no-restore
                dotnet test WeatherForecast.Tests/WeatherForecast.Tests.csproj --no-build --verbosity normal
                '''
            }
        }
        
        stage('SonarQube Analysis') {
            steps {
                withSonarQubeEnv('sonarqube') {
                    sh """
		    mkdir -p "${env.WORKSPACE}/WeatherForecast.Tests/TestResults/${env.BUILD_ID}"	
	
                    dotnet sonarscanner begin \
                        /k:"WeatherForecast" \
                        /d:sonar.host.url="http://192.168.1.21:9000" \
                        /d:sonar.login="$SONAR_TOKEN" \
			/d:sonar.scanner.scanAll=false \
			/d:sonar.plugins.downloadOnlyRequired=true \
			/d:sonar.language="cs" \
			/d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml" \
  			/d:sonar.exclusions="**/*.js,**/*.ts,**/bin/**,**/obj/**,**/wwwroot/**,**/Migrations/**,**/*.cshtml.css,**/Migrations/**/*.cs" \
			/d:sonar.css.file.suffixes=".css,.less,.scss" \
                        /n:"WeatherForecast" \
  			/v:"${BUILD_NUMBER}"
                    
                    dotnet build WeatherForecast.sln --configuration Release --no-restore
		    
		    dotnet test WeatherForecast.Tests/WeatherForecast.Tests.csproj \
                   	--no-build \
                    	--logger trx \
                    	/p:CollectCoverage=true \
                        /p:CoverletOutputFormat=opencover \
                        /p:CoverletOutput="WeatherForecast.Tests/TestResults/${env.BUILD_ID}/coverage.opencover.xml"

                    dotnet sonarscanner end /d:sonar.login="$SONAR_TOKEN"
                    """
                }
            }
        }
        
        stage('Docker Build & Push') {
            steps {
                script {
                    // Đăng nhập Docker Hub
		    def gitCommit = sh(script: 'git rev-parse --short HEAD', returnStdout: true).trim()
            	    def customTag = "${env.BUILD_ID}-${gitCommit}"
                    docker.withRegistry('https://index.docker.io/v1/', 'dockerhub-creds') {
                        def image = docker.build("${DOCKER_IMAGE}:${customTag}")
                        retry(3) {
        		   image.push()
    			}

    			retry(3) {
        		   image.push('latest')
    			}
                    }
                }
            }
        }
	
	stage('Publish Artifact to Nexus') {
    	    steps {
        	withCredentials([usernamePassword(credentialsId: 'nexus-creds', usernameVariable: 'NEXUS_USER', passwordVariable: 'NEXUS_PASS')]) {
            	    script {
                	def artifactName = "weatherforecast-${BUILD_NUMBER}.zip"
                	sh """
                	dotnet publish ./WeatherForecast/WeatherForecast.csproj -c Release -o ./publish
                	cd publish
                	zip -r ../../../${artifactName} .
                	curl -v -u $NEXUS_USER:$NEXUS_PASS --upload-file ../../../${artifactName} \
                    	http://192.168.1.22:8081/repository/weatherforecast-artifacts/${artifactName}
                	"""
            	     }
                }
            }
        }
        
        stage('Deploy to Kubernetes') {
            steps {
                withCredentials([file(credentialsId: 'kubeconfig', variable: 'KUBECONFIG_PATH')]) {
                    script {
                	def gitCommit = sh(script: 'git rev-parse --short HEAD', returnStdout: true).trim()
                	def customTag = "${env.BUILD_ID}-${gitCommit}"
                	sh """
                	export KUBECONFIG=${KUBECONFIG_PATH}
                	export BUILD_ID=${customTag}	
                	envsubst < k8s/deployment.yaml | kubectl apply -f -
                	kubectl apply -f k8s/service.yaml
                	kubectl rollout status deployment/weatherforecast-deployment --timeout=10m
                	"""
            	     }
		}
            }
        }
    }
    post {
        always {
            deleteDir()
        }
    }
}