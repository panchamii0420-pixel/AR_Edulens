using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;
using TMPro;

public class ImageModelMapper : MonoBehaviour
{
    [System.Serializable]
    public class ImagePrefabPair
    {
        public string imageName;
        public GameObject prefab;
    }

    public ARTrackedImageManager trackedImageManager;
    public List<ImagePrefabPair> imagePrefabPairs;

    // ✅ FIXED LINE (this is the only required change)
    public TextMeshProUGUI infoText;

    public float rotationSpeed = 0.3f;

    private Dictionary<string, GameObject> spawnedRoots = new Dictionary<string, GameObject>();

    // 🧠 ORGAN DESCRIPTIONS
    private Dictionary<string, string> organDescriptions = new Dictionary<string, string>()
    {
        { "heart", "Pumps oxygen-rich blood throughout the body\nMaintains blood pressure and circulation" },
        { "brain", "Controls thoughts, memory, and emotions\nCoordinates body movement and responses" },
        { "kidney", "Filters waste and excess fluids from blood\nRegulates electrolytes and blood pressure" },
        { "mitochondria", "Produces ATP, the cell’s energy source\nRegulates cellular metabolism" },
        { "lungs", "Enables oxygen exchange for respiration\nRemoves carbon dioxide and regulates blood pH" }
    };

    void OnEnable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    void OnDisable()
    {
        if (trackedImageManager != null)
            trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var img in args.added)
            SpawnOrUpdate(img);

        foreach (var img in args.updated)
            SpawnOrUpdate(img);

        foreach (var img in args.removed)
            Remove(img);
    }

    void SpawnOrUpdate(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name.ToLower().Trim();

        var pair = imagePrefabPairs.Find(p => p.imageName.ToLower().Trim() == imageName);
        if (pair == null) return;

        if (!spawnedRoots.ContainsKey(imageName))
        {
            GameObject root = new GameObject(imageName + "_Root");
            root.transform.SetParent(trackedImage.transform);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;

            GameObject model = Instantiate(pair.prefab, root.transform);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;

            spawnedRoots.Add(imageName, root);
        }

        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            spawnedRoots[imageName].SetActive(true);
            HandleTouchRotation(spawnedRoots[imageName].transform.GetChild(0));
            UpdateInfoText(imageName);
        }
        else
        {
            spawnedRoots[imageName].SetActive(false);
        }
    }

    void Remove(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name.ToLower().Trim();

        if (!spawnedRoots.ContainsKey(imageName)) return;

        Destroy(spawnedRoots[imageName]);
        spawnedRoots.Remove(imageName);

        if (infoText != null)
            infoText.text = "";
    }

    void UpdateInfoText(string imageName)
    {
        if (infoText == null) return;
        if (!organDescriptions.ContainsKey(imageName)) return;

        string displayName = char.ToUpper(imageName[0]) + imageName.Substring(1);
        infoText.text = $"Found: {displayName}\n{organDescriptions[imageName]}";
    }

    // 🔄 TOUCH ROTATION
    void HandleTouchRotation(Transform model)
    {
        if (Input.touchCount != 1) return;

        Touch touch = Input.GetTouch(0);

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            return;

        if (touch.phase == TouchPhase.Moved)
        {
            float rotX = touch.deltaPosition.y * rotationSpeed;
            float rotY = -touch.deltaPosition.x * rotationSpeed;

            model.Rotate(Vector3.right, rotX, Space.World);
            model.Rotate(Vector3.up, rotY, Space.World);
        }
    }
}