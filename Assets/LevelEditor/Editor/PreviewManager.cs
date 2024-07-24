using UnityEditor;
using UnityEngine;

public class PreviewManager
{
    public bool EnablePrefabPreview { get; set; }
    public Color PreviewColor { get; set; } = Color.white;
    public Vector3 PreviewPosition { get; private set; }
    public Quaternion PreviewRotation { get; private set; } = Quaternion.identity;
    private GameObject selectedPrefab;

    public void UpdatePreviewTransform(SceneView sceneView, GameObject selectedPrefab)
    {
        this.selectedPrefab = selectedPrefab;
        UpdatePreviewPosition();
        UpdatePreviewRotation();
        if (Event.current.type == EventType.MouseMove)
            sceneView.Repaint();
    }

    public void DrawPrefabPreview()
    {
        if (selectedPrefab == null || !EnablePrefabPreview) return;

        Mesh mesh = selectedPrefab.GetComponent<MeshFilter>().sharedMesh;
        if (mesh == null) return;

        Material previewMaterial = new Material(Shader.Find("Standard"))
        {
            color = PreviewColor
        };
        previewMaterial.SetPass(0);
        Graphics.DrawMeshNow(mesh, PreviewPosition, PreviewRotation);
    }

    private void UpdatePreviewPosition()
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag("SnapPoint"))
        {
            TrySnapPreviewPosition(hit, ray);
        }
        else
        {
            PreviewPosition = CalculatePreviewPosition(ray);
        }
    }

    public void UpdatePreviewRotation()
    {
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
            PreviewRotation *= Quaternion.Euler(0f, 90f, 0f);
    }

    private void TrySnapPreviewPosition(RaycastHit snapPoint, Ray ray)
    {
        if (!selectedPrefab.TryGetComponent(out SnappableObject snappableObject))
        {
            PreviewPosition = CalculatePreviewPosition(ray);
            return;
        }

        Vector3 direction = snapPoint.transform.parent.rotation * snapPoint.transform.localPosition.normalized;
        PreviewPosition = CanSnap(direction, snappableObject) ? snapPoint.transform.position : CalculatePreviewPosition(ray);
    }

    private bool CanSnap(Vector3 snapDirection, SnappableObject snappableObj)
    {
        foreach (Transform trs in snappableObj.SnapPoint)
        {
            if (Vector3.Dot(snapDirection, PreviewRotation * trs.localPosition.normalized) < -0.9f)
                return true;
        }
        return false;
    }

    private Vector3 CalculatePreviewPosition(Ray ray)
    {
        float hypotenuse = ray.origin.y / Vector3.Dot(Vector3.down, ray.direction);
        return ray.origin + ray.direction * hypotenuse;
    }
}