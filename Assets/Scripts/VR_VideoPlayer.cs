using FNI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System.IO;
using System.Security.Permissions;
using UnityEngine.UI;

namespace FNI
{
    public enum SeekState
    {
        Down,
        Drag,
        Up,
        Order,
        Complete
    }

    public enum LoadState
    {
        Fail,
        Succeed,
        Loading
    }

    public class VR_VideoPlayer : MonoBehaviour
    {
        #region Singleton
        private static VR_VideoPlayer _instance;
        public static VR_VideoPlayer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<VR_VideoPlayer>();
                }
                return _instance;
            }
        }
        #endregion

        #region Property
        private VideoPlayer videoPlayer;
        public VideoPlayer MyVideoPlayer
        {
            get
            {
                if (videoPlayer == null)
                    videoPlayer = transform.GetComponent<VideoPlayer>();

                return videoPlayer;
            }
        }

        /// <summary>
        /// 영상이 무엇이든 플레이 중인지 확인합니다.
        /// </summary>
        public bool IsPlaying { get { return MyVideoPlayer.isPlaying; } }

        /// <summary>
        /// 반복할지 안할지 확인합니다.
        /// </summary>
        public bool IsLooping { get { return isLoop; } }

        /// <summary>
        /// 재생중인 영상의 현재 프레임입니다.
        /// </summary>
        public long Frame { get { return MyVideoPlayer.frame; } set { MyVideoPlayer.frame = value; } }

        /// <summary>
        /// 재생중인 영상의 현재 시간입니다.
        /// </summary>
        public double VideoTime
        {
            get => MyVideoPlayer.time;
            set => MyVideoPlayer.time = value;
        }
        /// <summary>
        /// 영상의 총 길이입니다.
        /// </summary>
        public ulong Duration { get { return (ulong)(MyVideoPlayer.frameCount / MyVideoPlayer.frameRate); } }

        /// <summary>
        /// 현재 시간의 노말라이즈 입니다.
        /// </summary>
        public double NTime { get { return VideoTime / Duration; } set { MyVideoPlayer.time = value * Duration; } }

        /// <summary>
        /// 영상이 준비 되었는지 확인합니다.
        /// </summary>
        public bool IsPrepared { get { return MyVideoPlayer.isPrepared; } }

        /// <summary>
        /// Seek가 가능한지 확인합니다.
        /// </summary>
        public bool CanSeek { get { return MyVideoPlayer.canSetTime; } }

        /// <summary>
        /// 현재 영상이 반복 중인지 체크합니다.
        /// </summary>
        public bool IsRepeating { get { return isRepeat; } set { isRepeat = value; } }
        #endregion

        public bool isFinish = false;

        private bool pastPlaying;
        private double repeatSTime;
        private double repeatETime;
        private bool isLoop;
        private bool isRepeat;
        private bool isDraged = false;

        private bool endVideoCheck = false;

        public LoadState loadState;
        public SeekState seekState = SeekState.Complete;

        private IEnumerator loadRoutine;

        private void Awake()
        {
            //이벤트 연결
            MyVideoPlayer.errorReceived += ErrorReceived_Event;
            MyVideoPlayer.frameReady += FrameReady_Event;
            MyVideoPlayer.loopPointReached += LoopPointReached_Event;
            MyVideoPlayer.prepareCompleted += PrepareCompleted_Event;
            MyVideoPlayer.seekCompleted += SeekCompleted_Event;
            MyVideoPlayer.started += Started_Event;

        }

        private void Start()
        {
            StartCoroutine(Load(Application.dataPath + "/../Video/Elevator_01.mp4"));
        }


        private void Update()
        {
            if (isRepeat)
            {
                if (seekState == SeekState.Complete &&
                    (VideoTime < repeatSTime || repeatETime < VideoTime))
                {
                    seekState = SeekState.Order;
                    Seek(repeatSTime);
                }
            }
#if UNITY_EDITOR

            //if (Input.GetKeyDown(KeyCode.R))
            //{
            //    MyVideoPlayer.Prepare();
            //    Debug.Log("Prepare");
            //}

            //if (Input.GetKeyDown(KeyCode.P))
            //{
            //    MyVideoPlayer.Play();
            //}

#endif
        }

        ///// <summary>
        ///// Play Pause Stop 전용
        ///// </summary>
        ///// <param name="key"></param>
        //public void SetVideoCtrl(VideoState key)
        //{
        //    switch (key)
        //    {
        //        case VideoState.Play:
        //            Play();
        //            break;
        //        case VideoState.Pause:
        //            Pause();
        //            break;
        //        case VideoState.Stop:
        //            Stop();
        //            break;
        //    }
        //}


        /// <summary>
        /// Load 기능
        /// </summary>
        /// <param name="path">영상경로</param>
        /// <returns></returns>
        private IEnumerator Load(string path)
        {
            Debug.Log($"[Load/VR_VideoPlayer]\n비디오 경로 : <color=yellow>[{path}]</color>");

            if (path != "")
            {
                if (CheckFileExists(path))
                {
                    loadState = LoadState.Loading;
                    MyVideoPlayer.url = path; // 기존 영상 상태 초기화

                    MyVideoPlayer.Prepare();

                    while (!IsPrepared)
                    {
                        //Debug.Log("[Load] Preparing....");
                        yield return null;
                    }

                    Application.targetFrameRate = Convert.ToInt32(Math.Round(MyVideoPlayer.frameRate)); // 프레임 고정
                    //Debug.Log($"{Application.targetFrameRate}");

                    loadState = LoadState.Succeed;

                }
                else
                {
                    Debug.Log($"[{path}] File Not Exist");
                    loadState = LoadState.Fail;
                }
            }
            else
            {
                loadState = LoadState.Fail;
            }

            isFinish = true;

            if (loadState == LoadState.Succeed)
                Play();
        }

        /// <summary>
        /// 영상 Play
        /// </summary>
        private void Play()
        {
            if (!IsPrepared) return;
            if (loadState != LoadState.Succeed)
            {
                Debug.LogError("비디오가 로드되지 않았습니다.");

                return;
            }

            MyVideoPlayer.Play();
            endVideoCheck = false;

            
            // 재생 영상 정보 Debug
            Debug.Log($"[Play/VR_VideoPlayer] 영상 재생 정보\nTime : <color=yellow>{MyVideoPlayer.time}</color>\n" +
                $"영상 길이 : <color=yellow>{Duration}초</color>\n" +
                $"Clip Frame : <color=yellow>{MyVideoPlayer.frameRate}</color>\n" +
                $"Frame : <color=yellow>{MyVideoPlayer.frameRate} => {Application.targetFrameRate}</color>\n");
        }


        /// <summary>
        /// 플레이O -> 일시정지
        /// </summary>
        public void Pause()
        {
            if (!IsPlaying)
            {
                Debug.Log($"Video Playing : <color=red>{MyVideoPlayer.isPlaying}</color>");
                return;
            }

            MyVideoPlayer.Pause();
        }


        private void Restart()
        {
            if (!IsPrepared) return;
            Pause();
            Seek(0);            
        }

        /// <summary>
        /// 플레이O -> 정지
        /// </summary>
        private void Stop()
        {
            MyVideoPlayer.Stop();
        }

        /// <summary>
        /// 시간 점프 기능
        /// </summary>
        /// <param name="time">현재 시간에 추가로 점프하고자 하는 시간</param>
        private void JumpTime(double time)
        {
            double temp = MyVideoPlayer.time;

            temp += time;
            temp = Mathf.Clamp((float)temp, 0, Duration);

            MyVideoPlayer.time = temp;
        }



        public void SeekFrame(long frame, SeekState state = SeekState.Order)
        {
            if (!IsPrepared) return;

            seekState = state;
            long frameCount = Convert.ToInt64(MyVideoPlayer.frameCount);

            long temp = Math.Clamp(frame, 0, frameCount);
            MyVideoPlayer.frame = temp;

            Debug.Log($"[Seek Frame] <color=cyan> {MyVideoPlayer.frame}/ {Frame}/ {temp}/ {Application.targetFrameRate} {frameCount} </color>");

        }

        private void Seek(double time, SeekState state = SeekState.Order)
        {
            if (!MyVideoPlayer.canSetTime) return;
            if (!IsPrepared) return;

            seekState = state;
            MyVideoPlayer.time = time;
            Debug.Log($"[Seek] Cur Time :<color=yellow> {VideoTime} </color>=> <color=yellow>{time}</color>");

        }


        private IEnumerator WaitingEndVideo()
        {
            while (!endVideoCheck)
            {
                yield return null;
            }

            Pause();
            endVideoCheck = false;
            isFinish = true;
        }

        #region VideoPlayerEvent
        /// <summary>
        /// 영상 관련 디버그
        /// </summary>
        /// <param name="source"></param>
        /// <param name="message"></param>
        private void ErrorReceived_Event(VideoPlayer source, string message)
        {
            Debug.Log("[" + name + "] play Err : " + message);
        }
        private void FrameReady_Event(VideoPlayer source, long frameIdx)
        {
            Debug.Log("[" + name + "] FrameReady : " + frameIdx);
        }
        private void LoopPointReached_Event(VideoPlayer source)
        {
            endVideoCheck = true;
            Debug.Log($"[LoopPointReached_Event]\n<color=yellow>[{MyVideoPlayer.url}]</color> LoopPointReached");
        }
        private void PrepareCompleted_Event(VideoPlayer source)
        {
            //Debug.Log("[" + name + "] Video PrepareCompleted");
        }
        private void SeekCompleted_Event(VideoPlayer source)
        {
            if (seekState == SeekState.Drag) return;

            if (m_lateSeek_Routine != null)
                StopCoroutine(m_lateSeek_Routine);
            m_lateSeek_Routine = LateSeek_Routine();

            StartCoroutine(m_lateSeek_Routine);

        }
        private void Started_Event(VideoPlayer source)
        {
            //Debug.Log("[" + name + "] Started");
        }

        private IEnumerator m_lateSeek_Routine;
        private IEnumerator LateSeek_Routine()
        {
            yield return new WaitForSeconds(0.1f);
            seekState = SeekState.Complete;
            isFinish = true;

            Debug.Log("<color=cyan> SeekCompleted </color>");

        }
        #endregion

        private bool CheckFileExists(string path)
        {
            FileInfo file = new FileInfo(path);

            if (file.Exists)
                return true;
            else
                return false;
        }
    }
}