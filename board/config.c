#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <curl/curl.h>
#include <confuse.h>

#include "vars.h"

char * loaded_string_content;

void load_remote_config(char * url, char * user, char * hash);

int load_config (void) {


	cfg_opt_t opts[] =
	{
		CFG_STR("role", "server", CFGF_NONE),
		CFG_STR("server_addr", "127.0.0.1", CFGF_NONE),
		CFG_INT("port", "1082", CFGF_NONE),
		CFG_FLOAT("vref", 3.6, CFGF_NONE),
		CFG_INT("verbose", 1, CFGF_NONE),
		CFG_INT("alive_max_timeout", 2, CFGF_NONE),
		CFG_INT("wlantype", 0, CFGF_NONE),
		CFG_STR("wifi_iface", "wlan0", CFGF_NONE),
		CFG_STR("yota_status", "http://10.0.0.1/status", CFGF_NONE),

		CFG_INT("ppm0_con", 43, CFGF_NONE),
		CFG_INT("ppm1_con", 44, CFGF_NONE),
		CFG_INT("ppm2_con", 19, CFGF_NONE),
		CFG_INT("ppm3_con", 42, CFGF_NONE),

		CFG_INT("ppm0_min", 700, CFGF_NONE),
		CFG_INT("ppm0_max", 2200, CFGF_NONE),
		CFG_INT("ppm1_min", 700, CFGF_NONE),
		CFG_INT("ppm1_max", 2200, CFGF_NONE),
		CFG_INT("ppm2_min", 700, CFGF_NONE),
		CFG_INT("ppm2_max", 2200, CFGF_NONE),
		CFG_INT("ppm3_min", 700, CFGF_NONE),
		CFG_INT("ppm3_max", 2200, CFGF_NONE),

		CFG_INT("ppm0_zero", 90, CFGF_NONE),
		CFG_INT("ppm1_zero", 90, CFGF_NONE),
		CFG_INT("ppm2_zero", 90, CFGF_NONE),
		CFG_INT("ppm3_zero", 90, CFGF_NONE),

		CFG_INT("axis0", 0, CFGF_NONE),
		CFG_INT("axis1", 1, CFGF_NONE),
		CFG_INT("axis2", 2, CFGF_NONE),
		CFG_INT("axis3", 3, CFGF_NONE),

		CFG_END()
	};

	cfg_t *cfg;
	cfg = cfg_init(opts, CFGF_NONE);


	/* check config type (local or remote) */
	if (strlen(config.remoteconfiguser) > 0) {

		if (cmdline_verbose)
			printf("* try to load remote config for user %s from %s...\n", config.remoteconfiguser, config.remoteconfigurl);

		load_remote_config(config.remoteconfigurl, config.remoteconfiguser, config.remoteconfighash);

		if (loaded_string_content == 0) {
			printf("error: can't load remote config\n");
			exit(1);
		}

		cfg_parse_buf(cfg, loaded_string_content);

	} else {

		if (cmdline_verbose)
			printf("* using local config file\n");
		
		cfg_parse(cfg, "rcboard.conf");
	}


	char * role = cfg_getstr(cfg, "role");
	if (!strcmp(role, "client")) {
		config.role = 1;
	} else
	if (!strcmp(role, "server")) {
		config.role = 0;
	}


	sprintf(config.server_host, "%s", cfg_getstr(cfg, "server_addr"));
	config.port = cfg_getint(cfg, "port");
	config.vref = cfg_getfloat(cfg, "vref");
	config.verbose = cfg_getint(cfg, "verbose");
	config.alive_maxtimeout = cfg_getint(cfg, "alive_max_timeout");
	config.wlantype = cfg_getint(cfg, "wlantype");
	sprintf(config.wifi_iface, "%s", cfg_getstr(cfg, "wifi_iface"));
	sprintf(config.yota_status, "%s", cfg_getstr(cfg, "yota_status"));

	config.ppm[0].con = cfg_getint(cfg, "ppm0_con");
	config.ppm[1].con = cfg_getint(cfg, "ppm1_con");
	config.ppm[2].con = cfg_getint(cfg, "ppm2_con");
	config.ppm[3].con = cfg_getint(cfg, "ppm3_con");

	config.ppm[0].min = cfg_getint(cfg, "ppm0_min");
	config.ppm[0].max = cfg_getint(cfg, "ppm0_max");
	config.ppm[1].min = cfg_getint(cfg, "ppm1_min");
	config.ppm[1].max = cfg_getint(cfg, "ppm1_max");
	config.ppm[2].min = cfg_getint(cfg, "ppm2_min");
	config.ppm[2].max = cfg_getint(cfg, "ppm2_max");
	config.ppm[3].min = cfg_getint(cfg, "ppm3_min");
	config.ppm[3].max = cfg_getint(cfg, "ppm3_max");

	config.ppm[0].zero = cfg_getint(cfg, "ppm0_zero");
	config.ppm[1].zero = cfg_getint(cfg, "ppm1_zero");
	config.ppm[2].zero = cfg_getint(cfg, "ppm2_zero");
	config.ppm[3].zero = cfg_getint(cfg, "ppm3_zero");

	config.axis[0] = cfg_getint(cfg, "axis0");
	config.axis[1] = cfg_getint(cfg, "axis1");
	config.axis[2] = cfg_getint(cfg, "axis2");
	config.axis[3] = cfg_getint(cfg, "axis3");

	cfg_free(cfg);

	return 0;

}


int curlParser (char *data, size_t size, size_t nmemb, char *writerData) {

	if (writerData == NULL)
		return 0;

    loaded_string_content = strdup(data);
	
	return 1;

}


void load_remote_config(char * url, char * user, char * hash) {

	CURL *curl;
	CURLcode res;

	char postfields[256];
	sprintf(postfields, "id=%s&hash=%s", user, hash);

	curl = curl_easy_init();

    curl_easy_setopt(curl, CURLOPT_URL, url);
	curl_easy_setopt(curl, CURLOPT_POSTFIELDS, postfields);
	curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, curlParser);
    curl_easy_setopt(curl, CURLOPT_FOLLOWLOCATION, 1);
	curl_easy_setopt(curl, CURLOPT_TIMEOUT, 5);
    res = curl_easy_perform(curl);

}
