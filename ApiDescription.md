# PDF-STORAGE API

Generates PDF files from given html template and stores them in constant URI.

## Templating

Generated pdf:s supported limited mustache templating.

See [https://mustache.github.io/](https://mustache.github.io/)

Pdf request has two concepts for data: base and row. With rowData you can
generate multiple pdf:s with single request.

- baseData: Data that is applied for every document, for example
company name, company logo.
- rowData: Data that generates new document for each row.

For example:

```json
{
  "html": "{{ header }} {{ row }}",
  "baseData": {
      "header": "my_company"
  },
  "rowData": [
      {
         "row": "a"
      },
      {
         "row": "b"
      },
  ],
  "options": {}
}
```

Generates two PDF:s with data "a" and "b" per document.

## Translators

Storage templating accepts 'translators' which converts string to another asset. For example: string as base64 image which is easy to embed template.

```json
{
  "html": "string",
  "baseData": {},
  "rowData": [
    { barcode: "[translate:barcode]AE3011A"}
  ]
}
```

With template

```html
<img src="{{ barcode }}"/>
```

Generates

```html
<img src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAASIAAAB4CAYAAABW..."/>
```

### Format

In general form translator is either

```text
[translate:type]data
```

Or with options. Options are documented per translator and they are
different for each of them.

```text
[translate:type:{ optionValue: "value" }]data
```

### List of translators and options

| Type  | Options  | Description |
|---|---|---|
| barcode  | ` {type:'code128|ean13|ean8|upca|upce|itf14|code39', width: 290, height: 120, includeText: false, foregroundColor: "#ffffff" , backgroundColor: "#000000" } `|  Generates barcode image of type `code128|...` |