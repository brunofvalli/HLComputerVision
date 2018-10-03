using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleOut : MonoBehaviour
{
    public static TextMesh _debugText;

    #region event
    private void Start()
    {
        _debugText = debugText;
        _debugText.text += ".";
    }
    #endregion


    #region public fields
    /// <summary>
    /// The text 3d that will display our debug information
    /// </summary>
    public TextMesh debugText;
    #endregion


    #region public static methods
    /// <summary>
    /// Sends a text to output
    /// </summary>
    /// <param name="value"></param>
    public static void SendText(string value)
    {
        if (_debugText != null)
        {
            _debugText.text += Environment.NewLine + value;
        }
    }
    #endregion
}
