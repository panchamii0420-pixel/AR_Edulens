using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;
using System.Collections.Generic;

public class ARImageTrackingStatus : MonoBehaviour
{
    [Header("AR")]
    public ARTrackedImageManager trackedImageManager;

    [Header("UI")]
    public TextMeshProUGUI statusText;

    void OnEnable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;

        SetSearchingText();
    }

    void OnDisable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        // Check all currently tracked images
        foreach (var trackedImage in trackedImageManager.trackables)
        {
            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                statusText.text = "Image found: " + trackedImage.referenceImage.name;
                return; // Stop after first found image
            }
        }

        // If no image is actively tracked
        SetSearchingText();
    }

    void SetSearchingText()
    {
        statusText.text = "Scanning for images…";
    }
}