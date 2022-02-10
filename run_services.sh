#!/bin/bash -e
screen -d -m -S dashboard -L /home/pi/dnHome/dnHomeDashboard/run.sh
screen -d -m -S monitor -L  bash -c 'cd /home/pi/dnHome/MonitoringService/ && dotnet run'

exit 0

