#!/bin/sh 
export PATH=/usr/bin:/bin:/usr/sbin:/sbin 

currentUser=$( echo "show State:/Users/ConsoleUser" | scutil | awk '/Name :/ { print $3 }' ) 

if [ -z "$currentUser" -o "$currentUser" = "loginwindow" ]; then 
  echo "no user logged in, cannot proceed" 
  exit 1  
fi 

uid=$(id -u "$currentUser")  

runAsUser() {   
  if [ "$currentUser" != "loginwindow" ]; then 
    launchctl asuser "$uid" sudo -u "$currentUser" "$@" 
  else 
    echo "no user logged in" 
  fi 
} 

runAsUser launchctl load -w /System/Library/LaunchAgents/com.apple.notificationcenterui.plist 