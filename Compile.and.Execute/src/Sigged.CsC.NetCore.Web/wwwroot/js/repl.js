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
    
    let codeSamples = {};

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

    async function pullCodeSamples(){
        $.ajax({
            url: "/Home/GetCodeSamples",
            success: results => {
                replApp.codeSampleGroups = results
            }
        });
    }

    async function getCodeSample(){
        if(replApp.selectedCodeSampleId)
        {
            $.ajax({
                url: "/Home/GetCodeSample/" + replApp.selectedCodeSampleId,
                success: result => {
                    cEditor.getDoc().setValue(result.contents);
                }
            });
        }else{
            replApp.selectedCodeSample = null;
        }
    }

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

    hubconnection.on('ApplicationStateChanged', function(appStatus){
        //console.log("App State Changed", appStatus);
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

                await pullCodeSamples();

                var console = document.getElementById("console");
                consoleDomMutationObserver.observe(
                    console, 
                    { attributes: true, childList: true, characterData: true });
            });
        },
        data: {
            isBuilding: false,
            isRunning: false,
            codeSampleGroups: [],
            selectedCodeSampleId: null,
            consoleText: 
'============================================\n'+
'        .NET Standard C# Compiler           \n'+
'                        by Sigged           \n'+
'============================================\n'+
'1. Enter C# code in the editor on the right \n'+
'2. Press Build or Run                       \n'+
'3. Watch Roslyn in action!                  \n'+
'============================================\n'+
'                                            \n'+
'Ready.                                      \n'+
'                                            \n',
            statusCode: STATUSCODE.DEFAULT,
            statusText: "Ready for action...",
            builderrors: [],
            textMarkers: [],
        },
        // define methods under the `methods` object
        methods: {
            buildSource: async function () {
                this.$clearTextMarkers();
                this.isBuilding = true;
                this.statusText = "Building...";
                this.statusCode = STATUSCODE.BUSY;
                await this.$requestBuild(cEditor.getTextArea().value);
            },
            buildAndRunSource: async function () {
                this.$clearTextMarkers();
                this.isBuilding = true;
                this.statusText = "Building...";
                this.statusCode = STATUSCODE.BUSY;
                await this.$requestRun(cEditor.getTextArea().value);
            },
            stopAll: function () {
                this.$requestStop();
            },
            codeSampleSelected: function(event)
            {
                getCodeSample(this.selectedCodeSampleId)
            },
            $buildSuccess: function () {
                this.isBuilding = false;
                this.isRunning = false;
                this.builderrors = null;
                this.statusCode = STATUSCODE.SUCCESS;
                this.statusText = "Build succeeded";
                replApp.$resetConsole();
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
                        if (this.builderrors[i].severity === "error" || this.builderrors[i].severity === "warning"){
                            var marker = cEditor.markText(
                                { line: this.builderrors[i].startPosition.line, ch: this.builderrors[i].startPosition.character },
                                { line: this.builderrors[i].endPosition.line, ch: this.builderrors[i].endPosition.character },
                                {
                                    className: "CodeMirror-lint-mark-" + this.builderrors[i].severity,
                                    title: this.builderrors[i].description
                                });
                            this.textMarkers.push(marker);
                        }
                    }
                }
                replApp.$resetConsole();
            },
            $appRunning: function () {
                this.$nextTick(function () {
                    replApp.consoleText = '';
                    replApp.isBuilding = false;
                    replApp.isRunning = true;
                    replApp.statusCode = STATUSCODE.BUSY;
                    replApp.statusText = "Application is running...";
                });
            },
            $appWritesOutput: function (appState) {
                setTimeout((appState) => {
                    replApp.isBuilding = false;
                    replApp.isRunning = true;
                    replApp.statusCode = STATUSCODE.BUSY;
                    replApp.statusText = "Application is running...";
                    replApp.consoleText += appState.output;
                    replApp.$scrollDownConsole();
                }, 0, appState);
            },
            $appRequestsInput: function (appState, requestLine) {
                this.isRunning = true;
                this.statusCode = STATUSCODE.BUSY;
                this.statusText = "App is waiting for input...";
                this.consoleText += '<span class="consoleInputBox"></span>'
                this.$handleConsoleInput(requestLine);
                this.$scrollDownConsole();
            },
            $appStopped: function () {
                setTimeout(() => {
                    this.isBuilding = false;
                    this.isRunning = false;
                    this.statusCode = STATUSCODE.DEFAULT;
                    this.statusText = "Application ended";
                    replApp.$resetConsole();
                }, 0);
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
                replApp.$resetConsole();
            },
            $requestBuild: function (code) {
                hubconnection.invoke("Build", {
                    codingSessionId: '',
                    sourceCode: code,
                    runOnSuccess: false
                }).catch(err => console.error(err.toString()));
            },
            $requestRun: function (code) {
                hubconnection.invoke("Build", {
                    codingSessionId: '',
                    sourceCode: code,
                    runOnSuccess: true //instructs to run after good build
                }).catch(err => console.error(err.toString()));
            },
            $requestStop: function(){
                hubconnection.invoke("StopAll")
                .then(function() {
                    replApp.isBuilding = false;
                    replApp.isRunning = false;
                    replApp.statusText = "User cancelled";
                    replApp.statusCode = STATUSCODE.DEFAULT;
                })
                .catch(err => console.error(err.toString()))
                .finally(() => replApp.$resetConsole());
                
            },
            consoleFocus: function(){
                var cons = document.getElementById('console');
                var inputDiv = cons.querySelector('.consoleInputBox');
                if(inputDiv)
                    inputDiv.focus();
            },
            $clearTextMarkers: function(){
                this.$nextTick(function () {
                    replApp.textMarkers.forEach((m) => m.clear());
                    replApp.textMarkers = [];
                });
            },
            $scrollDownConsole: function(){
                this.$nextTick(function () {
                    var cons = document.getElementById('console');
                    cons.scrollTop = cons.scrollHeight;
                });
            },
            $handleConsoleInput: function(requestLine){
                this.$nextTick(function () {
                    var cons = document.getElementById('console');
                    var inputDiv = cons.querySelector('.consoleInputBox');
                    inputDiv.setAttribute('contenteditable', 'true');
                    var input = null;

                    inputDiv.addEventListener('keypress', function(kbdEvent){
                        if(requestLine){
                            if(kbdEvent.key == "Enter"){
                                kbdEvent.preventDefault();
                                input = this.innerText;
                                this.innerHTML = input ;
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
                            var inputToSend = input;
                            input = null;
                            inputDiv.remove();
                            replApp.consoleText = cons.innerHTML + inputToSend;
                            if(requestLine){
                                replApp.consoleText += "\n";
                            }
                            replApp.$scrollDownConsole();
                            hubconnection.invoke("ClientInput", inputToSend)
                                .then(function(){
                                    console.log("Sent Input: " + inputToSend);
                                })
                                .catch(err => console.error(err.toString()));
                        }
                    },100);
                });
            },
            $resetConsole: function(){
                this.$nextTick(function () {
                    //remove any input boxes
                    var cons = document.getElementById('console');
                    cons.querySelectorAll('.consoleInputBox').forEach(function(el){
                        el.remove();
                        cons.innerText += '\n'; //todo: no new line if inputbox was not a ReadLine operation
                    });
                    replApp.consoleText = cons.innerText;
                    replApp.$scrollDownConsole();
                });
            }
        }
    });

}());