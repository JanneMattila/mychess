# My Chess

[![Azure Static Web Apps CI/CD](https://github.com/JanneMattila/mychess/actions/workflows/azure-static-web-apps-lemon-sea-03362af03.yml/badge.svg)](https://github.com/JanneMattila/mychess/actions/workflows/azure-static-web-apps-lemon-sea-03362af03.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Introduction

My Chess is social (and not that serious) chess game where you can play chess online with your friends. You can comment your moves and put some pressure to your friends (in fun way of course!).

![My Chess](https://user-images.githubusercontent.com/2357647/159117778-8a70fdb7-2341-465d-b376-8291328ee56a.gif)

## Try it yourself

You can try My Chess at the [mychess.jannemattila.com](https://mychess.jannemattila.com).

## History

Interested in the history of My Chess? Checkout [history](./doc/history.md)!

## Local development

My Chess is built for [Azure Static Web Apps](https://docs.microsoft.com/en-us/azure/static-web-apps/overview).
Therefore, if you want to run application locally, you'll need [Azure Static Web Apps CLI](https://github.com/Azure/static-web-apps-cli).

Here are some example commands to run My Chess using SWA CLI:

```powershell
swa start https://localhost:5000 --run "cd ./src/MyChess.Client/ && dotnet watch run" --api-location ./src/MyChess.Functions --api-port=3000
```

```powershell
swa start https://localhost:5000 --run "cd ./src/MyChess.Client/ && dotnet watch run" --api-port=7071
```
