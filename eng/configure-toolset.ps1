# We can't use already installed dotnet cli since we need to install additional workloads.
# We could potentially try to find an existing installation that has all the required workloads,
# but it's unlikely one will be available.

$script:useInstalledDotNetCli = $false