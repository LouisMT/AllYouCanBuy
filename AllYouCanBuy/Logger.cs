using UnityEngine;

namespace AllYouCanBuy
{
    public static class Logger
    {
        public static void Info(string message)
        {
            Debug.Log($"[AllYouCanBuy] {message}");
        }
    }
}
