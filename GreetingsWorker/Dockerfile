FROM microsoft/dotnet:latest

# create a directory for the app source code 
RUN mkdir -p /usr/todo
WORKDIR /usr/todo

# copy the source code
COPY out .

# Run the app
ENTRYPOINT ["dotnet", "GreetingsWorker.dll"]
