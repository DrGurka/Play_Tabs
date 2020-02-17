using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace Play_Tabs.Tools
{

    public enum Arrangment
    {
        _lead,
        _rythm
    }

    public class SongObject
    {
        public string album;
        public string artist;
        public float length;
        public string title;
        public string year;
        public sbyte[] tuningLead;
        public sbyte[] tuningRhythm;
        public string source;

        public SongObject(string source)
        {
            this.source = source;
        }

        public string GetLength()
        {
            return TimeSpan.FromSeconds(length).ToString(@"mm\:ss");
        }

        public ArrangmentObject LoadArrangment(Arrangment arrangment)
        {
            XmlDocument document = new XmlDocument();
            using(var inputStream = File.OpenRead(source))
            {
                var psarc = new PlayStationArchive();
                psarc.Read(inputStream, true);

                try
                {
                    foreach (var entry in psarc.TOC)
                    {
                        if(entry.Name.Contains("songs") && entry.Name.Contains("arr") && entry.Name.EndsWith(arrangment.ToString() + ".xml"))
                        {
                            psarc.InflateEntry(entry);
                            using(var reader = new StreamReader(entry.Data, Encoding.UTF8))
                            {
                                document.LoadXml(reader.ReadToEnd());
                            }
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    if (psarc != null)
                    {
                        psarc.Dispose();
                        psarc = null;
                    }
                }
            }

            XmlNode node = document.GetElementsByTagName("transcriptionTrack")[0];
            

            return null;
        }
    }

    public class ArrangmentObject
    {
        public readonly float averageTempo;
        public readonly float startBeat;
        public readonly ebeat[] ebeats;
        public readonly note[] notes;
    }

    public struct ebeat
    {
        public readonly float time;
        public readonly short measure;

        public ebeat(float time, short measure)
        {
            this.time = time;
            this.measure = measure;
        }
    }

    public struct note
    {
        public readonly float time;
        /*public readonly bool linkNext;
        public readonly bool accent;
        public readonly bool bend;
        public readonly bendValue[] bendValues;
        public readonly byte fret;
        public readonly bool hammerOn;
        public readonly bool harmonic;
        public readonly bool hopo; //??
        public readonly bool ignore; //??
        public readonly sbyte leftHand; //??
        public readonly bool mute;
        public readonly bool palmMute;
        public readonly sbyte pluck; //??
        public readonly bool pullOff;
        public readonly sbyte slap; //??
        public readonly sbyte slideTo;
        public readonly sbyte guitarString;
        public readonly float sustain;
        public readonly bool tremolo;
        public readonly bool harmonicPinch;
        public readonly bool pickDirection; //??
        public readonly short rightHand; //??
        public readonly sbyte slideUnpitchTo; //??
        public readonly bool tap;
        public readonly short vibrato;*/

        public note(XmlNode note)
        {
            time = float.Parse(note.Attributes["time"].Value);
        }
    }

    public struct bendValue
    {
        public readonly float time;
        public readonly float step;
    }
}
