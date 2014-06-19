using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Threading;

using Gst;
using Gst.GLib;
using Gst.BasePlugins;

namespace v2r_player
{
    public class Videostream
    {
        Pipeline m_Pipeline;
        public Element m_Source, m_Sink, m_Queue1, m_Queue2, m_Gstrtpjitterbuffer, m_Rtph264depay, m_Ffdec_h264, m_Tee, m_H264parse, m_Matroskamux, m_Filesink;
        Element m_Ffmpegcolorspace0, m_Ffmpegcolorspace1, m_Effect0;

        IntPtr VideoWindowHandle;

        public Overlay overlay;
        
        int udp_port;

        String message;
        public Boolean ready;
        public Boolean record_state;

        private static DispatcherTimer Timer;

        public Videostream(int port)
        {
            udp_port = port;
            record_state = false;
            ready = true;

            Timer = new DispatcherTimer();
            Timer.Tick += new EventHandler(Timer_Tick);
            Timer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            Timer.Start();

        }

        public void CreatePipeline()
        {

            message = "";

            try
            {
                m_Pipeline = new Pipeline();
            }
            catch (Exception err)
            {
                ready = false;
                message += "Не могу создать pipeline\n";
            }



            /* common source */
            try
            {
                m_Source = Gst.Parse.Launch("udpsrc port=3000 do-timestamp=true typefind=false");
                if (Status.blocksize > 0) m_Source["blocksize"] = Status.blocksize;
                if (Status.port > 0) m_Source["port"] = udp_port;
                if (Status.udp_buffer_size > 0) m_Source["buffer-size"] = Status.udp_buffer_size;
                m_Source["do-timestamp"] = Status.do_timestamp;
                m_Source["caps"] = Gst.Caps.FromString("application/x-rtp,media=(string)video,clock-rate=(int)90000,encoding-name=(string)H264,sprop-parameter-sets=(string)\"Z2QAM62EBUViuKxUdCAqKxXFYqOhAVFYrisVHQgKisVxWKjoQFRWK4rFR0ICorFcVio6ECSFITk8nyfk/k/J8nm5s00IEkKQnJ5Pk/J/J+T5PNzZprQCgC3IAA\\=\\=\\,aO48sAA\\=\",payload=(int)96,ssrc=(uint)2838991767,clock-base=(uint)1459676256,seqnum-base=(uint)6120");
            }
            catch (Exception err)
            {
                ready = false;
                message += "Не могу создать udpsrc\n";
            }

            /* end common source */



            /* for view */

            try
            {
                m_Rtph264depay = Gst.ElementFactory.Make("rtph264depay", "rtph264depay");
                m_Rtph264depay["byte-stream"] = true;
            }
            catch (Exception err)
            {
                ready = false;
                message += "Не могу создать rtph264depay\n";
            }

            try
            {
                m_Ffdec_h264 = Gst.ElementFactory.Make("ffdec_h264");
                m_Ffdec_h264["do-padding"] = false;
                m_Ffdec_h264["direct-rendering"] = true;                
            }
            catch (Exception err)
            {
                ready = false;
                message += "Не могу создать ffdec_h264\n";
            }


            try
            {
                m_Gstrtpjitterbuffer = Gst.Parse.Launch("gstrtpjitterbuffer");
                m_Gstrtpjitterbuffer["latency"] = Status.latency;
                m_Gstrtpjitterbuffer["drop-on-latency"] = Status.drop;
                m_Gstrtpjitterbuffer["mode"] = Status.mode;
            }
            catch (Exception err)
            {
                ready = false;
                message += "Не могу создать gstrtpjitterbuffer\n";
            }

            try
            {
                m_Ffmpegcolorspace0 = Gst.Parse.Launch("ffmpegcolorspace");
                m_Ffmpegcolorspace0["qos"] = false;
            }
            catch (Exception err)
            {
                ready = false;
                message += "Не могу создать ffmpegcolorspace\n";
            }
            

            /* check effects for streams */
            try
            {
                if (Status.effect.Length > 0)
                    m_Effect0 = Gst.Parse.Launch(Status.effect);
            }
            catch (Exception err)
            {
                ready = false;
                message += "Не могу создать effects\n";
            }

            try
            {
                m_Ffmpegcolorspace1 = Gst.Parse.Launch("ffmpegcolorspace");
                m_Ffmpegcolorspace1["qos"] = false;
            }
            catch (Exception err)
            {
                ready = false;
                message += "Не могу создать второй ffmpegcolorspace\n";
            }
            
            /* end for view */

            /* for recording */
            try
            {
                m_Tee = Gst.Parse.Launch("tee");
            }
            catch (Exception err)
            {
                ready = false;
                message += "Не могу создать tee\n";
            }

            try
            {
                m_Queue1 = Gst.Parse.Launch("queue max-size-buffers=0 max-size-bytes=0 max-size-time=0");
            }
            catch (Exception err)
            {
                ready = false;
                message += "Не могу создать queue\n";
            }

            try
            {
                m_H264parse = Gst.Parse.Launch("h264parse");
            }
            catch (Exception err)
            {
                ready = false;
                message += "Не могу создать h264parse\n";
            }

            try
            {
                m_Queue2 = Gst.Parse.Launch("queue max-size-buffers=0 max-size-bytes=0 max-size-time=0");
            }
            catch (Exception err)
            {
                ready = false;
                message += "Не могу создать вторую queue\n";
            }

            try
            {
                m_Matroskamux = Gst.Parse.Launch("matroskamux streamable=true");
            }
            catch (Exception err)
            {
                ready = false;
                message += "Не могу создать matroskamux\n";
            }

            try
            {
                m_Filesink = Gst.Parse.Launch("filesink sync=false");
                m_Filesink["buffer-mode"] = 4;
            }
            catch (Exception err)
            {
                ready = false;
                message += "Не могу создать filesink\n";
            }
            
            /* end for recording */

            /* common videosink */

            try
            {
                m_Sink = Gst.Parse.Launch("d3dvideosink sync=false show-preroll-frame=false qos=false enable-last-buffer=false") as Gst.Video.VideoSink;
                m_Sink["force-aspect-ratio"] = Status.forceaspectratio;
                m_Sink["blocksize"] = (uint)Status.blocksize;
            }
            catch (Exception err)
            {
                ready = false;
                message += "Не могу создать d3dvideosink\n";
            }

            /* end common videosink */

            /* create Pipeline for view only */
            try
            {
                m_Pipeline.Add(m_Source);
                m_Pipeline.Add(m_Gstrtpjitterbuffer);
                m_Pipeline.Add(m_Rtph264depay);
                m_Pipeline.Add(m_Tee);
                m_Pipeline.Add(m_Ffdec_h264);
                //m_Pipeline.Add(m_Ffmpegcolorspace0); // for dshowvideosink
                m_Pipeline.Add(m_Sink);

                /* add Pipeline elements for record */
                m_Pipeline.Add(m_Queue1);
                m_Pipeline.Add(m_H264parse);
                m_Pipeline.Add(m_Queue2);
                m_Pipeline.Add(m_Queue1);
                m_Pipeline.Add(m_Matroskamux);

                /* link elements for view only */
                m_Source.Link(m_Gstrtpjitterbuffer);
                m_Gstrtpjitterbuffer.Link(m_Rtph264depay);
                m_Rtph264depay.Link(m_Tee);
                m_Tee.Link(m_Ffdec_h264);

                /* if effect needed - add them into pipeline */
                if (Status.effect.Length > 0)
                {

                    m_Pipeline.Add(m_Ffmpegcolorspace0);
                    m_Pipeline.Add(m_Effect0);
                    m_Pipeline.Add(m_Ffmpegcolorspace1);

                    m_Ffdec_h264.Link(m_Ffmpegcolorspace0);
                    m_Ffmpegcolorspace0.Link(m_Effect0);
                    m_Effect0.Link(m_Ffmpegcolorspace1);
                    m_Ffmpegcolorspace1.Link(m_Sink);
                }
                else
                {
                    m_Ffdec_h264.Link(m_Sink);
                }

            }
            catch (Exception err)
            {
                ready = false;
                message += "Не могу добавить элементы в pipeline\n";
            }

            if (!ready)
                MessageBox.Show(message);
        
        }


        public void SetRecord(Boolean state)
        {
            if (state)
            {
                /* turn on record */

                if (record_state) return;
                record_state = true;

                SetState(State.Null);

                /* create new filename */
                String hour = DateTime.Now.Hour.ToString();
                String minute = DateTime.Now.Minute.ToString();
                String second = DateTime.Now.Second.ToString();

                hour = hour.Length < 2 ? "0" + hour : hour;
                minute = minute.Length < 2 ? "0" + minute : minute;
                second = second.Length < 2 ? "0" + second : second;

                FileParts file = new FileParts(Status.savefilename);

                m_Filesink["location"] = file.GetPath() + file.GetName() + hour + minute + second + ".mkv";

                /* add Pipeline elements */
                m_Pipeline.Add(m_Filesink);
                
                /* add link elements */

                /* tee 2 (record) */
                m_Tee.Link(m_H264parse);
                m_H264parse.Link(m_Queue1);
                m_Queue1.Link(m_Matroskamux);
                m_Matroskamux.Link(m_Filesink);

                SetState(State.Playing);

                overlay.ShowRec(true, m_Filesink["location"].ToString());

            }
            else
            {
                /* turn off record */

                if (!record_state) return;
                record_state = false;

                SetState(State.Null);
                
                /* remove tee 2 (record) */
                m_Tee.Unlink(m_H264parse);
                m_H264parse.Unlink(m_Queue1);
                m_Queue1.Unlink(m_Matroskamux);
                m_Matroskamux.Unlink(m_Filesink);

                /* remove all Pipeline elements */
                m_Pipeline.Remove(m_Source);
                m_Pipeline.Remove(m_Gstrtpjitterbuffer);
                m_Pipeline.Remove(m_Rtph264depay);
                m_Pipeline.Remove(m_Tee);
                m_Pipeline.Remove(m_Queue1);
                m_Pipeline.Remove(m_Ffdec_h264);
                m_Pipeline.Remove(m_Sink);
                m_Pipeline.Remove(m_H264parse);
                m_Pipeline.Remove(m_Queue1);
                m_Pipeline.Remove(m_Matroskamux);
                m_Pipeline.Remove(m_Filesink);

                /* destroy old pipeline object */
                m_Pipeline = null;

                /* create new the same pipeline */
                CreatePipeline();

                /* resume playing */
                SetState(State.Playing);

                overlay.ShowRec(false, "");

            }

        }


        public void SetState(Gst.State state)
        {
            if (state == State.Playing)
                SetOutput(VideoWindowHandle);

            m_Pipeline.SetState(state);
        }

        public Gst.State GetState()
        {
            Gst.State state = new Gst.State();
            m_Pipeline.GetState(out state, Clock.TimeNone);
            return state;
        }

        public void SetOutput(IntPtr hwnd)
        {
            VideoWindowHandle = hwnd;
            var overlay = new Gst.Interfaces.XOverlayAdapter(m_Sink.Handle);
            overlay.XwindowId = (ulong)hwnd;
        }

        public void SetForceSeAspectRatio(Boolean value )
        {
            m_Sink["force-aspect-ratio"] = value;
        }

        public int GetCurrentPort()
        {
            return Convert.ToInt32(m_Source["port"]);
        }

        public void SetCurrentPort(int value)
        {
            SetState(State.Null);

            m_Source["port"] = value;
            
            SetState(State.Playing);
        }

        public int GetCurrentBuffer()
        {
            return Convert.ToInt32(m_Source["buffer-size"]);
        }

        public void SetCurrentBuffer(int value)
        {
            SetState(State.Null);

            m_Source["buffer-size"] = (int)value;

            SetState(State.Playing);
        }

        public int GetCurrentLatency()
        {
            return Convert.ToInt32(m_Gstrtpjitterbuffer["latency"]);
        }

        public void SetCurrentLatency(uint value)
        {
            m_Gstrtpjitterbuffer["latency"] = value;
        }

        public void SetCurrentDropFrames(bool value)
        {
            m_Gstrtpjitterbuffer["drop-on-latency"] = value;
        }

        public bool GetCurrentDropFrames()
        {
            return Convert.ToInt32(m_Gstrtpjitterbuffer["drop-on-latency"]) == 1 ? true : false;
        }

        public void SetCurrentDoTimestamp(bool value)
        {

            SetState(State.Null);

            m_Pipeline.Dispose();

            /* create new the same pipeline */
            CreatePipeline();

            m_Source["do-timestamp"] = value;

            /* resume playing */
            SetState(State.Playing);

        }

        
        public bool GetCurrentDoTimestamp()
        {
            return Convert.ToInt32(m_Source["do-timestamp"]) == 1 ? true : false;
        }


        public int GetJitterbufferPercent()
        {
            return Convert.ToInt32(m_Gstrtpjitterbuffer["percent"]);
        }

        public void SetJitterbufferMode(int value)
        {
            m_Gstrtpjitterbuffer["mode"] = value;
        }


        public void SetBlocksize(uint value)
        {
            m_Source["blocksize"] = value;
        }


        private void Timer_Tick(object sender, EventArgs e)
        {
            if (overlay != null)
                overlay.ShowBuffer(GetJitterbufferPercent());
        }


    }
}
