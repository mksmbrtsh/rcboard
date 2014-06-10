#include <stdio.h>
#include <sys/time.h>
#include <time.h>
#include <math.h>

#include "vars.h"

int lastspeed0, lastspeed1;

void Move (int val1, int val2);
void stopAll ();

void SendAlive() {

	char buffer[256];
	buffer[0] = 0; // cmd id 
	udp_client_send (buffer, 1);

}


void parseCommand(char * buf) {

	struct timespec time;
	unsigned long buttons = 0;
	char channel;
	int speed;
	int checksum;

	switch (buf[0]) {
		case 0:
			/* alive */
			checksum =  buf[0] ^ buf[1];
			if (checksum != buf[2]) return;
			clienttype = buf[1];

			if (config.verbose)
				printf("* client (type = %d) alive!\n", clienttype);

			clock_gettime(CLOCK_MONOTONIC, &time);
			aliveTimestamp = time.tv_sec;

			CheckRemoteClient(remote_host, remote_port);

		break;

		case 1:
			/* multichannel RC axis */

			if (!aliveTimestamp) break;

			channel =  buf[1];

			/* check sum */
			checksum =  buf[0] ^ buf[1] ^ buf[2] ^ buf[3];
			if (checksum != buf[4]) return;

			switch (channel) {
				case 0:
					/* regular wheel */
					speed = ceil( (buf[2] - 127) * 90 / 127 + config.ppm[config.axis[0]].zero);
					setPPM(config.axis[0], speed);
				break;
				case 1:
					/* regular accelerate */
					speed = ceil( (buf[2] - 127) * 90 / 127 + config.ppm[config.axis[1]].zero);
					setPPM(config.axis[1], speed);
				break;
				case 2:
					speed = ceil( (buf[2] - 127) * 90 / 127 + config.ppm[config.axis[2]].zero);
					setPPM(config.axis[2], speed);
				break;
				case 3:
					speed = ceil( (buf[2] - 127) * 90 / 127 + config.ppm[config.axis[3]].zero);
					setPPM(config.axis[3], speed);
				break;
		
			}

		break;

		case 2:
			/* multichannel axis for tank control (different tracks) */

			if (!aliveTimestamp) break;

			channel =  buf[1];

			/* check sum */
			checksum =  buf[0] ^ buf[1] ^ buf[2] ^ buf[3];
			if (checksum != buf[4]) return;

			switch (channel) {
				case 0:
					/* always wheel */
		
					speed = buf[2];

					/* save value for make component value */
					lastspeed1 = speed; 
					Move(lastspeed0, lastspeed1);

				break;
				case 1:
					/* always accelerate */
					speed = buf[2];

					/* save value for make component value */
					lastspeed0 = speed; 
					Move(lastspeed0, lastspeed1);

				break;
				case 2:
					speed = ceil( (buf[2] - 127) * 90 / 127 + config.ppm[config.axis[2]].zero);
					setPPM(config.axis[2], speed);
				break;
				case 3:
					speed = ceil( (buf[2] - 127) * 90 / 127 + config.ppm[config.axis[3]].zero);
					setPPM(config.axis[3], speed);
				break;

			}

		break;

		case 3:
			/* buttons */
			/* check sum */
			//checksum =  buf[0] ^ buf[1];
			//if (checksum != buf[2]) return;

		break;

		case 5:
			/* system commands */
			switch (buf[1]) {
				case 1:
					/* restart script command */
					checksum =  buf[0] ^ buf[1];
					if (checksum != buf[2]) return;
					exit(2); // exit script for restart
				break;
				case 2:
					/* change bitrate command */
					checksum =  buf[0] ^ buf[1] ^ buf[2] ^ buf[3] ^ buf[4] ^ buf[5];
					if (checksum != buf[6]) return;
					long bitrate = buf[2] | (buf[3] << 8) | ((buf[4] << 16)) | (buf[5] << 24);
					SetBitrate(bitrate);
				break;
				case 3:
					/* change mtu command */
					checksum =  buf[0] ^ buf[1] ^ buf[2] ^ buf[3] ^ buf[4] ^ buf[5];
					if (checksum != buf[6]) return;
					long mtu = buf[2] | (buf[3] << 8) | ((buf[4] << 16)) | (buf[5] << 24);
					SetMTU(mtu);
				break;
			}
		break;

	}
}

void stopAll () {

	int i;

	if (config.verbose) 
		printf("* Fail safe\n");

	switch (config.devicetype) {
		default:
		case 0:
			// RC
			for (i = 0; i < 4; i++) {
				setPPM(config.axis[i], config.ppm[config.axis[i]].zero);
			}
		break;
		case 1:
			// tank
			setPWM(config.axis[0], 0, 0);
			setPWM(config.axis[1], 0, 0);
		break;
	}

	ClientDisconnected();

}

void Move (int val1, int val2) {

	static int save_speed1, save_speed2;

	int dx = 127 - val1;
	int dy = 127 - val2;

	int speed1 = 0;
	int speed2 = 0;
	int dir1 = 0;
	int dir2 = 0;

	if (val2 > 127){
		speed1 = dy - dx;
		speed2 = dy + dx;
	} else
	if (val2 < 1127){
		speed1 = dy - dx;
		speed2 = dy + dx;
	}

	if (speed1 >= 0)
		dir1 = 1;
	else
	if (speed1 < 0)
		dir1 = 0;

	if (speed2 >= 0)
		dir2 = 1;
	else
	if (speed2 < 0)
		dir2 = 0;

	if (speed1 > 127) speed1 = 127;
	if (speed2 > 127) speed2 = 127;

	speed1 = abs(speed1) *  1190 / 127;
	speed2 = abs(speed2) *  1190 / 127;

	/* prevent command repeating */
	if (speed1 == save_speed1 && speed2 == save_speed2) return;

	save_speed1 = speed1;
	save_speed2 = speed2;

	if (dir1) {
		setPin(41, 1);
		setPin(40, 0);
	} else {
		setPin(41, 0);
		setPin(40, 1);
	}

	if (dir2) {
		setPin(39, 1);
		setPin(37, 0);
	} else {
		setPin(39, 0);
		setPin(37, 1);
	}

	if (!speed1){
		setPin(41, 0);
		setPin(40, 0);
	}

	if (!speed2){
		setPin(39, 0);
		setPin(37, 0);
	}

	setPWM(config.axis[1], speed1, 1190);
	setPWM(config.axis[0], speed2, 1190);

}
