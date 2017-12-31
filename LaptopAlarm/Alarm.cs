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

using System.Media;
using LaptopAlarm;

public class Alarm
{
    private bool myplayAudio;
    private onalarm_audio_settings mysoundType;
    private string myaudioLocation;
    private bool myincreaseVolume;
    private SoundPlayer player;
    private bool isAlarmPlaying = false;

    /// <summary>
    /// Constructor for Alarm class.
    /// </summary>
    /// <param name="playAudio">Whether or not to play audio on alarm.</param>
    /// <param name="soundType">"onalarm_audio_settings" value if audio is default or custom.</param>
    /// <param name="audioLocation">If custom, path to a .wav file to use. If default, this parameter has no effect.</param>
    /// <param name="increaseVolume">Whether or not to increase volume on alarm.</param>
	public Alarm(bool playAudio, LaptopAlarm.onalarm_audio_settings soundType, string audioLocation, bool increaseVolume)
	{
        myplayAudio = playAudio;
        mysoundType = soundType;
        myaudioLocation = audioLocation;
        myincreaseVolume = increaseVolume;
	}

    /// <summary>
    /// Trip the alarm.
    /// </summary>
    public void causeAlarm()
    {
        if (myplayAudio)
        {
            if (mysoundType == onalarm_audio_settings.defaultSound)
            {
                player = new SoundPlayer(LaptopAlarm.Properties.Resources.laptop_theft_alarm);
            }
            else
            {
                player = new SoundPlayer(myaudioLocation);
            }
            isAlarmPlaying = true;
            player.PlayLooping();
        }
    }

    /// <summary>
    /// Stop the alarm.
    /// </summary>
    public void stopAlarm()
    {
        if (isAlarmPlaying)
        {
            player.Stop();
            isAlarmPlaying = false;
        }
    }
}
