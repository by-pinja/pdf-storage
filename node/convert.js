var app = require('@protacon/html-to-pdf');

module.exports = function (callback, html, data) {
    app
        .createBuffer(html, data, {})
        .then(resultBuffer => {
            callback(/* error */ null, resultBuffer);
        }, error => callback(/* error */ null, error))
        .catch((error) => {
            callback(/* error */ null, error);
        });
};