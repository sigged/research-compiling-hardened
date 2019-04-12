/**
 * REPL Service
 * @copyright Siegfried Derdeyn 2019
 * @license MIT
 */

let replService = (function () {

    const consoleDomMutationObserver = new MutationObserver(function(mutations){
        mutations.forEach(function(mutation) {
            //console.log('Console Mutation!', mutation);
            replApp.consoleFocus();
          });
    });

    const APPSTATE = {
        NOTRUNNING: 0,
        RUNNING: 1,
        WRITEOUTPUT: 10,
        WAITFORINPUT: 11,
        WAITFORINPUTLINE: 12,
        CRASHED: 20,
        ENDED: 100
    }

    const STATUSCODE = {
        DEFAULT: '',
        BUSY: 'busy',
        ERROR: 'error',
        SUCCESS: 'success'
    }

    const hubconnection = new signalR.HubConnectionBuilder()
        .withUrl("/codeHub")
        .configureLogging(signalR.LogLevel.Information)
        .build();
    
    //for debugging
    hubconnection.serverTimeoutInMilliseconds = 1000 * 60 * 10; // 10 min timeout!!

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
            case APPSTATE.WRITEOUTPUT:
                replApp.$appWritesOutput(appStatus);
                break;
            case APPSTATE.WAITFORINPUT:
                replApp.$appRequestsInput(appStatus, false);
                break;
            case APPSTATE.WAITFORINPUTLINE:
                replApp.$appRequestsInput(appStatus, true);
                break;
            case APPSTATE.ENDED:
                replApp.$appStopped();
                break;
            case APPSTATE.CRASHED:
                replApp.$appCrashed(appStatus.exception);
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

                var console = document.getElementById("console");
                consoleDomMutationObserver.observe(
                    console, 
                    { attributes: true, childList: true, characterData: true });
            });
        },
        data: {
            isBuilding: false,
            isRunning: false,
            consoleText: 'Welcome!\nREPL\n<script></script>',
            statusCode: STATUSCODE.DEFAULT,
            statusText: "Ready for action...",
            builderrors: [
                // {
                //     severity: 'error',
                //     id: 'CS 00000',
                //     location: 'Line 15, Col 4',
                //     description: 'Totally bogus error for testing purproses'
                // },
            ]
        },
        // define methods under the `methods` object
        methods: {
            buildSource: async function () {
                this.isBuilding = true;
                this.statusText = "Building...";
                this.statusCode = STATUSCODE.BUSY;
                await this.$requestBuild(cEditor.getTextArea().value);
            },
            buildAndRunSource: async function () {
                this.isBuilding = true;
                this.statusText = "Running...";
                this.statusCode = STATUSCODE.BUSY;
                await this.$requestRun(cEditor.getTextArea().value);
            },
            stopAll: function () {
                this.isBuilding = false;
                this.isRunning = false;
                this.statusCode = STATUSCODE.DEFAULT;
                this.statusText = "User cancelled";
            },
            $buildSuccess: function () {
                this.isBuilding = false;
                this.isRunning = false;
                this.builderrors = null;
                this.statusCode = STATUSCODE.SUCCESS;
                this.statusText = "Build succeeded";
            },
            $buildFailed: function (errors) {
                this.isBuilding = false;
                this.isRunning = false;
                this.builderrors = errors;
                this.statusCode = STATUSCODE.ERROR;
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
                this.statusCode = STATUSCODE.BUSY;
                this.statusText = "Application is running...";
            },
            $appWritesOutput: function (appState) {
                this.isRunning = true;
                this.statusCode = STATUSCODE.BUSY;
                this.statusText = "Application is running...";
                this.consoleText += appState.output; //.replace("\n","<br />");
            },
            $appRequestsInput: function (appState, requestLine) {
                this.isRunning = true;
                this.statusCode = STATUSCODE.BUSY;
                this.statusText = "App is waiting for input...";

                this.consoleText += '<div class="consoleInputBox"></div>'
                this.$handleConsoleInput(requestLine);
            },
            $appStopped: function () {
                this.isBuilding = false;
                this.isRunning = false;
                this.statusCode = STATUSCODE.DEFAULT;
                this.statusText = "Application ended";
            },
            $appCrashed: function (exceptionInfo) {
                this.isBuilding = false;
                this.isRunning = false;
                this.statusCode = STATUSCODE.ERROR;
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
            consoleFocus: function(){
                var cons = document.getElementById('console');
                var inputDiv = cons.querySelector('.consoleInputBox');
                if(inputDiv)
                    inputDiv.focus();
            },
            $handleConsoleInput: function(requestLine){
                this.$nextTick(function () {
                    var cons = document.getElementById('console');
                    var inputDiv = cons.querySelector('.consoleInputBox');
                    inputDiv.setAttribute('contenteditable', 'true');
                    var input = null;

                    inputDiv.addEventListener('keyup', function(kbdEvent){
                        if(requestLine){
                            if(kbdEvent.key == "Enter"){
                                input = inputDiv.innerText;
                            }
                        }
                        else{
                            inputDiv.removeAttribute('contenteditable');
                            input = kbdEvent.key;
                        }
                        console.log("Keypress in input", kbdEvent);
                    });

                    //wait for enter before sumitting
                    setInterval(function(){
                        if(input !== null){
                            var inputToSend = input.replace('\n','');
                            input = null;
                            inputDiv.remove();
                            replApp.consoleText = cons.innerHTML + inputToSend;
                            hubconnection.invoke("ClientInput", inputToSend)
                                .then(function(){
                                    console.log("Sent Input: " + inputToSend);
                                })
                                .catch(err => console.error(err.toString()));
                            
                        }
                    },100);
                });
            }
        }
    });

}());
