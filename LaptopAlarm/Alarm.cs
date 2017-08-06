// This file is part of LaptopAlarm.
// 
// LaptopAlarm is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// LaptopAlarm is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with LaptopAlarm.  If not, see <http://www.gnu.org/licenses/>.

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
