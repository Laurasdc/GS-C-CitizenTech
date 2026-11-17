param(
    [string]$CSharp = "http://localhost:5239",
    [string]$Soa = "http://localhost:8080"
)
$ErrorActionPreference = "Stop"
$CSharp = $CSharp.TrimEnd('/')
$Soa = $Soa.TrimEnd('/')
Write-Host "Consultando a API C#..."
$resposta = Invoke-RestMethod -Uri "$CSharp/api/ocorrencias" -Method Get -TimeoutSec 15
$ocorrencias = @($resposta)
Write-Host "Gerando analise pelo SOA (HTTP -> C# -> PostgreSQL)..."
$analise = Invoke-RestMethod -Uri "$Soa/api/analises" -Method Post -ContentType "application/json" -Body '{}' -TimeoutSec 20
if ($analise.total -ne $ocorrencias.Count) {
    throw "Total divergente. Confira se CSHARP_API_URL aponta para esta mesma API C# e se os dados nao mudaram durante o teste."
}
$encerradas = @($ocorrencias | Where-Object { $_.status -eq 'Encerrada' }).Count
if ($analise.encerradas -ne $encerradas -or $analise.abertas -ne ($ocorrencias.Count - $encerradas)) {
    throw "Contagem de situacoes divergente."
}
foreach ($status in @('Registrada','EmAnalise','EmAcompanhamento','Encerrada')) {
    $esperado = @($ocorrencias | Where-Object { $_.status -eq $status }).Count
    if ($analise.porStatus.$status -ne $esperado) { throw "Status divergente: $status" }
}
foreach ($categoria in @('Iluminacao','LixoAcumulado','Buraco','CalcadaDanificada')) {
    $esperado = @($ocorrencias | Where-Object { $_.categoria -eq $categoria }).Count
    if ($analise.porCategoria.$categoria -ne $esperado) { throw "Categoria divergente: $categoria" }
}
$salva = Invoke-RestMethod -Uri "$Soa/api/analises/$($analise.id)" -Method Get -TimeoutSec 10
if ($salva.id -ne $analise.id -or $salva.total -ne $analise.total) { throw "Falha ao consultar a analise persistida." }
Write-Host "Integracao conferida: total, status, categorias e consulta da analise salva." -ForegroundColor Green
$salva | ConvertTo-Json -Depth 6
