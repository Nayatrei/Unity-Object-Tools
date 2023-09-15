#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public class CsObjectController : EditorWindow 
{

	private Vector2 scrollPos;

	public enum objectRotateAxis
	{
		Local,
		World
	}
	public objectRotateAxis selectedAxis;
	bool objectRotateXAxis;
	bool objectRotateYAxis;
	bool objectRotateZAxis;
	
	float objectSinkAmount;
	int m_LayerNumber;

	Vector3 currentPosition;
	Quaternion currentRotation;
	Vector3 currentScale;
	string oldObj;

	[SerializeField] private LayerMask GroundLayerMask;
	[SerializeField] private GameObject prefab;

	// Add submenu
	[MenuItem("Tools/Toolkit/Object Controller")]
	static public void ShowWindow()
	{
		// Get existing open window or if none, make a new one:
		CsObjectController window = EditorWindow.GetWindow<CsObjectController>();
		window.titleContent.text = "Object Controller";
	}


	void OnGUI()
	{
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height));

		EditorGUILayout.LabelField("Randomly Rotate Object", EditorStyles.boldLabel);
		selectedAxis = (objectRotateAxis)EditorGUILayout.EnumPopup("Axis:", selectedAxis);
		objectRotateXAxis = EditorGUILayout.Toggle("Rotate X Axis: ", objectRotateXAxis);
		objectRotateYAxis = EditorGUILayout.Toggle("Rotate Y Axis: ", objectRotateYAxis);
		objectRotateZAxis = EditorGUILayout.Toggle("Rotate Z Axis: ", objectRotateZAxis);

		if (GUILayout.Button("Rotate Object" , GUILayout.Height(30)))
		{
			RotateIt();
		}


		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Place Object on Ground", EditorStyles.boldLabel);


		GroundLayerMask = LayerMaskField("Select Ground Layer", GroundLayerMask);

		GUILayout.Label("Sink Amount: " + objectSinkAmount.ToString("0.00"));
		objectSinkAmount = EditorGUILayout.Slider(objectSinkAmount, -10, 10);

		EditorGUILayout.LabelField("Note: Selected Object need to have Static: ");
		if (GUILayout.Button("Place Object", GUILayout.Height(30)))
		{
			PlaceIt();
			Debug.Log("Placing Object Now");
		}


		EditorGUILayout.Separator();
		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Replace with Prefab", EditorStyles.boldLabel);
		prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);
		EditorGUILayout.LabelField("Selection count: " + Selection.objects.Length);
		if (GUILayout.Button("Replace Object" ,GUILayout.Height(30)))
		{
			ReplaceIt();
		}


		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Copy/Paste Object Position", EditorStyles.boldLabel);
		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Copy Position of : " + oldObj);
		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Transform Position X " + currentPosition.x + " Y " + currentPosition.y + " Z " + currentPosition.z);
		EditorGUILayout.Separator();
		EditorGUILayout.LabelField("Note: Can only copy transform of one selected gameobject", EditorStyles.boldLabel);
		if (GUILayout.Button("Copy Position", GUILayout.Height(30)))
		{
			GameObject obj = Selection.activeGameObject;
			oldObj = obj.ToString();
			currentPosition = obj.transform.localPosition;
		}
		if (GUILayout.Button("Copy All Transform", GUILayout.Height(30)))
		{
			GameObject obj = Selection.activeGameObject;
			oldObj = obj.ToString();
			currentPosition = obj.transform.localPosition;
			currentRotation = obj.transform.localRotation;
			currentScale = obj.transform.localScale;
		}

		EditorGUILayout.Separator();

		if (GUILayout.Button("Paste Position", GUILayout.Height(30)))
		{
			foreach (GameObject newObj in Selection.gameObjects)
			{
				newObj.transform.localPosition = currentPosition;
			}
		}

		if (GUILayout.Button("Paste All", GUILayout.Height(30)))
		{
			foreach (GameObject newObj in Selection.gameObjects)
			{
				newObj.transform.localPosition = currentPosition;
				newObj.transform.localRotation = currentRotation;
				newObj.transform.localScale = currentScale;
			}
		}

		EditorGUILayout.Separator();
		if (GUILayout.Button("Reset Rotation", GUILayout.Height(30)))
		{
			foreach (GameObject newObj in Selection.gameObjects)
			{
				newObj.transform.localRotation = new Quaternion(0, 0, 0, 0);
			}
		}

		EditorGUILayout.Separator();
		if (GUILayout.Button("Reset Scale", GUILayout.Height(30)))
		{
			foreach (GameObject newObj in Selection.gameObjects)
			{
				newObj.transform.localScale = Vector3.one;
			}
		}

		EditorGUILayout.Separator();
		if (GUILayout.Button("Reset All", GUILayout.Height(30)))
		{
			foreach (GameObject newObj in Selection.gameObjects)
			{
				newObj.transform.localPosition = Vector3.zero;
				newObj.transform.localRotation = new Quaternion(0, 0, 0, 0);
				newObj.transform.localScale = Vector3.one;
			}

		}

		EditorGUILayout.EndScrollView();
	}

	public void PlaceIt()
	{
		RaycastHit hit;

		foreach (GameObject obj in Selection.gameObjects)
		{
			if (Physics.Raycast(obj.transform.position + Vector3.up * 1000, -Vector3.up, out hit, 9999, GroundLayerMask))
			{
				obj.transform.position = hit.point + objectSinkAmount * Vector3.up;

			}
		}
	}

	public void RotateIt()
	{
		int RotateXValue = 0;
		int RotateYValue = 0;
		int RotateZValue = 0;


		if(objectRotateXAxis)
        {
			RotateXValue = 1;
        }
		if (objectRotateYAxis)
		{
			RotateYValue = 1;
		}
		if (objectRotateZAxis)
		{
			RotateZValue = 1;
		}

		switch (selectedAxis)
		{
			case objectRotateAxis.Local:
				foreach (GameObject obj in Selection.gameObjects)
					{
						obj.transform.Rotate(new Vector3(RotateXValue, RotateYValue, RotateZValue), Random.Range(0, 360), Space.Self);
					}
				break;
			case objectRotateAxis.World:
				foreach (GameObject obj in Selection.gameObjects)
					{
						obj.transform.Rotate(new Vector3(RotateXValue, RotateYValue, RotateZValue), Random.Range(0, 360), Space.World);
					}
				break;
		}

	}

	private LayerMask LayerMaskField(string label, LayerMask layerMask)
	{
		var layers = InternalEditorUtility.layers;
		var layerNumbers = new int[layers.Length];
		for (int i = 0; i < layers.Length; i++)
			layerNumbers[i] = LayerMask.NameToLayer(layers[i]);

		layerMask = EditorGUILayout.MaskField(label, layerMask, layers);
		int mask = 0;
		for (int i = 0; i < layerNumbers.Length; i++)
		{
			if ((layerMask & (1 << layerNumbers[i])) != 0)
				mask |= (1 << i);
		}
		layerMask = mask;

		return layerMask;
	}

	public void ReplaceIt()
    {
		var selection = Selection.gameObjects;

		for (var i = selection.Length - 1; i >= 0; --i)
		{
			var selected = selection[i];
			var prefabType = PrefabUtility.GetPrefabAssetType(prefab);
			GameObject newObject;

			if (prefabType != PrefabAssetType.NotAPrefab)
			{
				newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
			}
			else
			{
				newObject = Instantiate(prefab);
				newObject.name = prefab.name;
			}

			if (newObject == null)
			{
				Debug.LogError("Error instantiating prefab");
				break;
			}

			Undo.RegisterCreatedObjectUndo(newObject, "Replace With Prefabs");
			newObject.transform.parent = selected.transform.parent;
			newObject.transform.localPosition = selected.transform.localPosition;
			newObject.transform.localRotation = selected.transform.localRotation;
			newObject.transform.localScale = selected.transform.localScale;
			newObject.transform.SetSiblingIndex(selected.transform.GetSiblingIndex());
			Undo.DestroyObjectImmediate(selected);


		}



	}
}
#endif
