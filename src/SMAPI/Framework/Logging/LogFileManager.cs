using System;
using System.IO;

namespace StardewModdingAPI.Framework.Logging;

/// <summary>Manages reading and writing to log file.</summary>
internal class LogFileManager : IDisposable
{
    /*********
    ** Fields
    *********/
    /// <summary>The underlying stream writer.</summary>
    private readonly StreamWriter Stream;

    /// <summary>
    /// WriteLine() safe thread
    /// </summary>
    private readonly object _lock_WriteLine = new();


    /*********
    ** Accessors
    *********/
    /// <summary>The full path to the log file being written.</summary>
    public string Path { get; }


    /*********
    ** Public methods
    *********/
    /// <summary>Construct an instance.</summary>
    /// <param name="path">The log file to write.</param>
    public LogFileManager(string path)
    {
        this.Path = path;

        // create log directory if needed
        string? logDir = System.IO.Path.GetDirectoryName(path);
        if (logDir == null)
            throw new ArgumentException($"The log path '{path}' is not valid.");
        Directory.CreateDirectory(logDir);

        // open log file stream
        this.Stream = new StreamWriter(path, append: false) { AutoFlush = true };
    }

    /// <summary>Write a message to the log.</summary>
    /// <param name="message">The message to log.</param>
    public void WriteLine(string message)
    {
        // always use Windows-style line endings for convenience
        // (Linux/macOS editors are fine with them, Windows editors often require them)
#if SMAPI_FOR_ANDROID
        lock (this._lock_WriteLine)
        {
            this.Stream.Write(message + "\r\n");
        }
#else
        this.Stream.Write(message + "\r\n");
#endif
    }

    /// <summary>Release all resources.</summary>
    public void Dispose()
    {
        this.Stream.Dispose();
    }
}
