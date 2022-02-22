using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public struct HyperPlane
{
    public GameObject obj { get; set; }
    public Vector3 components { get; set; }

    public float constant { get; set; }

    public static Shader shader { get; set; }

    public HyperPlane(Vector3 components, float constant)
    {
        this.obj = new GameObject();
        MeshRenderer meshRenderer = this.obj.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(HyperPlane.shader);
        this.components = components;
        this.constant = constant;

        MeshFilter meshFilter = this.obj.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();



        mesh.vertices = new Vector3[]{
            new Vector3(constant / components.x, 0, 0),
            new Vector3(0, constant / components.y, 0),
            new Vector3(0, 0, constant / components.z),
        };

        mesh.triangles = new int[]{
            0, 1, 2
        };

        mesh.normals = new Vector3[]{
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
        };

        // meshFilter.mesh = mesh;

    }
}

public class Grapher : MonoBehaviour
{


    Vector4 vec3to4(Vector3 vec, float w)
    {
        return new Vector4(vec.x, vec.y, vec.z, w);
    }

    MeshFilter mf;
    MeshRenderer meshRenderer;

    public Shader toUse;

    HyperPlane[] planes;

    // Start is called before the first frame update
    void Start()
    {

        HyperPlane.shader = toUse;

        planes = new HyperPlane[7];
        planes[0] = new HyperPlane(new Vector3(1, 0, 0), 2);
        planes[1] = new HyperPlane(new Vector3(1, 0, 0), -2);
        planes[2] = new HyperPlane(new Vector3(0, 1, 0), 2);
        planes[3] = new HyperPlane(new Vector3(0, 1, 0), -2);
        planes[4] = new HyperPlane(new Vector3(0, 0, 1), 2);
        planes[5] = new HyperPlane(new Vector3(0, 0, 1), -2);
        planes[6] = new HyperPlane(new Vector3(1, 1, 1), 0.5f);

    }

    private (UnityEngine.Matrix4x4 mat, UnityEngine.Vector4 solution) getBasisMatrix(int i, int j, int k)
    {
        Matrix4x4 mat = new Matrix4x4();
        mat.SetRow(0, vec3to4(planes[i].components, 0));
        mat.SetRow(1, vec3to4(planes[j].components, 0));
        mat.SetRow(2, vec3to4(planes[k].components, 0));
        mat.SetRow(3, new Vector4(0, 0, 0, 1));

        Vector4 solution = mat.inverse * new Vector4(
            planes[i].constant,
            planes[j].constant,
            planes[k].constant,
            0
        );

        return (mat, solution);
    }


    private void OnDrawGizmos()
    {

        List<int[]> bases = new List<int[]>();

        if (planes == null) return;
        for (int i = 0; i < planes.Length; i++)
        {
            for (int j = i + 1; j < planes.Length; j++)
            {
                if (i == j) continue;
                for (int k = j + 1; k < planes.Length; k++)
                {
                    if (k == j || k == i) continue;

                    // figure out where these 3 planes intersect
                    Matrix4x4 mat = new Matrix4x4();
                    mat.SetRow(0, vec3to4(planes[i].components, 0));
                    mat.SetRow(1, vec3to4(planes[j].components, 0));
                    mat.SetRow(2, vec3to4(planes[k].components, 0));
                    mat.SetRow(3, new Vector4(0, 0, 0, 1));

                    Vector4 solution = mat.inverse * new Vector4(
                        planes[i].constant,
                        planes[j].constant,
                        planes[k].constant,
                        0
                    );

                    if (mat.determinant != 0)
                    {
                        bool satisfiesAll = true;
                        foreach (var constraint in planes)
                        {   
                            var prod = Vector3.Dot(solution, constraint.components);
                            if(constraint.constant < 0 && prod < constraint.constant){
                                satisfiesAll = false;
                                break;
                            }
                            if(constraint.constant > 0 && prod > constraint.constant){
                                satisfiesAll = false;
                                break;
                            }
                        }
                        if (satisfiesAll)
                        {
                            Gizmos.DrawSphere(new Vector3(solution.x, solution.y, solution.z), 0.2f);
                            bases.Add(new int[3] { i, j, k });
                        }
                    }
                }
            }
        }

        
        for (int i = 0; i < bases.Count; i++)
        {
            int[] basis = bases[i];
            for (int j = i + 1; j < bases.Count; j++)
            {
                int[] otherBasis = bases[j];
                int intersections = 0;

                foreach (var item in basis)
                {
                    foreach (var otherItem in otherBasis)
                    {
                        if(item == otherItem) intersections++;
                    }
                }

                if (intersections == 2)
                {
                    var first = getBasisMatrix(basis[0], basis[1], basis[2]);
                    var second = getBasisMatrix(otherBasis[0], otherBasis[1], otherBasis[2]);

                    Gizmos.DrawLine(stripVec4(first.solution), stripVec4(second.solution));
                }
                //  else if (differences == 2){
                //     Debug.Log(basis[0] + ", " + basis[1] + ", " + basis[2] + " vs " + otherBasis[0] + ", " + otherBasis[1] + ", " + otherBasis[2]);
                // }
            }
        }
    }

    private Vector3 stripVec4(Vector4 toReduce)
    {
        return new Vector3(toReduce.x, toReduce.y, toReduce.z);
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        for (int i = 0; i < planes.Length; i++)
        {
            HyperPlane hyperplane = planes[i];
            // Debug.DrawLine(Vector3.zero, hyperplane.components, Color.red);
        }

    }
}
