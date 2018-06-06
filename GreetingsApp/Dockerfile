FROM microsoft/aspnetcore:2.0

# create a directory for the app source code 
RUN mkdir -p /usr/todo
WORKDIR /usr/todo

# copy the source code
COPY out .

# Expose the port
EXPOSE 5000

#run the site
ENTRYPOINT ["dotnet", "GreetingsApp.dll"]