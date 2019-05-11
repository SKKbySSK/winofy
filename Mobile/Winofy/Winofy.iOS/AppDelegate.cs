using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using System.Diagnostics;
using UserNotifications;
using Firebase.CloudMessaging;
using System.Threading;
using System.Threading.Tasks;

namespace Winofy.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, IUNUserNotificationCenterDelegate
    {
        internal class MessagingDel : MessagingDelegate
        {
            public override void DidReceiveRegistrationToken(Messaging messaging, string fcmToken)
            {
                Debug.WriteLine("Registration Token:" + fcmToken);
                Notification.FcmToken = fcmToken;

                var d = NSDictionary<NSString, NSString>.FromObjectsAndKeys(new NSString[] { new NSString(fcmToken) }, new NSString[] { new NSString("token") }, 1);
                NSNotificationCenter.DefaultCenter.PostNotificationName("FCMToken", null, d);
            }
        }

        internal class NotificationDel : NSObject, IUNUserNotificationCenterDelegate
        {
            [Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
            public void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
            {
                completionHandler(UNNotificationPresentationOptions.Alert | UNNotificationPresentationOptions.Badge | UNNotificationPresentationOptions.Sound);
            }
        }

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Firebase.Core.App.Configure();
            Notification.FcmToken = Firebase.InstanceID.InstanceId.SharedInstance.Token;

            UNUserNotificationCenter.Current.Delegate = new NotificationDel();
            Messaging.SharedInstance.Delegate = new MessagingDel();

            Xfx.XfxControls.Init();
            global::Xamarin.Forms.Forms.Init();
            Flex.FlexButton.Init();
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            LoadApplication(new App());

            _ = AuthorizeNotificationAsync();

            return base.FinishedLaunching(app, options);
        }

        private async Task AuthorizeNotificationAsync()
        {
            var ops = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;

            var notification = UNUserNotificationCenter.Current;
            var res = await notification.RequestAuthorizationAsync(ops);

            if (res.Item1)
            {
                UIApplication.SharedApplication.RegisterForRemoteNotifications();
            }
        }
    }

}
