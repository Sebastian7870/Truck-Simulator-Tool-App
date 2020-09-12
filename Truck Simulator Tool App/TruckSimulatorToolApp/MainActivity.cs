using Android.App;
using Android.Bluetooth.LE;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Support.V7.View.Menu;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Org.Apache.Http;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Essentials;
using AlertDialog = Android.App.AlertDialog;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace Truck_Simulator_Tool_App
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        //Variables START

        bool isConnected = false;
        bool wasConnected = false;
        int offlineTicks = 0;
        string settingsPath = Path.Combine(FileSystem.AppDataDirectory + "config.json");
        Settings settings = new Settings();
        TST_ServerData tst_serverdata = new TST_ServerData();
        Timer timer1 = new Timer(500);

        TextView connectionStatus;
        LinearLayout linearLayout_connectionStatus;
        TextView contractStatus;
        LinearLayout linearLayout_contractStatus;
        TextView shiftStatus;
        LinearLayout linearLayout_shiftStatus;
        TextView currentArrival_dt;
        TextView currentArrival_ts;
        GridLayout gridLayout_currentArrival;
        TextView currentBestArrival_dt;
        TextView currentBestArrival_ts;
        TextView bestArrival_dt;
        TextView bestArrival_ts;
        TextView nextPauseTime;
        TextView remainingTime;
        TextView jobInfoFreight;
        TextView jobInfoMass;
        TextView jobInfoIncome;
        TextView source;
        TextView destination;
        TextView progressBarPercentage;
        TextView timebuffer;
        TextView remainingDistance;
        TextView timescale;
        GridLayout gridLayout_bottomData;
        TextView nextShiftEvent;
        TextView nextShiftPause;
        TextView shiftTimeLeft;
        ProgressBar progressBar_damage;
        ProgressBar progressBar_distance;
        TextView progressBarDamage;
        TextView progressBarDistance;

        PowerManager powerManager;
        PowerManager.WakeLock wakeLock;
        
        //Variables END
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            
            LoadCreate_Settings();
            
            SetContentView(Resource.Layout.activity_main);

            connectionStatus = FindViewById<TextView>(Resource.Id.textView_connectionStatus);
            contractStatus = FindViewById<TextView>(Resource.Id.textView_contractStatus);
            shiftStatus = FindViewById<TextView>(Resource.Id.textView_shiftStatus);
            currentArrival_dt = FindViewById<TextView>(Resource.Id.textView_currentArrival_dt);
            currentArrival_ts =  FindViewById<TextView>(Resource.Id.textView_currentArrival_ts);
            currentBestArrival_dt = FindViewById<TextView>(Resource.Id.textView_currentBestArrival_dt);
            currentBestArrival_ts = FindViewById<TextView>(Resource.Id.textView_currentBestArrival_ts);
            bestArrival_dt = FindViewById<TextView>(Resource.Id.textView_bestArrival_dt);
            bestArrival_ts = FindViewById<TextView>(Resource.Id.textView_bestArrival_ts);
            nextPauseTime = FindViewById<TextView>(Resource.Id.textView_nextPauseTime);
            remainingTime = FindViewById<TextView>(Resource.Id.textView_remainingTime);
            jobInfoFreight = FindViewById<TextView>(Resource.Id.textView_jobInfoFreight);
            jobInfoMass = FindViewById<TextView>(Resource.Id.textView_jobInfoMass);
            jobInfoIncome = FindViewById<TextView>(Resource.Id.textView_jobInfoIncome);
            source = FindViewById<TextView>(Resource.Id.textView_source);
            destination = FindViewById<TextView>(Resource.Id.textView_destination);
            progressBarPercentage = FindViewById<TextView>(Resource.Id.textView_progressBarPercentage);
            timebuffer = FindViewById<TextView>(Resource.Id.textView_timebuffer);
            remainingDistance = FindViewById<TextView>(Resource.Id.textView_remainingDistance);
            timescale = FindViewById<TextView>(Resource.Id.textView_timescale);
            progressBarDamage = FindViewById<TextView>(Resource.Id.textView_progressBarDamage);
            progressBarDistance = FindViewById<TextView>(Resource.Id.textView_progressBarDistance);

            linearLayout_connectionStatus = FindViewById<LinearLayout>(Resource.Id.linearLayout_connectionStatus);
            linearLayout_contractStatus = FindViewById<LinearLayout>(Resource.Id.linearLayout_contractStatus);
            linearLayout_shiftStatus = FindViewById<LinearLayout>(Resource.Id.linearLayout_shiftStatus);
            gridLayout_currentArrival = FindViewById<GridLayout>(Resource.Id.gridLayout_currentArrival);
            gridLayout_bottomData = FindViewById<GridLayout>(Resource.Id.gridLayout_bottomData);

            progressBar_damage = FindViewById<ProgressBar>(Resource.Id.progressBar_damage);
            progressBar_distance = FindViewById<ProgressBar>(Resource.Id.progressBar_distance);

            nextShiftEvent = FindViewById<TextView>(Resource.Id.textView_nextShiftEvent);
            nextShiftPause = FindViewById<TextView>(Resource.Id.textView_nextShiftPause);
            shiftTimeLeft = FindViewById<TextView>(Resource.Id.textView_shiftTimeLeft);


            powerManager = (PowerManager)GetSystemService(PowerService);
            wakeLock = powerManager.NewWakeLock(WakeLockFlags.ScreenBright, "TruckSimulatorTool");
            CreateToolbar();
        }
        private void CreateToolbar()
        {
            Android.Content.Res.Resources resources = this.Resources;
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.Title = resources.GetString(Resource.String.app_nameLong);
        }
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            if (isConnected == true)
            {
                MenuInflater.Inflate(Resource.Menu.menu_toolbar, menu);
            }
            else
            {
                MenuInflater.Inflate(Resource.Menu.menu_toolbar2, menu);
            }
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.menu_button)
            {
                Android.App.AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetTitle("IP Adresse");

                EditText editText = new EditText(this);
                if (settings.TSTServerIP != null)
                    settings.TSTServerIP = String.Concat(settings.TSTServerIP.Where(c => !Char.IsWhiteSpace(c)));
                editText.Text = settings.TSTServerIP;
                builder.SetView(editText);

                builder.SetPositiveButton("Annehmen", (EventHandler<DialogClickEventArgs>)null);
                builder.SetNegativeButton("Abbrechen", (EventHandler<DialogClickEventArgs>)null);
                builder.SetCancelable(false);

                var dialog = builder.Create();
                dialog.Show();

                var yesBtn = dialog.GetButton((int)DialogButtonType.Positive);
                var noBtn = dialog.GetButton((int)DialogButtonType.Negative);

                yesBtn.Click += (Bsender, args) =>
                {
                    if (editText.Text != null && !editText.Text.Contains(":"))
                    {
                        editText.Text = String.Concat(editText.Text.Where(c => !Char.IsWhiteSpace(c)));
                        settings.TSTServerIP = editText.Text;
                        Save_Settings();
                    }
                    else
                    {
                        Show_AlertMessage(this, "Error!", "Die Eingegebene IP Adresse hat ein falsches Format.");
                    }
                    dialog.Dismiss();
                };
                noBtn.Click += (Bsender, args) =>
                {
                    dialog.Dismiss();
                };
            }
            return base.OnOptionsItemSelected(item);
        }

        protected override void OnStart()
        {
            base.OnStart();

            wakeLock.Acquire();
            timer1.Start();
            timer1.Elapsed += Timer1_Elapsed;
        }

        protected override void OnStop()
        {
            base.OnStop();

            timer1.Stop();
            wakeLock.Release();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        // Update TST_ServerData
        private async Task Update_TSTServerData()
        {
            try
            {
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromMilliseconds(1000);
                Stream stream = await client.GetStreamAsync(string.Format("http://{0}:25558/", settings.TSTServerIP));

                StreamReader sr = new StreamReader(stream);
                string json = sr.ReadToEnd();
                sr.Close();

                tst_serverdata = JsonConvert.DeserializeObject<TST_ServerData>(json);
                isConnected = true;
            }
            catch
            {
                isConnected = false;
            }
        }


        // Load or Create Settings
        private void LoadCreate_Settings()
        {
            if (File.Exists(settingsPath))
            {
                try
                {
                    settings = (JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsPath)));
                }
                catch
                {
                    Show_AlertMessage(this, "Schwerwiegender Fehler gefunden!", "Fehler in der LoadCreate Settings Methode: DeserializeObject");
                    try { File.Delete(settingsPath); } catch { }
                    settings.TSTServerIP = "";
                }
            }
            else
            {
                settings.TSTServerIP = "";
            }
        }

        // Save Settings
        private void Save_Settings()
        {
            try
            {
                if (settings.TSTServerIP.Length > 0)
                {
                    string json = JsonConvert.SerializeObject(settings);
                    File.WriteAllText(settingsPath, json);
                }
                else
                {
                    settings.TSTServerIP = "";
                    string json = JsonConvert.SerializeObject(settings);
                    File.WriteAllText(settingsPath, json);
                }
            }
            catch
            {
                settings.TSTServerIP = "";
                Show_AlertMessage(this, "Schwerwiegender Fehler gefunden!", "Fehler in der Save Settings Methode");
            }
        }

        // Timer1 Tick
        private void Timer1_Elapsed(object sender, ElapsedEventArgs e)
        {
            RunOnUiThread(Timer1_Tick);
        }
        private async void Timer1_Tick()
        {


            await Update_TSTServerData();
            CreateToolbar();

            if (isConnected == true)
            {
                wasConnected = true;
                offlineTicks = 0;
                connectionStatus.Text = tst_serverdata.connectionStatusText;
                linearLayout_connectionStatus.SetBackgroundColor(Android.Graphics.Color.ParseColor(tst_serverdata.connectionStatusBrush));
                contractStatus.Text = tst_serverdata.contractStatusText;
                linearLayout_contractStatus.SetBackgroundColor(Android.Graphics.Color.ParseColor(tst_serverdata.contractStatusBrush));
                shiftStatus.Text = tst_serverdata.shiftStatusText;
                linearLayout_shiftStatus.SetBackgroundColor(Android.Graphics.Color.ParseColor(tst_serverdata.shiftStatusBrush));
                currentArrival_dt.Text = tst_serverdata.currentArrival_dtText;
                currentArrival_ts.Text = tst_serverdata.currentArrival_tsText;
                gridLayout_currentArrival.SetBackgroundColor(Android.Graphics.Color.ParseColor(tst_serverdata.currentArrivalBrush));
                currentBestArrival_dt.Text = tst_serverdata.currentBestArrival_dtText;
                currentBestArrival_ts.Text = tst_serverdata.currentBestArrival_tsText;
                bestArrival_dt.Text = tst_serverdata.bestArrival_dtText;
                bestArrival_ts.Text = tst_serverdata.bestArrival_tsText;
                nextPauseTime.Text = tst_serverdata.nextPauseTimeText;
                nextPauseTime.SetTextColor(Android.Graphics.Color.ParseColor(tst_serverdata.nextPauseTimeBrush));
                remainingTime.Text = tst_serverdata.remainingTimeText;
                remainingTime.SetTextColor(Android.Graphics.Color.ParseColor(tst_serverdata.remainingTimeBrush));
                jobInfoFreight.Text = tst_serverdata.jobInfo_FreightText;
                jobInfoMass.Text = tst_serverdata.jobInfo_MassText;
                jobInfoIncome.Text = tst_serverdata.jobInfo_IncomeText;
                progressBar_damage.SetProgress((int)(tst_serverdata.pb_damageProgress), true);
                progressBarDamage.Text = tst_serverdata.pb_damageText;
                progressBar_distance.SetProgress((int)(tst_serverdata.pb_distanceProgress), true);
                progressBarDistance.Text = tst_serverdata.pb_distanceText;
                source.Text = tst_serverdata.sourceText;
                destination.Text = tst_serverdata.destinationText;
                timebuffer.Text = tst_serverdata.timebufferText;
                timebuffer.SetBackgroundColor(Android.Graphics.Color.ParseColor(tst_serverdata.timebufferBrush));
                remainingDistance.Text = tst_serverdata.remainingDistanceText;
                timescale.Text = tst_serverdata.timescaleText;

                if (tst_serverdata.hasShift == true)
                {
                    gridLayout_bottomData.Visibility = ViewStates.Visible;
                    nextShiftEvent.Text = tst_serverdata.nextShiftEvent;
                    nextShiftPause.Text = tst_serverdata.nextShiftPause;
                    shiftTimeLeft.Text = tst_serverdata.shiftTimeLeft;
                }
                else
                {
                    ResetBottomlinearLayout();
                }
            }
            else
            {
                //after 3 seconds reset UI to default resources (+ GridLayout bottom values : weight 110, rows 3, visible visible)
                offlineTicks++;
                if (wasConnected == true && offlineTicks >= 6)
                {
                    ResetBottomlinearLayout();
                    SetStandardValues();
                    Show_AlertMessage(this, "Verbindung zum Server verloren!", "");
                    offlineTicks = 0;
                    wasConnected = false;
                }
                if (wasConnected == false)
                {
                    offlineTicks = 0;
                }
            }
        }

        private void ResetBottomlinearLayout()
        {
            Android.Content.Res.Resources resources = this.Resources;
            nextShiftEvent.Text = resources.GetString(Resource.String.textView_nextShiftEvent);
            nextShiftPause.Text = resources.GetString(Resource.String.textView_nextShiftPause2);
            shiftTimeLeft.Text = resources.GetString(Resource.String.textView_shiftTimeLeft2);
            gridLayout_bottomData.Visibility = ViewStates.Gone;
        }


        // Reset all textViews, imageViews, ... to standard
        private void SetStandardValues()
        {
            Android.Content.Res.Resources resources = this.Resources;
            connectionStatus.Text = resources.GetString(Resource.String.textView_connectionStatus);
            linearLayout_connectionStatus.SetBackgroundColor(Android.Graphics.Color.Brown);
            contractStatus.Text = resources.GetString(Resource.String.textView_contractStatus);
            linearLayout_contractStatus.SetBackgroundColor(Android.Graphics.Color.Brown);
            shiftStatus.Text = resources.GetString(Resource.String.textView_shiftStatus);
            linearLayout_shiftStatus.SetBackgroundColor(Android.Graphics.Color.Brown);
            currentArrival_dt.Text = resources.GetString(Resource.String.textView_currentArrival_dt);
            currentArrival_ts.Text = resources.GetString(Resource.String.textView_currentArrival_ts);
            gridLayout_currentArrival.SetBackgroundColor(Android.Graphics.Color.Brown);
            currentBestArrival_dt.Text = resources.GetString(Resource.String.textView_currentBestArrival_dt);
            currentBestArrival_ts.Text = resources.GetString(Resource.String.textView_currentBestArrival_ts);
            bestArrival_dt.Text = resources.GetString(Resource.String.textView_bestArrival_dt);
            bestArrival_ts.Text = resources.GetString(Resource.String.textView_bestArrival_ts);
            nextPauseTime.Text = resources.GetString(Resource.String.textView_nextPauseTime);
            nextPauseTime.SetTextColor(Android.Graphics.Color.Brown);
            remainingTime.Text = resources.GetString(Resource.String.textView_remainingTime);
            remainingTime.SetTextColor(Android.Graphics.Color.Brown);
            jobInfoFreight.Text = resources.GetString(Resource.String.textView_jobInfoFreight);
            jobInfoMass.Text = resources.GetString(Resource.String.textView_jobInfoMass);
            jobInfoIncome.Text = resources.GetString(Resource.String.textView_jobInfoIncome);
            progressBar_damage.SetProgress(0, false);
            progressBarDamage.Text = resources.GetString(Resource.String.textView_progressBarDamage);
            source.Text = resources.GetString(Resource.String.textView_source);
            destination.Text = resources.GetString(Resource.String.textView_destination);
            progressBar_distance.SetProgress(0, false);
            progressBarDistance.Text = resources.GetString(Resource.String.textView_progressBarDistance);
            progressBarPercentage.Text = resources.GetString(Resource.String.textView_progressBarPercentage);
            timebuffer.Text = resources.GetString(Resource.String.textView_timebuffer);
            timebuffer.SetBackgroundColor(Android.Graphics.Color.Brown);
            remainingDistance.Text = resources.GetString(Resource.String.textView_remainingDistance);
            timescale.Text = resources.GetString(Resource.String.textView_timescale2);
        }

        private static void Show_AlertMessage(Context c, string title, string message)
        {
            Android.App.AlertDialog.Builder msg = new Android.App.AlertDialog.Builder(c);
            msg.SetTitle(title);
            msg.SetMessage(message);
            msg.SetPositiveButton("OK", (EventHandler<DialogClickEventArgs>)null);
            msg.Show();
        }
    }
}
