#include <errno.h>
#include <string.h>
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <math.h>

#include "vars.h"

#define 	K 				0.05
#define 	ADC_DELAY		60000

void * adc_read_loop (void *arg) {

	int file;
	unsigned char buf[2];
	int value;

	double new_volts = 0, old_volts = 0;
	double max_raw = 1 << 10;
	double divider = config.vref / max_raw;
	double volts;

	if ((file = open("/dev/v2r_adc", O_RDWR)) < 0) {
    	printf("Failed to open ADC driver\n");
		exit(1);
	}

	while (1) {

		if (read(file, buf, 2) == 2) {

			value = buf[0] | (buf[1] << 8);
			volts = (double) value * divider;

			new_volts = (volts * K) + (old_volts * (1.0 - K));
			old_volts = new_volts;

			* (double *) arg = new_volts;

			lseek(file, 0, SEEK_SET);
			usleep(ADC_DELAY);
		} 
	}
}


void SendVoltage() {

	char buffer[256];

	int voltage_int = (int) floor(system_voltage);
	int voltage_dec = (int) floor((system_voltage - voltage_int)*100);

	buffer[0] = 1; // cmd id 
	buffer[1] = voltage_int;
	buffer[2] = voltage_dec;
	udp_client_send (buffer, 3);

}
