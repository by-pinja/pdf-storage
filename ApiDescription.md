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

#### pdf options

Pdf can contain following options

```json
{
  "format": "A4",
  "footerTemplate": "<div style=\"color: black; font-size: 12px; width: 100%; margin-left: 28px;\"><span class=\"pageNumber\"></span>/<span class=\"totalPages\"></span></div>",
  "headerTemplate": "<div style=\"color: black; font-size: 12px; width: 100%; margin-left: 28px;\">Some header</div>",
  "printBackground": true,
  "landscape": false,
  "preferCSSPageSize": false,
  "pageRanges": null,
  "marginTop": "120px",
  "marginBottom": "120px",
  "marginLeft": "20px",
  "marginRight": "20px",
  "width": null,
  "height": null,
  "scale": null
}
```
Format takes priority over width and height values. Values in width AND height (in inches) creates a custom sized paper. If format and size params are omitted the default A4 paper size will be used.

For further information, see [https://www.puppeteersharp.com/api/PuppeteerSharp.PdfOptions.html](https://www.puppeteersharp.com/api/PuppeteerSharp.PdfOptions.html).
