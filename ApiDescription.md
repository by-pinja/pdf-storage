# PDF-STORAGE API

Generates PDF files from HTML templates and stores them in constant URIs.

## Templating

Generated PDFs support limited mustache templating.
For further information, see [https://mustache.github.io/](https://mustache.github.io/).

A PDF request has two key concepts for data: baseData and rowData.

- baseData: data that is applied for every document. Examples:
company name, company logo.
- rowData: data that generates a new document for each row. Can be used to
generate multiple PDFs with a single request.

For example, the following input:

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

generates two PDFs with data "a" and "b" per document.

## Translators

The storage templating supports `translators`, which convert strings to other assets.
For example: a string as a base64 image, which is easy to embed into a template.

The following data

```json
{
  "html": "string",
  "baseData": {},
  "rowData": [
    { barcode: "[translate:barcode]AE3011A" }
  ]
}
```

used with the template

```html
<img src="{{ barcode }}"/>
```

generates the following output:

```html
<img src="data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAASIAAAB4CAYAAABW..."/>
```

### Syntax

```text
[translate:type]data
```

Translator specific options are supported and used as follows.

```text
[translate:type:{ optionValue: "value" }]data
```

Real example:

```text
[translate:barcode:{includeText: true, foregroundColor: '#4286f4'}]AE5C9B
```

### List of translators and options

#### pdf options

Pdf can contain following options

```json
{
  "footerTemplate": "<div style=\"color: black; font-size: 12px; width: 100%; margin-left: 28px;\"><span class=\"pageNumber\"></span>/<span class=\"totalPages\"></span></div>",
  "headerTemplate": "<div style=\"color: black; font-size: 12px; width: 100%; margin-left: 28px;\">Some header</div>",
  "printBackground": true,
  "preferCSSPageSize": false,
  "pageRanges": null,
  "marginTop": "120px",
  "marginBottom": "120px",
  "marginLeft": "20px",
  "marginRight": "20px"
}
```

For further information, see [https://www.puppeteersharp.com/api/PuppeteerSharp.PdfOptions.html](https://www.puppeteersharp.com/api/PuppeteerSharp.PdfOptions.html).

#### barcode

Generates a barcode image of type `code128|...`

```json
{
  "type":"code128|ean13|ean8|upca|upce|itf14|code39",
  "width": 290,
  "height": 120,
  "includeText": false,
  "foregroundColor": "#ffffff",
  "backgroundColor": "#000000"
}
```
