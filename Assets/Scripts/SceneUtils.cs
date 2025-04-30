using System.Collections.Generic;
using UnityEngine;

public class CollectionTree
{
    public SceneTransform transform;
    List<ObjectInScene> objects;
    List<CollectionTree> subcollections;
    public CollectionTree()
    {
        objects = new();
        subcollections = new();
    }

    public void AddTopLevelChild(ObjectInScene obj)
    {
        objects.Add(obj);
    }

    public void AddChildCollection(CollectionTree col)
    {
        subcollections.Add(col);
    }

    // this is for the root collection to transform all its children
    public void ApplyRootTransform()
    {
        Matrix4x4 transformModelMatrix = transform.ModelMatrix();
        ApplyTransform(transformModelMatrix);
    }

    public void ApplyTransform(Matrix4x4 parentTransform)
    {
        foreach (ObjectInScene obj in objects)
        {
            Matrix4x4 newModelMatrix = parentTransform * obj.transform.ModelMatrix();
            obj.SetModelMatrix(newModelMatrix);
        }

        foreach (CollectionTree ct in subcollections)
        {
            ct.ApplyTransform(parentTransform);
        }
    }
}

public class Collection
{
    public List<ObjectInScene> objs;
    public SceneTransform collectionTransform;

    /*
    public void GetCollectionBounds()
    {
        Vector3 bounds = new Vector3();
        foreach (ObjectInScene obj in objs) {
            // ...  obj.GetBoundingBoxDimensions();
        }
    }
    */
    public Collection()
    {
        objs = new();
        collectionTransform = new SceneTransform();
    }

    public void UpdateTransforms()
    {
        foreach (ObjectInScene obj in objs)
        {
            Matrix4x4 newModelMatrix = collectionTransform.ModelMatrix() * obj.transform.ModelMatrix();
            obj.SetModelMatrix(newModelMatrix);
        }
    }

}

public class SceneTransform
{
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;

    public SceneTransform()
    {
        // reasonable defaults: leave the object where it is
        scale = Vector3.one;
        rotation = Vector3.zero;
        position = Vector3.zero;
    }

    /*
    public Matrix4x4 ModelMatrixUnity()
    {
        Matrix4x4 t = Matrix4x4.Translate(this.position);
        Matrix4x4 r = Matrix4x4.Rotate(Quaternion.Euler(this.rotation));
        Matrix4x4 s = Matrix4x4.Scale(this.scale);

        // return (t*r*s).transpose; // interesting visual effect
        // return (t * r * s);
        return Matrix4x4.TRS(position, Quaternion.Euler(rotation), scale);
    }*/

    public Matrix4x4 ModelMatrix()
    {
        Matrix4x4 positionMatrix = new Matrix4x4(
            new Vector4(1.0f, 0.0f, 0.0f, position.x),
            new Vector4(0.0f, 1.0f, 0.0f, position.y),
            new Vector4(0.0f, 0.0f, 1.0f, position.z),
            new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
        );
        positionMatrix = positionMatrix.transpose;

        Matrix4x4 rotationMatrixX = new Matrix4x4(
            new Vector4(1.0f, 0.0f, 0.0f, 0.0f),
            new Vector4(0.0f, Mathf.Cos(rotation.x), -Mathf.Sin(rotation.x), 0.0f),
            new Vector4(0.0f, Mathf.Sin(rotation.x), Mathf.Cos(rotation.x), 0.0f),
            new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
        );

        Matrix4x4 rotationMatrixY = new Matrix4x4(
            new Vector4(Mathf.Cos(rotation.y), 0.0f, Mathf.Sin(rotation.y), 0.0f),
            new Vector4(0.0f, 1.0f, 0.0f, 0.0f),
            new Vector4(-Mathf.Sin(rotation.y), 0.0f, Mathf.Cos(rotation.y), 0.0f),
            new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
        );

        Matrix4x4 rotationMatrixZ = new Matrix4x4(
            new Vector4(Mathf.Cos(rotation.z), -Mathf.Sin(rotation.z), 0.0f, 0.0f),
            new Vector4(Mathf.Sin(rotation.z), Mathf.Cos(rotation.z), 0.0f, 0.0f),
            new Vector4(0.0f, 0.0f, 1.0f, 0.0f),
            new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
        );

        Matrix4x4 rotationMatrix = rotationMatrixZ * rotationMatrixY * rotationMatrixX;
        rotationMatrix = rotationMatrix.transpose;

        Matrix4x4 scaleMatrix = new Matrix4x4(
            new Vector4(scale.x, 0.0f, 0.0f, 0.0f),
            new Vector4(0.0f, scale.y, 0.0f, 0.0f),
            new Vector4(0.0f, 0.0f, scale.z, 0.0f),
            new Vector4(0.0f, 0.0f, 0.0f, 1.0f)
        );
        scaleMatrix = scaleMatrix.transpose;

        Matrix4x4 finalMatrix = positionMatrix;
        finalMatrix *= rotationMatrix;
        finalMatrix *= scaleMatrix;
        return finalMatrix;
    }
}
public class ObjectInScene
{
    public GameObject obj;
    public SceneTransform transform;
    public Matrix4x4 ObjectModelMatrix;

    public ObjectInScene()
    {
        transform = new SceneTransform();
        ObjectModelMatrix = new Matrix4x4();
    }

    public Bounds GetBoundingBoxDimensions()
    {
        return obj.GetComponent<MeshFilter>().mesh.bounds;
    }

    public void ToggleActive()
    {
        this.obj.SetActive(!this.obj.activeInHierarchy);
    }

    public void ComputeModelMatrix()
    {
        Matrix4x4 modelMatrix = transform.ModelMatrix();
        this.obj.GetComponent<Renderer>().material.SetMatrix("_ModelMatrix", modelMatrix);
        ObjectModelMatrix = modelMatrix;
    }

    public void SetColor(Color c)
    {
        obj.GetComponent<MeshRenderer>().material.SetColor("_color", c);
    }

    public void SetModelMatrix(Matrix4x4 newModelMatrix)
    {
        ObjectModelMatrix = newModelMatrix;
        this.obj.GetComponent<Renderer>().material.SetMatrix("_ModelMatrix", newModelMatrix);
    }

    public void SetViewMatrix(Matrix4x4 viewMatrix)
    {
        this.obj.GetComponent<Renderer>().material.SetMatrix("_ViewMatrix", viewMatrix);
    }

    public void SetProjectionMatrix(Matrix4x4 projectionMatrix)
    {
        this.obj.GetComponent<Renderer>().material.SetMatrix("_ProjectionMatrix", projectionMatrix);
    }
}