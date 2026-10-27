param([switch]$NoBuild)
$ErrorActionPreference = 'Stop'
$repoRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
Push-Location $repoRoot
try {
    $packArguments = @('pack', 'Lokad.Cli/Lokad.Cli.csproj', '-c', 'Release', '--tl:off', '--nologo', '-v', 'minimal')
    if ($NoBuild) { $packArguments += @('--no-build', '--no-restore') }
    dotnet @packArguments
    if ($LASTEXITCODE -ne 0) { throw 'Release pack failed.' }

    [xml]$project = Get-Content -LiteralPath 'Lokad.Cli/Lokad.Cli.csproj' -Raw
    $version = $project.Project.PropertyGroup.Version
    $package = Join-Path $repoRoot "artifacts/nugets/Lokad.Cli.$version.nupkg"
    $generator = dotnet msbuild Lokad.Cli.Generator/Lokad.Cli.Generator.csproj -p:Configuration=Release -getProperty:TargetPath --tl:off
    if ($LASTEXITCODE -ne 0) { throw 'Could not resolve generator output.' }

    $archive = [IO.Compression.ZipFile]::OpenRead($package)
    try {
        $reader = [IO.StreamReader]::new($archive.GetEntry('Lokad.Cli.nuspec').Open())
        try { [xml]$metadata = $reader.ReadToEnd() } finally { $reader.Dispose() }
        if ($metadata.SelectSingleNode("//*[local-name()='metadata']/*[local-name()='id']").InnerText -ne 'Lokad.Cli' -or
            $metadata.SelectSingleNode("//*[local-name()='metadata']/*[local-name()='version']").InnerText -ne $version -or
            $metadata.SelectSingleNode("//*[local-name()='metadata']/*[local-name()='icon']").InnerText -ne 'icon.png' -or
            $metadata.SelectSingleNode("//*[local-name()='metadata']/*[local-name()='readme']").InnerText -ne 'README.md') {
            throw 'Incorrect NuGet identity, version, icon or README metadata.'
        }
        if ($metadata.SelectNodes("//*[local-name()='dependency']").Count -ne 0) { throw 'Unexpected NuGet dependencies.' }
        $analyzers = @($archive.Entries | Where-Object { $_.FullName.StartsWith('analyzers/') })
        if ($analyzers.Count -ne 1 -or $analyzers[0].FullName -ne 'analyzers/dotnet/cs/Lokad.Cli.Generator.dll') {
            throw 'The package must contain exactly the bundled C# generator.'
        }
        foreach ($framework in @('net8.0', 'net10.0')) {
            foreach ($extension in @('dll', 'xml')) {
                if ($null -eq $archive.GetEntry("lib/$framework/Lokad.Cli.$extension")) { throw "Missing $framework $extension." }
            }
        }

        $files = @(
            @{ Entry = 'README.md'; Source = Join-Path $repoRoot 'README.md' },
            @{ Entry = 'CHANGELOG.md'; Source = Join-Path $repoRoot 'CHANGELOG.md' },
            @{ Entry = 'LICENSE.txt'; Source = Join-Path $repoRoot 'LICENSE.txt' },
            @{ Entry = 'icon.png'; Source = Join-Path $repoRoot 'icon.png' },
            @{ Entry = 'analyzers/dotnet/cs/Lokad.Cli.Generator.dll'; Source = $generator }
        )
        foreach ($file in $files) {
            $entry = $archive.GetEntry($file.Entry)
            if ($null -eq $entry) { throw "Package content missing: $($file.Entry)" }
            $buffer = [IO.MemoryStream]::new()
            $stream = $entry.Open()
            try { $stream.CopyTo($buffer) } finally { $stream.Dispose() }
            try {
                $bytes = $buffer.ToArray()
                $actual = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes))
                $expected = (Get-FileHash -LiteralPath $file.Source -Algorithm SHA256).Hash
                if ($actual -ne $expected) { throw "Package differs from current source: $($file.Entry)" }
                if ($file.Entry -eq 'icon.png' -and
                    ([Convert]::ToHexString($bytes[0..7]) -ne '89504E470D0A1A0A' -or
                    [Net.IPAddress]::NetworkToHostOrder([BitConverter]::ToInt32($bytes, 16)) -ne 128 -or
                    [Net.IPAddress]::NetworkToHostOrder([BitConverter]::ToInt32($bytes, 20)) -ne 128)) {
                    throw 'The package icon must be a 128x128 PNG.'
                }
            } finally { $buffer.Dispose() }
        }
    } finally { $archive.Dispose() }

    $symbols = [IO.Compression.ZipFile]::OpenRead((Join-Path $repoRoot "artifacts/nugets/Lokad.Cli.$version.snupkg"))
    try {
        foreach ($framework in @('net8.0', 'net10.0')) {
            if ($null -eq $symbols.GetEntry("lib/$framework/Lokad.Cli.pdb")) { throw "Missing $framework package symbols." }
        }
    } finally { $symbols.Dispose() }

    $consumer = Join-Path $repoRoot ('artifacts/package-smoke/' + [Guid]::NewGuid().ToString('N'))
    $feed = Join-Path $consumer 'feed'
    New-Item -ItemType Directory -Path $feed -Force | Out-Null
    Copy-Item -LiteralPath $package -Destination $feed
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Consumer.csproj'), (Join-Path $PSScriptRoot 'Program.cs') -Destination $consumer
    $consumerProject = Join-Path $consumer 'Consumer.csproj'
    # Only the selected local package can supply Lokad.Cli. Allow the SDK to
    # download targeting packs when a matching .NET 8 SDK is not installed.
    $config = [xml]@'
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="local"><package pattern="Lokad.Cli" /></packageSource>
    <packageSource key="nuget.org"><package pattern="Microsoft.*" /></packageSource>
  </packageSourceMapping>
</configuration>
'@
    $config.configuration.packageSources.add[0].value = $feed
    $configPath = Join-Path $consumer 'NuGet.Config'
    $config.Save($configPath)
    dotnet restore $consumerProject --configfile $configPath --packages (Join-Path $consumer 'packages') "-p:CliPackageVersion=$version" --tl:off
    if ($LASTEXITCODE -ne 0) { throw 'Isolated package restore failed.' }
    dotnet build $consumerProject -c Release --no-restore "-p:CliPackageVersion=$version" --tl:off --nologo -v minimal
    if ($LASTEXITCODE -ne 0) { throw 'C# 11 package consumer compilation failed.' }
    foreach ($framework in @('net8.0', 'net10.0')) {
        dotnet (Join-Path $consumer "bin/Release/$framework/Consumer.dll")
        if ($LASTEXITCODE -ne 0) { throw "$framework package consumer failed." }
    }
} finally { Pop-Location }
