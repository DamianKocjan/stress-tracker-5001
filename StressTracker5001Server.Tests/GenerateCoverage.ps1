# Generate code coverage report and open in browser

Write-Host "Running tests with coverage..." -ForegroundColor Cyan
dotnet test --collect:"XPlat Code Coverage" --results-directory:./TestResults

Write-Host "`nGenerating HTML report..." -ForegroundColor Cyan
reportgenerator -reports:"./TestResults/**/coverage.cobertura.xml" -targetdir:"./TestResults/CoverageReport" -reporttypes:Html

Write-Host "`nOpening coverage report..." -ForegroundColor Green
start ./TestResults/CoverageReport/index.html

Write-Host "`nCoverage report generated successfully!" -ForegroundColor Green
