#!/bin/bash

# sudo chown -R userName /Users/Shared/Jenkins
# sudo chown -R userName /var/log/jenkins
#重启Jenkins
# sudo launchctl load /Library/LaunchDaemons/org.jenkins-ci.plist

pid=`ps -ef | grep jenkins.war | grep -v 'grep'| awk '{print $2}'| wc -l`
if [ "$1" = "start" ];then
    if [ $pid -gt 0 ];then
        echo 'jenkins is running...'
    else
java -jar /opt/grow/jenkins.war --httpPort=8060 #>/dev/null 2>&1 &
    fi
elif [ "$1" = "stop" ];then
    exec ps -ef | grep jenkins | grep -v grep | awk '{print $2}'| xargs kill -9
    echo 'jenkins is stop..'
else
    echo "Please input like this:"./jenkins.sh start" or "./jenkins stop""
fi
