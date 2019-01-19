using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WIA;

namespace ChampionshipSolutions.ControlRoom.Scanning
{
    public static class WIAScanning
    {
        const string WIA_SCAN_COLOR_MODE = "6146";
        const string WIA_HORIZONTAL_SCAN_RESOLUTION_DPI = "6147";
        const string WIA_VERTICAL_SCAN_RESOLUTION_DPI = "6148";
        const string WIA_HORIZONTAL_SCAN_START_PIXEL = "6149";
        const string WIA_VERTICAL_SCAN_START_PIXEL = "6150";
        const string WIA_HORIZONTAL_SCAN_SIZE_PIXELS = "6151";
        const string WIA_VERTICAL_SCAN_SIZE_PIXELS = "6152";
        const string WIA_SCAN_BRIGHTNESS_PERCENTS = "6154";
        const string WIA_SCAN_CONTRAST_PERCENTS = "6155";

        private static void AdjustScannerSettings(IItem scannnerItem, int scanResolutionDPI, int scanStartLeftPixel, int scanStartTopPixel,
                    int scanWidthPixels, int scanHeightPixels, int brightnessPercents, int contrastPercents, int colorMode)
        {
            AdjustScannerSettings(scannnerItem, scanResolutionDPI);

            SetWIAProperty(scannnerItem.Properties, WIA_HORIZONTAL_SCAN_START_PIXEL, scanStartLeftPixel);
            SetWIAProperty(scannnerItem.Properties, WIA_VERTICAL_SCAN_START_PIXEL, scanStartTopPixel);
            SetWIAProperty(scannnerItem.Properties, WIA_HORIZONTAL_SCAN_SIZE_PIXELS, scanWidthPixels);
            SetWIAProperty(scannnerItem.Properties, WIA_VERTICAL_SCAN_SIZE_PIXELS, scanHeightPixels);
            SetWIAProperty(scannnerItem.Properties, WIA_SCAN_BRIGHTNESS_PERCENTS, brightnessPercents);
            SetWIAProperty(scannnerItem.Properties, WIA_SCAN_CONTRAST_PERCENTS, contrastPercents);
            SetWIAProperty(scannnerItem.Properties, WIA_SCAN_COLOR_MODE, colorMode);

        }

        private static void AdjustScannerSettings(IItem scannnerItem, int scanResolutionDPI)
        {
            SetWIAProperty(scannnerItem.Properties, WIA_HORIZONTAL_SCAN_RESOLUTION_DPI, scanResolutionDPI);
            SetWIAProperty(scannnerItem.Properties, WIA_VERTICAL_SCAN_RESOLUTION_DPI, scanResolutionDPI);
        }


        private static void SetWIAProperty(IProperties properties, object propName, object propValue)
        {
            Property prop = properties.get_Item(ref propName);
            prop.set_Value(ref propValue);
        }



        const string wiaFormatBMP = "{B96B3CAB-0728-11D3-9D7B-0000F81EF32E}";

        class WIA_DPS_DOCUMENT_HANDLING_SELECT
        {
            public const uint FEEDER = 0x00000001;
            public const uint FLATBED = 0x00000002;
        }

        class WIA_DPS_DOCUMENT_HANDLING_STATUS
        {
            public const uint FEED_READY = 0x00000001;
        }

        class WIA_PROPERTIES
        {
            public const uint WIA_RESERVED_FOR_NEW_PROPS = 1024;
            public const uint WIA_DIP_FIRST = 2;
            public const uint WIA_DPA_FIRST = WIA_DIP_FIRST + WIA_RESERVED_FOR_NEW_PROPS;
            public const uint WIA_DPC_FIRST = WIA_DPA_FIRST + WIA_RESERVED_FOR_NEW_PROPS;
            //
            // Scanner only device properties (DPS)
            //
            public const uint WIA_DPS_FIRST = WIA_DPC_FIRST + WIA_RESERVED_FOR_NEW_PROPS;
            public const uint WIA_DPS_DOCUMENT_HANDLING_STATUS = WIA_DPS_FIRST + 13;
            public const uint WIA_DPS_DOCUMENT_HANDLING_SELECT = WIA_DPS_FIRST + 14;
        }

        /// <summary>
        /// Use scanner to scan an image (with user selecting the scanner from a dialog).
        /// </summary>
        /// <returns>Scanned images.</returns>
        public static List<Image> Scan()
        {
            WIA.ICommonDialog dialog = new WIA.CommonDialog();
            WIA.Device device = dialog.ShowSelectDevice(WIA.WiaDeviceType.UnspecifiedDeviceType, true, false);

            if (device != null)
            {
                return Scan(device.DeviceID);
            }
            else
            {
                throw new Exception("You must select a device for scanning.");
            }
        }

        /// <summary>
        /// Use scanner to scan an image (scanner is selected by its unique id).
        /// </summary>
        /// <param name="scannerName"></param>
        /// <returns>Scanned images.</returns>
        public static List<Image> Scan(string scannerId)
        {
            List<Image> images = new List<Image>();

            bool hasMorePages = true;
            while (hasMorePages)
            {
                // select the correct scanner using the provided scannerId parameter
                WIA.DeviceManager manager = new WIA.DeviceManager();
                WIA.Device device = null;
                foreach (WIA.DeviceInfo info in manager.DeviceInfos)
                {
                    if (info.DeviceID == scannerId)
                    {
                        // connect to scanner
                        device = info.Connect();
                        break;
                    }
                }

                // device was not found
                if (device == null)
                {
                    // enumerate available devices
                    string availableDevices = "";
                    foreach (WIA.DeviceInfo info in manager.DeviceInfos)
                    {
                        availableDevices += info.DeviceID + "\n";
                    }

                    // show error with available devices

                    throw new Exception("The device with provided ID could not be found. Available Devices:\n" + availableDevices);
                }


                    WIA.Item item = device.Items[1] as WIA.Item;

                    try
                    {
                        // scan image

                        // Set DPI value
                        AdjustScannerSettings(item, 300); 

                        WIA.ICommonDialog wiaCommonDialog = new WIA.CommonDialog();
                        WIA.ImageFile image = (WIA.ImageFile)wiaCommonDialog.ShowTransfer(item, wiaFormatBMP, false);

                        if (image != null)
                        {
                            // save to temp file
                            string fileName = Path.GetTempFileName();
                            File.Delete(fileName);
                            image.SaveFile(fileName);
                            image = null;

                            // add file to output list
                            images.Add(Image.FromFile(fileName));
                        }
                    }
                    catch (Exception exc)
                    {
                        throw exc;
                    }
                    finally
                    {
                        item = null;

                        //determine if there are any more pages waiting
                        WIA.Property documentHandlingSelect = null;
                        WIA.Property documentHandlingStatus = null;

                        foreach (WIA.Property prop in device.Properties)
                        {
                            if (prop.PropertyID == WIA_PROPERTIES.WIA_DPS_DOCUMENT_HANDLING_SELECT)
                                documentHandlingSelect = prop;

                            if (prop.PropertyID == WIA_PROPERTIES.WIA_DPS_DOCUMENT_HANDLING_STATUS)
                                documentHandlingStatus = prop;
                        }

                        // assume there are no more pages
                        hasMorePages = false;

                        // may not exist on flatbed scanner but required for feeder
                        if (documentHandlingSelect != null)
                        {
                            // check for document feeder
                            if ((Convert.ToUInt32(documentHandlingSelect.get_Value()) & WIA_DPS_DOCUMENT_HANDLING_SELECT.FEEDER) != 0)
                            {
                                hasMorePages = ((Convert.ToUInt32(documentHandlingStatus.get_Value()) & WIA_DPS_DOCUMENT_HANDLING_STATUS.FEED_READY) != 0);
                            }
                        }

                    }
                }

            return images;
        }

        /// <summary>
        /// Gets the list of available WIA devices.
        /// </summary>
        /// <returns></returns>
        public static List<Tuple<string,string>> GetDevices()
        {
            List<Tuple<string,string>> devices = new List<Tuple<string,string>>();
            WIA.DeviceManager manager = new WIA.DeviceManager();
            
            foreach (WIA.DeviceInfo info in manager.DeviceInfos)
            {
                Device dev = info.Connect( );
                devices.Add( Tuple.Create( 
                    info.DeviceID,
                    GetDeviceProperty (dev, (int) WiaProperty .Name ).ToString()));
            }

            return devices;
        }

        // from https://stackoverflow.com/questions/6397457/get-wia-scanner-features

        public static DeviceInfo FindFirstScanner ( DeviceManager manager )
        {
            DeviceInfos infos = manager.DeviceInfos;
            foreach ( DeviceInfo info in infos )
                if ( info.Type == WiaDeviceType.ScannerDeviceType )
                    return info;
            return null;
        }

        public static Property FindProperty ( WIA.Properties properties , int propertyId )
        {
            foreach ( Property property in properties )
                if ( property.PropertyID == propertyId )
                    return property;
            return null;
        }

        public static void SetDeviceProperty ( Device device , int propertyId , object value )
        {
            Property property = FindProperty(device.Properties, propertyId);
            if ( property != null )
                property.set_Value( value );
        }

        public static object GetDeviceProperty ( Device device , int propertyId )
        {
            Property property = FindProperty(device.Properties, propertyId);
            return property != null ? property.get_Value( ) : null;
        }

        public static object GetItemProperty ( Item item , int propertyId )
        {
            Property property = FindProperty(item.Properties, propertyId);
            return property != null ? property.get_Value( ) : null;
        }

        public static void SetItemProperty ( Item item , int propertyId , object value )
        {
            Property property = FindProperty(item.Properties, propertyId);
            if ( property != null )
                property.set_Value( value );
        }

        public enum WiaProperty
        {
            DeviceId = 2,
            Manufacturer = 3,
            Description = 4,
            Type = 5,
            Port = 6,
            Name = 7,
            Server = 8,
            RemoteDevId = 9,
            UIClassId = 10,
            FirmwareVersion = 1026,
            ConnectStatus = 1027,
            DeviceTime = 1028,
            PicturesTaken = 2050,
            PicturesRemaining = 2051,
            ExposureMode = 2052,
            ExposureCompensation = 2053,
            ExposureTime = 2054,
            FNumber = 2055,
            FlashMode = 2056,
            FocusMode = 2057,
            FocusManualDist = 2058,
            ZoomPosition = 2059,
            PanPosition = 2060,
            TiltPostion = 2061,
            TimerMode = 2062,
            TimerValue = 2063,
            PowerMode = 2064,
            BatteryStatus = 2065,
            Dimension = 2070,
            HorizontalBedSize = 3074,
            VerticalBedSize = 3075,
            HorizontalSheetFeedSize = 3076,
            VerticalSheetFeedSize = 3077,
            SheetFeederRegistration = 3078,         // 0 = LEFT_JUSTIFIED, 1 = CENTERED, 2 = RIGHT_JUSTIFIED
            HorizontalBedRegistration = 3079,       // 0 = LEFT_JUSTIFIED, 1 = CENTERED, 2 = RIGHT_JUSTIFIED
            VerticalBedRegistraion = 3080,          // 0 = TOP_JUSTIFIED, 1 = CENTERED, 2 = BOTTOM_JUSTIFIED
            PlatenColor = 3081,
            PadColor = 3082,
            FilterSelect = 3083,
            DitherSelect = 3084,
            DitherPatternData = 3085,

            DocumentHandlingCapabilities = 3086,    // FEED = 0x01, FLAT = 0x02, DUP = 0x04, DETECT_FLAT = 0x08, 
                                                    // DETECT_SCAN = 0x10, DETECT_FEED = 0x20, DETECT_DUP = 0x40, 
                                                    // DETECT_FEED_AVAIL = 0x80, DETECT_DUP_AVAIL = 0x100
            DocumentHandlingStatus = 3087,          // FEED_READY = 0x01, FLAT_READY = 0x02, DUP_READY = 0x04, 
                                                    // FLAT_COVER_UP = 0x08, PATH_COVER_UP = 0x10, PAPER_JAM = 0x20
            DocumentHandlingSelect = 3088,          // FEEDER = 0x001, FLATBED = 0x002, DUPLEX = 0x004, FRONT_FIRST = 0x008
                                                    // BACK_FIRST = 0x010, FRONT_ONLY = 0x020, BACK_ONLY = 0x040
                                                    // NEXT_PAGE = 0x080, PREFEED = 0x100, AUTO_ADVANCE = 0x200
            DocumentHandlingCapacity = 3089,
            HorizontalOpticalResolution = 3090,
            VerticalOpticalResolution = 3091,
            EndorserCharacters = 3092,
            EndorserString = 3093,
            ScanAheadPages = 3094,                  // ALL_PAGES = 0
            MaxScanTime = 3095,
            Pages = 3096,                           // ALL_PAGES = 0
            PageSize = 3097,                        // A4 = 0, LETTER = 1, CUSTOM = 2
            PageWidth = 3098,
            PageHeight = 3099,
            Preview = 3100,                         // FINAL_SCAN = 0, PREVIEW = 1
            TransparencyAdapter = 3101,
            TransparecnyAdapterSelect = 3102,
            ItemName = 4098,
            FullItemName = 4099,
            ItemTimeStamp = 4100,
            ItemFlags = 4101,
            AccessRights = 4102,
            DataType = 4103,
            BitsPerPixel = 4104,
            PreferredFormat = 4105,
            Format = 4106,
            Compression = 4107,                     // 0 = NONE, JPG = 5, PNG = 8
            MediaType = 4108,
            ChannelsPerPixel = 4109,
            BitsPerChannel = 4110,
            Planar = 4111,
            PixelsPerLine = 4112,
            BytesPerLine = 4113,
            NumberOfLines = 4114,
            GammaCurves = 4115,
            ItemSize = 4116,
            ColorProfiles = 4117,
            BufferSize = 4118,
            RegionType = 4119,
            ColorProfileName = 4120,
            ApplicationAppliesColorMapping = 4121,
            StreamCompatibilityId = 4122,
            ThumbData = 5122,
            ThumbWidth = 5123,
            ThumbHeight = 5124,
            AudioAvailable = 5125,
            AudioFormat = 5126,
            AudioData = 5127,
            PicturesPerRow = 5128,
            SequenceNumber = 5129,
            TimeDelay = 5130,
            CurrentIntent = 6146,
            HorizontalResolution = 6147,
            VerticalResolution = 6148,
            HorizontalStartPostion = 6149,
            VerticalStartPosition = 6150,
            HorizontalExtent = 6151,
            VerticalExtent = 6152,
            PhotometricInterpretation = 6153,
            Brightness = 6154,
            Contrast = 6155,
            Orientation = 6156, // 0 = PORTRAIT, 1 = LANDSCAPE, 2 = 180°, 3 = 270°
            Rotation = 6157, // 0 = PORTRAIT, 1 = LANDSCAPE, 2 = 180°, 3 = 270°
            Mirror = 6158,
            Threshold = 6159,
            Invert = 6160,
            LampWarmUpTime = 6161,
        }

    }
}
