var app = require('html-to-pdf');

module.exports = function (callback, html) {
    app
        .createBuffer("c:\\temp\\example.html", "c:\\temp\\foo.pdf", {})
        .then(resultBuffer => {
            callback(/* error */ null, resultBuffer);
        })
        .catch((error) => {
            callback(error, null);
        });
};