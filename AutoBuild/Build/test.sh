#!/bin/sh
echo "start"
export dirname=$(dirname "$PWD")
echo $(dirname "$dirname")
echo $1
echo "end"
read flag
if ["$flag"="y" -o "$flag" = "Y"]; then 
	echo
else
	exit 0
fi