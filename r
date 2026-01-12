#!/bin/bash
dotnet run &
DOTNET_PID=$!
sleep 3
cmd.exe /c start chrome http://localhost:5228
wait $DOTNET_PID
