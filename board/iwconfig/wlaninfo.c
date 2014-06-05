/*
 *  based on
 *
 *	Wireless Tools
 *
 *		Jean II - HPLB 97->99 - HPL 99->07
 *
 * Main code for "iwconfig". This is the generic tool for most
 * manipulations...
 * You need to link this code against "iwlib.c" and "-lm".
 *
 * This file is released under the GPL license.
 *     Copyright (c) 1997-2007 Jean Tourrilhes <jt@hpl.hp.com>
 */

#include "iwlib.h"		/* Header */

#include "../vars.h"		/* Header */

static int get_info(int skfd, char * ifname, struct wireless_info *	info) {
  struct iwreq		wrq;

  memset((char *) info, 0, sizeof(struct wireless_info));

  /* Get basic information */
  if(iw_get_basic_config(skfd, ifname, &(info->b)) < 0)
    {
      /* If no wireless name : no wireless extensions */
      /* But let's check if the interface exists at all */
      struct ifreq ifr;

      strncpy(ifr.ifr_name, ifname, IFNAMSIZ);
      if(ioctl(skfd, SIOCGIFFLAGS, &ifr) < 0)
	return(-ENODEV);
      else
	return(-ENOTSUP);
    }

  /* Get ranges */
  if(iw_get_range_info(skfd, ifname, &(info->range)) >= 0)
    info->has_range = 1;

  /* Get AP address */
  if(iw_get_ext(skfd, ifname, SIOCGIWAP, &wrq) >= 0)
    {
      info->has_ap_addr = 1;
      memcpy(&(info->ap_addr), &(wrq.u.ap_addr), sizeof (sockaddr));
    }

  /* Get bit rate */
  if(iw_get_ext(skfd, ifname, SIOCGIWRATE, &wrq) >= 0)
    {
      info->has_bitrate = 1;
      memcpy(&(info->bitrate), &(wrq.u.bitrate), sizeof(iwparam));
    }

  /* Get Power Management settings */
  wrq.u.power.flags = 0;
  if(iw_get_ext(skfd, ifname, SIOCGIWPOWER, &wrq) >= 0)
    {
      info->has_power = 1;
      memcpy(&(info->power), &(wrq.u.power), sizeof(iwparam));
    }

  /* Get stats */
  if(iw_get_stats(skfd, ifname, &(info->stats),
		  &info->range, info->has_range) >= 0)
    {
      info->has_stats = 1;
    }

#ifndef WE_ESSENTIAL
  /* Get NickName */
  wrq.u.essid.pointer = (caddr_t) info->nickname;
  wrq.u.essid.length = IW_ESSID_MAX_SIZE + 1;
  wrq.u.essid.flags = 0;
  if(iw_get_ext(skfd, ifname, SIOCGIWNICKN, &wrq) >= 0)
    if(wrq.u.data.length > 1)
      info->has_nickname = 1;

  if((info->has_range) && (info->range.we_version_compiled > 9))
    {
      /* Get Transmit Power */
      if(iw_get_ext(skfd, ifname, SIOCGIWTXPOW, &wrq) >= 0)
	{
	  info->has_txpower = 1;
	  memcpy(&(info->txpower), &(wrq.u.txpower), sizeof(iwparam));
	}
    }

  /* Get sensitivity */
  if(iw_get_ext(skfd, ifname, SIOCGIWSENS, &wrq) >= 0)
    {
      info->has_sens = 1;
      memcpy(&(info->sens), &(wrq.u.sens), sizeof(iwparam));
    }

  if((info->has_range) && (info->range.we_version_compiled > 10))
    {
      /* Get retry limit/lifetime */
      if(iw_get_ext(skfd, ifname, SIOCGIWRETRY, &wrq) >= 0)
	{
	  info->has_retry = 1;
	  memcpy(&(info->retry), &(wrq.u.retry), sizeof(iwparam));
	}
    }

  /* Get RTS threshold */
  if(iw_get_ext(skfd, ifname, SIOCGIWRTS, &wrq) >= 0)
    {
      info->has_rts = 1;
      memcpy(&(info->rts), &(wrq.u.rts), sizeof(iwparam));
    }

  /* Get fragmentation threshold */
  if(iw_get_ext(skfd, ifname, SIOCGIWFRAG, &wrq) >= 0)
    {
      info->has_frag = 1;
      memcpy(&(info->frag), &(wrq.u.frag), sizeof(iwparam));
    }
#endif	/* WE_ESSENTIAL */

  return(0);
}

/*------------------------------------------------------------------*/
/*
 * Print on the screen in a neat fashion all the info we have collected
 * on a device.
 */
static void display_info(struct wireless_info *	info, char * ifname)
{
  char	buffer[128];	/* Temporary buffer */

  /* Display ESSID (extended network), if any */
  if(info->b.has_essid)
    {
      if(info->b.essid_on)
	{
	  /* Does it have an ESSID index ? */
	  if((info->b.essid_on & IW_ENCODE_INDEX) > 1)
	    sprintf(ifaceinfo.ssid, "\"%s\"", info->b.essid);
	  else
	    sprintf(ifaceinfo.ssid, "\"%s\"", info->b.essid);
	}
      else
	sprintf(ifaceinfo.ssid, "");
    }

  /* Display frequency / channel */
  if(info->b.has_freq)
    {
      double		freq = info->b.freq;	/* Frequency/channel */
      int		channel = -1;		/* Converted to channel */
      /* Some drivers insist of returning channel instead of frequency.
       * This fixes them up. Note that, driver should still return
       * frequency, because other tools depend on it. */
      if(info->has_range && (freq < KILO))
	channel = iw_channel_to_freq((int) freq, &freq, &info->range);
      /* Display */
      iw_print_freq(buffer, sizeof(buffer), freq, -1, info->b.freq_flags);
      sprintf(ifaceinfo.freq, "%s", buffer);
    }

  /* Display statistics */
  if(info->has_stats)
    {
      iw_print_stats(buffer, sizeof(buffer), &info->stats.qual, &info->range, info->has_range);
      sprintf(ifaceinfo.link,"%s", buffer);
    }
}


int getwlaninfo(char * ifacename) {

	int skfd; /* generic raw socket desc.	*/

	/* Create a channel to the NET kernel. */
	if((skfd = iw_sockets_open()) < 0) {
		// error open socket
    }

	struct wireless_info info;
	int rc = get_info(skfd, ifacename, &info);
	display_info(&info, ifacename);

	/* Close the socket. */
	iw_sockets_close(skfd);
}
