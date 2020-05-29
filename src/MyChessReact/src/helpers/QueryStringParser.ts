export class QueryStringParser {
    public static parse(qs: string): Map<string, string> {
        let parsedResult = new Map<string, string>();
        qs = qs.substring(1); // Ignore starting '?'
        const items = qs.split('&');
        items.forEach(item => {
            const params = item.split('=');
            parsedResult.set(params[0], params[1]);
        });
        return parsedResult;
    }
}
