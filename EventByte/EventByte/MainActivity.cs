using Android.App;
using Android.Widget;
using Android.OS;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http;

namespace EventByte
{
    [Activity(Label = "EventByte", MainLauncher = true, Icon = "@drawable/ic_launcher")]
    public class MainActivity : Activity, IOnMapReadyCallback, ILocationListener
    {

        //Declaración de variables globales.
        #region variables
        private GoogleMap GMap;
        Location currentLocation;
        LocationManager locationManager;
        Marker currentMarker;
        string locationProvider;
        #endregion
        protected override async void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            // Set our view from the "main" layout resource  
            SetContentView(Resource.Layout.Main);
            SetUpMap();

            InitializeLocationManager();
            string url = "http://eventbyteservice.azurewebsites.net/Events/GetAllEvents/";
            var eventsList = await GetAllEvents(url);
            DrawMarkers(eventsList);
        }
        private void SetUpMap()
        {
            if (GMap == null)
            {
                FragmentManager.FindFragmentById<MapFragment>(Resource.Id.googlemap).GetMapAsync(this);
            }
        }

        #region IOnMapReadyCallback Method
        public void OnMapReady(GoogleMap googleMap)
        {
            GMap = googleMap;
            GMap.UiSettings.ZoomControlsEnabled = true;
            GMap.UiSettings.CompassEnabled = true;
            LatLng latlng = new LatLng(Convert.ToDouble(4.6378516), Convert.ToDouble(-74.08881));
            CameraUpdate camera = CameraUpdateFactory.NewLatLngZoom(latlng, 15);
            GMap.MoveCamera(camera);
            MarkerOptions options = new MarkerOptions().SetPosition(latlng).SetTitle("UNAL");
            GMap.AddMarker(options);
        }
        #endregion

        //#region Add Markers
        private void AddMarkerCurrentLocationToMap(LatLng latlng)
        {
            MarkerOptions options = new MarkerOptions().SetPosition(latlng).SetTitle("Estás Aquí")
                .SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.xamarin30));


            if (currentMarker != null)
            {
                currentMarker.Remove();
            }
            else
            {
                CameraUpdate camera = CameraUpdateFactory.NewLatLng(latlng);
                GMap.MoveCamera(camera);
            }

            currentMarker = GMap.AddMarker(options);
        }
        private void DrawMarkers(List<Event> events)
        {
            foreach (Event ev in events)
            {
                LatLng latlng = new LatLng(Convert.ToDouble(ev.Latitude), Convert.ToDouble(ev.Longitude));
                decimal price = ev.Price;
                string priceStr;
                if (price == 0)
                {
                    priceStr = "Gratis";
                }
                else
                {
                    priceStr = price.ToString();
                }
                MarkerOptions options = new MarkerOptions().SetPosition(latlng).SetTitle(ev.Name + "\n $ " + priceStr + "\n" + ev.StartDate.ToShortDateString());
                GMap.AddMarker(options);
            }
        }
        

        #region REST Method
        public async Task<List<Event>> GetAllEvents(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                var content = JsonConvert.SerializeObject(new { });
                var body = new StringContent(content, Encoding.UTF8, "application/json");
                var result = await client.PostAsync(url, body);

                var data = await result.Content.ReadAsStringAsync();

                if (result.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<List<Event>>(data);
                }
                return new List<Event>();
            }
        }
#endregion

        #region ILocationListener Methods
        private void InitializeLocationManager()
        {
            locationManager = (LocationManager)GetSystemService(LocationService);
            Criteria criteriaForLocationService = new Criteria
            {
                Accuracy = Accuracy.Fine
            };

            IList<string> acceptableLocationProviders = locationManager.GetProviders(criteriaForLocationService, true);
            if (acceptableLocationProviders.Any())
            {
                locationProvider = acceptableLocationProviders.First();
            }
            else
            {
                locationProvider = string.Empty;
            }
        }

        //Estos métodos se deben implementar debido a la inteerfaz
        public void OnLocationChanged(Location location)
        {
            currentLocation = location;
            if (currentLocation == null)
            {
                string message = "Ha ocurrido un error. :(";
                Toast.MakeText(ApplicationContext, message, ToastLength.Long).Show();
            }
            else
            {
                double lat = currentLocation.Latitude;
                double log = currentLocation.Longitude;
                LatLng latlng = new LatLng(lat, log);
                AddMarkerCurrentLocationToMap(latlng);
            }
        }
        public void OnProviderDisabled(string provider)
        {
            string message = "Debes prender el GPS";
            Toast.MakeText(ApplicationContext, message, ToastLength.Long).Show();
        }
        public void OnProviderEnabled(string provider)
        {
            string message = "Ya prendiste tu GPS";
            Toast.MakeText(ApplicationContext, message, ToastLength.Long).Show();
            InitializeLocationManager();
        }
        public void OnStatusChanged(string provider, Availability status, Bundle extras) { }

        #endregion
        protected override void OnResume()
        {
            base.OnResume();
            try
            {
                locationManager.RequestLocationUpdates(locationProvider, 0, 0, this);
            }
            catch
            {
                string message = "Hola Extraño. Parece que tienes el GPS apagado.";
                Toast.MakeText(ApplicationContext, message, ToastLength.Long).Show();
            }
        }
        protected override void OnPause()
        {
            base.OnPause();
            locationManager.RemoveUpdates(this);
        }
    }
}

