using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class HouseScene : MonoBehaviour {
    private GameObject unityCamera;

    private List<ObjectInScene> sceneObjects; // each frame, the objects' matrices have to be recomputed as the camera moves
    private Matrix4x4 ViewMatrix;
    private Matrix4x4 ProjectionMatrix;

    void Start()
    {
        CreateUnityCamera();
        CreateCamera();
        CreateScene();
    }

    private SceneCamera currentCamera;
    private SceneCamera firstPersonCamera;
    private SceneCamera orbitalCamera;
    private void CreateCamera()
    {
        firstPersonCamera = new FirstPersonCamera(new Vector3(-5, 1, 0), new Vector3(0, 1, 0), new Vector3(0, 1, 0));

        Vector3 orbitalPos = new Vector3(-5, 1, 0);
        Vector3 orbitalLookAt = new Vector3(0, 1, 0);
        // Vector3 orbitalForward = (orbitalLookAt - orbitalPos).normalized;
        Vector3 orbitalUp = Vector3.up;
        orbitalCamera = new OrbitalCamera(orbitalPos, orbitalLookAt, orbitalUp);

        currentCamera = orbitalCamera;
        ViewMatrix = currentCamera.ComputeViewMatrix();

        float SceneFoV = 90.0f;
        ProjectionMatrix = currentCamera.ComputeProjectionMatrix(SceneFoV);
    }

    private ObjectInScene InstantiateObject(string name) => LoadObject("Assets/Models/" + name + ".obj");
    private void PlaceObjectInScene(ObjectInScene obj, List<ObjectInScene> listToAdd, Vector3 pos, Vector3 rot, Vector3 scale)
    {
        obj.transform.position = pos;
        obj.transform.rotation = rot;
        obj.transform.scale = scale;
        obj.ComputeModelMatrix();
        listToAdd.Add(obj);
    }

    private ObjectInScene MakeObjectForRoom(string name, Vector3 pos, Vector3 rot, Vector3 scale, List<ObjectInScene> toAdd)
    {
        ObjectInScene newObject = LoadObject("Assets/Models/" + name + ".obj");
        newObject.transform.position = pos;
        newObject.transform.rotation = rot;
        newObject.transform.scale = scale;
        newObject.ComputeModelMatrix();
        toAdd.Add(newObject);
        return newObject;
    }

    /*
    private List<ObjectInScene> MakeGenericRoom(Vector3 bounds, Vector3 floorMidpoint) {
        // make two planes, one differing by the height and the 180° Y rotation
        // Then four planes
        // The problem is doors and windows
        // Need a MakeDoorWall(wallwidth, wallheight, dooroffset, doorHeight) (creates three planes)
        // and MakeWindowWall(v2 wallSize, v2 windowSize, v2 windowPos) (creates four planes)

        return null;
    }
    */

    private (List<ObjectInScene> roofs, List<ObjectInScene> walls, Collection allRoomObjects) MakeKitchen()
    {
        Collection kitchenObjects = new Collection();
        List<ObjectInScene> walls = new();
        List<ObjectInScene> roofs = new();

        float kitchenWidth = 2.5f;
        float kitchenLength = 4.0f;
        float kitchenHeight = 3.5f;

        float wallGap = 0.05f;

        ObjectInScene floor = InstantiateObject("PlaneY");
        floor.transform.scale = new Vector3(kitchenWidth, 0, kitchenLength);
        floor.ComputeModelMatrix();
        kitchenObjects.objs.Add(floor);

        ObjectInScene roof = InstantiateObject("PlaneY");
        PlaceObjectInScene(roof, kitchenObjects.objs,
            new Vector3(0, kitchenHeight, 0),
            new Vector3(Mathf.Deg2Rad * 180.0f, 0.0f, 0.0f),
            new Vector3(kitchenWidth, 0, kitchenLength)
        );
        roofs.Add(roof);

        ObjectInScene fridge = InstantiateObject("Fridge");
        Bounds fridgeBounds = fridge.GetBoundingBoxDimensions();
        PlaceObjectInScene(fridge, kitchenObjects.objs,
            new Vector3(-fridgeBounds.extents.x/2.0f, 0, kitchenLength / 2.0f - fridgeBounds.extents.z / 2.0f - wallGap*2),
            new Vector3(0, Mathf.Deg2Rad * 90.0f, 0),
            new Vector3(0.55f, 0.55f, 0.55f)
        );

        ObjectInScene bin = InstantiateObject("Bin");
        Bounds binBounds = bin.GetBoundingBoxDimensions();
        PlaceObjectInScene(bin, kitchenObjects.objs,
            new Vector3(-kitchenWidth/2.0f + (binBounds.extents.x * 0.015f) / 2.0f + 0.4f + 0.1f, 0, kitchenLength / 2.0f + (binBounds.extents.z* 0.015f) - 0.2f),
            new Vector3(0, Mathf.Deg2Rad * 90.0f, 0),
            new Vector3(0.015f, 0.015f, 0.015f)
        );

        ObjectInScene cabinetWithSink = InstantiateObject("KitchenCabinetRounded");
        Bounds cabinetBounds = cabinetWithSink.GetBoundingBoxDimensions();
        PlaceObjectInScene(cabinetWithSink, kitchenObjects.objs,
            new Vector3(kitchenWidth / 2.0f - cabinetBounds.extents.x/ 2.0f - wallGap, 0, kitchenLength / 2.0f - cabinetBounds.extents.z/2.0f - wallGap),
            new Vector3(0, Mathf.Deg2Rad * 90.0f, 0),
            new Vector3(0.5f, 0.5f, 0.5f)
        );

        ObjectInScene oven = InstantiateObject("KitchenStove1");
        Bounds ovenBounds = oven.GetBoundingBoxDimensions();
        PlaceObjectInScene(oven, kitchenObjects.objs,
            new Vector3(kitchenWidth / 2.0f - ovenBounds.extents.x / 2.0f - wallGap, 0, ovenBounds.extents.z),
            new Vector3(0, Mathf.Deg2Rad * 180.0f, 0),
            new Vector3(0.5f, 0.5f, 0.5f)
        );

        ObjectInScene cabinet = InstantiateObject("Wardrobe2");
        Bounds wardrobe2Bounds = cabinet.GetBoundingBoxDimensions();
        PlaceObjectInScene(cabinet, kitchenObjects.objs,
            new Vector3(-wardrobe2Bounds.extents.x / 2.0f, 0, -kitchenLength / 2.0f + wardrobe2Bounds.extents.z / 2.0f - wallGap*2.0f),
            new Vector3(0, Mathf.Deg2Rad * 270.0f, 0),
            new Vector3(0.45f, 0.7f, 0.45f)
        );

        ObjectInScene windowWall = InstantiateObject("WindowWall");
        Bounds windowWallBounds = windowWall.GetBoundingBoxDimensions();
        PlaceObjectInScene(windowWall, kitchenObjects.objs,
            new Vector3(kitchenWidth / 2.0f, kitchenHeight / 2.0f, 0),
            new Vector3(Mathf.Deg2Rad * 90.0f * 3, 0, Mathf.Deg2Rad * 90.0f),
            new Vector3(kitchenLength, 0, kitchenWidth*1.4f)
        );
        walls.Add(windowWall);

        ObjectInScene SideWallA = InstantiateObject("PlaneY");
        PlaceObjectInScene(SideWallA, kitchenObjects.objs,
            new Vector3(0, kitchenHeight / 2.0f, kitchenLength / 2.0f),
            new Vector3(Mathf.Deg2Rad * 90.0f, 0),
            new Vector3(kitchenWidth, 0, kitchenHeight)
        );
        walls.Add(SideWallA);

        ObjectInScene SideWallB = InstantiateObject("PlaneY");
        PlaceObjectInScene(SideWallB, kitchenObjects.objs,
            new Vector3(0, kitchenHeight / 2.0f, -kitchenLength / 2.0f),
            new Vector3(-Mathf.Deg2Rad * 90.0f, 0),
            new Vector3(kitchenWidth, 0, kitchenHeight)
        );
        walls.Add(SideWallB);

        return (roofs, walls, kitchenObjects);
    }

    private (List<ObjectInScene> roofs, List<ObjectInScene> walls, Collection bathroomObjects) MakeBathroom()
    {
        Collection bathroomObjects = new Collection();
        List<ObjectInScene> roofs = new();
        List<ObjectInScene> walls = new();

        float bathroomWidth = 4.0f;
        float bathroomLength = 3.0f;
        float bathroomHeight = 3.0f;

        // X is "across"
        ObjectInScene floor = InstantiateObject("PlaneY");
        floor.transform.scale = new Vector3(bathroomWidth, 0, bathroomLength);
        floor.ComputeModelMatrix();
        bathroomObjects.objs.Add(floor);

        ObjectInScene roof = InstantiateObject("PlaneY");
        PlaceObjectInScene(roof, bathroomObjects.objs,
            new Vector3(0, bathroomHeight, 0),
            new Vector3(Mathf.Deg2Rad * 180.0f, 0.0f, 0.0f), // point normal downwards
            new Vector3(bathroomWidth, 0, bathroomLength)
        );
        roofs.Add(roof);

        ObjectInScene toilet = InstantiateObject("toilet2");
        PlaceObjectInScene(toilet, bathroomObjects.objs,
            new Vector3(-1.2f, 0, -1.0f),
            new Vector3(0, Mathf.Deg2Rad * -90.0f, 0),
            new Vector3(0.5f, 0.5f, 0.5f)
        );

        ObjectInScene sink = InstantiateObject("sink");
        Bounds sinkBounds = sink.GetBoundingBoxDimensions();
        PlaceObjectInScene(sink, bathroomObjects.objs,
            new Vector3(-1.2f, 0, 1.4f - sinkBounds.extents.z/2),
            new Vector3(0, Mathf.Deg2Rad * 90.0f, 0),
            new Vector3(0.5f, 0.5f, 0.5f)
        );

        ObjectInScene mirror = InstantiateObject("mirror");
        PlaceObjectInScene(mirror, bathroomObjects.objs,
            new Vector3(-1.2f, sinkBounds.size.y*0.8f - 0.25f, 1.4f),
            new Vector3(0, Mathf.Deg2Rad * 90.0f, 0),
            new Vector3(0.8f, 0.8f, 0.8f)
        );

        ObjectInScene shower = InstantiateObject("shower");
        PlaceObjectInScene(shower, bathroomObjects.objs,
            new Vector3(1.4f, 0, 1.0f),
            new Vector3(0, Mathf.Deg2Rad * 180.0f, 0),
            new Vector3(0.8f, 0.8f, 0.8f)
        );

        return (roofs, walls, bathroomObjects);
    }

    private (List<ObjectInScene> roofs, List<ObjectInScene> walls, Collection allRoomObjects) MakeMainRoom() {
        Collection livingRoomObjects = new Collection();
        List<ObjectInScene> roofs = new();
        List<ObjectInScene> walls = new();

        float mainRoomWidth = 8.0f;
        float mainRoomLength = 4.0f;
        float mainRoomHeight = 3.5f;

        ObjectInScene bed = InstantiateObject("bed1");
        Bounds bedBounds = bed.GetBoundingBoxDimensions();
        PlaceObjectInScene(bed, livingRoomObjects.objs,
            // new Vector3(-bedroomWidth/2 - bedBounds.extents.x, 0, -bedBounds.extents.z), // put the x midpoint exactly at the edge of the wall
            new Vector3(-mainRoomWidth / 2 + bedBounds.extents.x + 0.6f, 0, -bedBounds.extents.z), // remember we're moving the midpoint. that's why my formulas are a little off.
            new Vector3(0, Mathf.Deg2Rad * 270.0f, 0), new Vector3(0.8f, 0.8f, 0.8f)
        );

        ObjectInScene bedsideTable = InstantiateObject("littleOne");
        Bounds bedsideTableBounds = bedsideTable.GetBoundingBoxDimensions();
        PlaceObjectInScene(bedsideTable, livingRoomObjects.objs,
            // new Vector3(-bedroomWidth / 2 + bedsideTableBounds.extents.x + 0.5f, 0, -bedroomLength / 2 + bedsideTableBounds.extents.z),
            new Vector3(-mainRoomWidth / 2 + 0.4f, 0, -mainRoomLength / 4 + bedsideTableBounds.size.z),
            new Vector3(0, 0, 0),
            new Vector3(1, 1, 1)
        );

        ObjectInScene plant = InstantiateObject("plant");
        PlaceObjectInScene(plant, livingRoomObjects.objs,
            new Vector3(-mainRoomWidth / 2 - 0.1f + 0.4f, bedsideTableBounds.size.y + 0.01f, -mainRoomLength / 4 + bedsideTableBounds.size.z + 0.2f),
            Vector3.zero,
            new Vector3(0.08f, 0.08f, 0.08f)
        );

        ObjectInScene nearBedroomWall = InstantiateObject("2x2_Door");
        PlaceObjectInScene(nearBedroomWall, livingRoomObjects.objs,
            new Vector3(-mainRoomWidth / 2 + 3.0f, mainRoomLength / 2, 0),
            new Vector3(0.0f, Mathf.Deg2Rad * 180.0f, Mathf.Deg2Rad * 90.0f),
            new Vector3(mainRoomLength, 0, mainRoomHeight + 0.5f)
        );
        walls.Add(nearBedroomWall);

        // this needs a window
        ObjectInScene farBedroomWall = InstantiateObject("PlaneY");
        PlaceObjectInScene(farBedroomWall, livingRoomObjects.objs,
            new Vector3(-mainRoomWidth / 2, mainRoomLength / 2, 0),
            new Vector3(0.0f, 0.0f, -Mathf.Deg2Rad * 90.0f),
            new Vector3(mainRoomLength, 0, mainRoomHeight + 0.5f)
        );
        walls.Add(farBedroomWall);

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        // X is "across"
        ObjectInScene floor = InstantiateObject("PlaneY");
        floor.transform.scale = new Vector3(mainRoomWidth, 0, mainRoomLength);
        floor.ComputeModelMatrix();
        livingRoomObjects.objs.Add(floor);

        ObjectInScene roof = InstantiateObject("PlaneY");
        PlaceObjectInScene(roof, livingRoomObjects.objs,
            new Vector3(0, mainRoomHeight, 0),
            new Vector3(Mathf.Deg2Rad * 180.0f, 0.0f, 0.0f), // point normal downwards
            new Vector3(mainRoomWidth, 0, mainRoomLength)
        );
        roofs.Add(roof);

        // kitchen-side wall
        ObjectInScene shortWallA = InstantiateObject("PlaneY");
        PlaceObjectInScene(shortWallA, livingRoomObjects.objs,
            new Vector3(mainRoomWidth / 2, mainRoomLength / 2.0f, 0),
            new Vector3(0.0f, 0.0f, Mathf.Deg2Rad * 90.0f),
            new Vector3(mainRoomLength, 0, mainRoomHeight+0.5f)
        );
        walls.Add(shortWallA);

        ObjectInScene longWallA = InstantiateObject("PlaneY");
        longWallA.transform.scale = new Vector3(8, 0, 4);
        longWallA.transform.position = new Vector3(0, 4.0f/2.0f, -4.0f / 2.0f);
        longWallA.transform.rotation = new Vector3(Mathf.Deg2Rad * 90.0f, 0.0f, 0.0f);
        longWallA.ComputeModelMatrix();
        livingRoomObjects.objs.Add(longWallA);
        walls.Add(longWallA);

        ObjectInScene longWallB = InstantiateObject("PlaneY");
        longWallB.transform.scale = new Vector3(8, 0, 4);
        longWallB.transform.position = new Vector3(0, 4.0f / 2.0f, 4.0f / 2.0f);
        longWallB.transform.rotation = new Vector3(-Mathf.Deg2Rad * 90.0f, 0.0f, 0.0f);
        longWallB.ComputeModelMatrix();
        livingRoomObjects.objs.Add(longWallB);
        walls.Add(longWallB);

        // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        ObjectInScene pcTable = InstantiateObject("pcTable2");
        pcTable.transform.position = new Vector3(0, 0, -1.65f);
        pcTable.transform.rotation = new Vector3(0, Mathf.Deg2Rad * 270.0f, 0);
        pcTable.ComputeModelMatrix();
        livingRoomObjects.objs.Add(pcTable);

        ObjectInScene pcChair = InstantiateObject("chair2");
        pcChair.transform.position = new Vector3(0.2f, 0, -1.45f);
        pcChair.transform.rotation = new Vector3(0, Mathf.Deg2Rad * 84.5f, 0);
        pcChair.ComputeModelMatrix();
        livingRoomObjects.objs.Add(pcChair);

        ObjectInScene table = InstantiateObject("widetable");
        table.transform.position = new Vector3(2.0f, 0, 1.3f);
        table.transform.rotation = new Vector3(0, Mathf.Deg2Rad * 90.0f, 0);
        table.ComputeModelMatrix();

        ObjectInScene chair1 = InstantiateObject("chair4");
        chair1.transform.position = new Vector3(2.0f + 0.4f, 0, 1.3f + 0.25f);
        chair1.transform.rotation = new Vector3(0, Mathf.Deg2Rad * 90.0f, 0);
        chair1.ComputeModelMatrix();

        ObjectInScene chair2 = InstantiateObject("chair1");
        chair2.transform.position = new Vector3(2.0f - 0.4f, 0, 1.3f + 0.25f);
        chair2.transform.rotation = new Vector3(0, Mathf.Deg2Rad * 90.0f, 0);
        chair2.ComputeModelMatrix();

        ObjectInScene chair3 = InstantiateObject("chair4");
        chair3.transform.position = new Vector3(2.0f + 0.4f, 0, 1.3f - 0.25f);
        chair3.transform.rotation = new Vector3(0, -Mathf.Deg2Rad * 90.0f, 0);
        chair3.ComputeModelMatrix();

        ObjectInScene chair4 = InstantiateObject("chair1");
        chair4.transform.position = new Vector3(2.0f - 0.4f, 0, 1.3f - 0.25f);
        chair4.transform.rotation = new Vector3(0, -Mathf.Deg2Rad * 90.0f, 0);
        chair4.ComputeModelMatrix();

        ObjectInScene chair5 = InstantiateObject("chair1");
        chair5.transform.position = new Vector3(1.1f, 0, 1.3f);
        chair5.transform.rotation = new Vector3(0, Mathf.Deg2Rad * 15.4f, 0);
        chair5.ComputeModelMatrix();

        livingRoomObjects.objs.Add(table);
        livingRoomObjects.objs.Add(chair1);
        livingRoomObjects.objs.Add(chair2);
        livingRoomObjects.objs.Add(chair3);
        livingRoomObjects.objs.Add(chair4);
        livingRoomObjects.objs.Add(chair5);

        return (roofs, walls, livingRoomObjects);
    }

    private void AddRoom(List<ObjectInScene> newRoofs, List<ObjectInScene> newWalls, Collection allNewObjects,
        List<ObjectInScene> roofs, List<ObjectInScene> walls, List<ObjectInScene> allSceneObjects) {
        walls.AddRange(newWalls);
        roofs.AddRange(newRoofs);
        allSceneObjects.AddRange(allNewObjects.objs);
    }

    List<ObjectInScene> roofs;
    List<ObjectInScene> walls;
    private void CreateScene()
    {
        sceneObjects = new List<ObjectInScene>();
        roofs = new();
        walls = new();

        (List<ObjectInScene> livingRoomRoof, List<ObjectInScene> livingRoomWalls, Collection livingRoomObjects) = MakeMainRoom();
        livingRoomObjects.collectionTransform.position = new Vector3(-8.0f / 2.0f - 2.5f / 2.0f, 0, 0);
        livingRoomObjects.UpdateTransforms();
        AddRoom(livingRoomRoof, livingRoomWalls, livingRoomObjects, roofs, walls, sceneObjects);

        (List<ObjectInScene> kitchenRoofs, List<ObjectInScene> kitchenWalls, Collection kitchenObjects) = MakeKitchen();
        /* kitchenObjects.collectionTransform.position = new Vector3(0, 0, 0);
        kitchenObjects.UpdateTransforms(); */
        sceneObjects.AddRange(kitchenObjects.objs);
        roofs.AddRange(kitchenRoofs); walls.AddRange(kitchenWalls);

        (List<ObjectInScene> bathroomRoofs, List<ObjectInScene> bathroomWalls, Collection bathroomObjects) = MakeBathroom();
        bathroomObjects.collectionTransform.position = new Vector3(-7.25f, 0, 3.5f);
        bathroomObjects.UpdateTransforms();
        sceneObjects.AddRange(bathroomObjects.objs);
        roofs.AddRange(bathroomRoofs); walls.AddRange(bathroomWalls);
    }

    ObjectInScene LoadObject(string path)
    {
        OBJMeshData originalMeshData = OBJReader.Parse(path);

        // TODO: better error handling
        if (originalMeshData == null) { return LoadObject("Assets/Models/monkey.obj"); }

        OBJMeshData meshData = OBJReader.MoveToWorldOrigin(originalMeshData);

        // object basics
        string ObjectName = meshData.Name ?? "Unnamed object";
        GameObject newObject = new GameObject(ObjectName);
        newObject.AddComponent<MeshFilter>();
        newObject.GetComponent<MeshFilter>().mesh = new Mesh();
        newObject.AddComponent<MeshRenderer>();

        // vertices
        Vector3[] meshVertices = meshData.GetVertexArray();
        newObject.GetComponent<MeshFilter>().mesh.vertices = meshVertices;

        // triangles (substract by one because Unity uses 0-indexed triangle indices)
        int[] triangles = meshData.FacesToIntArray().Select(i => i - 1).ToArray();
        newObject.GetComponent<MeshFilter>().mesh.triangles = triangles;

        newObject.GetComponent<MeshFilter>().mesh.RecalculateNormals();
        
        // materials
        Material newMaterial = new Material(Shader.Find("CustomMatrixRenderer"));
        newObject.GetComponent<MeshRenderer>().material = newMaterial;

        newObject.GetComponent<MeshRenderer>().material.SetColor("_color", Random.ColorHSV());
        newObject.GetComponent<MeshRenderer>().material.SetVector("_LightColor", Color.white);
        newObject.GetComponent<MeshRenderer>().material.SetVector("_LightPos", Vector3.up);

        ObjectInScene _object = new ObjectInScene();
        _object.obj = newObject;
        _object.transform.position = new Vector3(0, 0, 0);
        _object.transform.rotation = new Vector3(0.0f, 0.0f, 0.0f);
        _object.transform.scale = new Vector3(0.5f, 0.5f, 0.5f); // check which commit added this
        _object.ComputeModelMatrix();
        _object.SetViewMatrix(ViewMatrix);
        _object.SetProjectionMatrix(ProjectionMatrix);

        // doesn't matter now, but it must be recomputed when the object gets placed in the scene
        // that's why every object shines the same way, because there's a single light at (0,1,0) and it shines on every object as if it had this basic configuration?
        // or rather, the transformed normal gets multiplied by this unchanging matrix even when the view matrix or model matrix change
        _object.obj.GetComponent<MeshRenderer>().material.SetMatrix("_PVMInverse", (ProjectionMatrix * ViewMatrix * _object.ObjectModelMatrix).inverse );

        sceneObjects.Add(_object);
        return _object;
    }

    private bool pressingLeft = false;
    private bool pressingRight = false;
    private bool pressingForward = false;
    private bool pressingBackward = false;

    void Update() {
        if (Input.GetKeyDown(KeyCode.A)) { pressingLeft = true; }
        if (Input.GetKeyUp(KeyCode.A)) { pressingLeft = false; }

        if (Input.GetKeyDown(KeyCode.D)) { pressingRight = true; }
        if (Input.GetKeyUp(KeyCode.D)) { pressingRight = false; }

        if (Input.GetKeyDown(KeyCode.W)) { pressingForward = true; }
        if (Input.GetKeyUp(KeyCode.W)) { pressingForward = false; }

        if (Input.GetKeyDown(KeyCode.S)) { pressingBackward = true; }
        if (Input.GetKeyUp(KeyCode.S)) { pressingBackward = false; }

        if (Input.GetKeyDown(KeyCode.C)) { SwitchCamera(); }
        if (Input.GetKeyDown(KeyCode.T)) { ReloadColors(); }
        if (Input.GetKeyDown(KeyCode.Y)) { ToggleWalls(); }
        if (Input.GetKeyDown(KeyCode.U)) { ToggleRoofs(); }

        if (currentCamera != null) { 
            currentCamera.Update(pressingLeft, pressingRight, pressingForward, pressingBackward);
            ViewMatrix = currentCamera.ComputeViewMatrix();
            foreach (ObjectInScene obj in sceneObjects)
            {
                obj.SetViewMatrix(ViewMatrix);
                // obj.obj.GetComponent<MeshRenderer>().material.SetMatrix("_PVMInverse", (ProjectionMatrix * ViewMatrix * obj.ObjectModelMatrix).inverse);
                // obj.RecomputeModelMatrix(); // the objects won't move for now
            }
        } else {
            // Debug.LogWarning("currentCamera == null");
            CreateCamera();
            CreateScene();
        }
    }

    private void SwitchCamera() {
        // The projection matrix should switch too
        // It's the same for both right now, so it works without any changes
        if (currentCamera == firstPersonCamera) {
            currentCamera = orbitalCamera;
        } else {
            currentCamera = firstPersonCamera;
        }
    }

    private void ReloadColors() {
        foreach (ObjectInScene obj in sceneObjects) {
            obj.SetColor(Random.ColorHSV());
        }
    }

    private void ToggleWalls() {
        foreach(ObjectInScene wall in walls) {
            wall.ToggleActive();
        }
    }

    private void ToggleRoofs() {
        foreach (ObjectInScene roof in roofs) {
            roof.ToggleActive();
        }
    }

    private void CreateUnityCamera()
    {
        unityCamera = new GameObject("Camera");
        unityCamera.AddComponent<Camera>();
        unityCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        unityCamera.GetComponent<Camera>().backgroundColor = Color.black;
    }
}