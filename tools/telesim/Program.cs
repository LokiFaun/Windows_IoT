﻿namespace TeleSim
{
    using System;
    using System.Text;
    using System.Threading;
    using uPLibrary.Networking.M2Mqtt;

    internal class Program
    {
        /// <summary>
        /// The topic for the current altitude
        /// </summary>
        private const string AltitudeTopic = "/schuetz/altitude";

        /// <summary>
        /// The topic for the current lux
        /// </summary>
        private const string LuxTopic = "/schuetz/lux";

        /// <summary>
        /// The topic for the current pressure
        /// </summary>
        private const string PressureTopic = "/schuetz/preassure";

        /// <summary>
        /// The topic for the current temperature
        /// </summary>
        private const string TemperatureTopic = "/schuetz/temperature";

        /// <summary>
        /// The MQTT client
        /// </summary>
        private static readonly MqttClient Client = new MqttClient("test.mosquitto.org");

        /// <summary>
        /// The current altitude
        /// </summary>
        private static double CurrentAltitude = 300;

        /// <summary>
        /// The current lux
        /// </summary>
        private static ulong CurrentLux = 3000;

        /// <summary>
        /// The current pressure
        /// </summary>
        private static double CurrentPressure = 1025;

        /// <summary>
        /// The current temperature
        /// </summary>
        private static double CurrentTemperature = 21.5;

        /// <summary>
        /// Callback method for the altitude timer
        /// </summary>
        /// <param name="state">not used</param>
        private static void AltitudeTimerCallback(object state)
        {
            CurrentAltitude = 44330.0 * (1.0 - Math.Pow(CurrentPressure / 1025.0, 0.1903));
            Console.WriteLine("Publishing altitude: " + CurrentAltitude);
            Client.Publish(AltitudeTopic, Encoding.UTF8.GetBytes(CurrentAltitude.ToString()));
        }

        /// <summary>
        /// Callback method for the lux timer
        /// </summary>
        /// <param name="state">not used</param>
        private static void LuxTimerCallback(object state)
        {
            var random = new Random();
            CurrentLux = (ulong)random.Next(1000, 6000);
            Console.WriteLine("Publishing lux: " + CurrentLux);
            Client.Publish(LuxTopic, Encoding.UTF8.GetBytes(CurrentLux.ToString()));
        }

        /// <summary>
        /// The main method
        /// </summary>
        /// <param name="args">console arguments</param>
        private static void Main(string[] args)
        {
            Console.WriteLine("Connecting to test.mosquitto.org...");
            Client.Connect(Guid.NewGuid().ToString());

            Console.WriteLine("Starting simulation...");
            using (var pressureTimer = new Timer(PressureTimerCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(10)))
            using (var temperatureTimer = new Timer(TemperatureTimerCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(10)))
            using (var altitudeTimer = new Timer(AltitudeTimerCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(10)))
            using (var luxTimer = new Timer(LuxTimerCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(10)))
            {
                Console.ReadKey();
                Console.WriteLine("Ending simulation...");
            }

            Client.Disconnect();
        }

        /// <summary>
        /// Callback method for the pressure timer
        /// </summary>
        /// <param name="state">not used</param>
        private static void PressureTimerCallback(object state)
        {
            var random = new Random();
            CurrentPressure = 1000 + (random.NextDouble() * 30);
            Console.WriteLine("Publishing pressure: " + CurrentPressure);
            Client.Publish(PressureTopic, Encoding.UTF8.GetBytes(CurrentPressure.ToString()));
        }

        /// <summary>
        /// Callback method for the temperature timer
        /// </summary>
        /// <param name="state">not used</param>
        private static void TemperatureTimerCallback(object state)
        {
            var random = new Random();
            CurrentTemperature = 19 + (random.NextDouble() * 5);
            Console.WriteLine("Publishing temperature: " + CurrentTemperature);
            Client.Publish(TemperatureTopic, Encoding.UTF8.GetBytes(CurrentTemperature.ToString()));
        }
    }
}