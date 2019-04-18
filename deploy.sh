#!/bin/sh
echo Deploying application
docker login --username=_ --password=${HEROKU_AUTHKEY} registry.heroku.com
docker build -t registry.heroku.com/${HEROKU_APPNAME}/web .
docker push registry.heroku.com/${HEROKU_APPNAME}/web