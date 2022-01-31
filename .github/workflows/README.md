# Customization

You need to add following piece to your workflow file:

```yaml
      - uses: actions/checkout@v2
      - shell: pwsh
        run: |
          .\deploy\deploy-configuration.ps1 `
            -ClientID ${{ secrets.CLIENT_ID }} `
            -ApplicationIdURI ${{ secrets.APPLICATION_ID_URI }} `
            -IntrumentationKey ${{ secrets.INSTRUMENTATION_KEY }} `
            -WebPushPublicKey ${{ secrets.WEB_PUSH_PUBLIC_KEY }} `
            -AppRootFolder "src/MyChess.Client/wwwroot"
          
      - name: Build And Deploy
        id: builddeploy
        uses: Azure/static-web-apps-deploy@v1
        with:
          azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN_HAPPY_BUSH_057760703 }}
          repo_token: ${{ secrets.GITHUB_TOKEN }} # Used for Github integrations (i.e. PR comments)
          action: "upload"
          ###### Repository/Build Configurations - These values can be configured to match your app requirements. ######
          # For more information regarding Static Web App workflow configurations, please visit: https://aka.ms/swaworkflowconfig
          app_location: "src/MyChess.Client" # App source code path
          api_location: "src/MyChess.Functions" # Api source code path - optional
          output_location: "wwwroot" # Built app content directory - optional
          ###### End of Repository/Build Configurations ######
```