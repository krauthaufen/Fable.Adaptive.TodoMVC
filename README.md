# Adaptive TodoMVC Example

This project uses [FSharp.Data.Adaptive](https://github.com/fsprojects/FSharp.Data.Adaptive) and [Fable.Elmish.Adaptive](https://github.com/krauthaufen/Fable.Elmish.Adaptive) for
implementing the famous TodoMVC example.

## Requirements

* dotnet >= 2.2
* npm 

## Scripts

`build -t Watch` starts a webpack-dev-server

`build -t RunDebug` packs the page and then runs a webserver

`build -t RunRelease` packs the page, minifies it and then runs a webserver
