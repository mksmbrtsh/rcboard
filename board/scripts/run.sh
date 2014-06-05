#!/bin/sh

#read params from config
. ./config.sh

if [ "$HOST" == "" ] ;
then
	SENDHOST=127.0.0.1
else
	SENDHOST=$HOST
fi


sysctl -w net.ipv4.tcp_congestion_control=lp > /dev/null
sysctl -w net.ipv4.tcp_fastopen=1 > /dev/null
sysctl -w net.ipv4.tcp_slow_start_after_idle=0 > /dev/null

ifconfig eth0 txqueuelen 5000 > /dev/null 2>&1
ifconfig wlan0 txqueuelen 5000 > /dev/null 2>&1

echo "none" > /sys/class/leds/v2r:green:user/trigger
echo "heartbeat" > /sys/class/leds/v2r:red:user/trigger
echo 0 > /proc/v2r_gpio/pwctr3

killall gstd > /dev/null 2>&1
gstd --system &
sleep 1
gst-client create "v4l2src always-copy=false chain-ipipe=true ! capsfilter caps=video/x-raw-yuv,format=(fourcc)NV12,width=1280,height=720,framerate=(fraction)30 ! dmaiaccel qos=false ! dmaienc_h264 copyOutput=false ddrbuf=false encodingpreset=2 ratecontrol=1 rcalgo=2 intraframeinterval=10 idrinterval=10 level=41 t8x8intra=true t8x8inter=true qpintra=28 qpinter=31 airrate=30 seqscaling=3 targetbitrate=$BITRATE maxbitrate=$MAXBITRATE bytestream=true ! rtph264pay mtu=$MTU pt=96 ! udpsink port=$PORT host=$SENDHOST sync=false enable-last-buffer=false send-duplicates=false blocksize=5000 max-lateness=100000000"

if [ $SENDHOST != "127.0.0.1" ] ;
then
	gst-client -p 0 play > /dev/null 2>&1
fi