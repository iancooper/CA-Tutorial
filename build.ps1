dotnet restore
dotnet build
pushd
cd .\GreetingsApp
rm "out" -recurse -force
dotnet publish -c Release -o out
popd