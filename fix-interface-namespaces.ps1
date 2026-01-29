# PowerShell script to fix interface namespaces after reorganization

# Define the mapping of folders to their correct namespaces
$namespaceMappings = @{
    'OnlineBookManagementSystem\Core\Application\Interfaces\Analytics' = 'OnlineBookManagementSystem.Core.Application.Interfaces.Analytics'
    'OnlineBookManagementSystem\Core\Application\Interfaces\Domain\Books' = 'OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Books'
    'OnlineBookManagementSystem\Core\Application\Interfaces\Domain\Categories' = 'OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Categories'
    'OnlineBookManagementSystem\Core\Application\Interfaces\Domain\Orders' = 'OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Orders'
    'OnlineBookManagementSystem\Core\Application\Interfaces\Domain\Reviews' = 'OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Reviews'
    'OnlineBookManagementSystem\Core\Application\Interfaces\Domain\Users' = 'OnlineBookManagementSystem.Core.Application.Interfaces.Domain.Users'
    'OnlineBookManagementSystem\Core\Application\Interfaces\Helpers' = 'OnlineBookManagementSystem.Core.Application.Interfaces.Helpers'
    'OnlineBookManagementSystem\Core\Application\Interfaces\Infrastructure\Authentication' = 'OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Authentication'
    'OnlineBookManagementSystem\Core\Application\Interfaces\Infrastructure\Email' = 'OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Email'
    'OnlineBookManagementSystem\Core\Application\Interfaces\Infrastructure\Logging' = 'OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Logging'
    'OnlineBookManagementSystem\Core\Application\Interfaces\Infrastructure\Payment' = 'OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure.Payment'
    'OnlineBookManagementSystem\Core\Application\Interfaces\Repositories\Books' = 'OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Books'
    'OnlineBookManagementSystem\Core\Application\Interfaces\Repositories\Categories' = 'OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Categories'
    'OnlineBookManagementSystem\Core\Application\Interfaces\Repositories\Orders' = 'OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Orders'
    'OnlineBookManagementSystem\Core\Application\Interfaces\Repositories\Users' = 'OnlineBookManagementSystem.Core.Application.Interfaces.Repositories.Users'
}

# Get all interface files in the organized folders
foreach ($folderPath in $namespaceMappings.Keys) {
    $correctNamespace = $namespaceMappings[$folderPath]
    
    if (Test-Path $folderPath) {
        $files = Get-ChildItem -Path $folderPath -Filter "*.cs"
        
        foreach ($file in $files) {
            Write-Host "Processing: $($file.FullName)"
            
            $content = Get-Content $file.FullName -Raw
            
            # Replace the old generic namespace with the correct specific namespace
            $oldNamespace = "namespace OnlineBookManagementSystem.Core.Application.Interfaces"
            $newContent = $content -replace [regex]::Escape($oldNamespace), "namespace $correctNamespace"
            
            if ($content -ne $newContent) {
                Set-Content -Path $file.FullName -Value $newContent -NoNewline
                Write-Host "  Updated namespace to: $correctNamespace"
            }
        }
    }
}

# Also fix the root level interfaces
$rootInterfacePath = "OnlineBookManagementSystem\Core\Application\Interfaces"
if (Test-Path $rootInterfacePath) {
    $rootFiles = Get-ChildItem -Path $rootInterfacePath -Filter "*.cs" | Where-Object { $_.Directory.Name -eq "Interfaces" }
    
    foreach ($file in $rootFiles) {
        Write-Host "Processing root interface: $($file.FullName)"
        
        $content = Get-Content $file.FullName -Raw
        
        # Keep root interfaces in the base namespace
        $oldNamespace = "namespace OnlineBookManagementSystem.Core.Application.Interfaces"
        $newContent = $content -replace [regex]::Escape($oldNamespace), "namespace OnlineBookManagementSystem.Core.Application.Interfaces.Infrastructure"
        
        if ($content -ne $newContent) {
            Set-Content -Path $file.FullName -Value $newContent -NoNewline
            Write-Host "  Updated root interface namespace"
        }
    }
}

Write-Host "Interface namespace fix completed!"