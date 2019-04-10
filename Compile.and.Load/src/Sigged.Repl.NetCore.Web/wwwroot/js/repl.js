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
            buildSource: async function (event) {
                this.isBuilding = true;
                this.statusText = "Building...";
                let result = await this.$submitCodeForBuilding(cEditor.getTextArea().value);
                if (result.isSuccess) {
                    this.$buildSuccess();
                } else {
                    this.$buildFailed(result.buildErrors);
                }
            },
            buildAndRunSource: async function (event) {
                this.isBuilding = true;
                this.statusText = "Building...";
                let result = await this.$submitCodeForBuilding(cEditor.getTextArea().value);
                if (result.isSuccess) {
                    this.$buildSuccess();



                } else {
                    this.$buildFailed(result.buildErrors);
                }
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
            $submitCodeForBuilding: function (code) {
                let app = this;

                return new Promise(function (resolve, reject) {
                    let xhr = new XMLHttpRequest();
                    xhr.onload = function () {
                        if (xhr.status !== 200) { // HTTP error?
                            // handle error
                            console.err('HTTP error while submitting code', xhr.status);
                            reject();
                            return;
                        }

                        let response = xhr.response;
                        let result = JSON.parse(response);
                        resolve(result);
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
                });
            },
            $submitRunRequest: function () {
                console.log("Running..");
                this.statusText = "Running...";
                this.isBuilding = false;
                this.isRunning = true;
            },
        }
    });

}());