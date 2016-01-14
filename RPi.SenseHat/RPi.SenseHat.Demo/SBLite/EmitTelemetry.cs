using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ppatierno.AzureSBLite;
using ppatierno.AzureSBLite.Messaging;
using Newtonsoft.Json;

namespace RPi.SenseHat.Demo
{
    class EmitTelemetry : IDisposable
    {
        #region members
        string sbConnectionString;
        string eventHubClient;
        string partitionID;
        bool enabled;
        ServiceBusConnectionStringBuilder builder;
        MessagingFactory factory;
        EventHubClient client;
        EventHubSender sender;
        #endregion 

        public EmitTelemetry()
        {
            ParseConfig();

            builder = new ServiceBusConnectionStringBuilder(sbConnectionString);
            builder.TransportType = TransportType.Amqp;

            factory = MessagingFactory.CreateFromConnectionString(sbConnectionString);
            client = factory.CreateEventHubClient(eventHubClient);
            sender = client.CreatePartitionedSender(partitionID);
        }

        private async void ParseConfig()
        {
            try
            {
                FileIOHelper oFileHelper = new FileIOHelper();
                List<SettingInfo> lstSettingInfo = oFileHelper.ReadFromDefaultFile("ms-appx:///Assets/config.json");//Call to helper file for getting the details

                sbConnectionString = lstSettingInfo.Single(s => s.keyName == "ConnectionString").keyValue;
                eventHubClient = lstSettingInfo.Single(s => s.keyName == "EventHubClientName").keyValue;
                partitionID = lstSettingInfo.Single(s => s.keyName == "PartitionID").keyValue;
                enabled = bool.Parse(lstSettingInfo.Single(s => s.keyName == "Enabled").keyValue);
            }
            catch (Exception ex)
            {
            }
        }

        public void Emit(Object telemetryData)
        {
            var messageString = JsonConvert.SerializeObject(telemetryData);

            EventData data = new EventData(Encoding.ASCII.GetBytes(messageString));

            if (enabled)
            {
                sender.Send(data);
            }
        }

        public void Dispose()
        {
            client.Close();
            factory.Close();
        }
    }
}
