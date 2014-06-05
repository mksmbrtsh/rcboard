#!/bin/sh

#read params from config
. ./config.sh

if [ "$CONFIG" == "remote" ] ;
then
	PARAMS="-a $ADDRESS -u $USER -p $HASH"
else
	PARAMS=
fi

quit()
{
    echo "" > /tmp/onair
    exit 1
}
trap quit INT

while [ 1 ]
do
	echo "FPV" > /tmp/onair
	./rcboard  $PARAMS
	gst-client -p 0 pause
	echo "" > /tmp/onair
	sleep 1

done

echo "" > /tmp/onair
