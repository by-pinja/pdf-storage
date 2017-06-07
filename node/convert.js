var app = require('html-to-pdf');

module.exports = function (callback, html, data) {
    app
        .createBuffer(html, data, {})
        .then(resultBuffer => {
            callback(/* error */ null, resultBuffer);
        })
        .catch((error) => {
            callback(error, null);
        });
};