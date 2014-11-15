namespace EyeTracking.SDK
{
    public interface IEyeTrackingAlgorithm
    {
        TrackedPosition DetectPupils();
        TrackedPosition TrackPupils();
    }
}
