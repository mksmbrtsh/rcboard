#include <errno.h>
#include <string.h>
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <string.h>

#include <curl/curl.h>
#include <confuse.h>

#include "vars.h"

void ReadWiFi();
void ReadYota();

/* for Curl*/
CURL *curl;
CURLcode res;

void * wlanstatus_read_loop (void *arg) {

	/* prepare for status reading (if needed) */
	switch (config.wlantype) {

		case CON_YOTA:
			/* Yota */
			curl = curl_easy_init();
		break;
	}

	while (1) {

		switch (config.wlantype) {

			case CON_WIFI:
				/* read Wi-Fi status */
				ReadWiFi();
			break;
			case CON_YOTA:
				/* read Yota status */
				ReadYota();
			break;
			case CON_LAN:
				/* read LAN status */
				ReadLan();
			break;
			case CON_UBIQ:
				/* read Ubiquity status */
				ReadUbiquity();
			break;
		}
		usleep(STATUS_DELAY);
	}

}


int YotaCurlParser (char *data, size_t size, size_t nmemb, char *writerData) {

	if (writerData == NULL)
		return 0;

	char sinr[5];
	char rssi[5];
	char rsrp[5];
	char rsrq[5];
	char txpwr[5];
    char devicename[20];

	/* parse recieved text buffer */
    char *p;
    char *temp_string;
    char *temp_param;
    char *param_name;
    char *param_value;

    temp_string = strdup(data);

    do
    {
	p = strsep(&temp_string, "\n");
	if (p) {
	    /* split param string into two parts */
	    temp_param = strdup(p);
	    param_name = strsep(&temp_param, "=");
	    if (!param_name) continue;
	    param_value = strsep(&temp_param, "|");
	    if (!param_value) continue;
   
	    if (!strcmp(param_name, "3GPP.SINR")) {
			sprintf(sinr, "%s", param_value);
	    }
	    if (!strcmp(param_name, "3GPP.RSSI")) {
			sprintf(rssi, "%s", param_value);
	    }
	    if (!strcmp(param_name, "3GPP.RSRP")) {
			sprintf(rsrp, "%s", param_value);
	    }
	    if (!strcmp(param_name, "3GPP.RSRQ")) {
			sprintf(rsrq, "%s", param_value);
	    }
	    if (!strcmp(param_name, "3GPP.TxPWR")) {
			sprintf(txpwr, "%s", param_value);
	    }
	    if (!strcmp(param_name, "DeviceName")) {
			sprintf(devicename, "%s", param_value);
	    }
	    if (!strcmp(param_name, "State")) {
			if (!strcmp(param_value, "Connected")) {
			    wlanstatus.isconnected = 1;
			} else {
			    wlanstatus.isconnected = 0;
			}
	    }

		}

    } while(p);

	wlanstatus.type = CON_YOTA; 
	sprintf(wlanstatus.message, "\"%s\" SINR %s RSSI %s RSRP %s RSRQ %s TxPWR %s", devicename, sinr, rssi, rsrp, rsrq, txpwr);
}


/* read Yota status */
void ReadYota () {
    curl_easy_setopt(curl, CURLOPT_URL, config.yota_status);
	curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, YotaCurlParser);
	curl_easy_setopt(curl, CURLOPT_TIMEOUT, 3);
    res = curl_easy_perform(curl);
}


/* read Wi-Fi status */
void ReadWiFi () {

	if (config.wifi_iface == 0)
		return;

	getwlaninfo(config.wifi_iface);
	wlanstatus.type = CON_WIFI;
	wlanstatus.isconnected = 1;
	sprintf(wlanstatus.message, "%s;%s;%s", ifaceinfo.ssid, ifaceinfo.freq, ifaceinfo.link);

}

/* read LAN status */
void ReadLan () {
	wlanstatus.type = CON_LAN;
	wlanstatus.isconnected = 1;
}

/* read Ubiquity status */
void ReadUbiquity () {
	wlanstatus.type = CON_UBIQ;
	wlanstatus.isconnected = 1;
}


void SendSignalStatus() {

	char buffer[256];
	int i;

	if (!wlanstatus.type) return;

	buffer[0] = 2;	// cmd id
	buffer[1] = wlanstatus.type; // wlan type
	buffer[2] = wlanstatus.isconnected;

	for (i = 0; i < WLANMESSAGEMAXLEN; i++) {
		buffer[i + 3] = (char) wlanstatus.message[i];
	}

	udp_client_send (buffer, 3 + WLANMESSAGEMAXLEN);

	/* clear wlanstatus struct */
	memset(&wlanstatus, 0, sizeof(wlanstatus));
}
