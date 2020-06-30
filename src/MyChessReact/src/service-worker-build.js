const workboxBuild = require('workbox-build');

const buildCustomServiceWorker = () => {
    return workboxBuild.injectManifest({
        swSrc: "src/service-worker-custom.js",
        swDest: "build/service-worker-custom.js",
        globDirectory: "build",
        globPatterns: ["**/*.{js,css,html,png,svg,json,ico}"]
    })
        .then(({ count, size, warnings }) => {
            warnings.forEach(console.warn);
            console.info(`${count} files will be precached with total size of ${Math.floor(size / 1024)} kBs.`);
        });
};

buildCustomServiceWorker();
