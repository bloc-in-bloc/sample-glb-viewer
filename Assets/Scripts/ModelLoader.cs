using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GLTFast;
using UnityEngine;
using UnityEngine.Networking;

public class ModelLoader : MonoBehaviour {

    public bool optimizeMaterial = false;
    public bool optimizeMesh = false;
    
    
    public string[] paths;
    BIBMaterialGenerator materialGenerator = new BIBMaterialGenerator ();

    // Start is called before the first frame update
    async void Start () {

        Application.targetFrameRate = 60;
        
        foreach (string path in paths) {
            await LoadGltfBinaryFromMemory (Application.streamingAssetsPath + "/" + path + ".glb");
        }
        
        if (optimizeMesh) {
            await CombineMesh (this.gameObject);
        }
    }

    async Task LoadGltfBinaryFromMemory (string path) {
#if UNITY_EDITOR
        byte[] data = File.ReadAllBytes (path);
#else
        UnityWebRequest uwr = UnityWebRequest.Get ("jar:file://" + path);
        uwr.SendWebRequest ();
        while (!uwr.isDone) {
            Debug.Log (uwr.downloadProgress);
            await Task.Delay (10);
        }
        Debug.Log (uwr.error);
        byte[] data = uwr.downloadHandler.data;
#endif
        Debug.Log (data.Length);
        var gltf = optimizeMaterial ? new GltfImport(null, null, materialGenerator) : new GltfImport ();
        bool success = await gltf.LoadGltfBinary (data, new Uri (path));
        if (success) {
            success = gltf.InstantiateMainScene (transform);
        }
    }
    
    
    private async Task CombineMesh (GameObject root) {
        StaticBatchingUtility.Combine (root);
        Resources.UnloadUnusedAssets ();
        GC.Collect ();
        await Task.Delay (100);
    }
}