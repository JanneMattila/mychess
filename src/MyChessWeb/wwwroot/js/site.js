let endpoint = "http://localhost:7071";
fetch("/configuration.json")
    .then(response => {
    return response.json();
})
    .then(data => {
    console.log(data);
})
    .catch(error => {
    console.log(error);
});
//# sourceMappingURL=site.js.map