// Based from demo application:
// https://github.com/Azure-Samples/application-insights-react-demo/blob/master/src/TelemetryService.js
import { ApplicationInsights } from '@microsoft/applicationinsights-web';
import { ReactPlugin } from '@microsoft/applicationinsights-react-js';
import { createBrowserHistory } from "history";
import { GetConfiguration } from "../ConfigurationManager";

let configuration = GetConfiguration();

const browserHistory = createBrowserHistory({ basename: '' });
let reactPlugin: ReactPlugin;
let appInsights: ApplicationInsights;

const createTelemetryService = () => {
    const initialize = (): void => {
        reactPlugin = new ReactPlugin();
        appInsights = new ApplicationInsights({
            config: {
                instrumentationKey: configuration.instrumentationKey,
                maxBatchInterval: 0,
                disableFetchTracking: false,
                enableCorsCorrelation: true,
                enableAutoRouteTracking: true,
                autoTrackPageVisitTime: true,
                extensions: [reactPlugin],
                extensionConfig: {
                    [reactPlugin.identifier]: {
                        history: browserHistory
                    }
                }
            }
        });

        appInsights.loadAppInsights();
    };

    return { reactPlugin, appInsights, initialize };
};

export const ai = createTelemetryService();
export const getAppInsights = () => appInsights;
