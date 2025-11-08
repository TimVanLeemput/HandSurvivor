using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[ExecuteInEditMode]
public class ScaledNavMeshApplier : MonoBehaviour
{
    [Header("Input NavMesh")]
    public NavMeshData sourceNavMesh; // ton .asset baké à l’échelle 1

    [Header("Scaling")]
    [Tooltip("Facteur d’échelle à appliquer à la géométrie et au NavMesh")]
    public float scaleFactor = 0.01f;

    [Header("Output")]
    public NavMeshSurface surface; // référence optionnelle si tu veux associer le nouveau NavMesh
    public bool autoApply = true;  // ajoute automatiquement le nouveau navmesh à la scène

    private NavMeshData scaledData;
    private NavMeshDataInstance instance;

    [ContextMenu("Apply Scaled NavMesh")]
    public void ApplyScaledNavMesh()
    {
        if (sourceNavMesh == null)
        {
            Debug.LogError("Aucun NavMeshData source n’est assigné.");
            return;
        }

        // Récupère les sources de la scène actuelle
        var sources = new List<NavMeshBuildSource>();
        NavMeshBuilder.CollectSources(
            transform,
            ~0, // tous les layers
            NavMeshCollectGeometry.RenderMeshes,
            0,
            new List<NavMeshBuildMarkup>(),
            sources
        );

        // Applique la matrice de scale à chaque source
        for (int i = 0; i < sources.Count; i++)
        {
            var src = sources[i];
            var m = src.transform;
            var scaled = Matrix4x4.TRS(
                m.MultiplyPoint(Vector3.zero) * scaleFactor,
                Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1)),
                m.lossyScale * scaleFactor
            );
            src.transform = scaled;
            sources[i] = src;
        }

        // Calcule les bounds du nouveau NavMesh
        var bounds = new Bounds(Vector3.zero, new Vector3(1000, 1000, 1000)); // adapte selon ta scène

        // Reconstruit un nouveau NavMeshData avec scale appliqué
        scaledData = NavMeshBuilder.BuildNavMeshData(
            NavMesh.GetSettingsByID(0), // agentTypeID 0 = Default Humanoid
            sources,
            bounds,
            Vector3.zero,
            Quaternion.identity
        );

        if (scaledData == null)
        {
            Debug.LogError("Échec de la génération du NavMesh scaled.");
            return;
        }

        // Applique le NavMesh dans la scène
        if (autoApply)
        {
            if (instance.valid) instance.Remove();
            instance = NavMesh.AddNavMeshData(scaledData, transform.position, transform.rotation);
            Debug.Log($"✅ NavMesh scaled x{scaleFactor} appliqué à la scène.");
        }

        // Si un NavMeshSurface est lié, on le met à jour
        if (surface != null)
        {
            surface.navMeshData = scaledData;
        }
    }

    private void OnDisable()
    {
        if (instance.valid)
            instance.Remove();
    }
}
