using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SpatialAnchorHandler
{
    public static IEnumerator LoadAnchor(GameObject go, string anchorKey)
    {
        var guidStr = ReadGuid(anchorKey);
        Debug.Log("Loading spatial anchor with uuid: " + guidStr);

        if (guidStr.Length > 0)
        {
            if (System.Guid.TryParse(guidStr, out var result))
            {
                System.Guid[] uuids = new System.Guid[] { result };
                Debug.Log("parsed uuid: " + uuids[0].ToString());
                //OVRSpatialAnchor.LoadOptions options = new OVRSpatialAnchor.LoadOptions();
                //options.StorageLocation = OVRSpace.StorageLocation.Local;
                //options.Uuids = uuids;
                //options.Timeout = 0;
                //var loadTask = OVRSpatialAnchor.LoadUnboundAnchorsAsync(options);
                List<OVRSpatialAnchor.UnboundAnchor> _unboundAnchors = new();
                var loadTask = OVRSpatialAnchor.LoadUnboundAnchorsAsync(uuids, _unboundAnchors);

                yield return new WaitUntil(() => loadTask.IsCompleted);

                var results = loadTask.GetResult();
                Debug.Log("Initiating loading anchor " + (results.Success ? "succeeded" : "failed"));

                if (results.Success)
                {
                    Debug.Log("Found " + results.Value.Count + " spatial anchor(s) to be loaded");

                    foreach (var anchor in results.Value)
                    {
                        Debug.Log("Localizing first anchor (" + anchor.Uuid + ")");
                        var taskLocalize = anchor.LocalizeAsync();

                        yield return new WaitUntil(() => taskLocalize.IsCompleted);

                        var success = taskLocalize.GetResult();
                        Debug.Log("Localizing anchor (" + anchor.Uuid + ") " + (success ? "succeeded" : "failed"));

                        if (success)
                        {
                            var newAnchor = go.GetComponent<OVRSpatialAnchor>();

                            if (newAnchor)
                                yield return EraseAnchor(go);

                            newAnchor = go.AddComponent<OVRSpatialAnchor>();

                            anchor.BindTo(newAnchor);
                            Debug.Log("Binding anchor (" + anchor.Uuid + ")");
                        }
                    }
                }
            }
            else
            {
                Debug.Log("Loading spatial failed due to corrupt uuid");
            }
        }
        else
        {
            Debug.Log("No spatial anchor found to be loaded");
        }
    }

    public static IEnumerator EraseAnchor(GameObject go)
    {
        // Erase Anchor
        Debug.Log("Attempting to erase existing anchor");
        var anchor = go.GetComponent<OVRSpatialAnchor>();
        if (anchor)
        {
            var erasetask = anchor.EraseAnchorAsync();
            yield return new WaitUntil(() => erasetask.IsCompleted);
            var eraseResult = erasetask.GetResult();

            Debug.Log("Erasing anchor (" + anchor.Uuid + ") " + (eraseResult.Success ? "succeeded" : "failed"));

            UnityEngine.Object.DestroyImmediate(anchor);
            yield return null;
        }
        else
        {
            Debug.Log("No anchor found attached on the object");
        }
    }

    public static IEnumerator CreateAndSaveAnchor(GameObject go, string anchorKey)
    {
        Debug.Log("Attempting to create new anchor");
        var newAnchor = go.GetComponent<OVRSpatialAnchor>();
        if (newAnchor)
        {
            Debug.Log("Anchor already exist. Erase it before creating a new anchor");
            yield break;
        }
        yield return new WaitForSeconds(1);
        newAnchor = go.AddComponent<OVRSpatialAnchor>();

        DateTime startCreationg = DateTime.Now;

        yield return new WaitUntil(() => (newAnchor && newAnchor.Created) || (DateTime.Now - startCreationg).TotalSeconds > 5);
        if (newAnchor && newAnchor.Created)
        {
            Debug.Log("Created new anchor (" + newAnchor.Uuid + "). Attempting to save.");
            //OVRSpatialAnchor.SaveOptions options = new OVRSpatialAnchor.SaveOptions();
            //options.Storage = OVRSpace.StorageLocation.Local;

            var savetask = newAnchor.SaveAnchorAsync();
            yield return new WaitUntil(() => savetask.IsCompleted);
            var saveResult = savetask.GetResult();

            Debug.Log("Saving anchor (" + newAnchor.Uuid + ") " + (saveResult.Success ? "succeeded" : "failed"));
            if (saveResult.Success)
                WriteGuid(newAnchor.Uuid.ToString(), anchorKey);
        }
        else
        {

            Debug.Log("Creating new anchor failed.");
        }

    }

    public static bool Exist(string anchorKey) // Changed parameter name for clarity
    {
        return PlayerPrefs.HasKey(anchorKey);
    }

    //public static bool Exist(string fileName)
    //{
    //    var filePath = Application.persistentDataPath + fileName;
    //    return File.Exists(filePath);
    //}

    public static void DeleteAnchorFile(string anchorKey)
    {
        //var filePath = Application.persistentDataPath + fileName;
        //File.Delete(filePath);
        if (PlayerPrefs.HasKey(anchorKey))
        {
            PlayerPrefs.DeleteKey(anchorKey);
            PlayerPrefs.Save(); // Ensure deletion is written to disk
            Debug.Log("Anchor data deleted from PlayerPrefs for key: " + anchorKey);
        }
        else
        {
            Debug.Log("No anchor data found in PlayerPrefs to delete for key: " + anchorKey);
        }
    }

    private static string ReadGuid(string anchorKey)
    {
        return PlayerPrefs.GetString(anchorKey, "");
        //var filePath = Application.persistentDataPath + fileName;
        //if (File.Exists(filePath))
        //    return File.ReadAllText(filePath);
        //else return "";
    }

    private static void WriteGuid(string guidStr, string anchorKey)
    {
        PlayerPrefs.SetString(anchorKey, guidStr);
        PlayerPrefs.Save();
        // File.WriteAllText(Application.persistentDataPath + filename, guidStr);
    }
}
