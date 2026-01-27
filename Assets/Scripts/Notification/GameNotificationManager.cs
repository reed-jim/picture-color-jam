// using Unity.Notifications.Android;
using UnityEngine;

public class GameNotificationManager : MonoBehaviour
{
    private void Awake()
    {
        // if (Application.platform == RuntimePlatform.Android)
        // {
        //     AndroidNotificationCenter.Initialize();
        // }

#if UNITY_ANDROID
        SendNotification();
#endif
    }

    // private void Register()
    // {
    //     var channel = new AndroidNotificationChannel()
    //     {
    //         Id = "channel_id",
    //         Name = "Default Channel",
    //         Importance = Importance.Default,
    //         Description = "Generic notifications",
    //     };

    //     AndroidNotificationCenter.Initialize();

    //     AndroidNotificationCenter.RegisterNotificationChannel(channel);
    // }

    // private void SendNotification()
    // {
    //     Register();

    //     var notification = new AndroidNotification();
    //     notification.Title = "Saferio";
    //     notification.Text = GetRandomText();
    //     notification.FireTime = System.DateTime.Now.AddHours(12);

    //     AndroidNotificationCenter.SendNotification(notification, "channel_id");
    // }

    private static readonly string[] TextPool =
    {
        "Fit the blocks and reveal the picture!",
        "A new block picture puzzle is waiting 🧩",
        "Can you complete the image with perfect moves?",
        "One puzzle away from a beautiful picture!",
        "Relax and enjoy a block + picture challenge",
        "Think smart, place blocks, unlock the picture",
        "Your next block puzzle masterpiece awaits"
    };

    public static string GetRandomText()
    {
        int index = Random.Range(0, TextPool.Length);
        return TextPool[index];
    }
}
