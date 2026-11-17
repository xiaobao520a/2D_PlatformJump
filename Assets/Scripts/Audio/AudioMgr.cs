using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 音乐和音效的管理器
/// </summary>
public class AudioMgr : Singleton_AutoWithMono<AudioMgr>
{
    void Awake()
    {
        backgroundMusicVolume=PlayerPrefs.GetFloat("MusicVolume",0.5f);
        soundEffectVolume=PlayerPrefs.GetFloat("SoundVolume",0.5f);
    }
    
    //背景音乐
    private AudioSource backgroundMusic=null;
    public float backgroundMusicVolume=0.5f; //背景音乐大小 默认0.5f

    //音效
    private GameObject soundEffectObj=null; //音效的各种AudioSource的依附对象
    private List<AudioSource>soundEffectList=new List<AudioSource>(); //管理正在播放的音效
    private Stack<AudioSource>sourcePool=new Stack<AudioSource>(); //音效的对象池

    public float soundEffectVolume=0.5f; //音效大小
    private bool isPlaySoundEffect=true; //音效是否在播放

    private const int PREWARM_COUNT=5; //预热音效的数量 
    private bool prewarm=false; //是否预热过音效

    //背景音乐相关
    //播放背景音乐
    public void PlayBackgroundMusic(string name,bool isPause=false)
    {
        //动态创建播放背景音乐的AudioSource组件
        //并且过场景时不移除 保证背景音乐在过场时也能播放
        if(backgroundMusic==null)
        {
            GameObject obj=new GameObject("BackgroundMusic");
            backgroundMusic=obj.AddComponent<AudioSource>();
            GameObject.DontDestroyOnLoad(obj);
        }

        //如果是暂停再播放 那就直接播放
        if(isPause)
        {
            if(backgroundMusic.clip!=null)
            {
                backgroundMusic.volume=backgroundMusicVolume;
                backgroundMusic.UnPause();
            }
        }
        else
        {
            ABMgr.Instance.LoadRes<AudioClip>("music", name, (clip) =>
            {
                backgroundMusic.clip=clip;
                backgroundMusic.loop=true;
                backgroundMusic.volume=backgroundMusicVolume;
                backgroundMusic.Play();
            });
        }
    }

    //停止背景音乐
    public void StopBackgroundMusic()
    {
        if(backgroundMusic==null) return;

        backgroundMusic.Stop();
    }

    //暂停背景音乐
    public void PauseBackgroundMusic()
    {
        if(backgroundMusic==null) return;

        backgroundMusic.Pause();
    }

    //设置背景音乐声音大小
    public void SetBackgroundMusicVolume(float v)
    {
        backgroundMusicVolume=Mathf.Clamp01(v);
        if(backgroundMusic!=null)
            backgroundMusic.volume=backgroundMusicVolume;
    }

    //音效相关
    //音效对象池相关
    //确保有音效父物体 并且在首次使用时预热池子 挂载初始的AudioSource组件
    private void EnsureSoundEffectObj()
    {
        if(soundEffectObj==null)
        {
            sourcePool.Clear(); //先清掉池子 防止切场景之后 池子里还存着原来的AudioSource
            soundEffectList.Clear(); //同理 清掉list 清理掉剩余的音效
            GameObject obj=new GameObject("SoundEffect");
            soundEffectObj=obj;
            prewarm=false; //切场景之后 需要重新预热
        }

        //如果没预热就先预热 挂载audioSource组件
        if(!prewarm)
        {
            for(int i=0;i<PREWARM_COUNT;i++)
            {
                AudioSource source=soundEffectObj.AddComponent<AudioSource>();
                source.playOnAwake=false;
                sourcePool.Push(source);
            }
            prewarm=true;
        }
    }

    //从池子中取AudioSource的方法 
    private AudioSource GetSource()
    {
        //如果池子里有就直接取
        while(sourcePool.Count>0)
        {
            AudioSource source=sourcePool.Pop();
            if(source!=null)
                return source;
        }

        //没有就先确保有音效父物体 并且预热池子
        EnsureSoundEffectObj();

        //这是比较保险的做法 新增一个一定可以拿到的 不论预热与否
        AudioSource newSource=soundEffectObj.AddComponent<AudioSource>();
        newSource.playOnAwake=false;
        return newSource;
    }

    //放回池子 重置状态
    private void ReturnSource(AudioSource source)
    {
        if(source==null) return;

        source.Stop();
        source.clip=null;
        source.loop=false;
        source.volume=soundEffectVolume;
        sourcePool.Push(source);
    }

    //播放结束后回收 比如非循环音效 播放完就回收
    private IEnumerator RecycleAfterPlay(AudioSource source,float delay)
    {
        //防止可能播到一半暂停了 没有播放完就销毁
        while (source.isPlaying) yield return null;

        yield return new WaitForSeconds(delay);

        if(source!=null&&soundEffectList.Contains(source))
        {
            soundEffectList.Remove(source);
            ReturnSource(source);
        }
    }

    //播放音效
    public void PlaySoundEffect(string name,bool isLoop=false,bool isSync=false,UnityAction<AudioSource>callBack=null)
    {
        //全局音效静音时 新音效也不播
        if(!isPlaySoundEffect) return;

        //每次播放前先预热池子 确保有音效父对象
        EnsureSoundEffectObj();

        ABMgr.Instance.LoadRes<AudioClip>("sound", name, (clip) =>
        {
            if(clip==null) return;

            AudioSource source=GetSource();
            source.clip=clip;
            source.loop=isLoop;
            source.volume=soundEffectVolume;
            source.Play();
            soundEffectList.Add(source);

            //如果是非循环音效 播放结束后就自动结束
            if(!isLoop) StartCoroutine(RecycleAfterPlay(source,clip.length));

            callBack?.Invoke(source);
        },isSync);
    }

    //停止音效
    public void StopSoundEffect(AudioSource source)
    {
        if(source==null) return;
        if(soundEffectList.Contains(source))
        {
            soundEffectList.Remove(source);
            ReturnSource(source);
        }
    }

    //设置所有音效大小
    public void SetSoundEffectVolume(float v)
    {
        soundEffectVolume=Mathf.Clamp01(v); //把音效大小钳制在0-1之间

        foreach(var source in soundEffectList)
        {
            if(source!=null)
            source.volume=soundEffectVolume;
        }
    }

    //播放或暂停所有音效
    public void PlayOrPauseAllSoundEffect(bool isPlay)
    {
        isPlaySoundEffect=isPlay;

        foreach(var source in soundEffectList)
        {
            if(source==null) continue;

            if(isPlay)
                source.UnPause();
            else
                source.Pause();
        }
    }


}
