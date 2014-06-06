#include "stdio.h"
#include "vars.h"

void PauseStream() {
	if (config.verbose)
		printf("* pause stream\n");
	system("gst-client -p 0 pause");
}


void StopStream() {
	if (config.verbose)
		printf("* stop stream\n");
	system("gst-client -p 0 null");
}


void PlayStream() {

	if (config.verbose)
		printf("try to play stream, clienttype = %d\n", clienttype);

	if (!clienttype)
		return;
	system("gst-client -p 0 play");
}


void PlayStreamTo(char * host) {

	if (config.verbose)
		printf("try to playto stream, clienttype = %d\n", clienttype);

	if (!clienttype)
		return;
	char gst_cmd[256];
	sprintf(gst_cmd, "gst-client -p 0 set udpsink0 host string %s && gst-client -p 0 play", host);
	system(gst_cmd);
}


void SetBitrate(long bitrate) {

	if (config.verbose)
		printf("* set bitrate %lu\n", bitrate);

	if (!clienttype)
		return;
	char gst_cmd[256];
	sprintf(gst_cmd, "gst-client -p 0 null && gst-client -p 0 set dmaienc_h2640 targetbitrate integer %lu && gst-client -p 0 play", bitrate);
	system(gst_cmd);
}


void SetMTU(long mtu) {

	if (config.verbose)
		printf("* set MTU %lu\n", mtu);

	char gst_cmd[256];
	sprintf(gst_cmd, "gst-client -p 0 set rtph264pay0 mtu integer %lu", mtu);
	system(gst_cmd);
}
