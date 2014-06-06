#include<stdio.h>
#include<string.h>
#include<stdlib.h>
#include<arpa/inet.h>
#include<sys/socket.h>

#include "vars.h"

#define BUFLEN 512

struct sockaddr_in si_other;
int udpsocket, i, slen=sizeof(si_other);

char buf[BUFLEN];
int recv_len;

static char * remote_host;
static char * prev_remote_host;
int remote_port;

void CheckRemoteClient(char * remote_host, int remote_port);

/* UDP client function */
void * udp_client_loop (void *arg) {

    if ( (udpsocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP)) == -1)
    {
	    fprintf(stderr, "socket() failed\n");
    	exit(1);
    }

    memset((char *) &si_other, 0, sizeof(si_other));
    si_other.sin_family = AF_INET;
    si_other.sin_port = htons(config.port);

    if (inet_aton(config.server_host, &si_other.sin_addr) == 0)
    {
        fprintf(stderr, "inet_aton() failed\n");
        exit(1);
    }

	remote_host = (char*) malloc(sizeof(char)*(IPADDRMAXLEN));
	prev_remote_host = (char*) malloc(sizeof(char)*(IPADDRMAXLEN));

	/* receive commands loop */
	while(1) {

        if (recv_len = recvfrom(udpsocket, buf, BUFLEN, 0, (struct sockaddr *) &si_other, &slen) == -1) {
			fprintf(stderr, "recvfrom() failed\n");
			stopAll ();
        }

		if (recv_len > -1) {

			parseCommand(buf);

			/* get remote host:port*/
			remote_host = inet_ntoa(si_other.sin_addr);
			remote_port = ntohs(si_other.sin_port);
			CheckRemoteClient(remote_host, remote_port);
		
		}
    }

    close(udpsocket);
}

/* UDP server function */
void udp_server_loop (void * arg) {

    if ( (udpsocket = socket(AF_INET, SOCK_DGRAM, IPPROTO_UDP)) == -1)
    {
	    fprintf(stderr, "socket() failed\n");
    }
     
    /* clear structure */
    memset((char *) &si_other, 0, sizeof(si_other));
     
    si_other.sin_family = AF_INET;
    si_other.sin_port = htons(config.port);
    si_other.sin_addr.s_addr = htonl(INADDR_ANY);
     
    /* bind socket to port */
    if( bind(udpsocket , (struct sockaddr*)&si_other, sizeof(si_other) ) == -1)
    {
	    fprintf(stderr, "bind() failed\n");
    	exit(1);
    }

	remote_host = (char*) malloc(sizeof(char)*(IPADDRMAXLEN));
	prev_remote_host = (char*) malloc(sizeof(char)*(IPADDRMAXLEN));

	/* receive commands loop */
    while(1) {
         
        if (recv_len = recvfrom(udpsocket, buf, BUFLEN, 0, (struct sockaddr *) &si_other, &slen) == -1) {
			fprintf(stderr, "recvfrom() failed\n");
			stopAll();
        } 

		if (recv_len > -1) {

			parseCommand(buf);

			/* get remote host:port*/
			remote_host = inet_ntoa(si_other.sin_addr);
			remote_port = ntohs(si_other.sin_port);
			CheckRemoteClient(remote_host, remote_port);

		}
        
    }

    close(udpsocket);

}


/* send the message to UDP server or client */
void udp_client_send (char * message, int length) {

	if (sendto(udpsocket, message, length, 0, (struct sockaddr *) &si_other, slen) == -1) {
		fprintf(stderr, "sendto() failed\n");
		stopAll();
	}

}

/*  check remote client host:port 
	and maybe start video stream to him
*/
void CheckRemoteClient(char * remote_host, int remote_port) {

	if (remote_host == 0)
		return;

	/* skip 127.0.0.1 */
	if (!strcmp(remote_host, "127.0.0.1")) {
		return;
	}

	/* if address is the same */
	if (!strcmp(remote_host, prev_remote_host)) {
		if (!videoneedstart)
			return;
	}

	/* client host changed, need to restart video stream to him */
	PlayStreamTo(remote_host);

	/* save new host value*/
	strncpy(prev_remote_host, remote_host, IPADDRMAXLEN);

	videoneedstart = 0;

}


void ClientDisconnected() {

	videoneedstart = 1;
	clienttype = 0;

	/* pause video stream */
	PauseStream();

}
