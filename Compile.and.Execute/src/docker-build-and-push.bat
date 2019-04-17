@ECHO OFF
ECHO Building Dockerimage
docker login
docker build -f Dockerfile -t sigged/insecure-csc .
docker push sigged/insecure-csc
PAUSE