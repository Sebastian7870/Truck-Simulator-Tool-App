using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using System;
using System.Timers;
using Org.Apache.Http.Client.Params;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Javax.Security.Auth;

namespace Truck_Simulator_Tool_App
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        //Variables START

        TelemetryData telemetryData = new TelemetryData();

        bool telemetryOnline = false;
        string situation = null;
        int timercounter = 0;
        double currentaveragespeed = 0;
        double bestcurrentaveragespeed = 0;
        double speedsummary = 0;
        double TimeScaleConstant = 19;
        bool bestarrivalset = false;
        DateTime dt_currentarrival = DateTime.Now;
        DateTime dt_bestarrival = DateTime.Now;
        TimeSpan ts_bestarrival = new TimeSpan();

        TextView textView;

        //Variables END

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            Timer timer1_calculate = new Timer();
            timer1_calculate.Interval = 1000;
            timer1_calculate.Elapsed += Timer1_calculate_Elapsed;
            timer1_calculate.Start();

            textView = FindViewById<TextView>(Resource.Id.textView1);
        }

        public async Task GetJsonFromUrl(string url)
        {
            try
            {
                HttpClient client = new HttpClient();
                Stream stream = await client.GetStreamAsync(url);

                StreamReader sr = new StreamReader(stream);
                string JsonString = sr.ReadToEnd();
                sr.Close();

                telemetryData = JsonConvert.DeserializeObject<TelemetryData>(JsonString);
                telemetryOnline = true;
            }
            catch
            {
                telemetryOnline = false;
            }
        }

        public async void Timer1_calculate_Elapsed(object sender, ElapsedEventArgs e)
        {
            await GetJsonFromUrl("http://192.168.178.22:25552/");

            // Telemetry online and Ingame connected
            if (telemetryOnline == true && telemetryData.ets2.game.connected == true && telemetryData.ets2.truck.id != null)
            {

                // set situation
                if (telemetryData.ets2.job.cargo.id != null)
                {
                    if(situation != "Contract")
                    {
                        timercounter = 0;
                        speedsummary = 0;
                        currentaveragespeed = 0;

                        //distancesummary = 0;
                        //drivendistance = 0;

                        bestarrivalset = false;
                        //ContractSaved = false;
                    }
                    situation = "Contract";
                }
                else
                {
                    if (situation != "DestinationOrFreeDrive")
                    {
                        timercounter = 0;
                        speedsummary = 0;
                        currentaveragespeed = 0;

                        //distancesummary = 0;
                        //drivendistance = 0;

                        bestarrivalset = false;

                        //try if file exists[...]
                    }
                    situation = "DestinationOrFreeDrive";
                }


                // average speed calculations
                currentaveragespeed = speedsummary / timercounter;
                if (telemetryData.ets2.game.paused == false && telemetryData.ets2.truck.speed > 5)
                {// current average speed
                    timercounter += 1;
                    speedsummary += GetSpeedDistanceUnit(telemetryData.ets2.truck.speed);
                    currentaveragespeed = speedsummary / timercounter;
                }
                if (telemetryData.ets2.truck.navigationEstimatedDistance > 0)
                {// best current average speed
                    bestcurrentaveragespeed = GetSpeedDistanceUnit(telemetryData.ets2.truck.navigationEstimatedDistance / 1000) / Convert.ToDouble(telemetryData.ets2.truck.navigationEstimatedTime / 3600);
                }
                else
                {
                    bestarrivalset = false;
                    bestcurrentaveragespeed = 0;

                    //[...]
                }
                if (bestcurrentaveragespeed > 0)
                {// set average data
                    // set best current arrival
                    TimeSpan ts_bestcurrentarrival;
                    DateTime dt_bestcurrentarrvial = DateTime.Now.AddSeconds(((GetSpeedDistanceUnit(telemetryData.ets2.truck.navigationEstimatedDistance / 1000) / bestcurrentaveragespeed) / TimeScaleConstant) * 3600);
                    ts_bestcurrentarrival = dt_bestcurrentarrvial.Subtract(DateTime.Now);
                    //[set labels]

                    // set best arrival
                    if (bestarrivalset == false)
                    {
                        ts_bestarrival = TimeSpan.FromSeconds(((GetSpeedDistanceUnit(telemetryData.ets2.truck.navigationEstimatedDistance / 1000) / bestcurrentaveragespeed) / TimeScaleConstant) * 3600);
                        dt_bestarrival = DateTime.Now.Add(ts_bestarrival);
                        //[set labels]
                        bestarrivalset = true;
                    }
                    ts_bestarrival = dt_bestarrival - DateTime.Now;
                    if (ts_bestarrival.TotalSeconds > 0)
                    {
                        //[set labels]
                    }
                    else
                    {
                        //[set labels]
                    }

                    // set current arrival
                    if (currentaveragespeed > 0)
                    {
                        dt_currentarrival = DateTime.Now.AddSeconds(((GetSpeedDistanceUnit(telemetryData.ets2.truck.navigationEstimatedDistance / 1000) / currentaveragespeed) / TimeScaleConstant) * 3600);
                        TimeSpan ts_currentarrival = dt_currentarrival.Subtract(DateTime.Now);


                    }
                }

                
                



                // get speed / distance  unit (ATS / ETS2)
                double GetSpeedDistanceUnit(double d)
                {
                    if (telemetryData.ets2.game.gameID == "eut2")
                    {// ETS2
                        return d;
                    }
                    else
                    {// ATS
                        return d / 1.609344;
                    }
                }

            }

        }




        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}