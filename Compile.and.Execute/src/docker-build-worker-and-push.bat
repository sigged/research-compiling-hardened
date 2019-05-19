@ECHO OFF
REM Build and pushes to dockerhub (not for CI/CD usage)
ECHO Building Dockerimage
docker login
docker build -f Worker.Dockerfile -t sigged/insecure-csc-worker .
docker push sigged/insecure-csc-worker
PAUSE