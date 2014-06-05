#define IPADDRMAXLEN 512
#define REQUESTMAXLEN 512

#define ALIVETIMERPERIOD	300000
#define STATUS_DELAY	3000000

#define CON_WIFI	1
#define CON_YOTA	2
#define CON_LAN	3
#define CON_UBIQ	4

typedef struct {

	int con;

	int min;
	int max;

	int ppm_period;
	int ppm_min;
	int ppm_max;
	int ppm_delta;

	int zero;
	
} PPM;

typedef struct {

	/* remote config request user */
	char remoteconfiguser [REQUESTMAXLEN];	

	/* remote config request hash */
	char remoteconfighash [REQUESTMAXLEN];	

	/* remote config URL */
	char remoteconfigurl [REQUESTMAXLEN];

	/* client/server type 	= 0 - server = 1 - client */
	int role;

	/* server host */
	char server_host [IPADDRMAXLEN];

	/* server port */
	int port;

	/* verbose flag */
	int verbose;

	/* board VREF voltage */
	double vref;

	/* max alive timeout */
	int alive_maxtimeout;

	/* WLAN type for send signal status */
	int wlantype;

	/* Wlan interface (only for Wi-fi link) */
	char wifi_iface [IPADDRMAXLEN];

	/* Yota gateway ip (only for Yota link) */
	char yota_status [IPADDRMAXLEN];

	PPM ppm[4];

	int axis[4];

} Config;

Config config;


/* alive timestamp */
long aliveTimestamp;

/* board voltage */
double system_voltage;

/* video need start flag */
int videoneedstart;


/* temp variable for override config file verbose param */
int cmdline_verbose;

#define WLANNAMEMAXLEN		20
#define WLANMESSAGEMAXLEN	100
typedef struct {

	char type;
	char isconnected;
	char message [WLANMESSAGEMAXLEN];

} WlanStatus;

WlanStatus wlanstatus;


/* wlan params (for WiFi status)*/
typedef struct iface_info {
	char ssid[20];
	char freq[30];
	char link[50];
} iface_info;

iface_info ifaceinfo;
