using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace AgentVI.Utils
{
    public static class Settings
    {
        public static Thickness iOSPlatformPadding { get; } = new Thickness(0,30,0,0);
        public static Thickness PaddingForViewCellInListView { get; } = new Thickness(5);
        public static Thickness ListViewMargins { get; } = new Thickness(0,5,0,5);
        public static Thickness HeaderLeftTopMargine { get; } = new Thickness(5, 5, 5, 0);
        public static Thickness HeaderMargine { get; } = new Thickness(5, 0, 5, 0);
        public static Thickness ListviewGridMargines { get; } = new Thickness(30, 0, 30, 0);
        public static Thickness ContentViewMargines { get; } = new Thickness(0, 5, 0, 5);
        public static Thickness FilterTextMargines { get; } = new Thickness(5, 0, 5, 0);
        public static Thickness PictureHolderFramePadding { get; } = new Thickness(5);
        public static Thickness PictureHolderOutsideFrameMargines { get; } = new Thickness(30, 15, 30, 15);
        public static Thickness PictureHolderOutsideFramePaddings { get; } = new Thickness(10);

        public static double GridElementSizeRequest { get; } = 140;
        
        public static double GenericTitleFontSize { get; } = 22;
        public static double HeaderTitleFontSize { get; } = 19;
        public static double SensorEventNameFontSize { get; } = 15;
        public static double SensorEventDetailsFontSize { get; } = 13;
        public static double BehaviorDescriptionHeaderFontSize { get; } = 15;
        public static double FilterTextSize { get; } = 15;
        public static double EmptyPageFillerFontSize { get; } = 22;
        public static double EmptyPageFillerFontSmallSize { get; } = 15;
        public static double ActionMenuItemFontSize { get; } = 20;

        public static Color FooterColor { get; } = Color.FromHex("#003F7F");
        public static Color FooterSeparatorColor { get; } = Color.FromHex("#DCDCDC");
        public static Color PictureHolderFrameBackground { get; } = Color.FromHex("#2C2D30");
        public static Color PictureHolderOutsideFrameBackground { get; } = Color.FromHex("#434449");
        public static Color PictureHolderBorderColor { get; } = Color.FromHex("#4E5663");
        public static Color SensorEventHeaderFontColor { get; } = Color.FromHex("#EFE4B0");
        public static Color SensorEventDetailsFontColor { get; } = Color.FromHex("#B4B7C6");
        public static Color FilterTextColor { get; } = Color.Black;
        public static Color FilterBarBorderColor { get; } = Color.FromHex("#7F7F7F");
        public static Color FilterBarBackgroundColor { get; } = Color.Transparent;
        public static Color AccountNameStringColor { get; } = Color.FromHex("#00366C");
        public static Color CustomViewCellBackgroundColor { get; } = Color.Transparent;
        public static Color FolderNameAndSensorHeaderName { get; } = Color.FromHex("#1A333F");
        public static Color LogoutButtonBackgroundColor { get; } = Color.FromHex("#0067CE");
        public static Color LoginButtonBackgroundColor { get; } = Color.FromHex("#2FC0FF");
        public static Color ForgotMyPasswordTextColor { get; } = Color.FromHex("#0089C4");
        public static Color BehaviorTypeAndDescriptionTextColor { get; } = Color.FromHex("#00274F");
        public static Color VisualColoredControllersColor { get; } = Color.Navy;

        public static string EventsTabSVGPath { get; } = "resource://AgentVI.Sources.Icons.events_inactive_24px.svg";
        public static string EventsTabSelectedSVGPath { get; } = "resource://AgentVI.Sources.Icons.events_active_24px.svg";
        public static string SensorsTabSVGPath { get; } = "resource://AgentVI.Sources.Icons.sensors_inactive_24px.svg";
        public static string SensorsTabSelectedSVGPath { get; } = "resource://AgentVI.Sources.Icons.sensors_active_24px.svg";
        public static string SettingsTabSVGPath { get; } = "resource://AgentVI.Sources.Icons.settings_inactive_24px.svg";
        public static string SettingsTabSelectedSVGPath { get; } = "resource://AgentVI.Sources.Icons.settings_active_24px.svg";
        public static string HealthTabSVGPath { get; } = "resource://AgentVI.Sources.Icons.health_inactive_24px.svg";
        public static string HealthTabSelectedSVGPath { get; } = "resource://AgentVI.Sources.Icons.health_active_24px.svg";

        public static string ObjectTypeBicycleSVGPath { get; } = "resource://AgentVI.Sources.Icons.object_bicycle_24px.svg";
        public static string ObjectTypeBusSVGPath { get; } = "resource://AgentVI.Sources.Icons.object_bus_24px.svg";
        public static string ObjectTypeCarSVGPath { get; } = "resource://AgentVI.Sources.Icons.object_car_24px.svg";
        public static string ObjectTypeMotorcycleSVGPath { get; } = "resource://AgentVI.Sources.Icons.object_motorcycl_24px.svg";
        public static string ObjectTypePersonSVGPath { get; } = "resource://AgentVI.Sources.Icons.object_person_24px.svg";
        public static string ObjectTypeTruckSVGPath { get; } = "resource://AgentVI.Sources.Icons.object_truck_24px.svg";

        public static string LoadingAnimationPath { get; } = "resource://AgentVI.Sources.LoadingAnimation.gif";
        public static string EmptyCollectionSVGPath { get; } = "resource://AgentVI.Sources.EmptyPageFiller.svg";
        public static string ContextMenuSVGPath { get; } = "resource://AgentVI.Sources.Icons.contextMenu.svg";
        public static string BackButtonSVGPath { get; } = "resource://AgentVI.Sources.Icons.leftArrow.svg";
        public static string NextLevelButtonSVGPath { get; } = "resource://AgentVI.Sources.Icons.rightArrow.svg";
        public static string UpButtonSVGPath { get; } = "resource://AgentVI.Sources.Icons.upArrow.svg";
        public static string ResetButtonSVGPath { get; } = "resource://AgentVI.Sources.Icons.rootIcon.svg";
        public static string BrowseButtonSVGPath { get; } = "resource://AgentVI.Sources.Icons.browseIcon.svg";
        public static string FolderIconSVGPath { get; } = "resource://AgentVI.Sources.Icons.folderIcon.svg";
        public static string FilterBrowseIconSVGPath { get; } = "resource://AgentVI.Sources.Icons.folderIcon.svg";
        public static string DividerIconSVGPath { get; } = "resource://AgentVI.Sources.Icons.dividerIcon.svg";

        public static ImageSource BackgroundPicturePath { get; } = ImageSource.FromResource("AgentVI.Sources.background.png");
        public static ImageSource LogoPicturePath { get; } = ImageSource.FromResource("AgentVI.Sources.innovi_logo.png");


        public static string EmptyFillerTextMessageSensors = "No sensors for selected folder";
        public static string EmptyFillerTextMessageEvents = "No events for selected folder";
        public static string EmptyFillerTextMessageSensorEvents = "No events for selected sensor";
        public static string EmptyFillerTextMessageClip = "Clip isn't available";
        public static string DateTimeFormat { get; } = "dd/MM/yyyy hh:mm tt";
        public static string SearchBarPlaceHolderText { get; } = "Search...";
        public static string ErrorTitleAlertText { get; } = "Error";
        public static string ErrorButtonAlertText { get; } = "Retry";

        public static bool IsHealthFetchingEnabled { get; } = true;
    }
}
