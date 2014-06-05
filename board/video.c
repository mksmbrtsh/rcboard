#include "stdio.h"
#include "vars.h"

void PauseStream() {
	system("gst-client -p 0 pause");
}


void StopStream() {
	system("gst-client -p 0 null");
}


void PlayStream() {
	system("gst-client -p 0 play");
}


void PlayStreamTo(char * host) {
	char gst_cmd[256];
	sprintf(gst_cmd, "gst-client -p 0 set udpsink0 host string %s && gst-client -p 0 play", host);
	system(gst_cmd);
}


void SetBitrate(long bitrate) {
	char gst_cmd[256];
	sprintf(gst_cmd, "gst-client -p 0 null && gst-client -p 0 set dmaienc_h2640 targetbitrate integer %lu && gst-client -p 0 play", bitrate);
	system(gst_cmd);
}


void SetMTU(long mtu) {
	char gst_cmd[256];
	sprintf(gst_cmd, "gst-client -p 0 set rtph264pay0 mtu integer %lu", mtu);
	system(gst_cmd);
}

