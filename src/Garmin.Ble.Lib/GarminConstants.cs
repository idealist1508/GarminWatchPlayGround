namespace Garmin.Ble.Lib;

public static class GarminConstants
{
    public enum FileTypes : int
    {
        FitFile = 128
    }

    public enum FitFileSubTypes : int
    {
        Device = 1,
        Settings = 2,
        Sport = 3,
        Activity = 4,
        Workout = 5,
        Course = 6,
        Schedules = 7,
        Weight = 9,
        Totals = 10,
        Goals = 11,
        BloodPressure = 14,
        MonitoringA = 15,
        ActivitySummary = 20,
        MonitoringDaily = 28,
        MonitoringB = 32,
        Segment = 34,
        SegmentList = 35,
        ExdConfiguration = 40,
        MfgRangeMin = 0xF7,
        MfgRangeMax = 0xFE,
        Invalid = 0xFF
    }


    public enum GarminMessageType
    {
        SCHEDULES,
        SETTINGS,
        GOALS,
        WORKOUTS,
        COURSES,
        ACTIVITIES,
        PERSONAL_RECORDS,
        UNKNOWN_TYPE,
        SOFTWARE_UPDATE,
        DEVICE_SETTINGS,
        LANGUAGE_SETTINGS,
        USER_PROFILE,
        SPORTS,
        SEGMENT_LEADERS,
        GOLF_CLUB,
        WELLNESS_DEVICE_INFO,
        WELLNESS_DEVICE_CCF,
        INSTALL_APP,
        CHECK_BACK,
        TRUE_UP,
        SETTINGS_CHANGE,
        ACTIVITY_SUMMARY,
        METRICS_FILE,
        PACE_BAND
    }


    public static Guid UUID_SERVICE_GARMIN_1 = Guid.Parse("6A4E2401-667B-11E3-949A-0800200C9A66");
    public static Guid UUID_SERVICE_GARMIN_2 = Guid.Parse("6A4E2500-667B-11E3-949A-0800200C9A66");
    public static Guid UUID_SERVICE_GARMIN_3 = Guid.Parse("6a4e2800-667b-11e3-949a-0800200c9a66");

    public static Guid UUID_CHARACTERISTIC_GARMINSWIM2_GFDI_SEND =    Guid.Parse("6a4e2820-667b-11e3-949a-0800200c9a66");
    public static Guid UUID_CHARACTERISTIC_GARMINSWIM2_GFDI_RECEIVE = Guid.Parse("6a4e2810-667b-11e3-949a-0800200c9a66");

    public static Guid UUID_CHARACTERISTIC_GARMIN_GFDI_SEND = Guid.Parse("6a4e4c80-667b-11e3-949a-0800200c9a66");
    public static Guid UUID_CHARACTERISTIC_GARMIN_GFDI_RECEIVE = Guid.Parse("6a4ecd28-667b-11e3-949a-0800200c9a66");

    public static Guid UUID_CHARACTERISTIC_GARMIN_HEART_RATE = Guid.Parse("6a4e2501-667b-11e3-949a-0800200c9a66");
    public static Guid UUID_CHARACTERISTIC_GARMIN_STEPS = Guid.Parse("6a4e2502-667b-11e3-949a-0800200c9a66");
    public static Guid UUID_CHARACTERISTIC_GARMIN_CALORIES = Guid.Parse("6a4e2503-667b-11e3-949a-0800200c9a66");
    public static Guid UUID_CHARACTERISTIC_GARMIN_STAIRS = Guid.Parse("6a4e2504-667b-11e3-949a-0800200c9a66");
    public static Guid UUID_CHARACTERISTIC_GARMIN_INTENSITY = Guid.Parse("6a4e2505-667b-11e3-949a-0800200c9a66");

    public static Guid UUID_CHARACTERISTIC_GARMIN_HEART_RATE_VARIATION =
        Guid.Parse("6a4e2507-667b-11e3-949a-0800200c9a66");

    public static Guid UUID_CHARACTERISTIC_GARMIN_2_9 = Guid.Parse("6a4e2509-667b-11e3-949a-0800200c9a66");

    public const int STATUS_ACK = 0;
    public const int STATUS_NAK = 1;
    public const int STATUS_UNSUPPORTED = 2;
    public const int STATUS_DECODE_ERROR = 3;
    public const int STATUS_CRC_ERROR = 4;
    public const int STATUS_LENGTH_ERROR = 5;

    public const int GADGETBRIDGE_UNIT_NUMBER = 22222;

    public const int GARMIN_DEVICE_XML_FILE_INDEX = 65533;

    // TODO: Better capability management/configuration
    public static HashSet<GarminCapability> OUR_CAPABILITIES = Enum.GetValues<GarminCapability>().ToHashSet();

    public const int MAX_WRITE_SIZE = 20;

    /**
     * Garmin zero time in seconds since Epoch: 1989-12-31T00:00:00Z
     */
    public const int GARMIN_TIME_EPOCH = 631065600;

    public const string ANCS_DATE_FORMAT = "yyyyMMdd'T'HHmmss";

    public const int MESSAGE_RESPONSE = 5000;
    public const int MESSAGE_REQUEST = 5001;
    public const int MESSAGE_DOWNLOAD_REQUEST = 5002;
    public const int MESSAGE_UPLOAD_REQUEST = 5003;
    public const int MESSAGE_FILE_TRANSFER_DATA = 5004;
    public const int MESSAGE_CREATE_FILE_REQUEST = 5005;
    public const int MESSAGE_DIRECTORY_FILE_FILTER_REQUEST = 5007;
    public const int MESSAGE_FILE_READY = 5009;
    public const int MESSAGE_FIT_DEFINITION = 5011;
    public const int MESSAGE_FIT_DATA = 5012;
    public const int MESSAGE_WEATHER_REQUEST = 5014;
    public const int MESSAGE_BATTERY_STATUS = 5023;
    public const int MESSAGE_DEVICE_INFORMATION = 5024;
    public const int MESSAGE_DEVICE_SETTINGS = 5026;
    public const int MESSAGE_SYSTEM_EVENT = 5030;
    public const int MESSAGE_SUPPORTED_FILE_TYPES_REQUEST = 5031;
    public const int MESSAGE_NOTIFICATION_SOURCE = 5033;
    public const int MESSAGE_GNCS_CONTROL_POINT_REQUEST = 5034;
    public const int MESSAGE_GNCS_DATA_SOURCE = 5035;
    public const int MESSAGE_NOTIFICATION_SERVICE_SUBSCRIPTION = 5036;
    public const int MESSAGE_SYNC_REQUEST = 5037;
    public const int MESSAGE_FIND_MY_PHONE = 5039;
    public const int MESSAGE_CANCEL_FIND_MY_PHONE = 5040;
    public const int MESSAGE_MUSIC_CONTROL_CAPABILITIES = 5042;
    public const int MESSAGE_PROTOBUF_REQUEST = 5043;
    public const int MESSAGE_PROTOBUF_RESPONSE = 5044;
    public const int MESSAGE_MUSIC_CONTROL_ENTITY_UPDATE = 5049;
    public const int MESSAGE_CONFIGURATION = 5050;
    public const int MESSAGE_CURRENT_TIME_REQUEST = 5052;
    public const int MESSAGE_AUTH_NEGOTIATION = 5101;
    public const int MESSAGE_NA = 65535;
}