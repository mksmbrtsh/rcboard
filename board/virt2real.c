#include<stdio.h>
#include<stdlib.h>
#include <fcntl.h>
#include <sys/stat.h>
#include <sys/types.h>
#include <math.h>

#include "vars.h"

int fd_pwm;
int fd_extpwm;
int fd_adc;

double tic = 0.042; // = 1000000 / 24000000;

void setPinPWM(char con);
void setPWM(char channel, long duty, long period);
void setPPM(char channel, int value);
void setPWM(char channel, long duty, long period);

int virt2real_init_pwm (void) {

	int i;

    fd_pwm = open("/dev/v2r_pins", O_WRONLY | O_CREAT, S_IRUSR | S_IWUSR);
	if (fd_pwm < 0)
	{
		printf("can't open pins driver\n");
		return 1;
	}

	for (i = 0; i < 4 ; i++)  {

		setPinPWM(config.ppm[i].con);

		config.ppm[i].ppm_period = 20000 / tic;
		config.ppm[i].ppm_min = config.ppm[i].min / tic;
		config.ppm[i].ppm_max = config.ppm[i].max / tic;
		config.ppm[i].ppm_delta = (config.ppm[i].ppm_max - config.ppm[i].ppm_min) / 180;
	}

	return 0;
}

int virt2real_init_extpwm (void) {

    fd_extpwm = open("/sys/bus/i2c/devices/1-0070/any", O_WRONLY | O_CREAT, S_IRUSR | S_IWUSR);
	if (fd_pwm < 0)
	{
		printf("can't open extpwm driver\n");
		return 1;
	}

	return 0;
}

void setPinPWM(char con) {
	char buffer[2];

	buffer[0] = 6;
	buffer[1] = con;
	write (fd_pwm, buffer, sizeof(buffer));
}

void setPWM(char channel, long duty, long period) {
	char buffer[6];

	buffer[0] = 7;
	buffer[1] = channel;
	buffer[2] = duty & 0xFF;
	buffer[3] = (duty >> 8) & 0xFF;
	buffer[4] = period & 0xFF;
	buffer[5] = (period >> 8) & 0xFF;

	write (fd_pwm, buffer, sizeof(buffer));
}


void setPPM(char channel, int value) {

	static char buffer[10];

	int ppm_duty = ceil(config.ppm[channel].ppm_min + config.ppm[channel].ppm_delta * value);

	buffer[0] = 8;
	buffer[1] = channel;
	buffer[2] = ppm_duty & 0xFF;
	buffer[3] = (ppm_duty >> 8) & 0xFF;
	buffer[4] = (ppm_duty >> 16) & 0xFF;
	buffer[5] = (ppm_duty >> 24) & 0xFF;
	buffer[6] = config.ppm[channel].ppm_period & 0xFF;
	buffer[7] = (config.ppm[channel].ppm_period >> 8) & 0xFF;
	buffer[8] = (config.ppm[channel].ppm_period >> 16) & 0xFF;
	buffer[9] = (config.ppm[channel].ppm_period >> 24) & 0xFF;

	write(fd_pwm, buffer, sizeof(buffer));
}


void setPin(char pin, char value) {
	char buffer[3];

	buffer[0] = 1;
	buffer[1] = pin;
	buffer[2] = 1 | (value << 1);

	write (fd_pwm, buffer, sizeof(buffer));
}

void setEXTPWM(char channel, unsigned short duty, unsigned short period) {

	char buffer[6];

	buffer[0] = 1;
	buffer[1] = channel;
	buffer[2] = duty & 0xFF;
	buffer[3] = (duty >> 8) & 0xFF;
	buffer[4] = period & 0xFF;
	buffer[5] = (period >> 8) & 0xFF;

	write (fd_extpwm, buffer, sizeof(buffer));
}
