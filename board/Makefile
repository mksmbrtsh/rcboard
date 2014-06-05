CFLAGS+= -D_REENTERANT 
LDFLAGS+=-L$(TARGET_DIR)/usr/lib/nptl -L$(TARGET_DIR)/usr/lib -lpthread -lm -lconfuse -lssl -lcrypto -lz -lcurl

all:
	$(CC) $(CFLAGS) $(LDFLAGS) -o rcboard iwconfig/wlaninfo.c iwconfig/iwlib.c config.c video.c virt2real.c timers.c cmdline.c commands.c voltage.c wlanstatus.c udpclient.c rcboard.c

clean:
	rm -f rcboard
