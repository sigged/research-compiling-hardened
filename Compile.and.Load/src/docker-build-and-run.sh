echo Building Dockerimage 

docker build -f Dockerfile -t sigged/insecure-repl .

#this app MUST be run on the same forward-facing port, or SignalRClientService won't work
docker run -it --rm -p 80:80 --name insecure-repl sigged/insecure-repl
