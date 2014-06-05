#include <signal.h>
#include <sys/time.h>
#include <time.h>

#include "vars.h"

struct itimerval timer;

void set_alive_timer (long period);

void alive_timer_handler (int signum) {

	long delta;
	struct timespec time;

	if (aliveTimestamp > 0) {

		/* check alive time delta */
		clock_gettime(CLOCK_MONOTONIC, &time);
	   	delta = time.tv_sec - aliveTimestamp;

		if (delta >= config.alive_maxtimeout) {
			aliveTimestamp = 0;
			stopAll();
		}

	}

	/* start check alive timer */
	set_alive_timer (ALIVETIMERPERIOD);

}


/* set alive timer */
void set_alive_timer (long period) {

	timer.it_interval.tv_sec = 0;
	timer.it_interval.tv_usec = period;
	timer.it_value.tv_sec = 0;
	timer.it_value.tv_usec = period;
	setitimer(ITIMER_REAL, &timer, 0);
	signal(SIGALRM, alive_timer_handler);
}

