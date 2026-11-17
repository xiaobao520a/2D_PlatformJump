using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//设置面板
public class SettingPanel : BasePanel
{
    private Slider slider_Music;
    private Slider slider_Sound;

    protected override void Init()
    {
        base.Init();

        slider_Music=GetControl<Slider>("Slider_Music");
        slider_Sound=GetControl<Slider>("Slider_Sound");

        slider_Music.value=AudioMgr.Instance.backgroundMusicVolume;
        slider_Sound.value=AudioMgr.Instance.soundEffectVolume;
    }

    //设置音效大小
    protected override void SliderValueChanged(string sliderName, float value)
    {
        switch(sliderName)
        {
            case "Slider_Music":
            AudioMgr.Instance.SetBackgroundMusicVolume(value);
            break;

            case "Slider_Sound":
            AudioMgr.Instance.SetSoundEffectVolume(value);
            break;
        }
    }

    //保存设置
    protected override void ClickBtn(string btnName)
    {
        switch(btnName)
        {
            case "Btn_Sure":
            PlayerPrefs.SetFloat("MusicVolume",slider_Music.value);
            PlayerPrefs.SetFloat("SoundVolume",slider_Sound.value);
            UIMgr.Instance.HidePanel<SettingPanel>();
            UIMgr.Instance.ShowPanel<StartPanel>(E_UILayer.Middle,null,true);
            break;
        }
    }
}
