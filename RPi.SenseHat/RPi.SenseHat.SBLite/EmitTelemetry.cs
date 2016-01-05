using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ppatierno.AzureSBLite;
using ppatierno.AzureSBLite.Messaging;
using Newtonsoft.Json;

namespace RPi.SenseHat.SBLite
{
    public class EmitTelemetry : IDisposable
    {
        #region members
        const string sbConnectionString = "Endpoint=sb://[servicebusname].servicebus.windows.net/;SharedAccessKeyName=[your-sas-name];SharedAccessKey=[your-sas-key]";
        const string eventHubClient = "[eh-client-name"; //ie. ioteventhub
        const string partitionID = "[partition-id]"; //ie. 1
        ServiceBusConnectionStringBuilder builder;
        MessagingFactory factory;
        EventHubClient client;
        EventHubSender sender;
        #endregion 

        public EmitTelemetry()
        {
            builder = new ServiceBusConnectionStringBuilder(sbConnectionString);
            builder.TransportType = TransportType.Amqp;

            factory = MessagingFactory.CreateFromConnectionString(sbConnectionString);
            client = factory.CreateEventHubClient(eventHubClient);
            sender = client.CreatePartitionedSender(partitionID);
        }

        public void Emit(Object telemetryData)
        {
            var messageString = JsonConvert.SerializeObject(telemetryData);

            EventData data = new EventData(Encoding.ASCII.GetBytes(messageString));

            sender.Send(data);
        }

        public void Dispose()
        {
            client.Close();
            factory.Close();
        }
    }
}
