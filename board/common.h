/* read ADC channel for board voltage function */
void * adc_read_loop (void *arg);

/* function for read wlan status */
void wlanstatus_read_loop (void *arg);

/* infinite loop UDP socket client function */
void * udp_client_loop (void *arg);

/* infinite loop UDP socket server function */
void * udp_server_loop (void *arg);

/* function for send message to server via UDP */
void udp_client_send (char * message, int length);
