// Author(s): UWBothell Cross Reality Collaboration Sandbox, Aaron Holloway
// Fall 2018
// Last Modified: 12/2/2018

using System;
using System.IO;
using System.Net;
using System.Threading;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

// Originally in Summer2018CRCSProject/Assets/ASL/RC/Scripts as WebSteam.cs

// Modified slightly, including option to render out to Texture2D or RawImage
// as needed. Also moved GetStream() into a coroutine in an attempt to solve
// Unity engine hangs during intermittent or timed out connection.

// Likely still needs improvement, especially moving the stream processing onto
// a secondary thread.

public class WebStream : MonoBehaviour {
    // Opentopia.com Webcam IP Adress
    // private const string sourceURL = "http://194.103.218.15/mjpg/video.mjpg";
    // UW Seattle Webcam IP Address
    //private const string sourceURL = "http://128.208.252.2/mjpg/video.mjpg";
    // RC Car IP Address
    public bool left;
    private string sourceURL;
    private Texture2D texture;
    private MeshRenderer frame;
    public RawImage image;

    /*
     * The Start method, called when the object is instantiated, initializes
     * variables for the script, assigns the appropriate sorting layer to
     * the MeshRenderer for the object, and calls the GetStream method.
     */
    void Start()
    {
        if (left)
            sourceURL = "http://172.24.1.1:8080/?action=stream";
        else
            sourceURL = "http://172.24.1.1:8070/?action=stream";
        frame = this.GetComponent<MeshRenderer>();

        texture = new Texture2D(2, 2);
        // texture = new Texture2D(640, 480, TextureFormat.YUY2, false);

        StartCoroutine(GetStream());
    }

    public Texture getTextureFeed()
    {
        return texture;
    }

    /*
     * The GetStream method creates and sends an HTTP GET request
     * to the sourceURL, receives the response from the address,
     * and starts the FillFrame coroutine.
     */
    IEnumerator GetStream() {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sourceURL);
        WebResponse response = request.GetResponse();
        Stream stream = response.GetResponseStream();
        StartCoroutine(FillFrame(stream));
        yield return null;
    }

    /*
     * The FillFrame method uses a MemoryStream created from reading
     * the bytes in the stream to load the image into a texture and
     * attach it to the MeshRenderer. The StreamLength() method is
     * called to determine the number of bytes to read.
     * @return IEnumerator The IEnumerator that determines How long
     * the coroutine will yield. In most cases the coroutine will
     * start again on the next update.
     */
    IEnumerator FillFrame(Stream stream) {
        Byte[] imageData = new Byte[150000];
        while(true) {
            int totalBytes = StreamLength(stream);
                if (totalBytes == -1)
                    yield break;
            int remainingBytes = totalBytes;
            while(remainingBytes > 0) {
                remainingBytes -= stream.Read(imageData, totalBytes - remainingBytes, remainingBytes);
                yield return null;
            }
            MemoryStream memStream = new MemoryStream(imageData, 0, totalBytes, false, true);

            texture.LoadImage(memStream.GetBuffer());
            // texture.LoadRawTextureData(memStream.GetBuffer());
            // texture.Apply();

            if (frame) {
                frame.material.mainTexture = texture;
            }

            if(image) {
                image.texture = texture;
            }

            stream.ReadByte();
            stream.ReadByte();
        }
    }

    /*
     * The StreamLength method returns the total number of bytes in
     * the stream excluding header and metadata information.
     */
    int StreamLength(Stream stream) {
        int b;
        string line = "";
        int result = -1;
        bool atEOL = false;
        while ((b = stream.ReadByte()) != -1)
        {
            if (b == 10) continue;
            if (b == 13)
            {
                if (atEOL)
                {
                    stream.ReadByte();
                    return result;
                }
                if (line.StartsWith("Content-Length:"))
                {
                    result = Convert.ToInt32(line.Substring("Content-Length:".Length).Trim());
                    line = "";
                }
                else
                {
                    line = "";
                }
                atEOL = true;
            }
            else
            {
                atEOL = false;
                line += (char)b;
            }
        }
        return -1;
    }
}

/*
    ------------------     Old Code     ---------------------
    // Debug Prints from GetStream()
         //  - For Debug -
        if(left)
            print("Left: Received response with Content Length: " + response.ContentLength);
        else
            print("Received response with Content Length: " + response.ContentLength);
        // - End Debug -
        // - For Debug -
        print("Stream created with Content Length: " + response.ContentLength);
        // -End Debug -

    // Debug Prints from FillFrame()
        // - For Debug -
        // print("Starting Coroutine");
        // - End Debug -
        // - For Debug -
        if (left)
            print("Left: Stream Length: " + totalBytes);
        else
            print("Stream Length: " + totalBytes);
        // - End Debug -
        // - For Debug -
        // print("Yielding Coroutine, because there are no bytes to read from the stream");
        // - End Debug -
        // - For Debug -
        if (left)
            print("Left: In Read Loop ... Remaining: " + remainingBytes);
        else
            print("In Read Loop ... Remaining: " + remainingBytes);
        // - End Debug -
        // - For Debug -
        // print("Yielding for one update ..");
        // - End Debug -
        // - For Debug -
        if(left)
            print("Left: Loading Image and Exiting Coroutine");
        else
            print("Loading Image and Exiting Coroutine");
        // - End Debug -
*/
