[CmdletBinding()]
Param(
    [string] $HostUri = "http://localhost",
    [string] $ApiKey = "apikeyfortesting",
    [int][Parameter(Mandatory)] $Count
)

1..$count | foreach {
    $body = "{ `"html`": `"LOAD TESTING PDF: $_ `", `"baseData`": {}, `"rowData`": [ {} ], `"options`": {}}"
    Invoke-Restmethod http://localhost:5000/v1/pdf/loadtester -Method POST -Headers @{ "Authorization" = "ApiKey $ApiKey"} -ContentType "application/json" -Body $body
}
