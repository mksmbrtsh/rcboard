#include <stdio.h>
#include <stdlib.h>
#include <errno.h>
#include <pthread.h>
#include <syslog.h>

#include "vars.h"
#include "common.h"

int main(int argc, char * argv[])
{

	int result;
	pthread_t thread_adc, thread_socket, thread_wlan;

	cmdline_verbose = 0;
	memset((char *) &config, 0, sizeof(config));

	/* parse cmd params */
	parse_cmdline(argc, argv);

	/* load config from file */
	load_config();

	/* override verbose param from config file */
	if (cmdline_verbose)
		config.verbose = 1;


	if (config.role) {
		if (config.verbose) printf("* Started as client, using server %s:%d\n", config.server_host, config.port);
		syslog (LOG_INFO, "RcBoard started as command client");
	}
	else {
		if (config.verbose) printf("* Started as server, using port %d\n", config.port);
		syslog (LOG_INFO, "RcBoard started as command server");
	}

	videoneedstart = 1;

	/* thread for reading voltage */
	result = pthread_create(&thread_adc, NULL, adc_read_loop, &system_voltage);
	if (result != 0) {
		perror("Creating the ADC thread failed");
		return EXIT_FAILURE;
	} else {
		if (config.verbose) printf("* ADC thread started\n");
	}

	/* thread for reading WLAN status */
	result = pthread_create(&thread_wlan, NULL, wlanstatus_read_loop, NULL);
	if (result != 0) {
		perror("Creating the WLAN status thread failed");
		return EXIT_FAILURE;
	} else {
		if (config.verbose) printf("* WLAN status thread started\n");
	}
	
	if (config.role) {

		/* thread for UDP client */
		result = pthread_create(&thread_socket, NULL, udp_client_loop, NULL);
		if (result != 0) {
			perror("Creating the second thread failed");
			exit (1);
		} else {
			if (config.verbose) printf("* UDP client thread started\n");
		}

	} else {

		/* thread for UDP server */
		result = pthread_create(&thread_socket, NULL, udp_server_loop, NULL);
		if (result != 0) {
			perror("Creating the second thread failed");
			exit (1);
		} else {
			if (config.verbose) printf("* UDP server thread started\n");
		}

	}

	/* open pwm and pin devices */
	if (virt2real_init_pwm()) {
		perror("open v2r_pins failed");
		exit (1);
	}

	/* wait while threads will start */
	usleep(100000);

	/* start check alive timer */
	set_alive_timer (ALIVETIMERPERIOD);


	stopAll();

	/* main loop */
	while (1) {

		/* send alive */
		SendAlive();

		/* send voltage */
		SendVoltage();
	
		/* send signal status */
		SendSignalStatus();

		usleep(ALIVETIMERPERIOD);

	}

	return EXIT_SUCCESS;

}
