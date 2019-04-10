/**
 * REPL Service
 * @copyright SpruceBit 2019
 * @license MIT
 */



let replService = (function () {
    
    const replApp = new Vue({
        el: "#repl-main-ide",
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
            buildSource: function (event) {
                //this.isBuilding = true;
                //this.statusText = "Building " + sourceInput.value.length + " chars of madness";
                this.isBuilding = true;
                this.statusText = "Building...";
                this.$submitCodeForBuilding(cEditor.getTextArea().value);
                //setTimeout(this.buildFailed, 1000);
            },
            buildAndRunSource: function (event) {
                
                //console.log("Building..");
                //setTimeout(this.runSource, 1000);
                //setTimeout(this.appStopped, 3000);
            },
            runSource: function (event) {
                console.log("Running..");
                this.statusText = "Running...";
                this.isBuilding = false;
                this.isRunning = true;
            },
            stopAll: function (event) {
                this.isBuilding = false;
                this.isRunning = false;
                this.statusText = "User cancelled";
            },
            appStopped: function (event) {
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
                                { line: this.builderrors[i].endPosition.line, ch: ch: this.builderrors[i].endPosition.character },
                                {
                                    className: "CodeMirror-lint-mark-" + this.builderrors[i].severity,
                                    title: this.builderrors[i].description
                                });
                    }
                }
            },
            $submitCodeForBuilding: function(code) {
                let xhr = new XMLHttpRequest();
                let app = this;

                xhr.onload = function () {
                    if (xhr.status !== 200) { // HTTP error?
                        // handle error
                        console.err('HTTP error while submitting code', xhr.status);
                        return;
                    }

                    let response = xhr.response;
                    let result = JSON.parse(response);
                    if (result.isSuccess) {
                        app.$buildSuccess();
                    } else {
                        app.$buildFailed(result.buildErrors);
                        
                    }
                    // get the response from xhr.response
                };

                xhr.onerror = function (event) {
                    console.err('Generic error while submitting code', xhr.event);
                };

                xhr.onprogress = function (event) {
                    // report progress
                    
                };

                let json = JSON.stringify({
                    sourceCode: code
                });

                xhr.open("POST", '/Home/Build');
                xhr.setRequestHeader('Content-type', 'application/json; charset=utf-8');

                xhr.send(json);
            }
        }
    });

}());