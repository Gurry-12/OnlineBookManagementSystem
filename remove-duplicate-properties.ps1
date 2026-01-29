# Remove duplicate properties that were added twice

Write-Host "Removing duplicate properties..." -ForegroundColor Cyan

# Fix Order.cs
$orderPath = "OnlineBookManagementSystem/Core/Domain/Entities/Order.cs"
$content = Get-Content $orderPath -Raw

# Remove the second occurrence of the extended properties block
$content = $content -replace '(\s+// Navigation properties\s+// Extended Address Information\s+public string\? PhoneNumber[^}]+DeliveredDate[^;]+;\s+)', "`n`n        // Navigation properties`n"

Set-Content $orderPath $content -NoNewline
Write-Host "✓ Fixed Order.cs" -ForegroundColor Green

# Fix OrderDetail.cs
$orderDetailPath = "OnlineBookManagementSystem/Core/Domain/Entities/OrderDetail.cs"
$content = Get-Content $orderDetailPath -Raw

# Remove duplicate TotalPrice and IsDeleted
$pattern = '(\s+// Calculated Properties\s+public decimal TotalPrice[^;]+;\s+public bool IsDeleted[^;]+;\s+)(\s+// Navigation properties\s+// Calculated Properties\s+public decimal TotalPrice[^;]+;\s+public bool IsDeleted[^;]+;\s+)'
$content = $content -replace $pattern, "`$1`n        // Navigation properties`n"

Set-Content $orderDetailPath $content -NoNewline
Write-Host "✓ Fixed OrderDetail.cs" -ForegroundColor Green

Write-Host "`nDuplicates removed successfully!" -ForegroundColor Green
