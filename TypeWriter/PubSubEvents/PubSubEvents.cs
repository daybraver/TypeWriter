namespace TypeWriter.PubSubEvents
{
    public enum AudioControlType
    {
        Next,
        Previous,
        Forward,
        Back,
        ResetSpeedRatio,
        IncrementSpeedRatio,
        DecrementSpeedRatio
    }

    public enum AudioPlayMode
    {
        SingleLoop,
        ListLoop,
        OrderPlay,
        RandomPlay
    }
}
