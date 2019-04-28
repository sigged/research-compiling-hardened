@ECHO OFF
REM Build and runs docker image (not for CI/CD usage)
ECHO Building Dockerimage
docker build -f Dockerfile -t sigged/insecure-csc-dev .

REM this app MUST be run on the same forward-facing port, or SignalRClientService won't work
docker run -it --rm -p 80:80 --name insecure-csc-dev sigged/insecure-csc-dev
PAUSE