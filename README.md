# .NET Standard C# Compiler

### Introduction

>.NET Standard C# Compiler was developed to research how to compile and execute arbitrary .NET Standard code securely.

This version of the application is the insecure starting point of the research. Any C# code can be executed on the server, which poses a big security threat.

> **Do not run this application in production. Use at your own risk.**

### Project guide

In the /src/ folder you will find that this repository consists of several projects.


- ![Build Status][travis-unknown] **<span>Sigged.CsC.NetFx.Wpf</span>** -- 
  Windows application to compile and run arbitrary C# code. Uses .NET Framework, compiles code to .NET Standard.
 
- [![Build Status][travis-realtime]](https://travis-ci.com/sigged/research-compiling) **<span>Sigged.CsC.NetCore.Web</span>** -- 
  Web application to compile and run arbitrary C# code. Uses .NET Core, compiles code to .NET Standard.

- [![Build Status][travis-realtime]](https://travis-ci.com/sigged/research-compiling) **<span>Sigged.Compiling.Core</span>** -- 
  .NET Standard library which exposes a Roslyn-based compiler API for the web and desktop apps.

- [![Build Status][travis-realtime]](https://travis-ci.com/sigged/research-compiling) **<span>Sigged.CodeHost.Worker</span>** -- 
  .NET Core Console application, which is run inside the web application to allow for compilation and execution in a multi-user environment.

- [![Build Status][travis-realtime]](https://travis-ci.com/sigged/research-compiling) **<span>Sigged.CodeHost.Core</span>** -- 
  .NET Standard library which exposes common features and protocols between the web application and he worker application


### Docker Container

Instead of building these projects yourself, a ready made Docker container is available on https://hub.docker.com/r/sigged/insecure-csc


[travis-unknown]: https://raw.githubusercontent.com/sigged/research-compiling/master/Compile.and.Execute/assets/travis-build-unknown.png "Build Unknown"

[travis-realtime]: https://travis-ci.com/sigged/research-compiling.svg?branch=master "Build Status"