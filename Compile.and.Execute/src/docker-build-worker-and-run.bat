@ECHO OFF
REM Build and runs docker image (not for CI/CD usage)
ECHO Building Worker Dockerimage
docker build -f Worker.Dockerfile -t sigged/insecure-csc-worker .

docker run -it --rm --name insecure-csc-worker sigged/insecure-csc-worker
PAUSE