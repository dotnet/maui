#!/bin/bash
dotnet="$1"

# Get the PID of dotnet-dsrouter
dsrouter_pid=$(pgrep -f "dotnet dsrouter")

if [ -z "$dsrouter_pid" ]; then
    dotnet dsrouter android > >(while IFS= read -r line
    do
        # Print the line to the terminal
        echo "$line"
        # Check if the line contains 'pid='
        if [[ $line == *"pid="* ]]; then
            # Extract the PID value
            pid_value=$(echo $line | grep -o 'pid=[0-9]*' | cut -d '=' -f2)
            echo "Found pid: $pid_value"
            # You can add code here to do something with the PID value
            echo "$pid_value" > /tmp/pid_value.txt
            break  # Exit the loop after finding the first value
        fi
    done) &

    dsrouter_pid=$(pgrep -f "dotnet dsrouter")
    trap "kill $dsrouter_pid" EXIT
fi

# # Run dotnet trace collect using the obtained PID and specified format
dotnet trace collect --name dotnet-dsrouter --providers Microsoft-Windows-DotNETRuntime:0x1F000080018:5 --duration 00:00:00:05 # --stopping-event-provider-name Microsoft-Windows-DotNETRuntime --stopping-event-event-name Method/JittingStarted --stopping-event-payload-filter MethodName:PrintA

# Check if there were any errors when running the command
if [ $? -ne 0 ]; then
    echo "Error occurred while running the command."
    exit 1
fi