using UnityEngine;

using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Collections.Generic;

public class OBJVertex {
    
    public int index; // starts at 1

    public OBJVertex(int i, Vector3 v)
    {
        index = i;
        vector = v;
    }

    public Vector3 vector;

    public OBJVertex(int i, float x, float y, float z) {
        index = i;
        vector = new Vector3(x, y, z);
    }

    public Vector3 ToV3() {
        return vector;
    }
}

public class OBJFace
{
    public const int UNSET = -1;
    public int v1, v2, v3, v4;
    public int v1_n, v2_n, v3_n, v4_n;
    // TODO: handle vt values

    public OBJFace()
    {
        v1 = UNSET; v2 = UNSET; v3 = UNSET; v4 = UNSET;
        v1_n = UNSET; v2_n = UNSET; v3_n = UNSET; v4_n = UNSET;
    }

    private Tuple<int, int, int> GetVertex(int index)
    {
        if (index == 0) { return Tuple.Create(v1, UNSET, v1_n); }
        if (index == 1) { return Tuple.Create(v2, UNSET, v2_n); ; }
        if (index == 2) { return Tuple.Create(v3, UNSET, v3_n); ; }
        if (index == 3) { return Tuple.Create(v4, UNSET, v4_n); ; }
        return Tuple.Create(UNSET, UNSET, UNSET); ;

    }

    private void SetVertex(int index, Tuple<int, int, int> values)
    {
        if (index == 0) { v1 = values.Item1; v1_n = values.Item3; }
        if (index == 1) { v2 = values.Item1; v2_n = values.Item3; }
        if (index == 2) { v3 = values.Item1; v3_n = values.Item3; }
        if (index == 3) { v4 = values.Item1; v4_n = values.Item3; }
    }

    public Tuple<int, int, int> this[int index]
    {
        get => GetVertex(index);
        set => SetVertex(index, value);
    }
}
public class OBJNormal
{
    public int index; // starts at 1
    public Vector3 vector;

    public OBJNormal(int i, Vector3 v)
    {
        index = i;
        vector = v;
    }

    public OBJNormal(int i, float x, float y, float z)
    {
        index = i;
        vector = new Vector3(x, y, z);
    }

    public Vector3 ToV3() { return vector; }
}

public class OBJMeshData
{
    public string Name;
    public OBJVertex[] Vertices;
    public OBJNormal[] VertexNormals;
    public OBJFace[] Faces;

    public Vector3[] GetMeshVertexNormals() {
        if (VertexNormals == null) { 
            Debug.LogError("[ObjMeshData::GetMeshVertexNormals] VertexNormals == null?"); 
        } else {
            Debug.LogError(VertexNormals.Length + " VERTEX NORMALS");
            return VertexNormals.Select(n => n.vector).ToArray();
        }
        return new Vector3[1];
    }

    public int[] FacesToIntArray()
    {
        List<int> vertexIndices = new List<int>();
        foreach (OBJFace face in Faces)
        {
            if (face.v4 == OBJFace.UNSET) {
                vertexIndices.Add(face.v1);
                vertexIndices.Add(face.v2);
                vertexIndices.Add(face.v3);
            } else {
                // We have A B C D
                vertexIndices.Add(face.v4); // A
                vertexIndices.Add(face.v1); // B
                vertexIndices.Add(face.v3); // C

                vertexIndices.Add(face.v3); // A
                vertexIndices.Add(face.v1); // C
                vertexIndices.Add(face.v2); // D
            }
        }

        int[] intArrayIndices = vertexIndices.ToArray();
        return intArrayIndices;
    }

    public Vector3[] GetVertexArray()
    {
        return this.Vertices.Select(v => v.vector).ToArray();
    }

}

public enum FaceValuesIndices {
    V_INDEX = 0,
    VT_INDEX,
    VN_INDEX
}

public class OBJReader
{
    static Func<string, bool> isComment = l => l.StartsWith("#");
    static Func<string, bool> isObjectNameDef = l => l.StartsWith("o ");
    static Func<string, bool> isVertexDef = l => l.StartsWith("v ");
    static Func<string, bool> isVertexNormalDef = l => l.StartsWith("vn ");
    static Func<string, bool> isVertexTextureDef = l => l.StartsWith("vt ");
    static Func<string, bool> isFaceDef = l => l.StartsWith("f ");

    public static OBJMeshData Parse(string path)
    {
        IEnumerable<string> lines = ReadNonEmptyLinesFromFile(path);

        OBJMeshData meshData = new OBJMeshData();
        List<OBJVertex> objectVertices = new List<OBJVertex>();
        List<OBJFace> objectFaces = new List<OBJFace>();
        List<OBJNormal> vertexNormals = new List<OBJNormal>();

        int vertexIndex = 1;
        int vnIndex = 1;
        // int vtIndex = 1; // Later?

        string ObjectName = "Unnamed";

        foreach (string line in lines)
        {
            // Debug.Log("Now procesing " + line);

            if (isComment(line))
            {
                continue;
            }
            else if (isObjectNameDef(line))
            {
                // Skip "o ", grab the rest, and trim whitespace around the text
                // this will end up as the name of a Unity GameObject
                // Probably wise to sanitize later according to Unity restrictions (if any)
                ObjectName = line[("o ".Length)..].Trim();
            }
            else if (isVertexDef(line))
            {
                // Debug.Log("Found Vertex: " + line);
                (OBJVertex parsedVertex, bool vertexParseError) = ParseVertexDefinition(line.Trim(), vertexIndex);

                if (vertexParseError)
                {
                    // exit early
                    return null; // ?
                }

                objectVertices.Add(parsedVertex);
                vertexIndex++;
            }
            else if (isVertexNormalDef(line))
            {
                (OBJNormal vertexNormal, bool faceParseError) = ParseVertexNormalDefinition(line.Trim(), vnIndex);
                if (faceParseError)
                {
                    // exit early
                    return null; // ?
                }
                vertexNormals.Add(vertexNormal);
                vnIndex++;
            }
            else if (isFaceDef(line))
            {
                (OBJFace face, bool faceParseError) = ParseFaceDefinition(line.Trim());

                if (faceParseError) {
                    return null;
                }

                objectFaces.Add(face);
            }
            else {
                // Debug.Log("Ignoring " + line);
            }
        }

        OBJFace[] facesArray = objectFaces.ToArray();
        Debug.Log(">>>>>>>>> Object faces found: " + facesArray.Length + "<<<<<<<<<<");
        OBJVertex[] verticesArray = objectVertices.ToArray();
        Debug.Log(">>>>>>>>> Object vertices found: " + verticesArray.Length + "<<<<<<<<<<");
        OBJNormal[] vertexNormalsArray = vertexNormals.ToArray();
        Debug.Log(">>>>>>>>> Vertex normals found: " + vertexNormalsArray.Length + "<<<<<<<<<<");
        meshData.Faces = facesArray;
        meshData.Vertices = verticesArray;
        meshData.VertexNormals = vertexNormalsArray;
        meshData.Name = ObjectName;
        return meshData;
    }

    public static (OBJNormal vertex, bool vertexParseError) ParseVertexNormalDefinition(string line, int currentVertexNormalIndex) {
        bool parseError = false;
        OBJNormal vertexNormal = new(currentVertexNormalIndex, Vector3.zero);
        string[] Parts = line.Split().Select(p => p.Trim()).ToArray();
        string[] Rest = Parts[1..];

        Vector3 normalCoordsV3 = new Vector3(0.0f, 0.0f, 0.0f);
        if (Rest.Length == 3)
        {
            for (int i = 0; i < 3; i++)
            {
                float result;
                bool isFloat = float.TryParse(Rest[i], out result);
                if (!isFloat)
                {
                    Debug.Log("Invalid vertex definition!!! (line = " + line + ")");
                    Debug.Log("Check vertex N° " + i.ToString() + " -> Not a float");
                    parseError = true;
                }
                else
                {
                    normalCoordsV3[i] = result;
                }
            }
        } else {
            Debug.Log("Invalid vertex definition!!! (line = " + line + ")");
            parseError = true;
        }

        vertexNormal.vector = normalCoordsV3;


        return (vertexNormal, parseError);
    }

    public static (OBJVertex vertex, bool vertexParseError) ParseVertexDefinition(string line, int currentVertexIndex)
    {
        bool parseError = false;
        OBJVertex vertex;

        string[] Parts = line.Split().Select(p => p.Trim()).ToArray();

        string[] Rest = Parts[1..];
        Vector3 vertexCoordsV3 = new Vector3(0.0f, 0.0f, 0.0f);
        if (Rest.Length == 3)
        {
            for (int i = 0; i < 3; i++)
            {
                float result;
                bool isFloat = float.TryParse(Rest[i], out result);
                if (!isFloat)
                {
                    Debug.Log("Invalid vertex definition!!! (line = " + line + ")");
                    Debug.Log("Check vertex N° " + i.ToString() + " -> Not a float");
                    parseError = true;
                }
                else
                {
                    vertexCoordsV3[i] = result;
                }
            }
        }
        else
        {
            Debug.Log("Invalid vertex definition!!! (line = " + line + ")");
            parseError = true;
        }

        // Debug.Log(vertexCoordsV3);
        vertex = new OBJVertex(currentVertexIndex, vertexCoordsV3);

        return (vertex, parseError);
    }

    public static (OBJFace, bool) ParseFaceDefinition(string line)
    {
        bool parseError = false;

        string[] Parts = line.Split().Select(p => p.Trim()).ToArray();

        string[] Rest = Parts[1..];
        OBJFace face = new OBJFace();

        if (Rest.Length < 1)
        {
            Debug.Log("Invalid face definition!!! (line = " + line + ")");
            parseError = true;
        }
        else
        {
            const int UNSET = -1;

            /*
            string RestAsStr = "";
            RestAsStr += "[";
            foreach (string r in Rest) { RestAsStr += r + "; "; }
            RestAsStr += "]";
            Debug.Log(RestAsStr);
            Debug.Log("found " + Rest.Length + " faces");
            */

            // standing here, there's going to be three elements in each string in Rest, something like "v/vt/vn v/vt/vn v/vt/vn".Split(" ")
            // each of these goes into one OBJFace (instantiated above)
            for (int i = 0; i < Rest.Length; i++)
            {
                string VertexToParse = Rest[i];

                (int parsed_v, int parsed_vt, int parsed_vn, bool vertexRefParseError) = ParseVertexForFace(VertexToParse, line);
                parseError = vertexRefParseError;
                if (parseError) {
                    break;
                } else {
                    // Debug.Log("Setting vertex " + i.ToString() + " of face to value " + v);
                    face[i] = Tuple.Create(parsed_v, UNSET, parsed_vn);
                    // wvt?
                }
            }
        }

        return (face, parseError);
    }

    public static (int v, int vt, int vn, bool parseError) ParseVertexForFace(string VertexToParse, string currentLine)
    {
        const int UNSET = -1;
        bool parseError = false;
        int v = UNSET, vt = UNSET, vn = UNSET;

        // Format: int (v) | int/int (v,vt) | int/nothing/int (v,vn) | int/int/int (v,vt,vn)
        bool isSingleInt = int.TryParse(VertexToParse, out int maybeVertexIndex); // inlined declaration
        if (isSingleInt)
        {
            // it's a single int, stored in maybeVertexIndex
            v = maybeVertexIndex;
        }
        else
        {
            // it can be v, v/vt, v//vn, or v/vt/vn
            // the case above handles v
            string[] v_vt_vn = VertexToParse.Split("/");
            if (v_vt_vn.Length > 3)
            {
                Debug.Log("Parse Error: Face vertex def not of the form V/VT, V//VN or V/VT/VN");
                parseError = true;
            }
            else if (v_vt_vn.Length == 3)
            {
                // we have v, vt, and vn
                bool v_is_an_int = int.TryParse(v_vt_vn[(int)FaceValuesIndices.V_INDEX], out v);

                bool vt_is_an_int = false;
                bool no_vt_on_facedef = v_vt_vn[(int)FaceValuesIndices.VT_INDEX] == "";
                if (no_vt_on_facedef)
                {
                    vt = UNSET;
                }
                else
                {
                    vt_is_an_int = int.TryParse(v_vt_vn[(int)FaceValuesIndices.VT_INDEX], out vt);
                }

                bool vn_is_an_int = int.TryParse(v_vt_vn[(int)FaceValuesIndices.VN_INDEX], out vn);

                if (!v_is_an_int || (!no_vt_on_facedef && !vt_is_an_int) || !vn_is_an_int)
                {
                    Debug.Log("Invalid face definition!!! See: " + currentLine);
                    parseError = true;
                }
            }
            else if (v_vt_vn.Length == 2)
            {
                // we have v and vt
                bool v_is_an_int = int.TryParse(v_vt_vn[(int)FaceValuesIndices.V_INDEX], out v);
                bool vt_is_an_int = int.TryParse(v_vt_vn[(int)FaceValuesIndices.VT_INDEX], out vt);
                if (!v_is_an_int || !vt_is_an_int)
                {
                    Debug.Log("Invalid face definition!!! See: " + currentLine);
                    parseError = true;
                }
            }

        }

        return (v, vt, vn, parseError);
    }

    public static OBJMeshData MoveToWorldOrigin(OBJMeshData model)
    {
        // first pass: for each axis, find min and max
        // working in terms of vertices here. the faces will move with the vertices
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;

        OBJVertex[] newVertices = new OBJVertex[model.Vertices.Length];

        foreach (OBJVertex v in model.Vertices) {
            if (v.vector.x < minX) { minX = v.vector.x; }
            if (v.vector.x > maxX) { maxX = v.vector.x; }

            if (v.vector.y < minY) { minY = v.vector.y; }
            if (v.vector.y > maxY) { maxY = v.vector.y; }

            if (v.vector.z < minZ) { minZ = v.vector.z; }
            if (v.vector.z > maxZ) { maxZ = v.vector.z; }
        }

        // midpoint at x and z, but the bottom of the bounding box lies flat on the Y = 0 plane
        Vector3 translationVector = new Vector3(
                minX + (/* bounding box x midpoint: */ (maxX - minX) / 2.0f),
                minY, 
                minZ + (/* bounding box z midpoint: */ (maxZ - minZ) / 2.0f)
        );

        Debug.LogWarning("Translation vector: " + translationVector);

        // now, foreach vector v, v' = v - translationVector
        newVertices = model.Vertices.Select(v => new OBJVertex(v.index, v.vector - translationVector)).ToArray();

        /*
        // for debugging purposes:
        for (int i = 0; i < model.Vertices.Length; i++)
        {
            Vector3 newVector = model.Vertices[i].vector - translationVector;
            Debug.LogWarning(">>>>>>>>>> New (translated) vector: " + newVector);
            newVertices[i] = new OBJVertex(model.Vertices[i].index, newVector);
        }
        */

        OBJMeshData newModel = new OBJMeshData();
        newModel.Vertices = newVertices;
        newModel.Faces = model.Faces;
        newModel.Name = model.Name + "AtWorldOrigin";

        return newModel;
    }

    public static IEnumerable<string> ReadNonEmptyLinesFromFile(string path)
    {
        StreamReader reader = new StreamReader(path);
        string ModelData = reader.ReadToEnd();
        reader.Close();
        string[] lines = ModelData.Split("\n");
        return lines.Where(line => line.Length > 0);
    }
}