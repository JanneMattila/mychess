# My Chess

[![Azure Static Web Apps CI/CD](https://github.com/JanneMattila/mychess/actions/workflows/azure-static-web-apps-lemon-sea-03362af03.yml/badge.svg)](https://github.com/JanneMattila/mychess/actions/workflows/azure-static-web-apps-lemon-sea-03362af03.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Introduction

My Chess is social (and not that serious) chess game where you can play chess online with your friends. You can comment your moves and put some pressure to your friends (in fun way of course!).

![My Chess board](https://user-images.githubusercontent.com/2357647/88582302-2afa4b80-d057-11ea-88d9-55f9ed02f5e2.png)

## Try it yourself

You can try My Chess at the [mychess.jannemattila.com](https://mychess.jannemattila.com).

## Static web app development

To test Blazor client using [Azure Static Web Apps CLI](https://github.com/Azure/static-web-apps-cli):

```powershell
swa start https://localhost:5000 --run "cd ./src/MyChess.Client/ && dotnet watch run" --api-location ./src/MyChess.Functions --api-port=3000
```

```powershell
swa start https://localhost:5000 --run "cd ./src/MyChess.Client/ && dotnet watch run" --api-port=7071
```
