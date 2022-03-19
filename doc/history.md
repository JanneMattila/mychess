# History

My Chess dates back to 2009 when cloud and
serverless was not a thing yet. Read the
first blog post about it [here](https://docs.microsoft.com/en-us/archive/blogs/jannemattila/my-chess-another-chess-application). (**Note**: Board was incorrectly implemented in the first screenshots 🤣):

![My Chess running as Windows App](https://user-images.githubusercontent.com/2357647/159117983-6c4c7869-0939-49f3-84d5-7d6ec1834a3c.png)

It was written in C++ and it ran on Windows and [Windows Mobile 5.0 for Pocket PC Phone Edition](https://en.wikipedia.org/wiki/Windows_Mobile#Smartphones):

![My Chess running in Windows Mobile 5.0 Pocket PC Phone Edition](https://user-images.githubusercontent.com/2357647/159118007-dd1f08bf-8383-4650-8d81-fced64999786.png)

After that My Chess has been re-written multiple times!

- [Windows Phone 7.5](https://en.wikipedia.org/wiki/Windows_Phone_7) ([Nokia Lumia 800](https://en.wikipedia.org/wiki/Nokia_Lumia_800) era)

![Windows Phone showing move notification toast](https://user-images.githubusercontent.com/2357647/159118085-de8eb513-5236-4a73-8bcd-a46f6569715f.png)

![Windows Phone MainPage showing Waiting For You](https://user-images.githubusercontent.com/2357647/159118129-8b816051-4431-4ed9-a149-6f9c448d36ee.png)

![Game page in the middle of the game](https://user-images.githubusercontent.com/2357647/159118142-3f1edc3f-7070-46e1-8b28-475f53f8db42.png)

Originally backend was using [Azure Cloud Services](https://docs.microsoft.com/en-us/azure/cloud-services/cloud-services-choose-me)
and then it was migrated to Azure Websites which was later branded as [Azure App Servce](https://docs.microsoft.com/en-us/azure/app-service/overview).

Fun fact: Apparently those times bundling external DLLs was thing:

![External DLLS in TFVC](https://user-images.githubusercontent.com/2357647/159118564-649b9e1e-435c-4afe-8d92-f25b0c931bbf.png)

Notice couple of special things from above screenshot:

- It's using [TFVC - Team Foundation Version Control](https://docs.microsoft.com/en-us/azure/devops/repos/tfvc/what-is-tfvc?view=azure-devops#team-foundation-version-control)
- It mentions move of source code to _TFS Preview_ which is now called [Azure DevOps](https://docs.microsoft.com/en-us/azure/devops/user-guide/what-is-azure-devops?view=azure-devops)

My Chess Azure DevOps project created:

![My Chess Azure DevOps project created](https://user-images.githubusercontent.com/2357647/159118741-77ab862a-5d31-4621-9933-856044ac739f.png)

- [Windows Phone 8](https://en.wikipedia.org/wiki/Windows_Phone_8)
- [Windows Phone 8.1](https://en.wikipedia.org/wiki/Windows_Phone_8.1)

![Windows 8 main page](https://user-images.githubusercontent.com/2357647/159118269-704208b1-e350-4033-8559-066f64481524.png)

![Windows 8 game page](https://user-images.githubusercontent.com/2357647/159118286-1767ac92-e423-4eb4-9d57-f505455fc7c4.png)

- [Windows 10](https://en.wikipedia.org/wiki/Universal_Windows_Platform_apps) Universal Windows Platform app

[My Chess in Windows Store](https://www.microsoft.com/fi-fi/p/my-chess/9wzdncrdcc5k)

- ASP.NET web app with jQuery and simple javascript
- React (hosted in Azure Storage account + Azure CDN certificate) with Azure Functions backend
  - This was using exactly same look & feel as the Blazor and can be still found [mychess / react](https://github.com/JanneMattila/mychess/tree/react)
- Blazor SPA running in [Static web apps](https://docs.microsoft.com/en-us/azure/static-web-apps/overview) with Azure Functions backend
