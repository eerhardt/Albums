$repoRoot = Split-Path $PSScriptRoot -Parent
$testDir = Join-Path -Path $repoRoot -ChildPath "test"
$testResultsDir = Join-Path -Path $testDir -ChildPath "results"
$benchmarkFile = Join-Path -Path $testDir -ChildPath "benchmarks.yml"

$commonArgs = "--config $benchmarkFile --profile aspnet-perf-lin --chart"

Invoke-Expression "& crank $commonArgs --scenario albums-netcore-alpine --json $testResultsDir\netcore-alpine-serverGC.json --property platform=netcore-alpine-serverGC --application.buildArguments SERVER_GC=true"
Invoke-Expression "& crank $commonArgs --scenario albums-netcore-alpine --json $testResultsDir\netcore-alpine-workstationGC.json --property platform=netcore-alpine-workstationGC --application.buildArguments SERVER_GC=false"

Invoke-Expression "& crank $commonArgs --scenario albums-netcore-debian --json $testResultsDir\netcore-debian.json --property platform=netcore-debian"

Invoke-Expression "& crank $commonArgs --scenario albums-go-alpine --json $testResultsDir\go-alpine.json --property platform=go-alpine"

Invoke-Expression "& crank compare $testResultsDir\go-alpine.json $testResultsDir\netcore-alpine-serverGC.json $testResultsDir\netcore-alpine-workstationGC.json $testResultsDir\netcore-debian.json"