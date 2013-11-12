Unity3D Download Manager
====================

Author: Michael Taylor Contact: www.michaeltaylor3d.com/contact

Copyright (c) Michael Taylor

This program is free software: you can redistribute it and/or modify it under the terms of the GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License along with this program. If not, see http://www.gnu.org/licenses/.





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



API MEMBERS:


public static string Sync(string path)

Async Overloads:

public static void Async(string path, DownloadCallback callback)

public static void Async(string path, DownloadCallback callback,  FailScript failscript)

public static void Async(string path, DownloadCallback callback, bool saveToLocal, int downloadRetries)

public static void Async(string path, DownloadCallback callback, int downloadRetries)

public static void Async(string path, DownloadCallback callback, bool saveToLocal)
