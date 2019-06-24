@ECHO OFF
REM Build and runs docker image (not for CI/CD usage)
ECHO Building Blazor Dockerimage
docker build -f Blazor.Dockerfile -t sigged/insecure-csc-blazor .

docker run -it --rm --name insecure-csc-blazor sigged/insecure-csc-blazor
PAUSE