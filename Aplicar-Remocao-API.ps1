$ErrorActionPreference = 'Stop'
# Remove somente os arquivos da antiga recuperacao de senha.
$marker = Join-Path $PSScriptRoot 'BairroEmDia.Api/Program.cs'
if (-not (Test-Path -LiteralPath $marker -PathType Leaf)) {
    throw 'Extraia este pacote na raiz do projeto antes de executar o script.'
}
$files = @(
    'BairroEmDia.Api/Controllers/RecuperacaoSenhaController.cs',
    'BairroEmDia.Api/DTOs/RecuperacaoSenhaDtos.cs',
    'BairroEmDia.Api/Email/IRecuperacaoEmail.cs',
    'BairroEmDia.Api/Email/RecuperacaoEmail.cs',
    'BairroEmDia.Api/Services/RecuperacaoSenhaService.cs',
    'BairroEmDia.Domain/Interfaces/IRecuperacaoSenhaRepository.cs',
    'BairroEmDia.Infrastructure/Repositories/RecuperacaoSenhaRepository.cs',
    'BairroEmDia.Tests/RecuperacaoSenhaApiTests.cs'
)
foreach ($relative in $files) {
    $path = Join-Path $PSScriptRoot $relative
    if (Test-Path -LiteralPath $path -PathType Leaf) {
        Remove-Item -LiteralPath $path -Force
        Write-Host "Removido: $relative"
    }
}
Write-Host 'Remocao concluida. Os dados e os demais arquivos foram preservados.'
