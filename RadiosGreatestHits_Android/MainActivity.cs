﻿using System;
using System.Timers;
using System.Xml;
using Android.App;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Xamarin.Essentials;

namespace RadiosGreatestHits_Android
{
    [Activity(LaunchMode = Android.Content.PM.LaunchMode.SingleTop, Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        string radioURL = "http://radiosgreatesthits.out.airtime.pro:8000/radiosgreatesthits_a";
        string trackInfoURL = "http://radiosgreatesthits.out.airtime.pro:8000/radiosgreatesthits_a.xspf";
        string currentSongTitle = "";

        MediaPlayer player;

        Button btnPlay;
        Button btnStop;
        Button btnMute;

        TextView lblNowPlaying;

        string iconPlay = "\uf144";
        string iconPause = "\uf28b";
        string iconMute = "\uf6a9";
        string iconStop = "\uf04d";
        string iconVolume = "\uf028";

        int currentStreamVolume = 0;

        public System.Timers.Timer timer;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            btnPlay = FindViewById<Button>(Resource.Id.btnPlay);
            btnStop = FindViewById<Button>(Resource.Id.btnStop);
            btnMute = FindViewById<Button>(Resource.Id.btnMute);

            lblNowPlaying = FindViewById<TextView>(Resource.Id.lblNowPlaying);

            btnPlay.Click += BtnPlay_Click;
            btnStop.Click += BtnStop_Click;
            btnMute.Click += BtnMute_Click;

            var FARegular = Typeface.CreateFromAsset(Application.Assets, "FontAwesome5FreeRegular.otf");
            var FASolid = Typeface.CreateFromAsset(Application.Assets, "FontAwesome5FreeSolid.otf");
            var FABrands = Typeface.CreateFromAsset(Application.Assets, "FontAwesome5BrandsRegular.otf");

            btnPlay.Typeface = FARegular;
            btnPlay.Text = iconPlay;


            btnStop.Typeface = FASolid;
            btnStop.Text = iconStop;


            btnMute.Typeface = FASolid;
            btnMute.Text = iconVolume;

        }

        public override void OnBackPressed()
        {
            //base.OnBackPressed();

            
        }

        protected override void OnResume()
        {
            base.OnResume();

            if(player != null)
            {
                if (player.IsPlaying)
                {
                    StartTimer();

                    System.Diagnostics.Debug.WriteLine("****************** STARTING ******************");
                }
            }
        }

        protected override void OnPause()
        {
            base.OnPause();

            var p = player == null;

            System.Diagnostics.Debug.WriteLine("****************** STOPPING ******************");

            StopTimer();
        }

        private void Player_Prepared(object sender, EventArgs e)
        {
            player.Start();
            btnPlay.Text = iconPause;
            StartTimer();
        }

        private void BtnPlay_Click(object sender, EventArgs e)
        {
            if (player == null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    lblNowPlaying.Text = "Buffering, please wait...";
                });
                

                player = new MediaPlayer();

                player.Prepared += Player_Prepared;

                currentStreamVolume = GetAudioManager().GetStreamVolume(Stream.Music);

                player.SetDataSource(radioURL);
                
                player.PrepareAsync();
            } else
            {

                // It's playing, pause it
                if (player.IsPlaying)
                {
                    player.Pause();
                    btnPlay.Text = iconPlay;
                    StopTimer();
                }
                else
                {
                    player.Start();
                    btnPlay.Text = iconPause;
                    StartTimer();
                }
            }
        }

        private void BtnMute_Click(object sender, EventArgs e)
        {
            if(player != null)
            {

                bool muted = IsMuted();

                if (muted)
                {
                    btnMute.Text = iconVolume;
                    btnMute.SetTextColor(Color.White);
                    GetAudioManager().SetStreamVolume(Stream.Music, currentStreamVolume, 0);

                    lblNowPlaying.Text = "unmuted";
                } else
                {
                    btnMute.Text = iconMute;
                    btnMute.SetTextColor(Color.Red);

                    GetAudioManager().SetStreamVolume(Stream.Music, 0, 0);

                    lblNowPlaying.Text = "muted";

                }
            }
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            if(player != null)
            {
                StopTimer();

                player.Stop();

                btnPlay.Text = iconPlay;

                btnMute.Text = iconVolume;
                btnMute.SetTextColor(Color.White);

                GetAudioManager().SetStreamVolume(Stream.Music, currentStreamVolume, 0);

                lblNowPlaying.Text = "stopped";

                player = null;

            }
        }


        private bool IsMuted()
        {
            return GetAudioManager().IsStreamMute(Stream.Music);

        }


        private AudioManager GetAudioManager()
        {
            return (AudioManager)GetSystemService(Android.Content.Context.AudioService);
        }

        private void StartTimer()
        {
            StopTimer();

            try
            {
                timer = new System.Timers.Timer(5000);
                timer.Elapsed += UpdateSongTitle;
                timer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Start Timer Error: " + ex.Message);
            }
        }



        public void StopTimer()
        {
            if (timer != null)
            {
                try
                {
                    timer.Stop();
                    timer.Dispose();
                    timer = null;
                }
                catch (Exception exTimer)
                {
                    Console.WriteLine("Timer stop error = " + exTimer.Message);
                }
            }
        }

        private void UpdateSongTitle(Object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                XmlTextReader reader = new XmlTextReader(trackInfoURL);

                bool getTitle = false;
                int gotIt = 0;

                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element: // The node is an element.
                            while (reader.MoveToNextAttribute())
                            {
                            }

                            if (reader.Name.ToLower() == "title")
                            {
                                getTitle = true;

                            }
                            break;
                        case XmlNodeType.Text: //Display the text in each element.
                            if (getTitle)
                            {

                                if (gotIt == 0)
                                {
                                    gotIt++;
                                }
                                else
                                {
                                    currentSongTitle = reader.Value;

                                    MainThread.BeginInvokeOnMainThread(() =>
                                    {
                                        lblNowPlaying.Text = currentSongTitle;
                                    });

                                    getTitle = false;

                                }
                            }
                            break;
                        case XmlNodeType.EndElement: //Display the end of the element.
                                                     //Console.Write("</" + reader.Name);
                                                     //Console.WriteLine(">");
                            break;
                    }
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Update Title Error: " + ex.Message);
            }




        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}

