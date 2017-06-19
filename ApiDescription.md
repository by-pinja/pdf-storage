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

# Usage calculations
Storage tracks openings of pdf files, see `v1/usage/` api for further examples.