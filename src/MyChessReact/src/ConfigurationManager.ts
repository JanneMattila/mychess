import { ConfigurationModel } from "./models/ConfigurationModel";

const configuration = (window as any).configuration as ConfigurationModel;

export function GetConfiguration(): ConfigurationModel {
    return configuration;
}