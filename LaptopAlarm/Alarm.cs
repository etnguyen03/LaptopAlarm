using System;

public class Alarm
{
    private bool myplayAudio;
    private LaptopAlarm.onalarm_audio_settings mysoundType;
    private string myaudioLocation;
    private bool myincreaseVolume;
    private System.Media.SoundPlayer player;
    private bool isAlarmPlaying = false;


	public Alarm(bool playAudio, LaptopAlarm.onalarm_audio_settings soundType, string audioLocation, bool increaseVolume)
	{
        myplayAudio = playAudio;
        mysoundType = soundType;
        myaudioLocation = audioLocation;
        myincreaseVolume = increaseVolume;
	}

    public void causeAlarm()
    {
        if (myplayAudio)
        {
            if (mysoundType == LaptopAlarm.onalarm_audio_settings.defaultSound)
            {
                player = new System.Media.SoundPlayer(LaptopAlarm.Properties.Resources.laptop_theft_alarm);
            }
            else
            {
                player = new System.Media.SoundPlayer(myaudioLocation);
            }
            isAlarmPlaying = true;
            player.PlayLooping();
        }
    }

    public void stopAlarm()
    {
        if (isAlarmPlaying)
        {
            player.Stop();
            isAlarmPlaying = false;
        }
    }
}
