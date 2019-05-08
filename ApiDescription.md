# PDF-STORAGE

Generates PDF files from given html template and stores them in constant URI.

## Features

- Generate PDF from html with templating data, this gives possibility to generate
  many PDFs without sending same template content multiple times -> just data.
- Merging pdfs
- Usage information: how many times pdf is opened and so on.
- Designed to survive

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
