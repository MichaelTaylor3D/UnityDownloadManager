//Copyright 2013 MichaelTaylor3D
//www.michaeltaylor3d.com

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public delegate void FailScript();
public delegate void DownloadCallback(WWW www);

public class Download : MonoBehaviour
{

    /////////////////////////////////////////////////
    //////-------------Public API--------------//////
    /////////////////////////////////////////////////

    /// <summary>
    /// Clears the download queue.
    /// </summary>
    public static void ClearQueue()
    {
        _downloadQueue.Clear();
    }

    /// <summary>
    ///  Downloads a file Async and sends it to the callback.
    /// </summary>
    public static void Async(string path, DownloadCallback callback)
    {
        Job job = new Job(path, callback, false, 0, null);
        _singleton.Enqueue(job);
    }

    /// <summary>
    ///  Downloads a file Async and sends it to the callback.
    ///  Invokes the failscript if the download failed
    /// </summary>
    public static void Async(string path, DownloadCallback callback, FailScript failscript)
    {
        Job job = new Job(path, callback, false, 0, failscript);
        _singleton.Enqueue(job);
    }

    /// <summary>
    ///  Downloads a file Async and sends it to the callback.
    ///  caches image files to local disk
    ///  retries the failed download a set number of times
    /// </summary>
    public static void Async(string path, DownloadCallback callback, bool saveToLocal, int downloadRetries)
    {
        Job job = new Job(path, callback, saveToLocal, downloadRetries, null);
        _singleton.Enqueue(job);
    }

    /// <summary>
    ///  Downloads a file Async and sends it to the callback.
    ///  retries the failed download a set number of times
    /// </summary>
    public static void Async(string path, DownloadCallback callback, int downloadRetries)
    {
        Async(path, callback, false, downloadRetries);
    }

    /// <summary>
    ///  Downloads a file Async and sends it to the callback.
    ///  caches image files to local disk
    /// </summary>
    public static void Async(string path, DownloadCallback callback, bool saveToLocal)
    {
        Async(path, callback, saveToLocal, 0);
    }

    /// <summary>
    /// Downloads a file in Sync and returns the result
    /// Useful for getting RESTful callbacks, NOT Images
    /// </summary>
    public static string Sync(string path)
    {
        string result;
        new Download.HTTP(path, out result);
        return result;
    }

    /// <summary>
    /// Gets a value indicating whether the download queue is active.
    /// </summary>
    /// <value>
    /// <c>true</c> if this download queue is active; otherwise, <c>false</c>.
    /// </value>	
    public static bool IsActive
    {
        get
        {
            return _IsActive;
        }
    }
    private static bool _IsActive;

    /////////////////////////////////////////////////
    //////-------------Queue Object--------------//////
    /////////////////////////////////////////////////
    public class Job
    {
        public string path { get; set; }
        public DownloadCallback callback { get; set; }
        public bool saveToLocal { get; set; }
        public int downloadRetries { get; set; }
        public FailScript failscript;

        public Job(string path, DownloadCallback callback, bool saveToLocal, int downloadRetries, FailScript failscript)
        {
            this.path = path;
            this.callback = callback;
            this.saveToLocal = saveToLocal;
            this.failscript = failscript;

            if (downloadRetries != default(int))
            {
                this.downloadRetries = downloadRetries;
            }
            else
            {
                this.downloadRetries = 0;
            }
        }
    }

    /////////////////////////////////////////////////
    //////---------Instance Members------------//////
    /////////////////////////////////////////////////

    #region Singleton
    private static Download _instance;
    private static Download _singleton
    {
        get
        {
            if (_instance.Equals(null))
            {
                GameObject runCode = GameObject.Find("RunCode");

                if (runCode.Equals(null))
                {
                    runCode = new GameObject("RunCode");
                }

                _instance = runCode.AddComponent(typeof(Download)) as Download;
            }

            return _instance;
        }
    }
    #endregion

    private static Queue<Job> _downloadQueue;
    private static string downloadDirectory;

    void Awake()
    {
        _IsActive = false;
        _jobIsProcessing = false;
        _downloadQueue = new Queue<Job>();
        downloadDirectory = Application.persistentDataPath + "/downloads/";
    }

    void FixedUpdate()
    {
        if (_downloadQueue.Count == 0)
        {
            _IsActive = false;
            return;
        }

        _IsActive = true;

        if (!_jobIsProcessing
        && Internet.isConnected())
        {
            StartCoroutine(ProcessJob());
        }
    }

    private static bool _jobIsProcessing;

    private static IEnumerator ProcessJob()
    {
        _jobIsProcessing = true;

        Job job = _downloadQueue.Dequeue() as Job;

        yield return null;

        if (job != null)
        {
            for (int i = 0; i <= job.downloadRetries; i++)
            {
                //initiate working variables
                WWW www = new WWW(job.path);
                yield return www;

                if (www.error != null)
                {
                    Debug.LogError("Download Error: " + www.url + " : " + www.error);
                    if (job.failscript != null)
                    {
                        job.failscript();
                    }
                    continue;
                }

                if (job.callback != null)
                {
                    job.callback(www);
                }

                if (job.saveToLocal)
                {
                    Byte[] bytes = www.texture.EncodeToPNG();

                    if (!Directory.Exists(downloadDirectory))
                    {
                        Directory.CreateDirectory(downloadDirectory);
                    }

                    string fileName = Path.GetFileNameWithoutExtension(www.url.Replace("%20", " "));
                    string fileExtension = Path.GetExtension(www.url);
                    string filePath = downloadDirectory + fileName + fileExtension;
                    Debug.Log(filePath);
#if !UNITY_WEBPLAYER
                    File.WriteAllBytes(filePath.Replace("%20", " "), bytes);
#endif

#if UNITY_IPHONE
					iPhone.SetNoBackupFlag(filePath); //Apple will reject the app if this is backed up
#endif
                }

                _jobIsProcessing = false;

                break;
            }
            _jobIsProcessing = false;
        }
    }

    private void Enqueue(Job job)
    {
        _downloadQueue.Enqueue(job);
    }

    ///////////////////////////////////////////////////////////////////
    /////////////////////Sync Downloading//////////////////////////////
    ////// Should Be used in editor only: Freezes IOS devices//////////

    private class HTTP
    {
        public HTTP(string request, out string result)
        {
            result = ServerRequest(request);
        }

        private IEnumerator DownloadString(string url)
        {
            WWW www = new WWW(url);
            float wwwStartTime = Time.realtimeSinceStartup;
            float wwwTimeOutSec = 50f;

            while (!www.isDone)
            {
                if (Time.realtimeSinceStartup - wwwStartTime > wwwTimeOutSec)
                {
                    Debug.LogError("Download time Out");
                    //yield return www.text;
                }
            }

            yield return www.text;
        }

        private string ServerRequest(string request)
        {
            string result = null;
            IEnumerator e;

            //Send a RESTful server Request
            e = DownloadString(request);

            //get the last result of the enumeration
            while (e.MoveNext())
            {
                result = e.Current.ToString();
            }

            //check for server response errors
            if (isServerResponseError(result))
            {
                Debug.LogError(result);
                return null;
            }
            return result;
        }

        private bool isServerResponseError(string www)
        {
            return www.Contains("Error:") ? true : false;
        }
    }
}

public static class Internet
{

    public static bool isConnected()
    {
        return (Network.player.ipAddress.ToString() == "127.0.0.1") ? false : true;
    }
}
