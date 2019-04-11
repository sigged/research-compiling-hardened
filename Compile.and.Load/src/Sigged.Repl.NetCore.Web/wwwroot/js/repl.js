/**
 * REPL Service
 * @copyright SpruceBit 2019
 * @license MIT
 */

let replService = (function () {

    const APPSTATE = {
        NOTRUNNING: 0,
        RUNNING: 1,
        WAITFORINPUT: 2,
        CRASHED: 3,
        ENDED: 4
    }

    const hubconnection = new signalR.HubConnectionBuilder()
        .withUrl("/codeHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();

    async function connectToHub() {
        try {
            await hubconnection.start();
            console.log("connected to codehub");
        } catch (err) {
            console.log(err);
            setTimeout(() => connectToHub(), 5000);
        }
    };

    hubconnection.onclose(async () => {
        await connectToHub();
    });

    hubconnection.on('BuildComplete', function(result){
        console.log("build complete", result);
        if(result.isSuccess){
            replApp.$buildSuccess();
        }else{
            replApp.$buildFailed(result.buildErrors);
        }
    });

    hubconnection.on('ApplicationStateChanged', function(sessionid, appStatus){
        console.log("App State Changed", appStatus);
        switch(appStatus.state){
            case APPSTATE.RUNNING:
                replApp.$appRunning();
                break;
            default:
                break;
        }
    });


    const replApp = new Vue({
        el: "#repl-main-ide",
        mounted:  function () {
            this.$nextTick(async function () {
                await connectToHub();
            });
        },
        data: {
            isBuilding: false,
            isRunning: false,
            statusText: "Ready for action...",
            builderrors: [
                {
                    severity: 'error',
                    id: 'CS 00000',
                    location: 'Line 15, Col 4',
                    description: 'Totally bogus error for testing purproses'
                },
            ]
        },
        // define methods under the `methods` object
        methods: {
            buildSource: async function (event) {
                this.isBuilding = true;
                this.statusText = "Building...";
                await this.$requestBuild(cEditor.getTextArea().value);
            },
            buildAndRunSource: async function (event) {
                this.isBuilding = true;
                this.statusText = "Running...";
                await this.$requestRun(cEditor.getTextArea().value);
            },
            stopAll: function (event) {
                this.isBuilding = false;
                this.isRunning = false;
                this.statusText = "User cancelled";
            },
            $appStopped: function (event) {
                this.isBuilding = false;
                this.isRunning = false;
                this.statusText = "Application ended";
            },
            $buildSuccess: function () {
                this.isBuilding = false;
                this.isRunning = false;
                this.builderrors = null;
                this.statusText = "Build succeeded";
            },
            $buildFailed: function (errors) {
                this.isBuilding = false;
                this.isRunning = false;
                this.builderrors = errors;
                this.statusText = "Build failed";
                //mark errors
                if (this.builderrors.length > 0) {
                    for (let i = 0; i < this.builderrors.length; i++) {
                        if (this.builderrors[i].severity === "error" || this.builderrors[i].severity === "warning")
                            cEditor.markText(
                                { line: this.builderrors[i].startPosition.line, ch: this.builderrors[i].startPosition.character },
                                { line: this.builderrors[i].endPosition.line, ch: this.builderrors[i].endPosition.character },
                                {
                                    className: "CodeMirror-lint-mark-" + this.builderrors[i].severity,
                                    title: this.builderrors[i].description
                                });
                    }
                }
            },
            $appRunning: function () {
                this.isBuilding = false;
                this.isRunning = true;
                this.statusText = "Application is running...";
            },
            $requestBuild: function (code) {
                let app = this;

                return new Promise(function (resolve, reject) {

                    hubconnection.invoke("Build", {
                        codingSessionId: '',
                        sourceCode: code
                    }).catch(err => console.error(err.toString()));

                    //let xhr = new XMLHttpRequest();
                    //xhr.onload = function () {
                    //    if (xhr.status !== 200) { // HTTP error?
                    //        // handle error
                    //        console.error('HTTP error while submitting code', xhr.status);
                    //        console.error(xhr.response);
                    //        reject();
                    //        this.$buildFailed(errors);
                    //        return;
                    //    }

                    //    let response = xhr.response;
                    //    let result = JSON.parse(response);
                    //    resolve(result);
                    //};

                    //xhr.onerror = function (event) {
                    //    console.err('Generic error while submitting code', xhr.event);
                    //};

                    //let json = JSON.stringify({
                    //    sourceCode: code
                    //});

                    //xhr.open("POST", '/Home/Build');
                    //xhr.setRequestHeader('Content-type', 'application/json; charset=utf-8');

                    //xhr.send(json);
                });
            },
            $requestRun: function (code) {
                console.log("Running..");
                this.statusText = "Running...";
                this.isBuilding = false;
                this.isRunning = true;

                hubconnection.invoke("BuildAndRunCode", {
                    codingSessionId: '',
                    sourceCode: code
                }).catch(err => console.error(err.toString()));
            },
        }
    });

}());