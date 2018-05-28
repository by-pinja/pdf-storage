var app = require('@protacon/html-to-pdf');

module.exports = function (callback, html, data, options) {
    app
        .createBuffer(html, data, options)
        .then(resultBuffer => {
            callback(/* error */ null, resultBuffer);
        }, error => callback(/* error */ null, error))
        .catch((error) => {
            callback(/* error */ null, error);
        });
};