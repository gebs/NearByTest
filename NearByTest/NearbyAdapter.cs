using Android.App;
using Android.Content;
using Android.Gms.Nearby.Connection;
using Android.Gms.Nearby;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NearByTest
{
    internal class NearbyAdapter
    {
        private readonly IConnectionsClient _connectionsClient;

        private readonly List<string> endpoints = new List<string>();

        private Android.App.AlertDialog.Builder _builder;

        private readonly LifecycleCallback lifecycleCallback;
        private readonly PresenceCallback presenceCallback;
        private readonly DiscoveryCallback discoveryCallback;

        public NearbyAdapter(Activity activity)
        {
            lifecycleCallback = new LifecycleCallback(this);
            presenceCallback = new PresenceCallback(this);
            discoveryCallback = new DiscoveryCallback(this);
            _connectionsClient = NearbyClass.GetConnectionsClient(activity);
            _builder = new AlertDialog.Builder(activity);
        }

        private async void SendWelcomeMessage(string endointId)
        {
            _connectionsClient.SendPayload(endointId, Payload.FromBytes(Encoding.UTF8.GetBytes("Test")));
        }


        private async void OnPresenceReceived(string endpointId, Payload payload)
        {
            var stringItem = Encoding.UTF8.GetString(payload.AsBytes());
        }


        private void OnPresenceTransferUpdate(string endpointId, PayloadTransferUpdate update)
        {

        }


        public void StartAdvertising()
        {
            try
            {
                var options = new AdvertisingOptions.Builder().SetStrategy(Strategy.P2pCluster).SetLowPower(false).Build();

                _connectionsClient.StartAdvertising("Test", "Test", lifecycleCallback, options);
            }
            catch (Exception ex)
            {
                AlertDialog dialog = _builder.Create();
                dialog.SetTitle("Error");
                dialog.SetMessage(ex.Message);
                dialog.SetButton("OK", (c, ev) => { dialog.Dismiss(); });
                dialog.Show();
            }
        }

        public void StartDiscovery()
        {
            try
            {
                var options = new DiscoveryOptions.Builder().SetStrategy(Strategy.P2pCluster).SetLowPower(false).Build();
                _connectionsClient.StartDiscovery("Test", discoveryCallback, options);
            }
            catch (Exception ex)
            {
                AlertDialog dialog = _builder.Create();
                dialog.SetTitle("Error");
                dialog.SetMessage(ex.Message);
                dialog.SetButton("OK", (c, ev) => { dialog.Dismiss(); });
                dialog.Show();
            }
        }

        private void OnConnectionInitiated(string endpointId, ConnectionInfo connectionInfo)
        {
            _connectionsClient.AcceptConnection(endpointId, presenceCallback);
        }

        private void OnConnectionResult(string endpointId, ConnectionResolution result)
        {
            if (result.Status.IsSuccess)
            {
                if (!this.endpoints.Contains(endpointId))
                {
                    this.endpoints.Add(endpointId);
                    this.SendWelcomeMessage(endpointId);
                }
            }
        }

        private async void OnDisconnected(string endpointId)
        {
        }

        private void OnEndpointFound(string endpointId, DiscoveredEndpointInfo info)
        {
            _connectionsClient.RequestConnection("Test", endpointId, lifecycleCallback);
        }

        public class LifecycleCallback : ConnectionLifecycleCallback
        {
            readonly NearbyAdapter _adapter;

            public LifecycleCallback(NearbyAdapter adapter)
            {
                _adapter = adapter;
            }
            public override void OnConnectionInitiated(string p0, ConnectionInfo p1) => _adapter.OnConnectionInitiated(p0, p1);
            public override void OnConnectionResult(string p0, ConnectionResolution p1) => _adapter.OnConnectionResult(p0, p1);
            public override void OnDisconnected(string p0) => _adapter.OnDisconnected(p0);
        }

        public class PresenceCallback : PayloadCallback
        {
            readonly NearbyAdapter _adapter;

            public PresenceCallback(NearbyAdapter adapter)
            {
                _adapter = adapter;
            }

            public override void OnPayloadReceived(string p0, Payload p1) => _adapter.OnPresenceReceived(p0, p1);

            public override void OnPayloadTransferUpdate(string p0, PayloadTransferUpdate p1) => _adapter.OnPresenceTransferUpdate(p0, p1);
        }

        public class DiscoveryCallback : EndpointDiscoveryCallback
        {
            readonly NearbyAdapter _adapter;

            public DiscoveryCallback(NearbyAdapter adapter)
            {
                _adapter = adapter;
            }

            public override void OnEndpointFound(string p0, DiscoveredEndpointInfo p1) => _adapter.OnEndpointFound(p0, p1);
            public override void OnEndpointLost(string p0)
            {
            }
        }
    }
}