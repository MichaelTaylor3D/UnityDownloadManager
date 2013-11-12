Unity3D Download Manager
====================

A download queue system for Unity3D. Automatically caches image files to disk. 

Supports Downloading Async (with callback) or Sync

Warning: Do not use sync on iOS devices, this will cause the device to freeze. Use Async instead.

API
====

KEY:

path: your download path

callback: Callback function to send downloaded data to

failscript: invokes the failScript function if the download fails

saveToLocal: saves and caches the download to the local disk (image downloads only)

downloadRetries: how many times to retry a failed download



public static string Sync(string path)

Async Overloads:

public static void Async(string path, DownloadCallback callback)

public static void Async(string path, DownloadCallback callback,  FailScript failscript)

public static void Async(string path, DownloadCallback callback, bool saveToLocal, int downloadRetries)

public static void Async(string path, DownloadCallback callback, int downloadRetries)

public static void Async(string path, DownloadCallback callback, bool saveToLocal)
