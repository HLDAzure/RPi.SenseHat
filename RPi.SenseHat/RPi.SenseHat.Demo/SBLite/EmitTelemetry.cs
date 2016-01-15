using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Exceptions;

namespace RPi.SenseHat.Demo
{
    class EmitTelemetry : IDisposable
    {
        public double TippingPoint { get; set; }
        #region members
        private DeviceClient _DeviceClient = null;
        string deviceConnectionString;
        bool enabled;
        #endregion 

        public EmitTelemetry()
        {
            TippingPoint = 0;
            ParseConfig();

            initSensor();
        }

        public async void initSensor()
        {
            _DeviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Http1);
            ReceiveCommands();
        }

        private void ParseConfig()
        {
            try
            {
                FileIOHelper oFileHelper = new FileIOHelper();
                List<SettingInfo> lstSettingInfo = oFileHelper.ReadFromDefaultFile("ms-appx:///Assets/config.json");//Call to helper file for getting the details

                deviceConnectionString = lstSettingInfo.Single(s => s.keyName == "DeviceConnectionString").keyValue;

                enabled = bool.Parse(lstSettingInfo.Single(s => s.keyName == "Enabled").keyValue);
            }
            catch (Exception ex)
            {
            }
        }

        public async void Emit(Object telemetryDataPoint)
        {
            var messageString = JsonConvert.SerializeObject(telemetryDataPoint);

            var message = new Message(Encoding.ASCII.GetBytes(messageString));

            if (enabled)
            {
                try
                {
                    await _DeviceClient.SendEventAsync(message);
                }
                catch (Exception ex)
                {
                }
            }
        }

        private async void ReceiveCommands()
        {
            Message receivedMessage;
            string messageData;

            while (true)
            {
                receivedMessage = await _DeviceClient.ReceiveAsync();
                if (receivedMessage != null)
                {
                    messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                    TippingPoint = double.Parse(messageData);
                    await _DeviceClient.CloseAsync();
                    await _DeviceClient.CompleteAsync(receivedMessage);
                }
            }
        }

        public void Dispose()
        {
            _DeviceClient = null;
        }
    }
}
