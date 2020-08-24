using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Text;
using Android.Widget;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Timers;
using Xamarin.Essentials;
using AlertDialog = Android.App.AlertDialog;

namespace Truck_Simulator_Tool_App
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        //Variables START

        bool IsActive = false;
        bool IsConnected = false;
        string settingsPath = Path.Combine(FileSystem.AppDataDirectory + "config.json");
        Settings settings = new Settings();
        TST_ServerData tst_serverdata = new TST_ServerData();
        Timer timer1 = new Timer(500);

        private EditText editText_port;
        private Button button_DEBUG;
        //Variables END
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            LoadCreate_Settings();

            await Update_TSTServerData();

            if (IsConnected == true)
            {
                SetContentView(Resource.Layout.activity_main);
            }
            else
            {
                SetContentView(Resource.Layout.activity_port);

                editText_port = FindViewById<EditText>(Resource.Id.editText1_ipAddress);
                button_DEBUG = FindViewById<Button>(Resource.Id.button_DEBUG);

                button_DEBUG.Click += button_DEBUG_Click;
            }
        }

        protected override void OnStart()
        {
            base.OnStart();
            bool IsActive = true;

            timer1.Start();
            timer1.Elapsed += Timer1_Elapsed;
        }

        protected override void OnStop()
        {
            base.OnStop();
            bool IsActive = false;

            timer1.Stop();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }


        //DEBUG
        private void button_DEBUG_Click(object sender, EventArgs e)
        {
            Android.App.AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle("IP Adresse");

            EditText editText = new EditText(this);
            editText.Text = settings.TSTServerIP;
            builder.SetView(editText);

            //builder.SetPositiveButton("Annehmen", Popup_IPAddressButtonPositive);
            builder.SetPositiveButton("Annehmen", (EventHandler<DialogClickEventArgs>)null);
            builder.SetNegativeButton("Abbrechen", (EventHandler<DialogClickEventArgs>)null);

            var dialog = builder.Create();
            dialog.Show();

            var yesBtn = dialog.GetButton((int)DialogButtonType.Positive);
            var noBtn = dialog.GetButton((int)DialogButtonType.Negative);
            dialog.SetCanceledOnTouchOutside(false);

            yesBtn.Click += (Bsender, args) =>
            {
                if (editText.Text != null && !editText.Text.Contains(":"))
                {
                    settings.TSTServerIP = editText.Text;
                    Save_Settings();
                }
                else
                {
                    Show_AlertMessage("Error!", "Die Eingegebene IP Adresse hat ein falsches Format.");
                }
                dialog.Dismiss();
            };
            noBtn.Click += (Bsender, args) =>
            {
                dialog.Dismiss();
            };
        }

        private void Popup_IPAddressButtonPositive(object sender, DialogClickEventArgs e)
        {
            
        }
        private void Popup_IPAddressButtonNegative(object sender, DialogClickEventArgs e)
        {
            
        }

        // Update TST_ServerData
        private async Task Update_TSTServerData()
        {
            try
            {
                HttpClient client = new HttpClient();
                Stream stream = await client.GetStreamAsync(string.Format("http://{0}/", settings.TSTServerIP));

                StreamReader sr = new StreamReader(stream);
                string json = sr.ReadToEnd();
                sr.Close();

                tst_serverdata = JsonConvert.DeserializeObject<TST_ServerData>(json);
                IsConnected = true;
            }
            catch
            {
                IsConnected = false;
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
                    Show_AlertMessage("Schwerwiegender Fehler gefunden!", "Fehler in der LoadCreate Settings Methode: DeserializeObject");
                    try { File.Delete(settingsPath); } catch { }
                }
            }
            else
            {
                settings.TSTServerIP = " ";
            }
        }

        // Save Settings
        private void Save_Settings()
        {
            try
            {
                if (editText_port.Text.Length > 0)
                {
                    string json = JsonConvert.SerializeObject(settings);
                    File.WriteAllText(settingsPath, json);
                }
                else
                {
                    settings.TSTServerIP = " ";
                }
            }
            catch
            {
                Show_AlertMessage("Schwerwiegender Fehler gefunden!", "Fehler in der Save Settings Methode");
            }
        }

        // Timer1 Tick
        private void Timer1_Elapsed(object sender, ElapsedEventArgs e)
        {
            RunOnUiThread(Timer1_Tick);
        }
        private void Timer1_Tick()
        {

        }


        // Message Method
        private void Show_AlertMessage(string title, string message)
        {
            Android.App.AlertDialog.Builder msg = new Android.App.AlertDialog.Builder(this);
            msg.SetTitle(title);
            msg.SetMessage(message);
            msg.SetPositiveButton("OK", (EventHandler<DialogClickEventArgs>)null);
            msg.Show();
        }
    }
}