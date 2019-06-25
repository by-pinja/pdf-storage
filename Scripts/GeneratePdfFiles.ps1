[CmdletBinding()]
Param(
    [string] $HostUri = "http://localhost:5000",
    [string] $ApiKey = "apikeyfortesting",
    [string] $HtmlToRender = "<html><body>Page content here.<body></html>",
    [int][Parameter(Mandatory)] $Count
)

1..$count | foreach {
    $body = @{
        html =  $HtmlToRender
        baseData = @{}
        rowData = @(@{})
        options = @{}
    } | ConvertTo-Json -Depth 30
    Invoke-Restmethod $HostUri/v1/pdf/loadtester -Method POST -Headers @{ "Authorization" = "ApiKey $ApiKey"} -ContentType "application/json" -Body $body
}
