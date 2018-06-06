dotnet restore
dotnet build
cd GreetingsApp
rm -rf out
dotnet publish -c Release -o out
cd ..