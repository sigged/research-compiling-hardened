@ECHO OFF
REM Build and pushes to dockerhub (not for CI/CD usage)
ECHO Building Blazor Dockerimage
docker login
docker build -f Blazor.Dockerfile -t sigged/insecure-csc-blazor .
docker push sigged/insecure-csc-blazor
PAUSE