using System;
using System.Text;
using Kitchen.NetworkSupport;
using UnityEngine;

namespace AllYouCanBuy.Helpers
{
    internal static class CycleBlueprintClientLog
    {
        private const string Prefix = "[AllYouCanBuy] [CycleBlueprintClient]";

        internal static bool IsClient
        {
            get
            {
                try
                {
                    foreach (var transport in NetworkServices.GetAllTransports())
                    {
                        if (transport.ConnectionStatus == TransportConnectionStatus.ActiveClient)
                        {
                            return true;
                        }
                    }
                }
                catch (Exception)
                {
                    // Networking can be unavailable while the game is starting or changing scenes.
                }

                return false;
            }
        }

        internal static void Info(string message)
        {
            if (IsClient)
            {
                Debug.Log($"{Prefix} {message}");
            }
        }

        internal static void Warning(string message)
        {
            if (IsClient)
            {
                Debug.LogWarning($"{Prefix} {message}");
            }
        }

        internal static void Error(string message, Exception exception)
        {
            if (IsClient)
            {
                Debug.LogError($"{Prefix} {message}: {exception}");
            }
        }

        internal static string DescribeTransports()
        {
            try
            {
                var result = new StringBuilder();
                foreach (var transport in NetworkServices.GetAllTransports())
                {
                    if (result.Length > 0)
                    {
                        result.Append("; ");
                    }

                    result.Append(transport.ConnectionType)
                        .Append(" status=").Append(transport.ConnectionStatus)
                        .Append(" hosting=").Append(transport.IsHosting)
                        .Append(" active=").Append(transport.ShouldBeActive)
                        .Append(" send=").Append(transport.SendStatus);
                }

                return result.Length > 0 ? result.ToString() : "none";
            }
            catch (Exception exception)
            {
                return $"unavailable ({exception.GetType().Name}: {exception.Message})";
            }
        }
    }
}
