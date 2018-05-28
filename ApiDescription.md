# PDF Storage
Generates PDF files from given html template and stores them in constant URI.

## Templating
See [https://mustache.github.io/](https://mustache.github.io/)

## Translators
Storage templating accepts 'translators' which converts string to another asset. For example: string as base64 image which is easy to embed template.

```json
{
  "html": "string",
  "baseData": {},
  "rowData": [
    { barcode: '[translate:type]value'}
  ]
}
```

```html
<img src="{{ barcode }}"/>
```

## Options
Rendered support certain options you can define:
```javascript
{
  // Papersize Options: http://phantomjs.org/api/webpage/property/paper-size.html
  "height": "10.5in",        // allowed units: mm, cm, in, px
  "width": "8in",            // allowed units: mm, cm, in, px
  // - or -
  "format": "Letter",        // allowed units: A3, A4, A5, Legal, Letter, Tabloid
  "orientation": "portrait", // portrait or landscape

  // Page options
  "border": "0",             // default is 0, units: mm, cm, in, px
  // - or -
  "border": {
    "top": "2in",            // default is 0, units: mm, cm, in, px
    "right": "1in",
    "bottom": "2in",
    "left": "1.5in"
  },

  paginationOffset: 1,       // Override the initial pagination number
  "header": {
    "height": "45mm",
    "contents": '<div style="text-align: center;">Author: Marc Bachmann</div>'
  },
  "footer": {
    "height": "28mm",
    "contents": {
      first: 'Cover page',
      2: 'Second page', // Any page number is working. 1-based index
      default: '<span style="color: #444;">{{page}}</span>/<span>{{pages}}</span>', // fallback value
      last: 'Last Page'
    }
  },

  // Zooming option, can be used to scale images if `options.type` is not pdf
  "zoomFactor": "1", // default is 1

  // File options
  "quality": "75",           // only used for types png & jpeg
}
```

# Usage calculations
Storage tracks openings of pdf files, see `v1/usage/` api for further examples.