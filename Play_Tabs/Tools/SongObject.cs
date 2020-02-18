using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Globalization;

namespace Play_Tabs.Tools
{

    public enum Arrangement
    {
        lead,
        rythm
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

        public ArrangementObject LoadArrangement(Arrangement arrangment)
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
                        if(entry.Name.Contains("songs") && entry.Name.Contains("arr") && entry.Name.EndsWith("_" + arrangment.ToString() + ".xml"))
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

            float averageTempo = float.Parse(document.GetElementsByTagName("averageTempo")[0].InnerText, CultureInfo.InvariantCulture);

            XmlNode node = document.GetElementsByTagName("transcriptionTrack")[0];
            int noteCount = int.Parse(node["notes"].GetAttribute("count"));
            int chordCount = int.Parse(node["chords"].GetAttribute("count"));

            if (noteCount <= 0 && chordCount <= 0) {
                node = document.GetElementsByTagName("level")[0];
                noteCount = int.Parse(node["notes"].GetAttribute("count"));
                chordCount = int.Parse(node["chords"].GetAttribute("count"));
            }

            ArrangementObject newArrangement = new ArrangementObject(averageTempo,
                noteCount, node["notes"].GetElementsByTagName("note"),
                chordCount, node["chords"].GetElementsByTagName("chord"));

            return newArrangement;
        }
    }

    public class ArrangementObject
    {
        public readonly float averageTempo;
        public readonly note[] notes;
        public readonly chord[] chords;

        public ArrangementObject(float averageTempo, int noteCount, XmlNodeList notes, int chordCount, XmlNodeList chords)
        {
            this.averageTempo = averageTempo;
            this.notes = new note[noteCount];
            this.chords = new chord[chordCount];

            int i = 0;
            foreach(XmlNode node in notes)
            {
                this.notes[i] = new note(node);
                i++;
            }

            i = 0;
            foreach(XmlNode node in chords)
            {
                this.chords[i] = new chord(node);
                i++;
            }
        }
    }

    public struct chord
    {
        public readonly note[] chordNotes;
        public readonly float time;
        public readonly bool linkNext;
        public readonly bool accent;
        public readonly bool fretHandMute;
        public readonly bool highDensity;
        public readonly bool ignore;
        public readonly bool palmMute;
        public readonly bool hopo;
        public readonly bool strumDown;

        public chord(XmlNode node)
        {

            time = float.Parse(node.Attributes["time"].Value, CultureInfo.InvariantCulture);
            linkNext = node.Attributes["linkNext"].Value == "1";
            accent = node.Attributes["accent"].Value == "1";
            fretHandMute = node.Attributes["fretHandMute"].Value == "1";
            highDensity = node.Attributes["highDensity"].Value == "1";
            ignore = node.Attributes["ignore"].Value == "1";
            palmMute = node.Attributes["palmMute"].Value == "1";
            hopo = node.Attributes["hopo"].Value == "1";
            strumDown = node.Attributes["strum"].Value == "down";

            XmlNodeList chordNotesNode = node.SelectNodes("chordNote");
            chordNotes = new note[chordNotesNode.Count];
            int i = 0;
            foreach(XmlNode chordNote in chordNotesNode)
            {
                chordNotes[i] = new note(chordNote);
                i++;
            }
        }
    }

    public struct note
    {
        public readonly float time;
        public readonly bool linkNext;
        public readonly bool accent;
        public readonly bool bend;
        public readonly bendValue[] bendValues;
        public readonly byte fret;
        public readonly bool hammerOn;
        public readonly bool harmonic;
        public readonly bool hopo; //??
        public readonly bool ignore; //??
        public readonly sbyte leftHand; //This number represents which finger to place on this note
        public readonly bool mute;
        public readonly bool palmMute;
        public readonly sbyte pluck; //??
        public readonly bool pullOff;
        public readonly sbyte slap; //??
        public readonly sbyte slideTo;
        public readonly byte guitarString;
        public readonly float sustain;
        public readonly bool tremolo;
        public readonly bool harmonicPinch;
        public readonly bool pickDirection; //??
        public readonly sbyte rightHand; //??
        public readonly sbyte slideUnpitchTo; //??
        public readonly bool tap;
        public readonly short vibrato;

        public note(XmlNode node)
        {
            time = float.Parse(node.Attributes["time"].Value, CultureInfo.InvariantCulture);
            linkNext = node.Attributes["linkNext"].Value == "1";
            accent = node.Attributes["accent"].Value == "1";
            fret = byte.Parse(node.Attributes["fret"].Value);
            hammerOn = node.Attributes["hammerOn"].Value == "1";
            harmonic = node.Attributes["harmonic"].Value == "1";
            hopo = node.Attributes["hopo"].Value == "1";
            ignore = node.Attributes["ignore"].Value == "1";
            leftHand = sbyte.Parse(node.Attributes["leftHand"].Value);
            mute = node.Attributes["mute"].Value == "1";
            palmMute = node.Attributes["palmMute"].Value == "1";
            pluck = sbyte.Parse(node.Attributes["pluck"].Value);
            pullOff = node.Attributes["pullOff"].Value == "1";
            slap = sbyte.Parse(node.Attributes["slap"].Value);
            slideTo = sbyte.Parse(node.Attributes["slideTo"].Value);
            guitarString = byte.Parse(node.Attributes["string"].Value);
            sustain = float.Parse(node.Attributes["sustain"].Value, CultureInfo.InvariantCulture);
            tremolo = node.Attributes["tremolo"].Value == "1";
            harmonicPinch = node.Attributes["harmonicPinch"].Value == "1";
            pickDirection = node.Attributes["pickDirection"].Value == "1";
            rightHand = sbyte.Parse(node.Attributes["rightHand"].Value);
            slideUnpitchTo = sbyte.Parse(node.Attributes["slideUnpitchTo"].Value);
            tap = node.Attributes["tap"].Value == "1";
            vibrato = short.Parse(node.Attributes["vibrato"].Value);

            bend = node.Attributes["bend"].Value == "1";
            bendValues = null;
            if(bend)
            {
                XmlNode bendNode = node.SelectSingleNode("bendValues");
                bendValues = new bendValue[int.Parse(bendNode.Attributes["count"].Value)];
                int i = 0;
                foreach (XmlNode n in bendNode.ChildNodes)
                {
                    bendValues[i] = new bendValue(float.Parse(n.Attributes["time"].Value, CultureInfo.InvariantCulture), float.Parse(n.Attributes["step"].Value, CultureInfo.InvariantCulture));
                    i++;
                }
            }
        }
    }

    public struct bendValue
    {
        public readonly float time;
        public readonly float step;

        public bendValue(float time, float step)
        {
            this.time = time;
            this.step = step;
        }
    }
}
