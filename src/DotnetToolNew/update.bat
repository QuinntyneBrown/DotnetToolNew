dotnet tool uninstall -g DotnetToolNew
dotnet pack
dotnet tool install --global --add-source ./nupkg DotnetToolNew
