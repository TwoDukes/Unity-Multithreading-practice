using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;

public class PerlinScroller : MonoBehaviour
{
    int cubeCount;
    public int width = 100;
    public int height = 100;
    public int layers = 2;

    GameObject[] cubes;
    Transform[] cubeTransforms;
    TransformAccessArray cubeTransformAccessArray;
    PositionUpdateJob cubeJob;
    JobHandle cubePositonJobHandle;

    Vector3 lastPos = Vector3.zero;

    struct PositionUpdateJob : IJobParallelForTransform
    {
        public int height;
        public int width;
        public int layers;
        public int xoffset;
        public int zoffset;

        public void Execute(int i, TransformAccess transform)
        {
            int x = i / (width * layers);
            int z = (i - x * height * layers) / layers;
            int yoffset = i - x * width * layers - z * layers;

            transform.position = new Vector3(x+xoffset,
                                                      GeneratePerlinHeight(x + xoffset, z + zoffset) + yoffset,
                                                      z + zoffset);
        }
    }

    private void Awake()
    {
        cubeCount = width * height * layers;
        cubes = new GameObject[cubeCount];
        cubeTransforms = new Transform[cubeCount];
    }

    // Start is called before the first frame update
    void Start()
    {
        cubes = CreateCubes(cubeCount);
        for (int i = 0; i < cubeCount; i++)
        {
            cubeTransforms[i] = cubes[i].transform;
        }

        cubeTransformAccessArray = new TransformAccessArray(cubeTransforms);
    }

    public GameObject[] CreateCubes(int count)
    {
        GameObject[] cubes = new GameObject[count];
        GameObject cubeToCopy = GameObject.CreatePrimitive(PrimitiveType.Cube);
        MeshRenderer renderer = cubeToCopy.GetComponent<MeshRenderer>();
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        Collider collider = cubeToCopy.GetComponent<Collider>();
        collider.enabled = false;

        for (int i = 0; i < count; i++)
        {
            GameObject currentCube = Instantiate(cubeToCopy);
            int x = i / (width * layers);
            int y = (i - x * height * layers) / layers;
            int z = i - x * width * layers - y * layers;
            cubeToCopy.transform.position = new Vector3(x, 0, y);
            cubes[i] = currentCube;
        }

        Destroy(cubeToCopy);

        return cubes;
    }

    int xoffset = 0;
    private void Update()
    {
        #region oldLoop
        //int xoffset = (int)(transform.position.x - width / 2.0f);
        //xoffset++;
        //int zoffset = (int)(transform.position.z - height / 2.0f);

        //for (int i = 0; i < cubeCount; i++)
        //{
        //    int x = i / (width * layers);
        //    int z = (i - x * height * layers) / layers;
        //    int yoffset = i - x * width * layers - z * layers;

        //    cubes[i].transform.position = new Vector3(x,
        //                                              GeneratePerlinHeight(x + xoffset, z + zoffset) + yoffset,
        //                                              z + zoffset);
        //}
        #endregion

        if (Input.GetKey(KeyCode.UpArrow))
            transform.Translate(0, 0, 2);
        else if(Input.GetKey(KeyCode.DownArrow))
            transform.Translate(0, 0, -2);
        else if (Input.GetKey(KeyCode.LeftArrow))
            transform.Translate(-2, 0, 0);
        else if (Input.GetKey(KeyCode.RightArrow))
            transform.Translate(2, 0, 0);

        if (transform.position == lastPos) return;
        lastPos = transform.position;

        cubeJob = new PositionUpdateJob()
        {
            xoffset = (int)(transform.position.x - width / 2.0f),
            zoffset = (int)(transform.position.z - height / 2.0f),
            height = height,
            width = width,
            layers = layers
        };

        cubePositonJobHandle = cubeJob.Schedule(cubeTransformAccessArray);

    }

    private void LateUpdate()
    {
        if (transform.position == lastPos) return;
        cubePositonJobHandle.Complete();
    }

    private void OnDestroy()
    {
        // Prevents a memory leak
        cubeTransformAccessArray.Dispose();
    }

    static float GeneratePerlinHeight(float posx, float posz)
    {
        float smooth = 0.03f;
        float heightMult = 5;
        float height = (Mathf.PerlinNoise(posx * smooth, posz * smooth * 2) * heightMult
                       + Mathf.PerlinNoise(posx * smooth, posz * smooth * 2) * heightMult) / 2;

        return height * 10;
    }
}
