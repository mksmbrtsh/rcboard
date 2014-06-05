ROOT=/opt1/SDK_FPV
CROSSCOMPILE?=$(ROOT)/codesourcery/arm-2013.05/bin/arm-none-linux-gnueabi-
CFLAGS=-I$(ROOT)/fs/output/staging/usr/include -I$(ROOT)/kernel -D_REENTERANT -I/usr/include/nptl -o thread -L$(ROOT)/fs/output/target/usr/lib/nptl -L$(ROOT)/fs/output/target/usr/lib -lpthread -lm -lconfuse -lssl -lcrypto -lz -lcurl

CC= $(CROSSCOMPILE)gcc

all:
	$(CC) $(CFLAGS) -o rcboard iwconfig/wlaninfo.c iwconfig/iwlib.c config.c video.c virt2real.c timers.c cmdline.c commands.c voltage.c wlanstatus.c udpclient.c rcboard.c

clean:
	rm -f rcboard
