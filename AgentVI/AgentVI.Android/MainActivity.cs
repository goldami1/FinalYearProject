using System;
using Android.App;
using Android.Content.PM;
using AgentVI.Droid;
using Xamarin.Forms;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Reflection;
using Octane.Xamarin.Forms.VideoPlayer.Android;
using FFImageLoading.Forms.Platform;
using AgentVI.Interfaces;
using AgentVI.Utils;
using Android.Content.Res;
using Plugin.CurrentActivity;

[assembly: Dependency(typeof(MainActivity))]
namespace AgentVI.Droid
{
    //Name = "com.test1.AgentVI",
    [Activity(Label = "AgentVI", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IBackButtonPressed, ISupportOrientation
    {
        private bool OrientationLocked { get; set; } = false;

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            CachedImageRenderer.Init(true);
            FormsVideoPlayer.Init();
            CrossCurrentActivity.Current.Activity = this;
            Rg.Plugins.Popup.Popup.Init(this, bundle);
            LoadApplication(new App());
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            if(OrientationLocked)
            {
                LockOrientation(Android.Content.Res.Orientation.Portrait);
            }
        }

        private void LockOrientation(Android.Content.Res.Orientation i_Orientation)
        {
            switch (i_Orientation)
            {
                case Android.Content.Res.Orientation.Landscape:
                    RequestedOrientation = ScreenOrientation.Landscape;
                    break;
                case Android.Content.Res.Orientation.Portrait:
                    RequestedOrientation = ScreenOrientation.Portrait;
                    break;
            }
            
        }

        public void NativeOnBackButtonPressed()
        {
            OnBackPressed();
        }

        public override void OnBackPressed()
        {
            MoveTaskToBack(true);
        }

        public void LockOrientation()
        {
            OrientationLocked = true;
        }

        public void UnLockOrientation()
        {
            OrientationLocked = false;
        }
    }
}