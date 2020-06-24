using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;
using Microsoft.Kinect.Face;

public class HDFaceSourceManager : MonoBehaviour
{
    private KinectSensor kinectSensor;
    private int bodyCount;
    private Body[] bodies;
    private HighDefinitionFaceFrameSource[] hdFaceFrameSources;
    private HighDefinitionFaceFrameReader[] hdFaceFrameReaders;

    public GameObject bodyManager;
    public GameObject hdFaceDot;

    // Required to access the face vertices.
    private FaceAlignment _faceAlignment = null;
    // Required to access the face model points.
    private FaceModel _faceModel = null;

    
    // Start is called before the first frame update
    void Start()
    {
        kinectSensor = KinectSensor.GetDefault();
        bodyCount = kinectSensor.BodyFrameSource.BodyCount;
        bodies = new Body[bodyCount];
        hdFaceFrameSources = new HighDefinitionFaceFrameSource[bodyCount];
        hdFaceFrameReaders = new HighDefinitionFaceFrameReader[bodyCount];
        _faceModel = FaceModel.Create();
        _faceAlignment = FaceAlignment.Create();
        for (int i = 0; i < bodyCount; i++)
        {
            hdFaceFrameSources[i] = HighDefinitionFaceFrameSource.Create(kinectSensor);
            hdFaceFrameReaders[i] = hdFaceFrameSources[i].OpenReader();
        }
        kinectSensor.Open();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        var bodySourceManager = bodyManager.GetComponent<BodySourceManager>();
        bodies = bodySourceManager.GetData();
        if (bodies == null)
        {
            Debug.Log("No Bodies");
            return;
        }
        Debug.Log("One loop");
        for (int i = 0; i < bodyCount; i++)
        {
            if (hdFaceFrameSources[i].IsTrackingIdValid)
            {
                using (var frame = hdFaceFrameReaders[i].AcquireLatestFrame())
                {
                    if (frame != null && frame.IsFaceTracked)
                    {
                        frame.GetAndRefreshFaceAlignmentResult(_faceAlignment);
                        var vertices = _faceModel.CalculateVerticesForAlignment(_faceAlignment);

                        for (int index = 0; index < vertices.Count; index++)
                        {
                            if(index%5 == 0)
                            {
                                CameraSpacePoint vertice = vertices[index];
                                ColorSpacePoint point = kinectSensor.CoordinateMapper.MapCameraPointToColorSpace(vertice);
                                generateHdPoint(point);
                            }
                            
                        }
                        
                            
                    }
                }
            }
            else
            {
                // check if the corresponding body is tracked 
                if (bodies[i].IsTracked)
                {
                    // update the face frame source to track this body
                    hdFaceFrameSources[i].TrackingId = bodies[i].TrackingId;
                }
            }

        }


    }
    void generateHdPoint(ColorSpacePoint point)
    {
        float xx = (point.X / 100) - 9.6f;
        float yy = 5.4f - (point.Y / 100);
        GameObject newHdFaceDot = Instantiate(hdFaceDot, new Vector3(xx, yy, 0.5f), Quaternion.identity) as GameObject;

    }
}
