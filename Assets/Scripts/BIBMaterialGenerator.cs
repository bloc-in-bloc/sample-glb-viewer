using System.Collections.Generic;
using GLTFast.Materials;
using UnityEngine;
using static GLTFast.Schema.Material;

namespace GLTFast {
    public class BIBMaterialGenerator : MaterialGenerator {
        public int nbMaterials { get { return _materials?.Count ?? -1; } }
        Material defaultMaterial;
        Material alphaMaterial;
        private Dictionary<string, Material> _materials = new Dictionary<string, Material> ();

        public override Material GetDefaultMaterial () {
            return defaultMaterial;
        }

        public UnityEngine.Material GetPbrMetallicRoughnessMaterial (bool doubleSided = false) {
            return null;
        }

        public override UnityEngine.Material GenerateMaterial (Schema.Material gltfMaterial, IGltfReadable gltf) {
            if (defaultMaterial == null) {
                defaultMaterial = Resources.Load ("StandardMaterial") as Material;
            }
            if (alphaMaterial == null) {
                alphaMaterial = Resources.Load ("StandardBaseAlphaMaterial") as Material;
            }

            bool isTransparent = (gltfMaterial.alphaModeEnum == AlphaMode.MASK) || (gltfMaterial.alphaModeEnum == AlphaMode.BLEND);
            Color color = (gltfMaterial.pbrMetallicRoughness != null) ? gltfMaterial.pbrMetallicRoughness.baseColor : Color.white;

            string materialIdentifer = isTransparent ? "StandardBaseAlphaMaterial" : "StandardMaterial";
            materialIdentifer += $"-{color.ToHex()}";

            UnityEngine.Material material;

            // Merge materials by color
            if (_materials.ContainsKey (materialIdentifer)) {
                material = _materials[materialIdentifer];
            } else {
                material = isTransparent ? new Material (alphaMaterial) : new Material (defaultMaterial);
                material.name = gltfMaterial.name;
                if (gltfMaterial.pbrMetallicRoughness != null) {
                    material.color = color;
                    material.SetFloat ("_Metallic", 0);
                    material.SetFloat ("_Smoothness", isTransparent ? 0 : 0.5f);
                }
                _materials.Add (materialIdentifer, material);
            }

            return material;
        }
    }
}

public static class ColorUtils{
    public static string ColorToHex (Color color) {
        string red = FloatNormalizedToHex (color.r);
        string green = FloatNormalizedToHex (color.g);
        string blue = FloatNormalizedToHex (color.b);
        string alpa = FloatNormalizedToHex (color.a);
        return red + green + blue + alpa;
    }

    public static string ToHex (this Color color) {
        return ColorToHex (color);
    }
    
    private static string DecToHex (int value) {
        return value.ToString ("X2");
    }

    private static string FloatNormalizedToHex (float value) {
        return DecToHex (Mathf.RoundToInt (value * 255f));
    }
}