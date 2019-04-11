/**
 * REPL Service
 * @copyright SpruceBit 2019
 * @license MIT
 */

let replService = (function () {

    const APPSTATE = {
        NOTRUNNING: 0,
        RUNNING: 1,
        WRITEOUTPUT: 10,
        WAITFORINPUT: 11,
        CRASHED: 20,
        ENDED: 100
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
            case APPSTATE.ENDED:
                replApp.$appStopped();
            case APPSTATE.CRASHED:
                replApp.$appCrashed(appStatus.exception);
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
            $appStopped: function () {
                this.isBuilding = false;
                this.isRunning = false;
                this.statusText = "Application ended";
            },
            $appCrashed: function (exceptionInfo) {
                this.isBuilding = false;
                this.isRunning = false;
                this.statusText = "Application crashed";
                this.builderrors = [
                    {
                        severity: 'error',
                        id: exceptionInfo.name,
                        location: null,
                        description: exceptionInfo.message
                    },
                ]
            },
            $requestBuild: function (code) {
                let app = this;

                return new Promise(function (resolve, reject) {

                    hubconnection.invoke("Build", {
                        codingSessionId: '',
                        sourceCode: code
                    }).catch(err => console.error(err.toString()));
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
