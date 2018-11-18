using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Linq;


namespace InnoviApiProxy
{
    public class Sensor : InnoviObject
    {
        private List<string> m_LiveViewTestLinks;

        [JsonProperty("id")]
        internal int sensorId { get; private set; }
        [JsonProperty]
        private int accountId { get; set; }
        [JsonProperty]
        private int folderId { get; set; }
        [JsonProperty]
        public string Name { get; private set; }
        [JsonProperty("type")]
        public eSensorType SensorType { get; private set; }
        [JsonProperty("status")]
        public eSensorStatus Status { get; private set; }
        [JsonProperty("streamType")]
        public eStreamType StreamType { get; private set; }
        [JsonProperty]
        public Coordinate GeoLocation { get; private set; }
        [JsonProperty]
        public List<Coordinate> GeoArea { get; private set; }
        [JsonProperty]
        public List<KeyValuePair<string, string>> Tags { get; private set; }
        [JsonProperty]
        public string LoginKey { get; private set; }
        [JsonProperty("recording")]
        public bool IsRecording { get; private set; }
        [JsonProperty("createEventImage")]
        public bool IsEventImageCreationRequired { get; private set; }
        [JsonProperty("createEventClip")]
        public bool IsEventClipCreationRequired { get; private set; }
        [JsonProperty("enabledByUser")]
        public bool IsEnabledByUser { get; private set; }
        [JsonProperty("optimizationEnabled")]
        public bool IsOptimizationEnabled { get; private set; }
        [JsonProperty]
        public uint AlarmInterval { get; private set; }
        protected override int Id => sensorId;

        // This property returns static data as this functionality is not covered by the API at this point
        public string LiveView
        {
            get
            {
                Random random = new Random();
                int liveViewListSize = m_LiveViewTestLinks.Count;
                int index = random.Next(liveViewListSize);

                return m_LiveViewTestLinks[index];
            }
        }


        public List<Health> SensorHealthArray
        {
            get
            {
                List<Health> healthArray = HttpUtils.GetSensorHealthArray(sensorId);

                foreach (Health healthObject in healthArray)
                {
                    eSensorStatus status = healthObject.Status;
                    string detailedDescription = string.Empty;

                    setHealthStatusFields(healthObject.Description, out status, out detailedDescription);

                    healthObject.Status = status;
                    healthObject.DetailedDescription = detailedDescription;

                    healthObject.SensorName = Name;
                }

                foreach (Health healthObject in healthArray)
                {
                    if (healthObject != healthArray.Last())
                    {
                        long currentTime = healthObject.StatusTimeStamp;
                        long nextTime = healthArray[healthArray.IndexOf(healthObject) + 1].StatusTimeStamp;
                        healthObject.Duration = nextTime - currentTime;
                    }
                }

                return healthArray;
            }
        }


        public InnoviObjectCollection<SensorEvent> SensorEvents
        {
            get
            {
                return new InnoviObjectCollection<SensorEvent>(HttpUtils.GetSensorEvents, sensorId);
            }
            private set
            {
                SensorEvents = value;
            }
        }

        public string ReferenceImage
        {
            get
            {
                return Settings.InnoviApiEndpoint + "sensorImage?accountId=" + accountId.ToString() + "&sensorId=" + sensorId.ToString();
            }
        }

        public void Arm()
        {
            throw new Exception("Not yet implemented");
        }

        public void Disarm()
        {
            throw new Exception("Not yet implemented");
        }

        public enum eSensorEventTag
        {
            None,
            False,
            True
        }

        public enum eSensorStatus
        {
            Undefined,
            Active,
            Warning,
            Error,
            Inactive
        }

        public enum eSensorType
        {
            Undefined,
            Ccd,
            Thermal
        }

        public enum eStreamType
        {
            Undefined,
            Rtsp,
            Onvif,
            Clip,
            Rtp,
            Multicast,
            Kinesis
        }

       
        public enum eStatusDescription : long 
        {
            Undefined = 0x0,
            Active = 0x00000001,
            NoVideo = 0x00000004,
            NoCommunication = 0x00000008,
            DetectionMalfunction = 0x00000020,
            ImageBlocked = 0x00000040,
            ImageSaturated = 0x00000080,
            ImageDark = 0x00000100,
            FeatureTimeOut = 0x00001000,
            LowFrameRate = 0x00100000,
            NoAgent = 0x00400000,
            Disabled = 0x00800000,
            StreamNotConfigured = 0x01000000,
            StreamDisabled = 0x02000000,
            HighLatency = 0x04000000,
            LowFr = 0x08000000,
            ResourceNotExist = 0x0000000010000000,
            StreamAuthError = 0x0000000020000000,
            OnVifUnknownError = 0x0000000040000000,
            RtspUnknownError = 0x0000000080000000,
            StreamResolutionLow = 0x0000000100000000,
            StreamResolutionHigh = 0x0200000000,
            StreamFpsLow = 0x0000000400000000,
            StreamFpsHigh = 0x00800000000,
            StreamCodecNotSupported = 0x01000000000,
            StreamGeneralUnknownError = 0x0000002000000000,
            AgentVersionDeprecated = 0x0000004000000000,
            LowResolutionThreshold = 0x0000008000000000

        }

        internal Sensor()
        {
            m_LiveViewTestLinks = new List<string>();
            m_LiveViewTestLinks.Add("http://96.70.163.66/mjpg/video.mjpg");
            m_LiveViewTestLinks.Add("http://166.161.110.55/-wvhttp-01-/GetOneShot?image_size=640x480&frame_count=1000000000");
            m_LiveViewTestLinks.Add("http://24.221.30.55:8080/-wvhttp-01-/GetOneShot?image_size=640x480&frame_count=1000000000");
        }

        public class Health
        {
            [JsonProperty("timestamp")]
            public long StatusTimeStamp { get; private set; }
      
            public eSensorStatus Status { get; internal set; }
            [JsonProperty("value")]
            public eStatusDescription Description { get; private set; }

            public string DetailedDescription { get; internal set; }
            public long Duration { get; internal set; }
            public string SensorName { get; internal set; }
        }

        private void setHealthStatusFields(eStatusDescription i_StatusDescription, out eSensorStatus o_Status, out string o_DetailedDescription)
        {
            switch (i_StatusDescription)
            {
                case eStatusDescription.Undefined:
                    o_Status = eSensorStatus.Undefined;
                    o_DetailedDescription = string.Empty;
                    break;
                case eStatusDescription.Active:
                    o_Status = eSensorStatus.Active;
                    o_DetailedDescription = string.Empty;
                    break;
                case eStatusDescription.NoVideo:
                    o_Status = eSensorStatus.Error;
                    o_DetailedDescription = "No video";
                    break;
                case eStatusDescription.NoCommunication:
                    o_Status = eSensorStatus.Error;
                    o_DetailedDescription = "AS service not available. set from health-service";
                    break;
                case eStatusDescription.DetectionMalfunction:
                    o_Status = eSensorStatus.Error;
                    o_DetailedDescription = "Failed to add detector to sensor or feature timeout";
                    break;
                case eStatusDescription.ImageBlocked:
                    o_Status = eSensorStatus.Error;
                    o_DetailedDescription = "70% of the image is FG for 4 second. Maximum blocked time is 10 minutes";
                    break;
                case eStatusDescription.ImageSaturated:
                    o_Status = eSensorStatus.Error;
                    o_DetailedDescription = "75% of the ROI (overlapping rules - if no rules whole image) has greyscale levels above 245 for 10 seconds";
                    break;
                case eStatusDescription.ImageDark:
                    o_Status = eSensorStatus.Error;
                    o_DetailedDescription = "60% of the ROI (overlapping rules - if no rules whole image) has greyscale levels below 16 for 10 seconds";
                    break;
                case eStatusDescription.FeatureTimeOut:
                    o_Status = eSensorStatus.Error;
                    o_DetailedDescription = "No feature some time (5 sec). for active sensor";
                    break;
                case eStatusDescription.LowFrameRate:
                    o_Status = eSensorStatus.Warning;
                    o_DetailedDescription = "Feature processing is low (depends: threshold and duration";
                    break;
                case eStatusDescription.NoAgent:
                    o_Status = eSensorStatus.Error;
                    o_DetailedDescription = "No Agent";
                    break;
                case eStatusDescription.Disabled:
                    o_Status = eSensorStatus.Undefined;
                    o_DetailedDescription = "Sensor in disable mode (feature sending disabled)";
                    break;
                case eStatusDescription.StreamNotConfigured:
                    o_Status = eSensorStatus.Error;
                    o_DetailedDescription = "Sensor first connection";
                    break;
                case eStatusDescription.StreamDisabled:
                    o_Status = eSensorStatus.Undefined;
                    o_DetailedDescription = "Video source disabled (no streaming video from video source)";
                    break;
                case eStatusDescription.HighLatency:
                    o_Status = eSensorStatus.Warning;
                    o_DetailedDescription = "Network latency";
                    break;
                case eStatusDescription.LowFr:
                    o_Status = eSensorStatus.Warning;
                    o_DetailedDescription = "Video source low frame rate";
                    break;
                case eStatusDescription.ResourceNotExist:
                    o_Status = eSensorStatus.Error;
                    o_DetailedDescription = "Ping failed or file not exist";
                    break;
                case eStatusDescription.StreamAuthError:
                    o_Status = eSensorStatus.Error;
                    o_DetailedDescription = "RTSP authorization error";
                    break;
                case eStatusDescription.OnVifUnknownError:
                    o_Status = eSensorStatus.Error;
                    o_DetailedDescription = "ONVIF error";
                    break;
                case eStatusDescription.RtspUnknownError:
                    o_Status = eSensorStatus.Error;
                    o_DetailedDescription = "RTSP Error";
                    break;
                case eStatusDescription.StreamResolutionLow:
                    o_Status = eSensorStatus.Error;
                    o_DetailedDescription = "Resolution not in range (with < 320)";
                    break;
                case eStatusDescription.StreamResolutionHigh:
                    o_Status = eSensorStatus.Error;
                    o_DetailedDescription = "resolution not in range (with > 1920)";
                    break;
                case eStatusDescription.StreamFpsLow:
                    o_Status = eSensorStatus.Error;
                    o_DetailedDescription = "Frame rate not in range (< 6 fps)";
                    break;
                case eStatusDescription.StreamFpsHigh:
                    o_Status = eSensorStatus.Error;
                    o_DetailedDescription = "Frame rate not in range (> 35 fps)";
                    break;
                case eStatusDescription.StreamCodecNotSupported:
                    o_Status = eSensorStatus.Error;
                    o_DetailedDescription = "Codec format not supported (support formats: MJPEG/H264/MPEG Part2/H265)";
                    break;
                case eStatusDescription.StreamGeneralUnknownError:
                    o_Status = eSensorStatus.Error;
                    o_DetailedDescription = "General stream error";
                    break;
                case eStatusDescription.AgentVersionDeprecated:
                    o_Status = eSensorStatus.Warning;
                    o_DetailedDescription = "Agent version is deprecated";
                    break;
                case eStatusDescription.LowResolutionThreshold:
                    o_Status = eSensorStatus.Warning;
                    o_DetailedDescription = "Low resolution";
                    break;
                default:
                    o_Status = eSensorStatus.Undefined;
                    o_DetailedDescription = string.Empty;
                    break;
            }
        }
    }
}
