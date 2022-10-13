$repoRoot = Split-Path $PSScriptRoot -Parent
$testDir = Join-Path -Path $repoRoot -ChildPath "test"
$testResultsDir = Join-Path -Path $repoRoot -ChildPath "test-results"
$benchmarkFile = Join-Path -Path $testDir -ChildPath "benchmarks.yml"

function Run($commandLine)
{
    Write-Host "Running: $commandLine" 
    Invoke-Expression "& $commandLine"
}

$commonArgs = "--config $benchmarkFile --profile aspnet-perf-lin --chart"

Run "crank $commonArgs --scenario albums-net-alpine --json $testResultsDir\net-alpine-serverGC.json --property platform=net-alpine-serverGC --application.buildArguments SERVER_GC=true"
Run "crank $commonArgs --scenario albums-net-alpine --json $testResultsDir\net-alpine-workstationGC.json --property platform=net-alpine-workstationGC --application.buildArguments SERVER_GC=false"

Run "crank $commonArgs --scenario albums-net-fdd --json $testResultsDir\net-fdd-serverGC.json --property platform=net-fdd-serverGC --application.buildArguments SERVER_GC=true"
Run "crank $commonArgs --scenario albums-net-fdd --json $testResultsDir\net-fdd-workstationGC.json --property platform=net-fdd-workstationGC --application.buildArguments SERVER_GC=false"

Run "crank $commonArgs --scenario albums-httplistener-alpine --json $testResultsDir\httplistener-alpine.json --property platform=httplistener-alpine"

Run "crank $commonArgs --scenario albums-net-debian --json $testResultsDir\net-debian.json --property platform=net-debian"

Run "crank $commonArgs --scenario albums-go-alpine --json $testResultsDir\go-alpine.json --property platform=go-alpine"

Run "crank compare $testResultsDir\go-alpine.json $testResultsDir\net-alpine-serverGC.json $testResultsDir\net-alpine-workstationGC.json $testResultsDir\net-fdd-serverGC.json $testResultsDir\net-fdd-workstationGC.json $testResultsDir\net-debian.json $testResultsDir\httplistener-alpine.json"
