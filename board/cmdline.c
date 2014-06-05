#include <stdio.h>
#include <stdlib.h>
#include <getopt.h>

#include "vars.h"

/* usage info */
void usage() {
	printf ("RC car control \n");
	printf ("by Gol, 2014\n");
	printf ("Usage: [-u -p] [-a] [-v] \n");
	printf ("-u\t\tremote config request user ID\n");
	printf ("-p\t\tremote config request hash\n");
	printf ("-a\t\tremote config URL\n");
	printf ("-v\t\tverbose output\n");
	printf ("-? or -h\tshow this info\n");
}


/* parse command line params */
void parse_cmdline(int argc, char * argv[]) {

	int c;

	/* parse cmdline */
	while (1) {
		int option_index = 0;
		static struct option long_options[] = { {0, 0, 0, 0} };
		c = getopt_long (argc, argv, "u:p:a:vh?", long_options, &option_index);

		if (c == -1) 
			break;

		switch (c) {

			case 'u':
				sprintf(config.remoteconfiguser, "%s", optarg);
			break;
			case 'p':
				sprintf(config.remoteconfighash, "%s", optarg);
			break;
			case 'a':
				sprintf(config.remoteconfigurl, "%s", optarg);
			break;
			case 'v':
				cmdline_verbose = 1;
			break;
			case 'h':
			case '?':
				usage();
				exit(1);
			break;
			default:
			break;
		}
	}

}
