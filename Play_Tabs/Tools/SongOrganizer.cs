using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Models;
using Microsoft.Xna.Framework.Graphics;

namespace Play_Tabs.Tools
{
    class SongOrganizer
    {

        public static List<SongObject> songObjects = new List<SongObject>();
        private const string CONTENT_PATH = "Custom Songs";
        private static SpotifyWebAPI spotify;
        public static Dictionary<string, AlbumImage> albumImages = new Dictionary<string, AlbumImage>();

        /// <summary>
        /// Load all the songs from our songs folder into an list of <c>SongObject</c>
        /// </summary>
        public async static void Initialize(GraphicsDevice graphics)
        {

            if(!Directory.Exists(CONTENT_PATH)) {
                Directory.CreateDirectory(CONTENT_PATH);
            }

            CredentialsAuth auth = new CredentialsAuth("e1c3ce28971a4396bd46eba97d14c271", "951448c26c5544ce8af238bdda3277d8");
            Token token = await auth.GetToken();
            spotify = new SpotifyWebAPI()
            {
                AccessToken = token.AccessToken,
                TokenType = token.TokenType
            };

            foreach (string song in Directory.EnumerateFiles(CONTENT_PATH, "*_p.psarc", SearchOption.AllDirectories))
            {
                using(var inputStream = File.OpenRead(song))
                {
                    SongObject newSong = UnpackArchive(song, inputStream, graphics);
                    if (newSong != null) {
                        songObjects.Add(newSong);
                    }
                }
            }
        }

        private static SongObject UnpackArchive(string archivePath, Stream inputStream, GraphicsDevice graphics)
        {
            var psarc = new PlayStationArchive();
            psarc.Read(inputStream, true);
            SongObject newSong = new SongObject(archivePath);

            try
            {
                // InflateEntries - compatible with RS1 and RS2014 files
                foreach (var entry in psarc.TOC)
                {
                    if (entry.Name.Contains("manifests") && entry.Name.EndsWith(".hsan"))
                    {
                        psarc.InflateEntry(entry);
                        using (var reader = new StreamReader(entry.Data, Encoding.UTF8))
                        {
                            JProperty json = JObject.Parse(reader.ReadToEnd()).Property("Entries");

                            JContainer lead = FindArrangment("Lead", json);
                            JContainer rhythm = FindArrangment("Rhythm", json);
                            
                            if(lead == null && rhythm == null) {
                                newSong = null;
                                break;
                            }

                            foreach (JProperty property in lead.Values())
                            {
                                if (property.Name.Equals("AlbumName"))
                                {
                                    newSong.album = property.Value.ToString();
                                }
                                else if (property.Name.Equals("ArtistName"))
                                {
                                    newSong.artist = property.Value.ToString();
                                }
                                else if (property.Name.Equals("SongLength"))
                                {
                                    float seconds;
                                    bool success = float.TryParse(property.Value.ToString(), out seconds);
                                    newSong.length = seconds;
                                }
                                else if (property.Name.Equals("SongName"))
                                {
                                    newSong.title = property.Value.ToString();
                                }
                                else if (property.Name.Equals("SongYear"))
                                {
                                    newSong.year = property.Value.ToString();
                                }
                                else if (property.Name.Equals("Tuning"))
                                {
                                    int count = 0;
                                    newSong.tuningLead = new sbyte[6];
                                    foreach (JProperty attribute in property.Values())
                                    {
                                        newSong.tuningLead[count] = sbyte.Parse(attribute.Value.ToString());
                                        count++;
                                    }
                                }
                            }

                            foreach (JProperty property in rhythm.Values())
                            {
                                if (property.Name.Equals("Tuning"))
                                {
                                    int count = 0;
                                    newSong.tuningRhythm = new sbyte[6];
                                    foreach (JProperty attribute in property.Values())
                                    {
                                        newSong.tuningRhythm[count] = sbyte.Parse(attribute.Value.ToString());
                                        count++;
                                    }
                                }
                            }
                        }
                    }
                    // Close
                    if (entry.Data != null)
                        entry.Data.Dispose();
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

            if (newSong != null && !albumImages.ContainsKey(newSong.artist + "+" + newSong.album)) {
                albumImages.Add(newSong.artist + "+" + newSong.album, new AlbumImage(spotify, newSong.artist + "+" + newSong.album, graphics));
            }
            return newSong;
        }

        private static JContainer FindArrangment(string arrangment, JProperty property)
        {
            foreach (JToken token in property.Values())
            {
                foreach (JProperty attribute in token.Values("Attributes").Values())
                {
                    if (attribute.Name.Equals("ArrangementName") && attribute.Value.ToString().Equals(arrangment))
                    {
                        return attribute.Parent.Parent;
                    }
                }
            }
            return null;
        }
    }
}
