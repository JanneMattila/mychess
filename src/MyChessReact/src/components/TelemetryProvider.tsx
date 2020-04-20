// Based from demo application:
// https://github.com/Azure-Samples/application-insights-react-demo/blob/master/src/telemetry-provider.jsx
import React, { Component, Fragment } from "react";
import { withAITracking } from "@microsoft/applicationinsights-react-js";
import { ai } from "./TelemetryService";
import { withRouter } from "react-router-dom";

/**
 * This Component provides telemetry with Azure App Insights
 *
 * NOTE: the package '@microsoft/applicationinsights-react-js' has a HOC withAITracking that requires this to be a Class Component rather than a Functional Component
 */
class TelemetryProvider extends Component<any> {
    state = {
        initialized: false
    };

    componentDidMount() {
        const { initialized } = this.state;
        if (!initialized) {
            ai.initialize();
            this.setState({ initialized: true });
        }
    }

    render() {
        const { children } = this.props;
        return (
            <Fragment>
                {children}
            </Fragment>
        );
    }
}

export default withRouter(withAITracking(ai.reactPlugin, TelemetryProvider));
